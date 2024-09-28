﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Microsoft.Windows.Sdk;

public class Log
{
    public static readonly bool VTEnabled;
    public static bool VerboseEnabled = true;

    static Log()
    {
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch (IOException)
        {
        }

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (!Console.IsOutputRedirected)
            {
                var h = new SafeFileHandle(PInvoke.GetStdHandle(STD_HANDLE_TYPE.STD_OUTPUT_HANDLE).Value, false);
                if (!h.IsInvalid && PInvoke.GetConsoleMode(h, out var mode))
                    VTEnabled = mode.HasFlag(CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING)
                        || PInvoke.SetConsoleMode(h, mode | CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }
        else
            VTEnabled = !Console.IsOutputRedirected;
    }

    public static void SetTitle(string _x)
    {
        try
        {
            Console.Title = _x;
        }
        catch (IOException)
        {
        }
    }

    public static float Progress
    {
        set
        {
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException();

            if (!VTEnabled)
                return;

            if (value == 1)
                Console.Write("\x1b]9;4;0;100\x07");
            else
                Console.Write($"\x1b]9;4;1;{(int)(value * 99)}\x07");
        }
    }

    public static void WriteLine() => WriteLine("");
    public static void WriteLine(object? _o) => WriteLine(_o?.ToString());
    public static void WriteLine(string? _x)
    {
        Console.WriteLine(_x);
    }

    public static void VerboseLine() => VerboseLine("");
    public static void VerboseLine(object? _o) => VerboseLine($"{_o}");
    public static void VerboseLine(string? _x)
    {
        if (VerboseEnabled)
            Console.WriteLine($"       > {_x}".StyleDarkYellow());
    }

    [Conditional("DEBUG")]
    public static void Debug(string _x)
    {
        Console.WriteLine(_x);
    }

    public static void LimitOutputTo(int? _lines = null)
    {
        if (!VTEnabled)
            return;

        if (_lines is null)
        {
            var cursorY = Console.CursorTop;
            Console.Write("\u001b[;r");
            Console.CursorTop = cursorY;
            return;
        }

        if (_lines < 1)
            throw new ArgumentOutOfRangeException(nameof(_lines));

        var lines = System.Math.Min(_lines.Value, Console.WindowHeight);
        Console.Write(string.Join("", Enumerable.Repeat(Environment.NewLine, lines)));

        {
            var cursorY = Console.CursorTop;
            var marginEnd = System.Math.Min(Console.WindowHeight, cursorY + 1);
            Console.Write($"\u001b[{marginEnd - lines};{marginEnd}r");
            Console.CursorTop = cursorY;
        }
    }

    public class MultiLine
    {
        private List<Line> Lines = new();
        private static object ConsoleLock = new();

        public Line AddLine(string _text = "")
        {
            lock (ConsoleLock)
            {
                Console.WriteLine(_text);
                var after = Console.CursorTop;

                var l = new Line();
                Lines.Add(l);

                for (var i = 0; i < Lines.Count; i++)
                    Lines[i].Y = Math.Min(0, after - Lines.Count + i);

                return l;
            }
        }

        public class Line
        {
            internal int Y;

            public string Text
            {
                set
                {
                    lock (ConsoleLock)
                    {
                        if (!VTEnabled)
                        {
                            Console.WriteLine(value);
                            return;
                        }

                        var before = Console.GetCursorPosition();

                        Console.CursorTop = Y;
                        Console.CursorLeft = 0;

                        Console.Write($"\u001b[2K\u001b[?7l{value}\u001b[?7h");

                        Console.SetCursorPosition(before.Left, before.Top);
                    }
                }
            }
        }
    }

    public class Table
    {
        internal string[] Headers;
        internal int[] Widths;
        internal bool[] BoldColumns;
        public int WrittenLines { get; private set; } = 0;
        public int RepeatHeaderEvery { get; init; } = 0;

        public Table(IEnumerable<string> _headers, IEnumerable<int>? _widths = null, IEnumerable<bool>? _boldColumns = null)
        {
            Headers = _headers.ToArray();

            if (_widths is null)
                Widths = Headers.Select(h => Math.Max(10, h.Length)).ToArray();
            else
                Widths = _widths.Select(w => Math.Max(10, w)).ToArray();

            if (_boldColumns is null)
                BoldColumns = new bool[Headers.Length];
            else
                BoldColumns = _boldColumns.ToArray();

            if (Widths.Length != Headers.Length)
                throw new ArgumentException(nameof(_widths));
            if (BoldColumns.Length != Headers.Length)
                throw new ArgumentException(nameof(_boldColumns));
        }

        private static string Format(long _x, int _width) => _x switch
        {
            >= 100000000 or <= -10000000 => $"{_x,10:0.00e0}".PadLeft(_width),
            _ => $"{_x,10:#,0}".PadLeft(_width)
        };

        private string Format(double _x, int _width) => _x switch
        {
            0 => "0.0000".PadLeft(_width),
            >= 100000000 or <= -10000000 => $"{_x,10:0.00e0}".PadLeft(_width),
            > -0.0002 and < 0.0002 => $"{_x,10:0.00e0}".PadLeft(_width),
            _ => $"{_x,10:0.000,0}".PadLeft(_width)
        };

        private static string Format(ulong _x, int _width) => _x switch
        {
            >= 100000000 => $"{_x,10:0.00e0}".PadLeft(_width),
            _ => $"{_x,10:#,0}".PadLeft(_width)
        };

        private static string Format(string _s, int _width) =>
            _s.FixLengthToAndKeepFormatting(_width);

        private static string Format(TimeSpan _ts, int _width) =>
            $"{(int)_ts.TotalMinutes,5}:{_ts:ss\\.f}".PadLeft(_width);

        public void WriteHeader()
        {
            Log.WriteLine(string.Join(" ", Headers.Select((h, i) => Format(h, Widths[i])))
                .StyleBrightCyan()
                .StyleUnderline());
        }

        public void WriteLine(params object?[] _data) =>
            WriteLineFormatted(false, _data);

        public void WriteBoldLine(params object?[] _data) =>
            WriteLineFormatted(true, _data);

        private void WriteLineFormatted(bool _boldLine, params object?[] _data)
        {
            if (WrittenLines == 0 || (RepeatHeaderEvery > 0 && (WrittenLines % RepeatHeaderEvery) == 0))
                WriteHeader();

            Log.WriteLine(string.Join(" ", _data.Select((o, i) =>
            {
                var res = o switch
                {
                    short x => Format(x, Widths[i]),
                    ushort x => Format(x, Widths[i]),
                    int x => Format(x, Widths[i]),
                    uint x => Format(x, Widths[i]),
                    long x => Format(x, Widths[i]),
                    ulong x => Format(x, Widths[i]),
                    byte x => Format(x, Widths[i]),
                    sbyte x => Format(x, Widths[i]),
                    float x => Format(x, Widths[i]),
                    double x => Format(x, Widths[i]),
                    null => "".PadLeft(Widths[i]),
                    string s => Format(s, Widths[i]),
                    TimeSpan ts => Format(ts, Widths[i]),
                    _ => Format(o?.ToString() ?? "", Widths[i])
                };
                if (BoldColumns[i] || _boldLine)
                    res = res.StyleBold();
                return res;
            })));

            WrittenLines++;
        }
    }
}

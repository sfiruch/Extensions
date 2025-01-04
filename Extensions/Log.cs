using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.System.Console;

public class Log
{
    public static readonly bool VTEnabled;
    public static bool VerboseEnabled = true;
    public static readonly Lock ConsoleLock = new Lock();

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
                var h = new SafeFileHandle(PInvoke.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE).Value, false);
                if (!h.IsInvalid && PInvoke.GetConsoleMode(h, out var mode))
                    VTEnabled = PInvoke.SetConsoleMode(h, mode | CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
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
            if (!VTEnabled)
                return;

            lock (ConsoleLock)
                if (value >= 1)
                    Console.Write("\e]9;4;0;100\x07");
                else
                    Console.Write($"\e]9;4;1;{(int)(Math.Max(0, value) * 99)}\x07");
        }
    }

    public static void WriteLine() => WriteLine("");
    public static void WriteLine(object? _o) => WriteLine(_o?.ToString());
    public static void WriteLine(string? _x)
    {
        lock (ConsoleLock)
            Console.WriteLine(_x);
    }

    public static void VerboseLine() => VerboseLine("");
    public static void VerboseLine(object? _o) => VerboseLine($"{_o}");
    public static void VerboseLine(string? _x)
    {
        if (VerboseEnabled)
            lock (ConsoleLock)
                Console.WriteLine($"       > {_x}".StyleDarkYellow());
    }

    [Conditional("DEBUG")]
    public static void Debug(string _x)
    {
        lock (ConsoleLock)
            Console.WriteLine(_x);
    }

    public static void LimitOutputTo(int? _lines = null)
    {
        if (!VTEnabled)
            return;

        lock (ConsoleLock)
        {
            if (statusLines.Count != 0)
                throw new InvalidOperationException("Output limiting only supported while no status lines are shown.");

            if (_lines is null)
            {
                var cursorY = Console.CursorTop;
                Console.Write("\e[;r");
                Console.CursorTop = cursorY;
                return;
            }

            if (_lines < 1)
                throw new ArgumentOutOfRangeException(nameof(_lines));

            var lines = Math.Min(_lines.Value, Console.WindowHeight);
            Console.Write(string.Join("", Enumerable.Repeat(Environment.NewLine, lines)));

            {
                var cursorY = Console.CursorTop;
                var marginEnd = Math.Min(Console.WindowHeight, cursorY + 1);
                Console.Write($"\e[{marginEnd - lines};{marginEnd}r");
                Console.CursorTop = cursorY;
            }
        }
    }

    private static readonly List<Line> statusLines = new();
    public static Line AddStatusLine(string _text = "", string? _prefix = null, float? _progress = null)
    {
        lock (ConsoleLock)
        {
            var newLine = new Line(_prefix, _text, _progress);
            statusLines.Add(newLine);

            if (VTEnabled)
            {
                using (var _ = new ConsoleStateRestorer())
                {
                    if (statusLines.Count == 1)
                        Console.Write($"\e[;r\e[1;H\e[1L\e[1;r\e[;r\e[2;H\e[1L\e[2;r");

                    Console.Write($"\e[;r\e[{2 + statusLines.Count};H\e[1L\e[{3 + statusLines.Count};r");
                    UpdateStatusLines();
                }

                if (statusLines.Count == 1)
                    Console.Write("\e[3B");
                else
                    Console.Write("\e[B");
            }

            return newLine;
        }
    }

    internal class ConsoleStateRestorer : IDisposable
    {
        int Left;
        int Top;
        ConsoleColor Foreground;
        ConsoleColor Background;

        internal ConsoleStateRestorer(int _yOffset = 0)
        {
            (Left, Top) = Console.GetCursorPosition();
            Top += _yOffset;
            if (Top < 0)
                Top = 0;
            Foreground = Console.ForegroundColor;
            Background = Console.BackgroundColor;
            Console.Write("\e[39m\e[?25l");
        }

        public void Dispose()
        {
            try
            {
                Console.SetCursorPosition(Left, Top);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
            Console.Write("\e[?25h");
            Console.ForegroundColor = Foreground;
            Console.BackgroundColor = Background;
        }
    }

    private static void UpdateStatusLines()
    {
        lock (ConsoleLock)
        {
            if (statusLines.Count == 0)
                return;

            void WriteLine(int row, string text)
                => Console.Write($"\e[48;5;235m\e[{2 + row};H\e[2K\e[?7l{text}\e[?7h");

            var barWidth = Math.Clamp(Console.WindowWidth / 4, 5, 30);

            using var _ = new ConsoleStateRestorer();
            WriteLine(-1, "");
            for (var i = 0; i < statusLines.Count; i++)
                WriteLine(i, $" {statusLines[i].GetConsoleText(barWidth)} ");

            WriteLine(statusLines.Count, "");
        }
    }

    public class Line : IDisposable
    {
        private string? _Prefix;
        private string _Text;
        private float? _Progress;
        internal bool Active = true;
        private System.Threading.Timer? RemovalTimer;

        private static volatile System.Threading.Timer? UpdateTimer = null;
        private volatile System.Threading.Timer? UpdateLineTimer = null;

        internal Line(string? _prefix, string _text, float? _progress)
        {
            _Prefix = _prefix?.Replace('\n', ' ');
            _Text = _text.Replace('\n', ' ');
            _Progress = _progress;

            if (!VTEnabled)
                AsyncUpdate();
        }

        internal string GetConsoleText(int barWidth)
        {
            string progress = "";
            if (Progress is float p)
            {
                var barFilled = (int)Math.Round(p * barWidth);

                var firstPart = $"{p,4:P0} {new string(VTEnabled ? '━' : '#', barFilled)}";

                var secondPart = new string(VTEnabled ? '━' : '.', barWidth - barFilled);

                if (Active)
                {
                    if (p >= 0.995)
                        firstPart = firstPart.StyleBrightGreen();
                    else
                        firstPart = firstPart.StyleOrange();
                }
                else
                {
                    if (p >= 0.995)
                        firstPart = firstPart.StyleDarkGreen();
                    else
                        firstPart = firstPart.StyleDarkYellow();
                    secondPart = secondPart.StyleDarkGray();
                }

                progress = $"{firstPart}{secondPart} ";
            }

            string text;
            if (Active)
            {
                text = $"{progress}{Text}";
                if (Prefix is not null)
                    text = $"{Prefix}: {text}";
            }
            else
            {
                text = $"{progress}{Text.StyleDarkGray()}";
                if (Prefix is not null)
                    text = $"{Prefix.StyleDarkGray()}: {text}";
            }
            return text;
        }

        private void AsyncUpdate()
        {
            if (VTEnabled)
                UpdateTimer ??= new System.Threading.Timer(_ =>
                {
                    UpdateTimer = null;
                    UpdateStatusLines();
                }, null, 50, Timeout.Infinite);
            else
                UpdateLineTimer ??= new System.Threading.Timer(_ =>
                {
                    UpdateLineTimer = null;

                    lock (ConsoleLock)
                        Console.WriteLine(GetConsoleText(8));
                }, null, 250, Timeout.Infinite);
        }

        public string? Prefix
        {
            get => _Prefix;
            set
            {
                var oldValue = _Prefix;
                _Prefix = value?.Replace('\n', ' ');
                if (_Prefix != oldValue)
                    AsyncUpdate();
            }
        }

        public string Text
        {
            get => _Text;
            set
            {
                var oldValue = _Text;
                _Text = value.Replace('\n', ' ');
                if (oldValue != _Text)
                    AsyncUpdate();
            }
        }

        public float? Progress
        {
            get => _Progress;
            set
            {
                var oldValue = _Progress;
                if (value is null)
                    _Progress = null;
                else
                    _Progress = Math.Clamp(value.Value, 0, 1);

                if (oldValue != _Progress)
                    AsyncUpdate();
            }
        }

        public void Dispose() => Remove();

        public void Remove(TimeSpan? Delay = null)
        {
            Active = false;

            if (!VTEnabled)
            {
                lock (ConsoleLock)
                {
                    statusLines.Remove(this);
                    return;
                }
            }

            Delay ??= TimeSpan.FromSeconds(2);

            if (Delay.Value != TimeSpan.Zero)
            {
                RemovalTimer ??= new System.Threading.Timer(_ => Remove(TimeSpan.Zero), null, Delay.Value, Timeout.InfiniteTimeSpan);
                return;
            }

            lock (ConsoleLock)
            {
                var index = statusLines.IndexOf(this);
                if (index == -1)
                    return;

                statusLines.RemoveAt(index);

                using var _ = new ConsoleStateRestorer(-1);
                Console.WriteLine($"\e[;r\e[{3 + index};H\e[1M\e[{3 + statusLines.Count};r");

                if (statusLines.Count == 0)
                {
                    Console.WriteLine($"\e[;r\e[2;H\e[1M\e[3;r");
                    Console.WriteLine($"\e[;r\e[1;H\e[1M\e[2;r");
                }

                UpdateStatusLines();
            }

            RemovalTimer?.Dispose();
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
                .Style(_underline: true));
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
                    res = res.Style(_bold: true);
                return res;
            })));

            WrittenLines++;
        }
    }
}

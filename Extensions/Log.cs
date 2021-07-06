using System;
using System.Diagnostics;
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
        Console.OutputEncoding = Encoding.UTF8;

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
        Console.Title = _x;
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

    public class Table
    {
        internal string[] Headers;
        internal int Lines = 0;

        public Table(string[] _headers)
        {
            Headers = _headers;
        }

        private static string Format(long _x) => _x switch
        {
            >= 100000000 => $"{_x,10:0.00e0}",
            <= -10000000 => $"{_x,10:0.00e0}",
            _ => $"{_x,10:#,0}"
        };

        private static string Format(ulong _x) => _x switch
        {
            >= 100000000 => $"{_x,10:0.00e0}",
            _ => $"{_x,10:#,0}"
        };

        private static string Format(string _s) =>
            _s.Length > 10 ? _s[0..9] + "…" : _s.PadRight(10);

        private static string Format(TimeSpan _ts) =>
            $"{(int)_ts.TotalMinutes,5}:{_ts:ss\\.f}";

        public void WriteHeader()
        {
            Log.WriteLine(string.Join(" ", Headers.Select(h => h.PadRight(10)))
                .StyleBrightCyan()
                .StyleUnderline());
        }

        public void WriteLine(params object[] _data)
        {
            if ((Lines % 50) == 0)
                WriteHeader();

            Log.WriteLine(string.Join(" ", _data.Select(o => (o switch
            {
                short x => Format(x),
                ushort x => Format(x),
                int x => Format(x),
                uint x => Format(x),
                long x => Format(x),
                ulong x => Format(x),
                byte x => Format(x),
                sbyte x => Format(x),
                null => "".PadLeft(10),
                string s => Format(s),
                TimeSpan ts => Format(ts),
                _ => o?.ToString()
            }))));

            Lines++;
        }
    }
}

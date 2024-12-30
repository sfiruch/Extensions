using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

public static class StringExtensions
{
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkRed(this string? _s) => Log.VTEnabled ? $"\e[31m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkGreen(this string? _s) => Log.VTEnabled ? $"\e[32m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkYellow(this string? _s) => Log.VTEnabled ? $"\e[33m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkBlue(this string? _s) => Log.VTEnabled ? $"\e[34m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkMagenta(this string? _s) => Log.VTEnabled ? $"\e[35m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkCyan(this string? _s) => Log.VTEnabled ? $"\e[36m{_s}\e[39m" : _s;

    [return: NotNullIfNotNull("_s")] public static string? StyleBrightRed(this string? _s) => Log.VTEnabled ? $"\e[91m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightGreen(this string? _s) => Log.VTEnabled ? $"\e[92m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightYellow(this string? _s) => Log.VTEnabled ? $"\e[93m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightBlue(this string? _s) => Log.VTEnabled ? $"\e[94m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightMagenta(this string? _s) => Log.VTEnabled ? $"\e[95m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightCyan(this string? _s) => Log.VTEnabled ? $"\e[96m{_s}\e[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightWhite(this string? _s) => Log.VTEnabled ? $"\e[97m{_s}\e[39m" : _s;

    [return: NotNullIfNotNull("_s")] public static string? StyleOrange(this string? _s) => Log.VTEnabled ? $"\e[38;5;214m{_s}\e[39m" : _s;

    [return: NotNullIfNotNull("_s")] public static string? Style(this string? _s, bool _underline = false, bool _italic = false, bool _bold = false, bool _strikethrough = false) => Log.VTEnabled ? $"\e[{(_bold ? "1" : "")}{(_underline ? "4" : "")}{(_italic ? "3" : "")}{(_strikethrough ? "9" : "")}m{_s}\e[m" : _s;

    public static string FixLengthToAndKeepFormatting(this string _s, int _length)
    {
        var len = 0;
        {
            var inVT = false;
            foreach (var ch in _s)
            {
                inVT |= (ch == '\e');

                if (!inVT)
                    len++;

                inVT &= (ch != 'm');
            }
        }

        if (len == _length)
            return _s;
        else if (len < _length)
            return _s + new string(' ', _length - len);

        var res = new StringBuilder();
        {
            var resLen = 0;
            var inVT = false;
            foreach (var ch in _s)
            {
                inVT |= (ch == '\e');

                if (inVT)
                    res.Append(ch);
                else
                {
                    resLen++;
                    if (resLen < _length)
                        res.Append(ch);
                    if (resLen == _length)
                        res.Append('…');
                }

                inVT &= (ch != 'm');
            }
        }
        return res.ToString();
    }

    /// <summary>
    /// Computes a ASCII histogram for the supplied set of numbers. It will
    /// return (2+_buckets) chars. Elements outside of _min _max will be clipped
    /// to the corresponding boundaries.
    /// </summary>
    /// <param name="_e">Numbers to compute the histogram for</param>
    /// <param name="_buckets">Number of buckets</param>
    /// <param name="_min">Optional: Lower bound of first bucket</param>
    /// <param name="_max">Optional: Upper bound of last bucket</param>
    /// <returns></returns>
    public static string ToHistogram(this IEnumerable<double> _e, int _buckets = 10, double _min = double.NaN, double _max = double.NaN)
    {
        if (!_e.Any())
            return $"{new string('-', _buckets)}";

        if (double.IsNaN(_min))
            _min = _e.Min();

        if (double.IsNaN(_max))
            _max = _e.Max();

        if (_min > _max)
            _min = _max;

        if (_min == _max)
        {
            _min--;
            _max++;
        }

        var hist = new int[_buckets];
        foreach (var v in _e)
        {
            var slot = (int)(_buckets * (v - _min) / (_max - _min));
            if (slot < 0)
                slot = 0;
            if (slot > _buckets - 1)
                slot = _buckets - 1;

            hist[slot]++;
        }

        int maxCount = hist.Max();
        //string bars = " ▁▂▃▄▅▆▇█"; //not that well-supported by most fonts
        //string bars = " ░▒▓█";
        string bars = " -=≡";
        //string bars = " _▄█";
        //string bars = " ─▬■█";

        var histChar = hist.Select(v =>
        {
            if (v == 0)
                return bars[0];

            return bars[System.Math.Clamp(1 + (int)((bars.Length - 1) * (double)(v - 1) / (maxCount - 1)), 1, bars.Length - 1)];
        });

        return $"{new string(histChar.ToArray())}";
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

public static class StringExtensions
{
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkRed(this string? _s) => Log.VTEnabled ? $"\u001b[31m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkGreen(this string? _s) => Log.VTEnabled ? $"\u001b[32m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkYellow(this string? _s) => Log.VTEnabled ? $"\u001b[33m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkBlue(this string? _s) => Log.VTEnabled ? $"\u001b[34m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkMagenta(this string? _s) => Log.VTEnabled ? $"\u001b[35m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleDarkCyan(this string? _s) => Log.VTEnabled ? $"\u001b[36m{_s}\u001b[39m" : _s;

    [return: NotNullIfNotNull("_s")] public static string? StyleBrightRed(this string? _s) => Log.VTEnabled ? $"\u001b[91m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightGreen(this string? _s) => Log.VTEnabled ? $"\u001b[92m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightYellow(this string? _s) => Log.VTEnabled ? $"\u001b[93m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightBlue(this string? _s) => Log.VTEnabled ? $"\u001b[94m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightMagenta(this string? _s) => Log.VTEnabled ? $"\u001b[95m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightCyan(this string? _s) => Log.VTEnabled ? $"\u001b[96m{_s}\u001b[39m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBrightWhite(this string? _s) => Log.VTEnabled ? $"\u001b[97m{_s}\u001b[39m" : _s;

    [return: NotNullIfNotNull("_s")] public static string? StyleUnderline(this string? _s) => Log.VTEnabled ? $"\u001b[4m{_s}\u001b[24m" : _s;
    [return: NotNullIfNotNull("_s")] public static string? StyleBold(this string? _s) => Log.VTEnabled ? $"\u001b[1m{_s}\u001b[22m" : _s;

    public static string FixLengthToAndKeepFormatting(this string _s, int _length)
    {
        var len = 0;
        {
            var inVT = false;
            foreach (var ch in _s)
            {
                inVT |= (ch == '\u001b');

                if (!inVT)
                    len++;

                inVT &= (ch != 'm');
            }
        }

        if (len == _length)
            return _s;
        else if (len < _length)
            return _s.PadRight(_length);
        
        var res=new StringBuilder();
        {
            var resLen = 0;
            var inVT = false;
            foreach (var ch in _s)
            {
                inVT |= (ch == '\u001b');

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

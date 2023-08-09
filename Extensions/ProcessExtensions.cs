using Microsoft.Win32.SafeHandles;
using Microsoft.Windows.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class ProcessExtensions
{
    [DllImport("libc.so.6", SetLastError = true)]
    private static extern int sched_setaffinity(int pid, IntPtr cpusetsize, ulong[] cpuset);

    public static TimeSpan Time
    {
        get => Process.GetCurrentProcess().TotalProcessorTime;
    }

    public static void AssignCurrentThreadToCore(int _core)
    {
        if (_core < 0 || _core >= 64)
            throw new ArgumentOutOfRangeException(nameof(_core));

        Thread.BeginThreadAffinity();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (PInvoke.SetThreadAffinityMask(new UnownedSafeHandle(PInvoke.GetCurrentThread()), 1u << _core) == 0)
                throw new Win32Exception();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            sched_setaffinity(0, new IntPtr(sizeof(ulong)), new[] { (1uL << _core) });
        }
    }

    public static void AssignCurrentThreadToAllCores()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (PInvoke.SetThreadAffinityMask(new UnownedSafeHandle(PInvoke.GetCurrentThread()), nuint.MaxValue) == 0)
                throw new Win32Exception();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            sched_setaffinity(0, new IntPtr(sizeof(ulong)), new[] { ulong.MaxValue });
        }

        Thread.EndThreadAffinity();
    }

    private static long tempFilenameCounter = 0;
    public static string GetTempFileName()
    {
        for (; ; )
        {
            var hc = new HashCode();
            hc.Add(DateTime.Now.Ticks);
            var a = hc.ToHashCode();
            hc.Add(Interlocked.Increment(ref tempFilenameCounter));
            var b = hc.ToHashCode();
            var fn = Path.Combine(Path.GetTempPath(), $"{Process.GetCurrentProcess().ProcessName}-{a}-{b}.tmp");

            try
            {
                using (var fs = new FileStream(fn, FileMode.CreateNew))
                    return fn;
            }
            catch (IOException ioe) when (ioe.HResult == unchecked((int)0x800700B7) || ioe.HResult == unchecked((int)0x80070050))
            {
                //file already exists, try again
            }
        }
    }
}

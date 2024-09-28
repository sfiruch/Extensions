using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
    internal class MemoryConsoleLogger
    {
        private static long lastTotalAllocatedReset = 0;
        private static readonly NumberFormatInfo nfi = new NumberFormatInfo()
        {
            NumberDecimalDigits = 0,
            NumberGroupSeparator = "'",

        };

        private static void Log(string _s)
            => Console.WriteLine($"[GC] {_s}".StyleBrightYellow().Style(_italic: true));

        static MemoryConsoleLogger()
        {
            var config = GC.GetConfigurationVariables();

            var sServerGC = "?";
            {
                if (config.TryGetValue("ServerGC", out var o) && o is bool)
                    sServerGC = (bool)o ? "ServerGC" : "WorkstationGC";
            }

            var sConcurrentGC = "?";
            {
                if (config.TryGetValue("ConcurrentGC", out var o) && o is bool)
                    sConcurrentGC = (bool)o ? "concurrent" : "blocking";
            }

            var sDATAS = "?";
            {
                if (config.TryGetValue("GCDynamicAdaptationMode", out var o) && o is long)
                    sDATAS = ((long)o == 0) ? "no DATAS" : $"DATAS({(long)o})";
            }

            Log($"Using {sConcurrentGC} {sServerGC} with {sDATAS} for garbage collection");

            var loggerThread = new Thread(() =>
            {
                for (; ; )
                {
                    //The WaitForFullGCApproach and the WaitForFullGCComplete methods are designed to work
                    //together. Using one without the other can produce indeterminate results.
                    if (GC.WaitForFullGCApproach() != GCNotificationStatus.Succeeded ||
                        GC.WaitForFullGCComplete() != GCNotificationStatus.Succeeded)
                    {
                        Log($"Notifications stopped");
                        break;
                    }

                    var info = GC.GetGCMemoryInfo(GCKind.FullBlocking);
                    if (info.Generation != 2)
                        throw new InvalidOperationException();

                    var processWorkingSet = Process.GetCurrentProcess().WorkingSet64;
                    var managedHeap = info.HeapSizeBytes;
                    var totalAllocated = GC.GetTotalAllocatedBytes(false) - lastTotalAllocatedReset;

                    Log(string.Create(nfi, $"Gen2 {totalAllocated / 1024 / 1024:N} MB managed total, {managedHeap / 1024 / 1024:N} MB managed active, {processWorkingSet / 1024 / 1024:N0} MB working set"));
                }
            });
            loggerThread.Name = "Memory Logger";
            loggerThread.Priority = ThreadPriority.Highest;
            loggerThread.IsBackground = true;
            GC.RegisterForFullGCNotification(99, 99);
            loggerThread.Start();
        }


        public static void Start()
        {
            lastTotalAllocatedReset = GC.GetTotalAllocatedBytes(false);
            GC.Collect(int.MaxValue, GCCollectionMode.Forced, true, true);
        }
    }
}

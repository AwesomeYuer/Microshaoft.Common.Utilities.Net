//#define NET35

namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    public static class CodeTimerPerformance
    {
        public static void Initialize()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Time("", 1, () => { }, MultiPerformanceCountersTypeFlags.None, string.Empty, string.Empty);
        }
        //public static void AttachPerformanceCountersCategoryInstance
        //            (
        //                string performanceCountersCategoryName
        //                , string performanceCountersCategoryInstanceName
        //            )
        //{
        //    SessionsPerformanceCountersContainer sessionsPerformanceCountersContainer = null;
        //    EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
        //        .AttachPerformanceCountersCategoryInstance
        //            (
        //                performanceCountersCategoryName
        //                , performanceCountersCategoryInstanceName
        //                , out sessionsPerformanceCountersContainer
        //            );
        //}
        public static void ParallelTime
                                (
                                    string name
                                    , int iterations
                                    , Action actionOnce
                                    , int maxDegreeOfParallelism //= 1
                                    , MultiPerformanceCountersTypeFlags enablePerformanceCounters //= false
                                    , string performanceCountersCategoryName
                                    , string performanceCountersCategoryInstanceName
                                )
        {
            // 1.
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(name);
            // 2.
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }
            IntPtr threadID = GetCurrentThreadId();
            Stopwatch watch = Stopwatch.StartNew();
            ulong cycleCount = GetCurrentThreadCycleCount();

            Parallel.For
                        (
                            0
                            , iterations
                            , new ParallelOptions()
                            {
                                MaxDegreeOfParallelism = maxDegreeOfParallelism
                                //, TaskScheduler = null
                            }
                            , (x) =>
                            {
                                //EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>.CountPerformance
                                //                (
                                //                    enablePerformanceCounters
                                //                    , performanceCountersCategoryName
                                //                    , performanceCountersCategoryInstanceName
                                //                    , null
                                //                    , actionOnce
                                //                    , null
                                //                );
                            }
                        );
            ulong cpuCycles = GetCurrentThreadCycleCount() - cycleCount;
            watch.Stop();
            //watch = null;
            // 4.
            Console.ForegroundColor = currentForeColor;
            Console.WriteLine
                            (
                                "{0}Time Elapsed:{0}{1}ms"
                                , "\t"
                                , watch.ElapsedMilliseconds.ToString("N0")
                            );
            Console.WriteLine
                            (
                                "{0}CPU Cycles:{0}{1}"
                                , "\t"
                                , cpuCycles.ToString("N0")
                            );
            // 5.
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                Console.WriteLine
                            (
                                "{0}Gen{1}:{0}{0}{2}"
                                , "\t"
                                , i
                                , count
                            );
            }
            Console.WriteLine();
        }
        public static void Time
                            (
                                string name
                                , int iterations
                                , Action actionOnce
                                , MultiPerformanceCountersTypeFlags enablePerformanceCounters //= false
                                , string performanceCountersCategoryName
                                , string performanceCountersCategoryInstanceName
                            )
        {
            ParallelTime
                        (
                            name
                            , iterations
                            , actionOnce
                            , 1
                            , enablePerformanceCounters
                            , performanceCountersCategoryName
                            , performanceCountersCategoryInstanceName
                        );
        }
        private static ulong GetThreadCycleCount(IntPtr threadID)
        {
            ulong cycleCount = 0;
            QueryThreadCycleTime(threadID, ref cycleCount);
            return cycleCount;
        }
        private static ulong GetCurrentThreadCycleCount()
        {
            IntPtr threadID = GetCurrentThread();
            return GetThreadCycleCount(threadID);
        }
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryThreadCycleTime(IntPtr threadHandle, ref ulong cycleTime);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThreadId();
    }
}
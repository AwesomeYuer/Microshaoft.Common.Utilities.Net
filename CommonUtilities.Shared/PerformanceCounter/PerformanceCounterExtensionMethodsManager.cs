#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    public static class PerformanceCounterExtensionMethodsManager
    {
        //public static void CountBeginAverageTimerCounter
        //                        (
        //                            this PerformanceCounter performanceCounter
        //                            , PerformanceCounter basePerformanceCounter
        //                            , Stopwatch stopwatch
        //                        )
        //{
        //        //stopwatch.Reset();
        //        //stopwatch.Start();
        //        stopwatch.Restart();
        //}
        public static void CountEndAverageTimerCounter
                        (
                            this PerformanceCounter performanceCounter
                            , PerformanceCounter basePerformanceCounter
                            , Stopwatch stopwatch
                            , Func<PerformanceCounterProcessingFlagsType, PerformanceCounter, long> onBasePerformanceCounterChangeValueProcessFunc = null
                        )
        {
            if
                (
                    stopwatch != null
                )
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
                performanceCounter
                        .IncrementBy(stopwatch.ElapsedTicks);
                //stopwatch = null;
                var increment = 1L;
                if (onBasePerformanceCounterChangeValueProcessFunc != null)
                {
                    increment = onBasePerformanceCounterChangeValueProcessFunc
                                        (
                                            PerformanceCounterProcessingFlagsType.TimeBasedOnEnd
                                            , basePerformanceCounter
                                        );
                }
                if (increment == 1)
                {
                    basePerformanceCounter.Increment();
                }
                else
                {
                    basePerformanceCounter.IncrementBy(increment);
                }
                
            }
        }
        public static void TryChangeAverageTimerCounterValue
                                (
                                    this PerformanceCounter performanceCounter
                                    , PerformanceCounter basePerformanceCounter
                                    , Stopwatch stopwatch
                                    , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                    , Action onCountPerformanceInnerProcessAction = null
                                    , Func<PerformanceCounter, Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                    , Action<PerformanceCounter, PerformanceCounter, bool, Exception, Exception, string> onFinallyProcessAction = null
                                )
        {
            //Stopwatch stopwatch = null;
            var enabledCountPerformance = true;
            if (onEnabledCountPerformanceProcessFunc != null)
            {
                enabledCountPerformance = onEnabledCountPerformanceProcessFunc();
            }
            if (enabledCountPerformance)
            {
                //stopwatch.Reset();
                //stopwatch.Start();
                stopwatch.Restart();
            }
            if (onCountPerformanceInnerProcessAction != null)
            {
                bool reThrowException = false;
                TryCatchFinallyProcessHelper
                    .TryProcessCatchFinally
                        (
                            true
                            , () =>
                            {
                                onCountPerformanceInnerProcessAction();
                            }
                            , reThrowException
                            , (x, y, z) =>
                            {
                                var r = reThrowException;
                                if (onCaughtExceptionProcessFunc != null)
                                {
                                    r = onCaughtExceptionProcessFunc
                                            (
                                                performanceCounter
                                                , x
                                                , y
                                                , z
                                            );
                                }
                                return r;
                            }
                            , (x, y, z, w) =>
                            {
                                if 
                                    (
                                        enabledCountPerformance
                                    )
                                {
                                    CountEndAverageTimerCounter
                                        (
                                            performanceCounter
                                            , basePerformanceCounter
                                            , stopwatch
                                        );
                                }
                                if (onFinallyProcessAction != null)
                                {
                                    onFinallyProcessAction
                                        (
                                            performanceCounter
                                            , basePerformanceCounter
                                            , x
                                            , y
                                            , z
                                            , w
                                        );
                                }
                            }
                        );
            }
        }
    }
}
#endif

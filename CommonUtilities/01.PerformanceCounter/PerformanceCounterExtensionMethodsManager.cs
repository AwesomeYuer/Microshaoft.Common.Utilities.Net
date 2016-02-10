namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    public static class PerformanceCounterExtensionMethodsManager
    {
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
                stopwatch.Reset();
                stopwatch.Start();
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
                                        &&
                                        stopwatch != null
                                        &&
                                        stopwatch.IsRunning
                                    )
                                {
                                    stopwatch.Stop();
                                    performanceCounter.IncrementBy(stopwatch.ElapsedTicks);
                                    //stopwatch = null;
                                    basePerformanceCounter.Increment();
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


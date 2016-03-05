/*
	PowerShell:
	[System.Diagnostics.PerformanceCounterCategory]::Delete("Microshaoft ConcurrentAsyncQueue Performance Counters")
	[System.Diagnostics.PerformanceCounterCategory]::GetCategories() | Format-Table -auto
	[System.Diagnostics.PerformanceCounterCategory]::GetCategories() | Where {$_.CategoryName -like "*microshaoft*" } | Format-Table -auto
	[Diagnostics.PerformanceCounterCategory]::Delete( "Your Category Name" )
	[Diagnostics.PerformanceCounterCategory]::GetCategories() | Format-Table -auto
	[Diagnostics.PerformanceCounterCategory]::GetCategories() | Where {$_.CategoryName -like "*microshaoft*" } | Format-Table -auto
	[Diagnostics.PerformanceCounterCategory]::GetCategories() | Where {$_.CategoryName -like "*network*" } | Format-Table -auto
	[Diagnostics.PerformanceCounterCategory]::GetCategories() | Where {$_.CategoryName -match "SQL.*Stat.*" } | Format-Table -auto

	$categoryName = "Microshaoft ConcurrentAsyncQueue Performance Counters"
	$counterName = ""
	$instanceName = ""
	if ([System.Diagnostics.PerformanceCounterCategory]::Exists($categoryName))
	{
		#if ([System.Diagnostics.PerformanceCounterCategory]::CounterExists($counterName,$categoryName))
		{
	
		}
		if ([System.Diagnostics.PerformanceCounterCategory]::InstanceExists($instanceName, $categoryName))
		{
			$pc = New-Object [System.Diagnostics.PerformanceCounter]  ($categoryName, $counterName, $instanceName)
			$pc.RemoveInstance()
			[System.Console]::WriteLine("RemoveInstance")
		}
		[System.Diagnostics.PerformanceCounterCategory]::Delete($categoryName)
		[System.Console]::WriteLine("Delete")
	}

*/
namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    public static partial class EasyPerformanceCountersHelper<TPerformanceCountersContainer>
                                        where TPerformanceCountersContainer :
                                                                AbstractPerformanceCountersContainer
                                                                //, class
                                                                , IPerformanceCountersContainer
                                                                , ICommonPerformanceCountersContainer
                                                                , new()
    {
        public static void TryCountPerformance
                                    (
                                        PerformanceCounterProcessingFlagsType enabledPerformanceCounterProcessingFlagsType
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                        , Action onCountPerformanceInnerProcessAction = null
                                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                        , PerformanceCounterInstanceLifetime
                                                        performanceCounterInstanceLifetime
                                                                = PerformanceCounterInstanceLifetime.Global
                                        , long? initializePerformanceCounterInstanceRawValue = null
                                    )
        {
            var enabledCountPerformance = true;
            var stopwatches = CountPerformanceBegin
                                    (
                                        enabledPerformanceCounterProcessingFlagsType
                                        , performanceCountersCategoryName
                                        , performanceCountersCategoryInstanceName
                                        , out enabledCountPerformance
                                        , onEnabledCountPerformanceProcessFunc
                                        , performanceCounterInstanceLifetime
                                        , initializePerformanceCounterInstanceRawValue
                                    );
            var reThrowException = false;

            #region Count Inner TryCatchFinally
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
                                                x
                                                , y
                                                , z
                                            );
                                }
                                return r;
                            }
                            , (x, y, z, w) =>
                            {
                                if (onFinallyProcessAction != null)
                                {
                                    onFinallyProcessAction
                                        (
                                            x
                                            , y
                                            , z
                                            , w
                                        );
                                }
                            }
                        );
            #endregion

            if (enabledCountPerformance)
            {
                CountPerformanceEnd
                    (
                        enabledPerformanceCounterProcessingFlagsType
                        , performanceCountersCategoryName
                        , performanceCountersCategoryInstanceName
                        , stopwatches
                    );
            }
        }
        public static Stopwatch[] CountPerformanceBegin
                            (
                                PerformanceCounterProcessingFlagsType enabledPerformanceCounterProcessingFlagsType
                                , string performanceCountersCategoryName
                                , string performanceCountersCategoryInstanceName
                                , out bool enabledCountPerformance
                                , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                , PerformanceCounterInstanceLifetime
                                            performanceCounterInstanceLifetime
                                                    = PerformanceCounterInstanceLifetime.Global
                                , long? initializePerformanceCounterInstanceRawValue = null

                            )
        {
            Stopwatch[] stopwatches = null;
            enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance
                            = onEnabledCountPerformanceProcessFunc();
                }
            }
            if
                (
                    enabledCountPerformance
                    &&
                        enabledPerformanceCounterProcessingFlagsType
                        !=
                        PerformanceCounterProcessingFlagsType.None
                )
            {
                var enableTimeBasedOnBeginOnEnd = false;
                PerformanceCountersPair[] performanceCountersPairs = null;

                var stopwatchIndex = 0;
                TPerformanceCountersContainer container = null;
                if (enabledCountPerformance)
                {
                    #region get Container
                    string key = string
                                    .Format
                                        (
                                            "{1}{0}{2}"
                                            , "-"
                                            , performanceCountersCategoryName
                                            , performanceCountersCategoryInstanceName
                                        );
                    if (!_dictionary.TryGetValue(key, out container))
                    {
                        lock (_lockerObject)
                        {
                            container = new TPerformanceCountersContainer();
                            _dictionary
                                    .Add
                                        (
                                            key
                                            , container
                                        );
                            container
                                .AttachPerformanceCountersToProperties
                                        (
                                            performanceCountersCategoryName
                                            , performanceCountersCategoryInstanceName
                                        );
                        }
                    }
                    #endregion
                    var enableIncrementOnBegin
                            = enabledPerformanceCounterProcessingFlagsType
                                                    .HasFlag
                                                        (
                                                            PerformanceCounterProcessingFlagsType
                                                                    .IncrementOnBegin
                                                        );
                    if (enableIncrementOnBegin)
                    {
                        Array
                            .ForEach
                                (
                                    container
                                            .IncrementOnBeginPerformanceCounters
                                    , (x) =>
                                    {
                                        x.Increment();
                                    }
                                );
                    }
                    enableTimeBasedOnBeginOnEnd
                                = enabledPerformanceCounterProcessingFlagsType
                                            .HasFlag
                                                (
                                                    PerformanceCounterProcessingFlagsType
                                                        .TimeBasedOnBeginOnEnd
                                                );

                    if (enableTimeBasedOnBeginOnEnd)
                    {
                        performanceCountersPairs
                                        = container
                                                .TimeBasedOnBeginOnEndPerformanceCountersPairs;
                        stopwatches = new Stopwatch[performanceCountersPairs.Length];
                        Array
                            .ForEach
                                (
                                    performanceCountersPairs
                                    , (x) =>
                                    {
                                        var stopwatch = _stopwatchsPool.Get();
                                        stopwatches[stopwatchIndex++] = stopwatch;
                                        stopwatch.Restart();
                                    }
                                );
                    }
                }
            }
            return stopwatches;
        }
        public static void CountPerformanceEnd
                                    (
                                        PerformanceCounterProcessingFlagsType enabledPerformanceCounterProcessingFlagsType
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Stopwatch[] stopwatches
                                    )
        {
            var stopwatchesCount = 0;
            if (stopwatches != null)
            {
                stopwatchesCount = stopwatches.Length;
            }
            var enableTimeBasedOnBeginOnEnd
                        = enabledPerformanceCounterProcessingFlagsType
                                    .HasFlag
                                        (
                                            PerformanceCounterProcessingFlagsType
                                                .TimeBasedOnBeginOnEnd
                                        );

            #region get Container
            TPerformanceCountersContainer container = null;
            string key = string
                            .Format
                                (
                                    "{1}{0}{2}"
                                    , "-"
                                    , performanceCountersCategoryName
                                    , performanceCountersCategoryInstanceName
                                );
            if (!_dictionary.TryGetValue(key, out container))
            {
                return;
                //lock (_lockerObject)
                //{
                //    container = new TPerformanceCountersContainer();
                //    _dictionary
                //            .Add
                //                (
                //                    key
                //                    , container
                //                );
                //    container
                //        .AttachPerformanceCountersToProperties
                //                (
                //                    performanceCountersCategoryName
                //                    , performanceCountersCategoryInstanceName
                //                );
                //}
            }
            #endregion
            if (enableTimeBasedOnBeginOnEnd)
            {
                if (stopwatchesCount > 0)
                {
                    var stopwatchIndex = 0;
                    var performanceCountersPairs
                                = container
                                        .TimeBasedOnBeginOnEndPerformanceCountersPairs;
                    Array
                        .ForEach
                            (
                                performanceCountersPairs
                                , (x) =>
                                {
                                    var stopwatch
                                            = stopwatches[stopwatchIndex++];
                                    x
                                        .Counter
                                        .CountEndAverageTimerCounter
                                            (
                                                x.BaseCounter
                                                , stopwatch
                                            );
                                    var rr = _stopwatchsPool.Put(stopwatch);
                                    if (!rr)
                                    {
                                        stopwatch.Stop();
                                        stopwatch = null;
                                    }
                                }
                            );
                    stopwatches = null;
                }
            }
            var enableIncrementOnEnd
                        = enabledPerformanceCounterProcessingFlagsType
                                .HasFlag
                                    (
                                        PerformanceCounterProcessingFlagsType
                                                                    .IncrementOnEnd
                                    );
            if (enableIncrementOnEnd)
            {
                Array
                   .ForEach
                       (
                           container
                                   .IncrementOnEndPerformanceCounters
                           , (x) =>
                           {
                               x.Increment();
                           }
                       );
            }
            var enableDecrementOnEnd
                        = enabledPerformanceCounterProcessingFlagsType
                                    .HasFlag
                                        (
                                            PerformanceCounterProcessingFlagsType
                                                        .DecrementOnEnd
                                        );
            if (enableDecrementOnEnd)
            {
                Array
                   .ForEach
                       (
                           container
                                   .DecrementOnEndPerformanceCounters
                           , (x) =>
                           {
                               x.Decrement();
                           }
                       );
            }
        }
    }
}

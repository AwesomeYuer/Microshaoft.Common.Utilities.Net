
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
#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    public static partial class
                    EasyPerformanceCountersHelper
                        <TPerformanceCountersContainer>
                            where TPerformanceCountersContainer :
                                                    AbstractPerformanceCountersContainer
                                                    //, class
                                                    , IPerformanceCountersContainer
                                                    , ICommonPerformanceCountersContainer
                                                    , new()
    {
        public static void TryCountPerformance
                                (
                                    PerformanceCounterProcessingFlagsType enabledProcessingFlagsType
                                    , string categoryName
                                    , string instanceName
                                    , Func<bool> onGetEnableCountProcessFunc = null
                                    , Action onCountPerformanceInnerProcessAction = null
                                    , Func<PerformanceCounterProcessingFlagsType, PerformanceCounter, long>
                                                                onPerformanceCounterChangeValueProcessFunc = null
                                    , Func<Exception, Exception, string, bool>
                                                                onCaughtExceptionProcessFunc = null
                                    , Action<bool, Exception, Exception, string>
                                                                onFinallyProcessAction = null
                                    , PerformanceCounterInstanceLifetime
                                                    instanceLifetime
                                                            = PerformanceCounterInstanceLifetime.Global
                                    , long? initializeInstanceRawValue = null
                                )
        {
            var enabledCountPerformance = true;
            Stopwatch[] stopwatches = null;
            var container = CountPerformanceBegin
                                (
                                    enabledProcessingFlagsType
                                    , categoryName
                                    , instanceName
                                    , out enabledCountPerformance
                                    , out stopwatches
                                    , onGetEnableCountProcessFunc
                                    , onPerformanceCounterChangeValueProcessFunc
                                    , instanceLifetime
                                    , initializeInstanceRawValue
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
                                container
                                        .CaughtExceptionsPerformanceCounter
                                        .Increment();
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
                        enabledProcessingFlagsType
                        , categoryName
                        , instanceName
                        , stopwatches
                        , onPerformanceCounterChangeValueProcessFunc
                    );
            }
        }
        public static TPerformanceCountersContainer
                            CountPerformanceBegin
                                (
                                    PerformanceCounterProcessingFlagsType
                                                enabledProcessingFlagsType
                                    , string categoryName
                                    , string instanceName
                                    , out bool enabledCount
                                    , out Stopwatch[] stopwatches 
                                    , Func<bool> 
                                                onGetEnableCountProcessFunc = null
                                    , Func<PerformanceCounterProcessingFlagsType , PerformanceCounter, long>
                                                                onPerformanceCounterChangeValueProcessFunc = null
                                    , PerformanceCounterInstanceLifetime
                                                instanceLifetime
                                                        = PerformanceCounterInstanceLifetime.Global
                                    , long? initializeInstanceRawValue = null
                                )
        {
            TPerformanceCountersContainer container = null;
            Stopwatch[] stopwatchesInline = null;
            enabledCount = true;
            {
                if (onGetEnableCountProcessFunc != null)
                {
                    enabledCount
                            = onGetEnableCountProcessFunc();
                }
            }
            if
                (
                    enabledCount
                    &&
                        enabledProcessingFlagsType
                        !=
                        PerformanceCounterProcessingFlagsType.None
                )
            {
                var enableTimeBasedOnBeginOnEnd = false;
                PerformanceCountersPair[] performanceCountersPairs = null;
                var stopwatchIndex = 0;
                if (enabledCount)
                {
#region get Container
                    string key = string
                                    .Format
                                        (
                                            "{1}{0}{2}"
                                            , "-"
                                            , categoryName
                                            , instanceName
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
                                .AttachPerformanceCountersToMembers
                                        (
                                            categoryName
                                            , instanceName
                                        );
                        }
                    }
#endregion
                    var enableIncrementOnBegin
                            = enabledProcessingFlagsType
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
                                        var increment = 1L;
                                        if (onPerformanceCounterChangeValueProcessFunc != null)
                                        {
                                            increment = onPerformanceCounterChangeValueProcessFunc
                                                                (
                                                                    PerformanceCounterProcessingFlagsType.IncrementOnBegin
                                                                    , x
                                                                );
                                        }
                                        if (increment == 1)
                                        {
                                            x.Increment();
                                        }
                                        else
                                        {
                                            x.IncrementBy(increment);
                                        }
                                    }
                                );
                    }
                    enableTimeBasedOnBeginOnEnd
                            = enabledProcessingFlagsType
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
                        stopwatchesInline = new Stopwatch[performanceCountersPairs.Length];
                        Array
                            .ForEach
                                (
                                    performanceCountersPairs
                                    , (x) =>
                                    {
                                        _stopwatchsPool.TryGet(out var stopwatch);
                                        stopwatchesInline[stopwatchIndex++] = stopwatch;
                                        stopwatch.Restart();
                                    }
                                );
                    }
                }
            }
            stopwatches = stopwatchesInline;
            return container;
        }
        public static void CountPerformanceEnd
                                (
                                    PerformanceCounterProcessingFlagsType
                                                    enabledProcessingFlagsType
                                    , string categoryName
                                    , string instanceName
                                    , Stopwatch[] stopwatches
                                    , Func<PerformanceCounterProcessingFlagsType, PerformanceCounter, long>
                                                                onPerformanceCounterChangeValueProcessFunc = null
                                )
        {
            var stopwatchesCount = 0;
            if (stopwatches != null)
            {
                stopwatchesCount = stopwatches.Length;
            }
            var enableTimeBasedOnBeginOnEnd
                        = enabledProcessingFlagsType
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
                                    , categoryName
                                    , instanceName
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
                                                , onPerformanceCounterChangeValueProcessFunc
                                            );
                                    var rr = _stopwatchsPool.TryPut(stopwatch);
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
                        = enabledProcessingFlagsType
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
                               var increment = 1L;
                               if (onPerformanceCounterChangeValueProcessFunc != null)
                               {
                                   increment = onPerformanceCounterChangeValueProcessFunc
                                                       (
                                                           PerformanceCounterProcessingFlagsType
                                                                    .IncrementOnEnd
                                                           , x
                                                       );
                               }
                               if (increment == 1)
                               {
                                   x.Increment();
                               }
                               else
                               {
                                   x.IncrementBy(increment);
                               }
                           }
                       );
            }
            var enableDecrementOnEnd
                        = enabledProcessingFlagsType
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
                               var decrement = 1L;
                               if (onPerformanceCounterChangeValueProcessFunc != null)
                               {
                                   decrement = onPerformanceCounterChangeValueProcessFunc
                                                       (
                                                           PerformanceCounterProcessingFlagsType
                                                                    .DecrementOnEnd
                                                           , x
                                                       );
                               }
                               if (decrement == 1)
                               {
                                   x.Decrement();
                               }
                               else
                               {
                                   decrement *= -1;
                                   x.IncrementBy(decrement);
                               }
                           }
                       );
            }
        }
    }
}

#endif

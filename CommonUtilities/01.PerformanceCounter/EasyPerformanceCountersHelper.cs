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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    //using System.Collections.Concurrent;

    public static class EasyPerformanceCountersHelper<TPerformanceCountersContainer>
                                        where TPerformanceCountersContainer :
                                                                AbstractPerformanceCountersContainer
                                                                //, class
                                                                , IPerformanceCountersContainer
                                                                , ICommonPerformanceCountersContainer
                                                                , new()
    {
        private static readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = new QueuedObjectsPool<Stopwatch>(10 * 10000);
        private static Dictionary<string, TPerformanceCountersContainer> _dictionary
                        = new Dictionary<string, TPerformanceCountersContainer>();

        public static IEnumerable<TPerformanceCountersContainer> Containers
        {
            get
            {
                return
                    _dictionary
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        x.Value;
                                }
                            );
            }
        }

        public static void AttachPerformanceCountersCategoryInstance
                            (
                                string performanceCountersCategoryName
                                , string performanceCountersCategoryInstanceName
                                , out TPerformanceCountersContainer container
                                , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                , long? initializePerformanceCounterInstanceRawValue = null
                            )
        {
            string key = string.Format
                                    (
                                        "{1}{0}{2}"
                                        , "-"
                                        , performanceCountersCategoryName
                                        , performanceCountersCategoryInstanceName
                                    );
            //TPerformanceCountersContainer container = null;
            if (!_dictionary.TryGetValue(key, out container))
            {
                container = new TPerformanceCountersContainer(); //default(TPerformanceCountersContainer);
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
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue

                        );
            }
        }
        private static readonly object _lockerObject = new object();
        public static Stopwatch CountPerformanceBegin
                                    (
                                        MultiPerformanceCountersTypeFlags enabledPerformanceCounters
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                        , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                        , long? initializePerformanceCounterInstanceRawValue = null
                                    )
        {
            Stopwatch r = null;
            var enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance = onEnabledCountPerformanceProcessFunc();
                }
            }
            if
                (
                    enabledCountPerformance
                    &&
                    enabledPerformanceCounters != MultiPerformanceCountersTypeFlags.None
                )
            {
                string key = string
                                .Format
                                    (
                                        "{1}{0}{2}"
                                        , "-"
                                        , performanceCountersCategoryName
                                        , performanceCountersCategoryInstanceName
                                    );
                TPerformanceCountersContainer container = null;
                if (!_dictionary.TryGetValue(key, out container))
                {
                    lock (_lockerObject)
                    {
                        container = new TPerformanceCountersContainer();
                        _dictionary.Add
                                    (
                                        key
                                        , container
                                    );
                        container
                            .AttachPerformanceCountersToProperties
                                    (
                                        performanceCountersCategoryName
                                        , performanceCountersCategoryInstanceName
                                        , performanceCounterInstanceLifetime
                                        , initializePerformanceCounterInstanceRawValue
                                    );
                    }
                }
                var enableProcessCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessCounter)
                {
                    container.PrcocessPerformanceCounter.Increment();
                }
                var enableProcessingCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessingCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessingCounter)
                {
                    container.ProcessingPerformanceCounter.Increment();
                }
                var enableProcessedAverageTimerCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessedAverageTimerCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessedAverageTimerCounter)
                {
                    r = _stopwatchsPool.Get(); //Stopwatch.StartNew();
                    r.Restart();
                }
            }
            return r;
        }
        public static void CountPerformanceEnd
                                    (
                                        MultiPerformanceCountersTypeFlags enabledPerformanceCounters
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Stopwatch stopwatch
                                        , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                    )
        {
            var enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance = onEnabledCountPerformanceProcessFunc();
                }
            }
            if
                (
                    enabledCountPerformance
                    &&
                    enabledPerformanceCounters != MultiPerformanceCountersTypeFlags.None
                )
            {
                string key = string.Format
                        (
                            "{1}{0}{2}"
                            , "-"
                            , performanceCountersCategoryName
                            , performanceCountersCategoryInstanceName
                        );
                TPerformanceCountersContainer container = null;
                if (!_dictionary.TryGetValue(key, out container))
                {
                    return;
                }
                var enableProcessedAverageTimerCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessedAverageTimerCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessedAverageTimerCounter)
                {
                    if (stopwatch != null)
                    {
                        PerformanceCounter performanceCounter
                                                = container.ProcessedAverageTimerPerformanceCounter;
                        PerformanceCounter basePerformanceCounter
                                                = container.ProcessedAverageBasePerformanceCounter;

                        performanceCounter.IncrementBy(stopwatch.ElapsedTicks);
                        stopwatch.Reset();
                        var r = _stopwatchsPool.Put(stopwatch);
                        if (!r)
                        {
                            stopwatch.Stop();
                            stopwatch = null;
                        }
                        basePerformanceCounter.Increment();
                        //stopwatch = null;
                    }
                }
                var enableProcessingCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessingCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessingCounter)
                {
                    long l = container
                                .ProcessingPerformanceCounter
                                .Decrement();
                    if (l < 0)
                    {
                        container
                                .ProcessingPerformanceCounter
                                .RawValue = 0;
                    }
                }
                var enableProcessedPerformanceCounter =
                                            (
                                                (
                                                    enabledPerformanceCounters
                                                    &
                                                    MultiPerformanceCountersTypeFlags
                                                        .ProcessedCounter
                                                )
                                                != MultiPerformanceCountersTypeFlags.None
                                            );
                if (enableProcessedPerformanceCounter)
                {
                    container.ProcessedPerformanceCounter.Increment();
                }
                var enableProcessedRateOfCountsPerSecondPerformanceCounter =
                            (
                                (
                                    enabledPerformanceCounters
                                    &
                                    MultiPerformanceCountersTypeFlags
                                        .ProcessedRateOfCountsPerSecondCounter
                                )
                                != MultiPerformanceCountersTypeFlags.None
                            );
                if (enableProcessedRateOfCountsPerSecondPerformanceCounter)
                {
                    container
                        .ProcessedRateOfCountsPerSecondPerformanceCounter
                        .Increment();
                }
            }
        }
        public static void TryCountPerformance
                                    (
                                        MultiPerformanceCountersTypeFlags enabledPerformanceCounters
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                        , Action onBeforeCountPerformanceInnerProcessAction = null
                                        , Action onCountPerformanceInnerProcessAction = null
                                        , Action onAfterCountPerformanceInnerProcessAction = null
                                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                                    )
        {
            var enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance = onEnabledCountPerformanceProcessFunc();
                }
            }
            if 
                (
                    enabledCountPerformance
                    &&
                    enabledPerformanceCounters != MultiPerformanceCountersTypeFlags.None
                    &&
                    onCountPerformanceInnerProcessAction != null
                )
            {
                if (onCountPerformanceInnerProcessAction != null)
                {
                    string key = string
                                    .Format
                                        (
                                            "{1}{0}{2}"
                                            , "-"
                                            , performanceCountersCategoryName
                                            , performanceCountersCategoryInstanceName
                                        );
                    TPerformanceCountersContainer container = null;
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
                    if (enabledCountPerformance)
                    {
                        var enableProcessCounter =
                                                    (
                                                        (
                                                            enabledPerformanceCounters
                                                            &
                                                            MultiPerformanceCountersTypeFlags
                                                                .ProcessCounter
                                                        )
                                                        != MultiPerformanceCountersTypeFlags.None
                                                    );
                        if (enableProcessCounter)
                        {
                            container
                                .PrcocessPerformanceCounter
                                .Increment();
                        }
                        var enableProcessingCounter =
                                                    (
                                                        (
                                                            enabledPerformanceCounters
                                                            &
                                                            MultiPerformanceCountersTypeFlags
                                                                .ProcessingCounter
                                                        )
                                                        != MultiPerformanceCountersTypeFlags.None
                                                    );
                        if (enableProcessingCounter)
                        {
                            container
                                .ProcessingPerformanceCounter
                                .Increment();
                        }
                        var enableProcessedAverageTimerCounter =
                                                    (
                                                        (
                                                            enabledPerformanceCounters
                                                            &
                                                            MultiPerformanceCountersTypeFlags
                                                                .ProcessedAverageTimerCounter
                                                        )
                                                        != MultiPerformanceCountersTypeFlags.None
                                                    );
                        var reThrowException = false;
                        var stopwatch = _stopwatchsPool.Get();
                        //stopwatch.Reset();
                        container
                            .ProcessedAverageTimerPerformanceCounter
                                .TryChangeAverageTimerCounterValue
                                            (
                                                
                                                container
                                                    .ProcessedAverageBasePerformanceCounter
                                                , stopwatch
                                                , () =>
                                                {
                                                    return enableProcessedAverageTimerCounter;
                                                }
                                                , () =>
                                                {
                                                    if (onCountPerformanceInnerProcessAction != null)
                                                    {
                                                        if (onBeforeCountPerformanceInnerProcessAction != null)
                                                        {
                                                            onBeforeCountPerformanceInnerProcessAction();
                                                        }
                                                        onCountPerformanceInnerProcessAction();
                                                        if (onAfterCountPerformanceInnerProcessAction != null)
                                                        {
                                                            onAfterCountPerformanceInnerProcessAction();
                                                        }
                                                    }
                                                }
                                                , (x, y, z, w) =>        //catch
                                                {
                                                    container
                                                        .CaughtExceptionsPerformanceCounter
                                                        .Increment();
                                                    var r = reThrowException;
                                                    if (onCaughtExceptionProcessFunc != null)
                                                    {
                                                        r = onCaughtExceptionProcessFunc(y, z, w);
                                                    }
                                                    return r;
                                                }
                                                , (x, y, z, exception, newException, innerExceptionMessage) =>        //Finally
                                                {
                                                    if (enableProcessingCounter)
                                                    {
                                                        long l = container
                                                                    .ProcessingPerformanceCounter
                                                                    .Decrement();
                                                        if (l < 0)
                                                        {
                                                            container
                                                                .ProcessingPerformanceCounter
                                                                .RawValue = 0;
                                                        }
                                                    }
                                                    var enableProcessedPerformanceCounter
                                                            =
                                                                (
                                                                    (
                                                                        enabledPerformanceCounters
                                                                        &
                                                                        MultiPerformanceCountersTypeFlags
                                                                            .ProcessedCounter
                                                                    )
                                                                    != MultiPerformanceCountersTypeFlags.None
                                                                );
                                                    if (enableProcessedPerformanceCounter)
                                                    {
                                                        container
                                                            .ProcessedPerformanceCounter
                                                            .Increment();
                                                    }
                                                    var enableProcessedRateOfCountsPerSecondPerformanceCounter
                                                                        =
                                                                            (
                                                                                (
                                                                                    enabledPerformanceCounters
                                                                                    &
                                                                                    MultiPerformanceCountersTypeFlags
                                                                                        .ProcessedRateOfCountsPerSecondCounter
                                                                                )
                                                                                != MultiPerformanceCountersTypeFlags.None
                                                                            );
                                                    if (enableProcessedRateOfCountsPerSecondPerformanceCounter)
                                                    {
                                                        container
                                                            .ProcessedRateOfCountsPerSecondPerformanceCounter
                                                            .Increment();
                                                    }
                                                }
                                            );
                        stopwatch.Reset();
                        var rr = _stopwatchsPool.Put(stopwatch);
                        if (!rr)
                        {
                            stopwatch.Stop();
                            stopwatch = null;
                        }
                    }
                }
            }
            else
            {
                if (onCountPerformanceInnerProcessAction != null)
                {
                    onCountPerformanceInnerProcessAction();
                }
            }
        }
    }
}

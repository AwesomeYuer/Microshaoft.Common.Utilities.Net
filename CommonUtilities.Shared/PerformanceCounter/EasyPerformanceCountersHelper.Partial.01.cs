
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    //using System.Collections.Concurrent;

    public static partial class 
                            EasyPerformanceCountersHelper
                                    <TPerformanceCountersContainer>
                                        where
                                            TPerformanceCountersContainer
                                                :
                                                    AbstractPerformanceCountersContainer
                                                    //, class
                                                    , IPerformanceCountersContainer
                                                    , ICommonPerformanceCountersContainer
                                                    , new()
    {
        
        
        public static Stopwatch CountPerformanceBegin
                                    (
                                        MultiPerformanceCountersTypeFlags enabledPerformanceCounters
                                        , string performanceCountersCategoryName
                                        , string performanceCountersCategoryInstanceName
                                        , Func<bool> onEnabledCountPerformanceProcessFunc = null
                                        , PerformanceCounterInstanceLifetime
                                                    performanceCounterInstanceLifetime
                                                            = PerformanceCounterInstanceLifetime.Global
                                        , long? initializePerformanceCounterInstanceRawValue = null
                                    )
        {
            Stopwatch r = null;
            var enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance = onEnabledCountPerformanceProcessFunc.Invoke();
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
                            .AttachPerformanceCountersToMembers
                                    (
                                        performanceCountersCategoryName
                                        , performanceCountersCategoryInstanceName
                                        , performanceCounterInstanceLifetime
                                        , initializePerformanceCounterInstanceRawValue
                                    );
                    }
                }
                var enableProcessCounter = enabledPerformanceCounters
                                                .HasFlag
                                                    (
                                                        MultiPerformanceCountersTypeFlags
                                                            .ProcessCounter
                                                    );
                if (enableProcessCounter)
                {
                    container
                            .PrcocessPerformanceCounter
                            .Increment();
                }
                var enableProcessingCounter = enabledPerformanceCounters
                                                    .HasFlag
                                                        (
                                                            MultiPerformanceCountersTypeFlags
                                                                .ProcessingCounter
                                                        );
                if (enableProcessingCounter)
                {
                    container.ProcessingPerformanceCounter.Increment();
                }
                var enableProcessedAverageTimerCounter
                                    = enabledPerformanceCounters
                                            .HasFlag
                                                (
                                                    MultiPerformanceCountersTypeFlags
                                                            .ProcessedAverageTimerCounter
                                                );
                if (enableProcessedAverageTimerCounter)
                {
                    _stopwatchsPool.TryGet(out r); //Stopwatch.StartNew();
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
                var enableProcessedAverageTimerCounter
                                = enabledPerformanceCounters
                                        .HasFlag
                                            (
                                                MultiPerformanceCountersTypeFlags
                                                        .ProcessedAverageTimerCounter
                                            );
                if (enableProcessedAverageTimerCounter)
                {
                    if (stopwatch != null)
                    {
                        var performanceCounter
                                        = container
                                                .ProcessedAverageTimerPerformanceCounter;
                        var basePerformanceCounter
                                        = container
                                                .ProcessedAverageBasePerformanceCounter;

                        performanceCounter
                                    .IncrementBy(stopwatch.ElapsedTicks);
                        stopwatch.Reset();
                        var r = _stopwatchsPool.TryPut(stopwatch);
                        if (!r)
                        {
                            stopwatch.Stop();
                            stopwatch = null;
                        }
                        basePerformanceCounter.Increment();
                        //stopwatch = null;
                    }
                }
                var enableProcessingCounter
                                = enabledPerformanceCounters
                                        .HasFlag
                                            (
                                                MultiPerformanceCountersTypeFlags
                                                    .ProcessingCounter
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
                var enableProcessedPerformanceCounter
                                = enabledPerformanceCounters
                                        .HasFlag
                                            (
                                                MultiPerformanceCountersTypeFlags
                                                        .ProcessedCounter
                                            );
                if (enableProcessedPerformanceCounter)
                {
                    container
                        .ProcessedPerformanceCounter
                        .Increment();
                }
                var enableProcessedRateOfCountsPerSecondPerformanceCounter
                            = enabledPerformanceCounters
                                    .HasFlag
                                        (
                                            MultiPerformanceCountersTypeFlags
                                                .ProcessedRateOfCountsPerSecondCounter
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
                    enabledCountPerformance
                            = onEnabledCountPerformanceProcessFunc();
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
                                .AttachPerformanceCountersToMembers
                                        (
                                            performanceCountersCategoryName
                                            , performanceCountersCategoryInstanceName
                                        );
                        }
                    }
                    if (enabledCountPerformance)
                    {
                        var enableProcessCounter = enabledPerformanceCounters
                                                        .HasFlag
                                                            (
                                                                MultiPerformanceCountersTypeFlags
                                                                    .ProcessCounter
                                                            );
                                                    
                        if (enableProcessCounter)
                        {
                            container
                                .PrcocessPerformanceCounter
                                .Increment();
                        }
                        var enableProcessingCounter =
                                                    enabledPerformanceCounters
                                                        .HasFlag
                                                            (
                                                                MultiPerformanceCountersTypeFlags
                                                                    .ProcessingCounter
                                                            );
                        if (enableProcessingCounter)
                        {
                            container
                                .ProcessingPerformanceCounter
                                .Increment();
                        }
                        var enableProcessedAverageTimerCounter =
                                                    enabledPerformanceCounters
                                                            .HasFlag
                                                                (
                                                                    MultiPerformanceCountersTypeFlags
                                                                            .ProcessedAverageTimerCounter
                                                                );
                        var reThrowException = false;
                        _stopwatchsPool.TryGet(out var stopwatch);
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
                                                ,
                                                    (
                                                        senderPerformanceCounter
                                                        , exception
                                                        , newException
                                                        , caughtInnerExceptionMessage
                                                    ) =>        //catch
                                                {
                                                    container
                                                        .CaughtExceptionsPerformanceCounter
                                                        .Increment();
                                                    var r = reThrowException;
                                                    if (onCaughtExceptionProcessFunc != null)
                                                    {
                                                        r = onCaughtExceptionProcessFunc(exception, newException, caughtInnerExceptionMessage);
                                                    }
                                                    return r;
                                                }
                                                , 
                                                    (
                                                        senderPerformanceCounter
                                                        , senderBasePerformanceCounter
                                                        , isCaughtException
                                                        , exception
                                                        , newException
                                                        , caughtInnerExceptionMessage
                                                    ) =>        //Finally
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
                                                            = enabledPerformanceCounters
                                                                    .HasFlag
                                                                        (
                                                                            MultiPerformanceCountersTypeFlags
                                                                                    .ProcessedCounter
                                                                        );
                                                    if (enableProcessedPerformanceCounter)
                                                    {
                                                        container
                                                            .ProcessedPerformanceCounter
                                                            .Increment();
                                                    }
                                                    var enableProcessedRateOfCountsPerSecondPerformanceCounter
                                                            = enabledPerformanceCounters
                                                                        .HasFlag
                                                                            (
                                                                                MultiPerformanceCountersTypeFlags
                                                                                        .ProcessedRateOfCountsPerSecondCounter
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
                        var rr = _stopwatchsPool.TryPut(stopwatch);
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
                onCountPerformanceInnerProcessAction?.Invoke();
            }
        }
    }
}

#endif

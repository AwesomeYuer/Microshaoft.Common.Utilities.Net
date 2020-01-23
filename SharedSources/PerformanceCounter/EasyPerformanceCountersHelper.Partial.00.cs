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
        private static readonly QueuedObjectsPool<Stopwatch>
                                    _stopwatchsPool
                                        = new QueuedObjectsPool<Stopwatch>(10 * 10000);
        private static Dictionary<string, TPerformanceCountersContainer>
                                _dictionary
                                        = new Dictionary<string, TPerformanceCountersContainer>();
        private static readonly object _lockerObject = new object();

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
                lock (_lockerObject)
                {
                    container = new TPerformanceCountersContainer(); //default(TPerformanceCountersContainer);
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
                                , performanceCounterInstanceLifetime
                                , initializePerformanceCounterInstanceRawValue
                            );
                }
            }
        }

    }
}

#endif

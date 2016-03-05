namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    //private static class QueuedObjectsPoolManager
    //{
    //    public static readonly QueuedObjectsPool<Stopwatch> StopwatchsPool = new QueuedObjectsPool<Stopwatch>(0);
    //}

    public class ConcurrentAsyncQueue<T> :
                                            IPerformanceCountersValuesClearable
    {
        private readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = null;
        public delegate void QueueEventHandler(T item);
        public event QueueEventHandler OnDequeue;
        public delegate void QueueLogEventHandler(string logMessage);
        public QueueLogEventHandler
                                OnQueueLog
                                , OnDequeueThreadStart
                                , OnDequeueThreadEnd;

        public delegate bool CaughtExceptionEventHandler
                                    (
                                        ConcurrentAsyncQueue<T> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException
                                            , OnEnqueueProcessCaughtException
                                            , OnDequeueProcessCaughtException;
        public ConcurrentAsyncQueue(long stopwatchsPoolMaxCapacity = 10 * 10000)
        {
            _stopwatchsPool = new QueuedObjectsPool<Stopwatch>
                                        (
                                            stopwatchsPoolMaxCapacity
                                        );
        }
        private ConcurrentQueue<Tuple<Stopwatch, T>>
                            _queue = new ConcurrentQueue<Tuple<Stopwatch, T>>();

        public ConcurrentQueue<Tuple<Stopwatch, T>> InternalQueue
        {
            get
            {
                return _queue;
            }
            //set { _queue = value; }
        }
        private ConcurrentQueue<Action> _callbackProcessBreaksActions;
        //Microshaoft 用于控制并发线程数
        private long _concurrentDequeueThreadsCount = 0;
        private ConcurrentQueue<ThreadProcessor> _dequeueThreadsProcessorsPool;
        private int _dequeueIdleSleepSeconds = 10;
        public QueuePerformanceCountersContainer PerformanceCountersContainer
        {
            get;
            private set;
        }
        public int DequeueIdleSleepSeconds
        {
            set
            {
                _dequeueIdleSleepSeconds = value;
            }
            get
            {
                return _dequeueIdleSleepSeconds;
            }
        }
        private bool _isAttachedPerformanceCounters = false;
        private readonly object _clearPerformanceCountersValueslocker = new object();

        //public void ClearPerformanceCountersValues(int level)
        //{
        //    //if (this != null)
        //    //ClearPerformanceCountersValues(ref _enabledCountPerformance, level);
        //}
        public void ClearPerformanceCountersValues(int level)
        {
            ////if (this != null)
            lock (_clearPerformanceCountersValueslocker)
            {
                var func = _onEnabledCountPerformanceProcessFunc;
                try
                {
                    _onEnabledCountPerformanceProcessFunc =
                                    (
                                        () =>
                                        {
                                            return false;
                                        }
                                    );
                    var counters = GetPerformanceCountersByLevel(level);
                    foreach (var counter in counters)
                    {
                        counter
                            .RawValue = 0;
                    }
                }
                finally
                {
                    _onEnabledCountPerformanceProcessFunc = func;
                }
            }
        }
        public IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                //yield return PerformanceCountersContainer.EnqueuePerformanceCounter;
                //yield return PerformanceCountersContainer.EnqueueRateOfCountsPerSecondPerformanceCounter;
                //yield return PerformanceCountersContainer.QueueLengthPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeuePerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueProcessedRateOfCountsPerSecondPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueProcessedPerformanceCounter;
                //yield return PerformanceCountersContainer.QueuedWaitAverageTimerPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadStartPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadsCountPerformanceCounter;
                //yield return PerformanceCountersContainer.DequeueThreadEndPerformanceCounter;
                //yield return PerformanceCountersContainer.CaughtExceptionsPerformanceCounter;
                return
                    PerformanceCountersContainer
                                .PerformanceCounters;
            }
        }
        public PerformanceCounter GetPerformanceCounterByName(string name)
        {
            return
                PerformanceCountersContainer[name];
        }

        public IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            return
                PerformanceCountersContainer
                    .GetPerformanceCountersByLevel(level);
        }
        private class ThreadProcessor
        {
            public bool Break
            {
                set;
                get;
            }
            public EventWaitHandle Waiter
            {
                private set;
                get;
            }
            public ConcurrentAsyncQueue<T> Sender
            {
                private set;
                get;
            }
            public void StopOne()
            {
                Break = true;
            }
            public ThreadProcessor
                            (
                                ConcurrentAsyncQueue<T> queue
                                , EventWaitHandle wait
                            )
            {
                Waiter = wait;
                Sender = queue;
            }
            public void ThreadProcess()
            {
                long l = 0;
                Interlocked.Increment(ref Sender._concurrentDequeueThreadsCount);
                //bool counterEnabled = Sender._isAttachedPerformanceCounters;
                QueuePerformanceCountersContainer qpcc = Sender.PerformanceCountersContainer;
                var queue = Sender.InternalQueue;
                var reThrowException = false;

                PerformanceCounter[] incrementCountersBeforeCountPerformanceInThread = null;
                PerformanceCounter[] decrementCountersAfterCountPerformanceInThread = null;
                PerformanceCounter[] incrementCountersAfterCountPerformanceInThread = null;
                var enabledCountPerformance = true;
                {
                    if (Sender._onEnabledCountPerformanceProcessFunc != null)
                    {
                        enabledCountPerformance = Sender._onEnabledCountPerformanceProcessFunc();
                    }
                }
                if 
                    (
                        qpcc != null
                        &&
                        enabledCountPerformance
                    )
                {
                    incrementCountersBeforeCountPerformanceInThread =
                        new PerformanceCounter[]
									{
										qpcc
											.DequeueThreadStartPerformanceCounter
										, qpcc
											.DequeueThreadsCountPerformanceCounter
									};
                    decrementCountersAfterCountPerformanceInThread
                                    = new PerformanceCounter[]
									        {
										        qpcc
                                                    .DequeueThreadsCountPerformanceCounter
									        };
                    incrementCountersAfterCountPerformanceInThread
                                    = new PerformanceCounter[]
									        {
										        qpcc
                                                    .DequeueThreadEndPerformanceCounter	
									        };
                }

                PerformanceCountersHelper
                        .TryCountPerformance
                            (
                                () =>
                                {
                                    return enabledCountPerformance;
                                }
                                , reThrowException
                                , incrementCountersBeforeCountPerformanceInThread
                                , null
                                , null
                                , () =>
                                {
                                    #region Try Process
                                    if (Sender.OnDequeueThreadStart != null)
                                    {
                                        l = Interlocked.Read(ref Sender._concurrentDequeueThreadsCount);
                                        Sender
                                            .OnDequeueThreadStart
                                                (
                                                    string
                                                        .Format
                                                            (
                                                                "{0} Threads Count {1},Queue Count {2},Current Thread: {3} at {4}"
                                                                , "Threads ++ !"
                                                                , l
                                                                , queue.Count
                                                                , Thread.CurrentThread.Name
                                                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                                            )
                                                );
                                    }
                                    while (true)
                                    {
                                        #region while true loop
                                        if (Break)
                                        {
                                            break;
                                        }
                                        while (!queue.IsEmpty)
                                        {
                                            #region while queue.IsEmpty loop
                                            if (Break)
                                            {
                                                break;
                                            }
                                            Tuple<Stopwatch, T> item = null;
                                            if (queue.TryDequeue(out item))
                                            {
                                                Stopwatch stopwatchDequeue = Sender._stopwatchsPool.Get();
                                                Stopwatch stopwatchEnqueue = item.Item1;
                                                PerformanceCounter[] incrementCountersBeforeCountPerformanceForDequeue = null;
                                                PerformanceCounter[] decrementCountersBeforeCountPerformanceForDequeue = null;
                                                PerformanceCounter[] incrementCountersAfterCountPerformanceForDequeue = null;
                                                Tuple
                                                    <
                                                        bool
                                                        , Stopwatch
                                                        , PerformanceCounter
                                                        , PerformanceCounter
                                                    >[] timerCounters = null;

                                                if
                                                    (
                                                        qpcc != null
                                                        &&
                                                        enabledCountPerformance
                                                    )
                                                {
                                                    incrementCountersBeforeCountPerformanceForDequeue =
                                                        new PerformanceCounter[]
																        {
																	        qpcc
																		        .DequeuePerformanceCounter
																        };
                                                    decrementCountersBeforeCountPerformanceForDequeue
                                                                        = new PerformanceCounter[]
																                {
																	                qpcc
																		                .QueueLengthPerformanceCounter
																                };
                                                    timerCounters = new Tuple
                                                                        <
                                                                            bool
                                                                            , Stopwatch
                                                                            , PerformanceCounter
                                                                            , PerformanceCounter
                                                                        >[]
																    {

                                                                        Tuple
                                                                            .Create
                                                                                (
																			        false
																			        , stopwatchEnqueue
                                                                                    , qpcc
																				        .QueuedWaitAverageTimerPerformanceCounter
																			        , qpcc
																				        .QueuedWaitAverageBasePerformanceCounter
																		        )
																	    , Tuple
                                                                                .Create
                                                                                    (
																			            true
																			            , stopwatchDequeue
																			            , qpcc
																				            .DequeueProcessedAverageTimerPerformanceCounter
																			            , qpcc
																				            .DequeueProcessedAverageBasePerformanceCounter
																		            )
																    };

                                                    incrementCountersAfterCountPerformanceForDequeue
                                                            = new PerformanceCounter[]
																            {
																	            qpcc
																		            .DequeueProcessedPerformanceCounter
																	            , qpcc
																		            .DequeueProcessedRateOfCountsPerSecondPerformanceCounter
																            };
                                                }

                                                //var enabledCountPerformance = true;
                                                {
                                                    if (Sender._onEnabledCountPerformanceProcessFunc != null)
                                                    {
                                                        enabledCountPerformance = Sender._onEnabledCountPerformanceProcessFunc();
                                                    }
                                                }
                                                PerformanceCountersHelper
                                                        .TryCountPerformance
                                                            (
                                                                () =>
                                                                {
                                                                    return enabledCountPerformance;
                                                                }
                                                                , reThrowException
                                                                , incrementCountersBeforeCountPerformanceForDequeue
                                                                , decrementCountersBeforeCountPerformanceForDequeue
                                                                , timerCounters
                                                                , () =>			//try
                                                                {
                                                                    if (Sender.OnDequeue != null)
                                                                    {
                                                                        var element = item.Item2;
                                                                        item = null;
                                                                        Sender.OnDequeue(element);
                                                                    }
                                                                }
                                                                , (x, y, z) =>		//catch
                                                                {
                                                                    qpcc
                                                                       .CaughtExceptionsPerformanceCounter
                                                                       .Increment();
                                                                    if (Sender.OnDequeueProcessCaughtException != null)
                                                                    {
                                                                        reThrowException = Sender.OnDequeueProcessCaughtException(Sender, x, y, z);
                                                                    }
                                                                    if (!reThrowException)
                                                                    {
                                                                        if (Sender.OnCaughtException != null)
                                                                        {
                                                                            reThrowException = Sender.OnCaughtException(Sender, x, y, z);
                                                                        }
                                                                    }
                                                                    return reThrowException;
                                                                }
                                                                , null			//finally
                                                                , null
                                                                , incrementCountersAfterCountPerformanceForDequeue
                                                            );
                                                //池化
                                                stopwatchDequeue.Reset();
                                            
                                                var r = Sender._stopwatchsPool.Put(stopwatchDequeue);
                                                if (!r)
                                                {
                                                    stopwatchDequeue.Stop();
                                                    stopwatchDequeue = null;
                                                }
                                                stopwatchEnqueue.Reset();

                                                r = Sender._stopwatchsPool.Put(stopwatchEnqueue); ;
                                                if (!r)
                                                {
                                                    stopwatchEnqueue.Stop();
                                                    stopwatchEnqueue = null;
                                                }
                                            }
                                            #endregion while queue.IsEmpty loop
                                        }
                                        #region wait
                                        Sender
                                            ._dequeueThreadsProcessorsPool
                                            .Enqueue(this);
                                        if (Break)
                                        {
                                        }
                                        if (!Waiter.WaitOne(Sender.DequeueIdleSleepSeconds * 1000))
                                        {
                                        }
                                        #endregion wait
                                        #endregion while true loop
                                    }
                                    #endregion
                                }
                                , (x, y, z) =>			//catch
                                {
                                    #region Catch Process
                                    if (Sender.OnCaughtException != null)
                                    {
                                        reThrowException = Sender.OnCaughtException(Sender, x, y, z);
                                    }
                                    return reThrowException;
                                    #endregion
                                }
                                , (x, y, z, w) =>		//finally
                                {
                                    #region Finally Process
                                    l = Interlocked.Decrement(ref Sender._concurrentDequeueThreadsCount);
                                    if (l < 0)
                                    {
                                        Interlocked.Exchange(ref Sender._concurrentDequeueThreadsCount, 0);
                                    }
                                    if (Sender.OnDequeueThreadEnd != null)
                                    {
                                        Sender
                                            .OnDequeueThreadEnd
                                                (
                                                    string.Format
                                                            (
                                                                "{0} Threads Count {1},Queue Count {2},Current Thread: {3} at {4}"
                                                                , "Threads--"
                                                                , l
                                                                , Sender.InternalQueue.Count
                                                                , Thread.CurrentThread.Name
                                                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                                                            )
                                                );
                                    }
                                    if (!Break)
                                    {
                                        Sender.StartIncreaseDequeueProcessThreads(1);
                                    }
                                    Break = false;
                                    #endregion
                                }
                                , decrementCountersAfterCountPerformanceInThread
                                , incrementCountersAfterCountPerformanceInThread
                                
                            );
            }
        }
        public void AttachPerformanceCounters
                        (
                            string instanceNamePrefix
                            , string categoryName
                            , QueuePerformanceCountersContainer performanceCounters
                            , Func<bool> onEnabledCountPerformanceProcessFunc = null
                            , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                            , long? initializePerformanceCounterInstanceRawValue = null
                        )
        {
            var process = Process.GetCurrentProcess();
            var processName = process.ProcessName;
            var instanceName = string
                                    .Format
                                        (
                                            "{0}-{1}"
                                            , instanceNamePrefix
                                            , processName
                                        );
            PerformanceCountersContainer = performanceCounters;
            PerformanceCountersContainer
                .AttachPerformanceCountersToProperties
                        (
                            categoryName
                            , instanceName
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue
                        );
            _isAttachedPerformanceCounters = true;
            _onEnabledCountPerformanceProcessFunc = onEnabledCountPerformanceProcessFunc;
        }
        public int Count
        {
            get
            {
                return _queue.Count;
            }
        }
        public long ConcurrentThreadsCount
        {
            get
            {
                return _concurrentDequeueThreadsCount;
            }
        }
        public QueuedObjectsPool<Stopwatch> StopwatchsPool
        {
            get
            {
                return _stopwatchsPool;
            }
        }
        private void DecreaseDequeueProcessThreads(int count)
        {
            Action action;
            for (var i = 0; i < count; i++)
            {
                if (_callbackProcessBreaksActions.TryDequeue(out action))
                {
                    action();
                    action = null;
                }
            }
        }
        public void StartDecreaseDequeueProcessThreads(int count)
        {
            new Thread
                    (
                        new ThreadStart
                                (
                                    () =>
                                    {
                                        DecreaseDequeueProcessThreads(count);
                                    }
                                )
                    ).Start();
        }
        public void StartIncreaseDequeueProcessThreads(int count)
        {
            new Thread
                    (
                        new ThreadStart
                                (
                                    () =>
                                    {
                                        IncreaseDequeueProcessThreads(count);
                                    }
                                )
                    ).Start();
        }
        private void IncreaseDequeueProcessThreads(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Interlocked.Increment(ref _concurrentDequeueThreadsCount);
                if (_dequeueThreadsProcessorsPool == null)
                {
                    _dequeueThreadsProcessorsPool = new ConcurrentQueue<ThreadProcessor>();
                }
                var processor = new ThreadProcessor
                                                (
                                                    this
                                                    , new AutoResetEvent(false)
                                                );
                var thread = new Thread
                                    (
                                        new ThreadStart
                                                    (
                                                        processor.ThreadProcess
                                                    )
                                    );
                if (_callbackProcessBreaksActions == null)
                {
                    _callbackProcessBreaksActions = new ConcurrentQueue<Action>();
                }
                var callbackProcessBreakAction = new Action
                                                        (
                                                            processor.StopOne
                                                        );
                _callbackProcessBreaksActions.Enqueue(callbackProcessBreakAction);
                _dequeueThreadsProcessorsPool.Enqueue(processor);
                thread.Start();
            }
        }
        //private bool _enabledCountPerformance = false;

        //public bool EnabledCountPerformance
        //{
        //    get { return _enabledCountPerformance; }
        //    set { _enabledCountPerformance = value; }
        //}

        //private bool _enabledCountPerformance = false;

        private Func<bool> _onEnabledCountPerformanceProcessFunc;
        //{
        //    get;
        //    set;
        //}


        public bool Enqueue(T item)
        {
            var r = false;
            var reThrowException = false;
            //var enableCount = _isAttachedPerformanceCounters;

            var enabledCountPerformance = true;
            if (_onEnabledCountPerformanceProcessFunc != null)
            {
                enabledCountPerformance = _onEnabledCountPerformanceProcessFunc();
            }

            PerformanceCounter[] incrementCountersBeforeCountPerformance = null;
            var qpcc = PerformanceCountersContainer;
            if 
                (
                    qpcc != null
                    &&
                    enabledCountPerformance
                )
            {
                incrementCountersBeforeCountPerformance =
                   new PerformanceCounter[]
							{
								qpcc
									.EnqueuePerformanceCounter
								, qpcc
									.EnqueueRateOfCountsPerSecondPerformanceCounter
								, qpcc
									.QueueLengthPerformanceCounter
							};
            }

            PerformanceCountersHelper
                .TryCountPerformance
                    (
                        () =>
                        {
                            return enabledCountPerformance;
                        }
                        , reThrowException
                        , incrementCountersBeforeCountPerformance
                        , null
                        , null
                        , () =>
                        {
                            Stopwatch stopwatchEnqueue = null;
                            if (_isAttachedPerformanceCounters)
                            {
                                stopwatchEnqueue = _stopwatchsPool.Get();
                            }
                            stopwatchEnqueue.Start();
                            var element = Tuple
                                            .Create
                                                (
                                                    stopwatchEnqueue
                                                    , item
                                                );
                            _queue.Enqueue(element);
                            r = true;
                        }
                        , (x, y, z) =>
                        {
                            qpcc
                                .CaughtExceptionsPerformanceCounter
                                .Increment();
                            if (OnEnqueueProcessCaughtException != null)
                            {
                                reThrowException = OnEnqueueProcessCaughtException(this, x, y, z);
                            }
                            if (!reThrowException)
                            {
                                if (OnCaughtException != null)
                                {
                                    reThrowException = OnCaughtException(this, x, y, z);
                                }
                            }
                            return reThrowException;
                        }
                        , (x, y, z, w) =>
                        {
                            if
                                (
                                    _dequeueThreadsProcessorsPool != null
                                    && !_dequeueThreadsProcessorsPool.IsEmpty
                                )
                            {
                                ThreadProcessor processor;
                                if 
                                    (
                                        _dequeueThreadsProcessorsPool
                                                .TryDequeue(out processor)
                                    )
                                {
                                    processor.Waiter.Set();
                                    processor = null;
                                    //Console.WriteLine("processor = null;");
                                }
                            }
                        }
                    );
            return r;
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    public class QueuePerformanceCountersContainer :
                                                        AbstractPerformanceCountersContainer
                                                        , IPerformanceCountersContainer
    {
        #region PerformanceCounters
        private PerformanceCounter _caughtExceptionsPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "99.捕获异常次数(次)"
                    , Level = 10
                )
        ]
        public PerformanceCounter CaughtExceptionsPerformanceCounter
        {
            private set
            {
                _caughtExceptionsPerformanceCounter = value;
            }
            get
            {
                return _caughtExceptionsPerformanceCounter;
            }
        }
        private PerformanceCounter _enqueuePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "01.入队列累计总数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter EnqueuePerformanceCounter
        {
            private set
            {
                _enqueuePerformanceCounter = value;
            }
            get
            {
                return _enqueuePerformanceCounter;
            }
        }
        private PerformanceCounter _enqueueRateOfCountsPerSecondPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
                    , CounterName = "02.每秒入队列笔数(笔/秒)"
                )
        ]
        public PerformanceCounter EnqueueRateOfCountsPerSecondPerformanceCounter
        {
            private set
            {
                _enqueueRateOfCountsPerSecondPerformanceCounter = value;
            }
            get
            {
                return _enqueueRateOfCountsPerSecondPerformanceCounter;
            }
        }
        private PerformanceCounter _queueLengthPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "03.队列当前长度(笔)"
                    , Level = 10
                )
        ]
        public PerformanceCounter QueueLengthPerformanceCounter
        {
            private set
            {
                _queueLengthPerformanceCounter = value;
            }
            get
            {
                return _queueLengthPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeuePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "04.出队列累计总数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter DequeuePerformanceCounter
        {
            private set
            {
                _dequeuePerformanceCounter = value;
            }
            get
            {
                return _dequeuePerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedRateOfCountsPerSecondPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
                    , CounterName = "05.每秒出队列并完成处理笔数(笔/秒)"
                )
        ]
        public PerformanceCounter DequeueProcessedRateOfCountsPerSecondPerformanceCounter
        {
            private set
            {
                _dequeueProcessedRateOfCountsPerSecondPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedRateOfCountsPerSecondPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "06.已出队列并完成处理累计总笔数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter DequeueProcessedPerformanceCounter
        {
            private set
            {
                _dequeueProcessedPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedAverageTimerPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "07.每笔已出队列并完成处理平均耗时秒数(秒/笔)"
                )
        ]
        public PerformanceCounter DequeueProcessedAverageTimerPerformanceCounter
        {
            private set
            {
                _dequeueProcessedAverageTimerPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedAverageTimerPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedAverageBasePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageBase
                )
        ]
        public PerformanceCounter DequeueProcessedAverageBasePerformanceCounter
        {
            private set
            {
                _dequeueProcessedAverageBasePerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedAverageBasePerformanceCounter;
            }
        }
        private PerformanceCounter _queuedWaitAverageTimerPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "08.每笔入出队列并完成处理平均耗时秒数(秒/笔)"
                )
        ]
        public PerformanceCounter QueuedWaitAverageTimerPerformanceCounter
        {
            private set
            {
                _queuedWaitAverageTimerPerformanceCounter = value;
            }
            get
            {
                return _queuedWaitAverageTimerPerformanceCounter;
            }
        }
        private PerformanceCounter _queuedWaitAverageBasePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageBase
                )
        ]
        public PerformanceCounter QueuedWaitAverageBasePerformanceCounter
        {
            private set
            {
                _queuedWaitAverageBasePerformanceCounter = value;
            }
            get
            {
                return _queuedWaitAverageBasePerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadStartPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "09.新建出队列处理线程启动次数(次)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadStartPerformanceCounter
        {
            private set
            {
                _dequeueThreadStartPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadStartPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadsCountPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "10.当前出队列并发处理线程数(个)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadsCountPerformanceCounter
        {
            private set
            {
                _dequeueThreadsCountPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadsCountPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadEndPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "11.出队列处理线程退出次数(次)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadEndPerformanceCounter
        {
            private set
            {
                _dequeueThreadEndPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadEndPerformanceCounter;
            }
        }
        #endregion
        // indexer declaration
        public override PerformanceCounter this[string name]
        {
            get
            {
                return
                    GetPerformanceCounterByName<QueuePerformanceCountersContainer>
                        (
                            this
                            , name
                        );
            }
        }
        public override IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                //throw new NotImplementedException();
                return
                    (
                        GetPropertiesPerformanceCounters<QueuePerformanceCountersContainer>
                            (
                                this
                            )
                    );
            }
        }

        public override PerformanceCounter[] IncrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] DecrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] IncrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] DecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] IncrementOnBeginDecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] TimeBasedOnBeginOnEndPerformanceCounters
        {
            get;
            set;
        }

        public override PerformanceCountersPair[] TimeBasedOnBeginOnEndPerformanceCountersPairs
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            //throw new NotImplementedException();
            return
                GetPerformanceCountersByLevel<QueuePerformanceCountersContainer>
                    (
                        this
                        , level
                    );
        }

        private bool _isAttachedPerformanceCounters = false;
        public override void AttachPerformanceCountersToProperties
                                (
                                    string categoryName
                                    , string instanceName
                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                    , long? initializePerformanceCounterInstanceRawValue = null
                                )
        {
            if (!_isAttachedPerformanceCounters)
            {
                //var type = this.GetType();
                AttachPerformanceCountersToProperties<QueuePerformanceCountersContainer>
                        (
                            categoryName
                            , instanceName
                            , this
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue
                        );
            }
            _isAttachedPerformanceCounters = true;
        }
    }
}

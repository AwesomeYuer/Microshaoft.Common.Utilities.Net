namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessor<T>
                        //where T : class
    {
        public Func<bool> OnGetEnabledCountPerformanceProcessFunc
        {
            private set;
            get;
        }

        private bool _isAttachedPerformanceCounters = false;
        private readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = null;
        private ConcurrentQueue<Tuple<Stopwatch, T>>
                            _queue = new ConcurrentQueue<Tuple<Stopwatch, T>>();

        private QueuePerformanceCountersContainer
                    _queuePerformanceCountersContainer
                        = new QueuePerformanceCountersContainer();

        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessor<T> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException
                                            , OnEnqueueProcessCaughtException;
                                            //, OnDequeueProcessCaughtException
                                            //, OnBatchProcessCaughtException;


        private string _performanceCountersCategoryNameForQueueProcess = string.Empty;
        private string _performanceCountersCategoryNameForBatchProcess = string.Empty;
        private string _performanceCountersCategoryInstanceNameForQueueProcess = string.Empty;
        private string _performanceCountersCategoryInstanceNameForBatchProcess = string.Empty;


        public SingleThreadAsyncDequeueProcessor(long stopwatchsPoolMaxCapacity = 10 * 100)
        {
            _stopwatchsPool = new QueuedObjectsPool<Stopwatch>
                                        (
                                            stopwatchsPoolMaxCapacity
                                        );
            
        }
        public void AttachPerformanceCountersCategoryInstance
                            (
                                string performanceCountersCategoryNamePrefix
                                , string performanceCountersCategoryInstanceNamePrefix
                                , Func<bool> onGetEnabledCountPerformanceProcessFunc = null
                                , PerformanceCounterInstanceLifetime
                                        performanceCounterInstanceLifetime
                                            = PerformanceCounterInstanceLifetime.Process
                            )
        {
            //EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>.AttachPerformanceCountersCategoryInstance
            var process = Process.GetCurrentProcess();
            var processName = process.ProcessName;
            var instanceNamePrefix = string
                                    .Format
                                        (
                                            "{0}-{1}"
                                            , processName
                                            , performanceCountersCategoryInstanceNamePrefix
                                        );
            var suffix = "-Queue";
            _performanceCountersCategoryNameForQueueProcess = performanceCountersCategoryNamePrefix + suffix;
            _performanceCountersCategoryInstanceNameForQueueProcess = instanceNamePrefix + suffix;
            var qpcc = _queuePerformanceCountersContainer;
            qpcc
                .AttachPerformanceCountersToMembers
                        (
                            _performanceCountersCategoryNameForQueueProcess
                            , _performanceCountersCategoryInstanceNameForQueueProcess
                            , performanceCounterInstanceLifetime
                        );
            qpcc
                .RegisterCountersUsage();

            suffix = "-BatchProcess";

            _performanceCountersCategoryNameForBatchProcess = performanceCountersCategoryNamePrefix + suffix;
            _performanceCountersCategoryInstanceNameForBatchProcess = instanceNamePrefix + suffix;
            CommonPerformanceCountersContainer container = null;
            EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                .AttachPerformanceCountersCategoryInstance
                    (
                        _performanceCountersCategoryNameForBatchProcess
                        , _performanceCountersCategoryInstanceNameForBatchProcess
                        , out container
                        , performanceCounterInstanceLifetime
                    );
            

            _isAttachedPerformanceCounters = true;
            OnGetEnabledCountPerformanceProcessFunc = onGetEnabledCountPerformanceProcessFunc;
        }

        public bool Enqueue(T item)
        {
            var r = false;
            var reThrowException = false;
            //var enableCount = _isAttachedPerformanceCounters;

            var enabledCountPerformance = true;
            var qpcc = _queuePerformanceCountersContainer;
            if (OnGetEnabledCountPerformanceProcessFunc != null)
            {
                enabledCountPerformance = OnGetEnabledCountPerformanceProcessFunc();
            }

            PerformanceCountersHelper
                .TryCountPerformance
                    (
                        () =>
                        {
                            return enabledCountPerformance;
                        }
                        , reThrowException
                        , //incrementCountersBeforeCountPerformance
                            qpcc
                                .IncrementCountersBeforeCountPerformanceForEnqueue
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
                        
                    );
            return r;
        }

        public void StartRunDequeuesThreadProcess
             (
                Action<long, T> onOnceDequeueProcessAction = null
                , int sleepInMilliseconds = 1000
                , Action<long, List<Tuple<long, T>>> onBatchDequeuesProcessAction = null
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
                , Func<Exception, Exception, string, bool> onDequeueProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeueProcessFinallyProcessAction = null
                , Func<Exception, Exception, string, bool> onDequeuesBatchProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeuesBatchProcessFinallyProcessAction = null
            )
        {
            new Thread
                    (
                        () =>
                        {
                            DequeueProcess
                                (
                                    onOnceDequeueProcessAction
                                    , sleepInMilliseconds
                                    , onBatchDequeuesProcessAction
                                    , waitOneBatchTimeOutInMilliseconds
                                    , waitOneBatchMaxDequeuedTimes
                                    , onDequeueProcessCaughtExceptionProcessFunc
                                    , onDequeueProcessFinallyProcessAction
                                    , onDequeuesBatchProcessCaughtExceptionProcessFunc
                                    , onDequeuesBatchProcessFinallyProcessAction

                                );
                        }
                    ).Start();
        }
        private bool _isStartedDequeueProcess = false;
        private void DequeueProcess
            (
                Action<long, T> onOnceDequeueProcessAction = null
                , int sleepInMilliseconds = 100
                , Action<long, List<Tuple<long, T>>> onBatchDequeuesProcessAction = null
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
                , Func<Exception, Exception, string, bool> onDequeueProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeueProcessFinallyProcessAction = null
                , Func<Exception, Exception, string, bool> onDequeuesBatchProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeuesBatchProcessFinallyProcessAction = null
            )
        {
            if (!_isStartedDequeueProcess)
            {
                return;
            }
            List<Tuple<long, T>> list = null;
            long i = 0;
            Stopwatch stopwatch = null;
            var timerCounters = new WriteableTuple
                                        <
                                            bool
                                            , Stopwatch
                                            , PerformanceCounter
                                            , PerformanceCounter
                                        >[]
                                    {
                                        WriteableTuple
                                            .Create
                                                <
                                                    bool
                                                    , Stopwatch
                                                    , PerformanceCounter
                                                    , PerformanceCounter
                                                >
                                                (
                                                    false
                                                    , null
                                                    , _queuePerformanceCountersContainer
                                                        .QueuedWaitAverageTimerPerformanceCounter
                                                    , _queuePerformanceCountersContainer
                                                        .QueuedWaitAverageBasePerformanceCounter
                                                )
                                        , WriteableTuple
                                                .Create
                                                    <
                                                        bool
                                                        , Stopwatch
                                                        , PerformanceCounter
                                                        , PerformanceCounter
                                                    >
                                                    (
                                                        true
                                                        , null
                                                        , _queuePerformanceCountersContainer
                                                            .DequeueProcessedAverageTimerPerformanceCounter
                                                        , _queuePerformanceCountersContainer
                                                            .DequeueProcessedAverageBasePerformanceCounter
                                                    )
                                    };

            if (onBatchDequeuesProcessAction != null)
            {
                list = new List<Tuple<long, T>>();
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            while (true)
            {
                TryCatchFinallyProcessHelper
                    .TryProcessCatchFinally
                        (
                            true
                            , () =>
                                {
                                    if (!_queue.IsEmpty)
                                    {
                                        Tuple<Stopwatch, T> element = null;
                                        if (_queue.TryDequeue(out element))
                                        {
                                            if (onOnceDequeueProcessAction != null)
                                            {
                                                #region while queue.IsEmpty loop
                                                var enabledCountPerformance = true;
                                                {
                                                    if (OnGetEnabledCountPerformanceProcessFunc != null)
                                                    {
                                                        enabledCountPerformance = OnGetEnabledCountPerformanceProcessFunc();
                                                    }
                                                }
                                                var qpcc = _queuePerformanceCountersContainer;
                                                
                                                var stopwatchDequeue = _stopwatchsPool.Get();
                                                var stopwatchEnqueue = element.Item1;
                                                timerCounters[0].Item2 = stopwatchEnqueue;
                                                timerCounters[1].Item2 = stopwatchDequeue;

                                                if (OnGetEnabledCountPerformanceProcessFunc != null)
                                                {
                                                    enabledCountPerformance = OnGetEnabledCountPerformanceProcessFunc();
                                                }

                                                var reThrowException = false;
                                                PerformanceCountersHelper
                                                            .TryCountPerformance
                                                                (
                                                                    () =>
                                                                    {
                                                                        return enabledCountPerformance;
                                                                    }
                                                                    , reThrowException
                                                                    , qpcc.IncrementCountersBeforeCountPerformanceForDequeue
                                                                    , qpcc.DecrementCountersBeforeCountPerformanceForDequeue
                                                                    , null //timerCounters
                                                                    , () =>         //try
                                                                    {
                                                                        if (onOnceDequeueProcessAction != null)
                                                                        {
                                                                            var item = element.Item2;
                                                                            element = null;
                                                                            onOnceDequeueProcessAction
                                                                                (i, item);
                                                                        }
                                                                    }
                                                                    , (x, y, z) =>      //catch
                                                                    {
                                                                        qpcc
                                                                           .CaughtExceptionsPerformanceCounter
                                                                           .Increment();
                                                                        if (onDequeueProcessCaughtExceptionProcessFunc != null)
                                                                        {
                                                                            reThrowException = onDequeueProcessCaughtExceptionProcessFunc
                                                                                                            (
                                                                                                                x
                                                                                                                , y
                                                                                                                , z
                                                                                                            );
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
                                                                    , onDequeueProcessFinallyProcessAction          //finally
                                                                    , null
                                                                    , qpcc.IncrementCountersAfterCountPerformanceForDequeue
                                                                );
                                                    //池化
                                                    stopwatchEnqueue.Reset();
                                                    stopwatchDequeue.Reset();

                                                    var r = _stopwatchsPool.Put(stopwatchDequeue);
                                                    if (!r)
                                                    {
                                                        stopwatchDequeue.Stop();
                                                        stopwatchDequeue = null;
                                                    }
                                                    r = _stopwatchsPool.Put(stopwatchEnqueue);
                                                    if (!r)
                                                    {
                                                        stopwatchEnqueue.Stop();
                                                        stopwatchEnqueue = null;
                                                    }
                                                
                                                #endregion while queue.IsEmpty loop
                                            }
                                            if (onBatchDequeuesProcessAction != null)
                                            {
                                                i++;
                                                var item = element.Item2;
                                                var tuple
                                                        = Tuple
                                                            .Create<long, T>
                                                                  (
                                                                    i
                                                                    , item
                                                                  );
                                                list.Add(tuple);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (sleepInMilliseconds > 0)
                                        {
                                            Thread.Sleep(sleepInMilliseconds);
                                        }
                                    }
                                    if (onBatchDequeuesProcessAction != null)
                                    {
                                        if
                                        (
                                            i >= waitOneBatchMaxDequeuedTimes
                                            ||
                                            stopwatch.ElapsedMilliseconds > waitOneBatchTimeOutInMilliseconds
                                        )
                                        {
                                            if (i > 0)
                                            {
                                                if (stopwatch != null)
                                                {
                                                    stopwatch.Stop();
                                                }
                                                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                                                    .TryCountPerformance
                                                        (
                                                            PerformanceCounterProcessingFlagsType.All
                                                            , _performanceCountersCategoryNameForBatchProcess
                                                            , _performanceCountersCategoryInstanceNameForBatchProcess
                                                            , () =>
                                                            {
                                                                return OnGetEnabledCountPerformanceProcessFunc();
                                                            }
                                                            , () =>
                                                            {
                                                                onBatchDequeuesProcessAction
                                                                    (
                                                                        i
                                                                        , list
                                                                    );
                                                            }
                                                            , null
                                                            , (xx, yy, zz) =>
                                                            {
                                                                var rrr = false;
                                                                if (onDequeuesBatchProcessCaughtExceptionProcessFunc != null)
                                                                {
                                                                    rrr = onDequeuesBatchProcessCaughtExceptionProcessFunc
                                                                            (
                                                                                xx, yy, zz
                                                                            );
                                                                }
                                                                return rrr;
                                                            }
                                                            , (xx, yy, zz, ww) =>
                                                            {
                                                                i = 0;
                                                                list.Clear();
                                                                if (stopwatch != null)
                                                                {
                                                                    stopwatch.Restart();
                                                                }
                                                                onDequeuesBatchProcessFinallyProcessAction?
                                                                        .Invoke(xx, yy, zz, ww);
                                                            }
                                                        );
                                            }
                                        }
                                    }
                                }
                           , false
                           , (x, y, z) =>
                               {
                                   var rr = false;
                                   if (OnCaughtException != null)
                                   {
                                       rr = OnCaughtException
                                               (
                                                   this, x, y, z
                                               );
                                   }
                                   return rr;
                               }
                        );
            }
        }
    }
}
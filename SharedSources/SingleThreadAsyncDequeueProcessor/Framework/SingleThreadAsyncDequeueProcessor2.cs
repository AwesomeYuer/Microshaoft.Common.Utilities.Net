#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessor<TQueueElement, TDequeueElement>
    {
        public Func<bool> OnGetEnabledCountPerformanceProcessFunc
        {
            private set;
            get;
        }

        public int QueueLength
        {
            get
            {
                return
                    _queue.Count;
            }

        }
        private bool _isAttachedPerformanceCounters = false;
        private readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = null;
        private ConcurrentQueue<Tuple<Stopwatch, TQueueElement>>
                            _queue
                                    = new ConcurrentQueue<Tuple<Stopwatch, TQueueElement>>();
        private QueuePerformanceCountersContainer
                            _queuePerformanceCountersContainer
                                    = new QueuePerformanceCountersContainer();

        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessor<TQueueElement, TDequeueElement> sender
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
            var process = Process.GetCurrentProcess();
            var processName = process.ProcessName;
            var instanceNamePrefix = string
                                        .Format
                                            (
                                                "{0}-{1}"
                                                , processName
                                                , performanceCountersCategoryInstanceNamePrefix
                                            );
            instanceNamePrefix = performanceCountersCategoryInstanceNamePrefix;
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

            if (_timerCounters == null)
            {
                _timerCounters = new WriteableTuple
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
            }
            _isAttachedPerformanceCounters = true;
            OnGetEnabledCountPerformanceProcessFunc = onGetEnabledCountPerformanceProcessFunc;
        }

        public bool Enqueue(TQueueElement item)
        {
            var r = false;
            var reThrowException = false;

            var enabledCountPerformance = true;
            if (OnGetEnabledCountPerformanceProcessFunc != null)
            {
                enabledCountPerformance = OnGetEnabledCountPerformanceProcessFunc();
            }

            var qpcc = _queuePerformanceCountersContainer;
            PerformanceCountersHelper
                .TryCountPerformance
                    (
                        () =>
                        {
                            return enabledCountPerformance;
                        }
                        , reThrowException
                        , qpcc
                            .IncrementCountersBeforeCountPerformanceForEnqueue
                        //incrementCountersBeforeCountPerformance
                        , null
                        , null
                        , () =>
                        {
                            Stopwatch stopwatchEnqueue = null;
                            if (_isAttachedPerformanceCounters)
                            {
                                _stopwatchsPool.TryGet(out stopwatchEnqueue);
                            }
                            stopwatchEnqueue.Start();
                            var element = Tuple
                                            .Create
                                                (
                                                    stopwatchEnqueue
                                                    , item
                                                );
                            _queue.Enqueue(element);
                            //Thread.Sleep(100);
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

        public void StartRunDequeueThreadProcess
             (
                Func<long, TQueueElement, TDequeueElement> onOnceDequeueProcessFunc = null
                , int sleepInMilliseconds = 1000
                , Action<long, List<Tuple<long, TDequeueElement>>> onBatchDequeuesProcessAction = null
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
                                    onOnceDequeueProcessFunc
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
        private WriteableTuple
                                   <
                                       bool
                                       , Stopwatch
                                       , PerformanceCounter
                                       , PerformanceCounter
                                   >[] _timerCounters = null;
        private bool _isStartedDequeueProcess = false;
        private void DequeueProcess
            (
                Func<long, TQueueElement, TDequeueElement> onOnceDequeueProcessFunc
                , int sleepInMilliseconds = 100
                , Action<long, List<Tuple<long, TDequeueElement>>> onBatchDequeuesProcessAction = null
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
                , Func<Exception, Exception, string, bool> onDequeueProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeueProcessFinallyProcessAction = null
                , Func<Exception, Exception, string, bool> onDequeuesBatchProcessCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onDequeuesBatchProcessFinallyProcessAction = null
            )
        {
            if (_isStartedDequeueProcess)
            {
                return;
            }
            List<Tuple<long, TDequeueElement>> list = null;
            long i = 0;
            Stopwatch stopwatch = null;
            if (onBatchDequeuesProcessAction != null)
            {
                list = new List<Tuple<long, TDequeueElement>>();
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
                                    Tuple<Stopwatch, TQueueElement> element = null;
                                    if (_queue.TryDequeue(out element))
                                    {
                                        var enabledCountPerformance = true;
                                        {
                                            if (OnGetEnabledCountPerformanceProcessFunc != null)
                                            {
                                                enabledCountPerformance = OnGetEnabledCountPerformanceProcessFunc();
                                            }
                                        }

                                        var qpcc = _queuePerformanceCountersContainer;
                                        Stopwatch stopwatchDequeue = null;
                                        var stopwatchEnqueue = element.Item1;
                                        if (enabledCountPerformance)
                                        {
                                            _stopwatchsPool.TryGet(out stopwatchDequeue);
                                        }

                                        _timerCounters[0].Item2 = stopwatchEnqueue;
                                        _timerCounters[1].Item2 = stopwatchDequeue;

#region while queue.IsEmpty loop
                                        var reThrowException = false;
                                        TDequeueElement elementWrapper = default(TDequeueElement);
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
                                                            , _timerCounters
                                                            , () =>         //try
                                                            {
                                                                if (onOnceDequeueProcessFunc != null)
                                                                {
                                                                    var item = element.Item2;
                                                                    elementWrapper = onOnceDequeueProcessFunc
                                                                        (i, item);
                                                                }
                                                            }
                                                            , (x, y, z) =>      //catch
                                                            {
                                                                qpcc
                                                                    .CaughtExceptionsPerformanceCounter
                                                                    .Increment();

                                                                z = "OnDequeueProcessCaughtExceptionProcessFunc\r\n" + z;
                                                                if (onDequeueProcessCaughtExceptionProcessFunc != null)
                                                                {
                                                                    reThrowException = onDequeueProcessCaughtExceptionProcessFunc
                                                                                                    (
                                                                                                        x
                                                                                                        , y
                                                                                                        , z
                                                                                                    );
                                                                }
                                                                else if (OnCaughtException != null)
                                                                {
                                                                    reThrowException = OnCaughtException(this, x, y, z);
                                                                }
                                                                return reThrowException;
                                                            }
                                                            , onDequeueProcessFinallyProcessAction          //finally
                                                            , null
                                                            , qpcc.IncrementCountersAfterCountPerformanceForDequeue
                                                        );
#endregion while queue.IsEmpty loop

                                        //池化

                                        if (stopwatchEnqueue != null)
                                        {
                                            stopwatchEnqueue.Reset();
                                            var r = _stopwatchsPool.TryPut(stopwatchEnqueue);
                                            if (!r)
                                            {
                                                stopwatchEnqueue.Stop();
                                                stopwatchEnqueue = null;
                                            }
                                        }
                                        if (stopwatchDequeue != null)
                                        {
                                            stopwatchDequeue.Reset();

                                            var r = _stopwatchsPool.TryPut(stopwatchDequeue);
                                            if (!r)
                                            {
                                                stopwatchDequeue.Stop();
                                                stopwatchDequeue = null;
                                            }
                                        }
                                        if (onBatchDequeuesProcessAction != null)
                                        {
                                            i++;
                                            //var item = element.Item2;
                                            var tuple
                                                    = Tuple
                                                        .Create<long, TDequeueElement>
                                                              (
                                                                i
                                                                , elementWrapper
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
                                                            zz = "OnDequeueProcessCaughtExceptionProcessFunc\r\n" + zz;
                                                            if (onDequeuesBatchProcessCaughtExceptionProcessFunc != null)
                                                            {
                                                                rrr = onDequeuesBatchProcessCaughtExceptionProcessFunc
                                                                        (
                                                                            xx, yy, zz
                                                                        );
                                                            }
                                                            else if (OnCaughtException != null)
                                                            {
                                                                rrr = OnCaughtException(this, xx, yy, zz);
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
#endif

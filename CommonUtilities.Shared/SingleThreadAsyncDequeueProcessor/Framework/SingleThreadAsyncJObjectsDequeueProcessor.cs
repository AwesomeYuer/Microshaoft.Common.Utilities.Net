#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    public class SingleThreadAsyncJTokensDequeueProcessor
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
        private ConcurrentQueue<Tuple<Stopwatch, JToken>>
                            _queue
                                    = new ConcurrentQueue<Tuple<Stopwatch, JToken>>();
        private QueuePerformanceCountersContainer
                            _queuePerformanceCountersContainer
                                    = new QueuePerformanceCountersContainer();

        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncJTokensDequeueProcessor sender
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
        public SingleThreadAsyncJTokensDequeueProcessor(long stopwatchsPoolMaxCapacity = 10 * 100)
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
        public bool Enqueue(JToken item)
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

        public void StartRunDequeueThreadProcess<TBatchGroupByKey>
             (
                Func<long, JToken, bool> onOnceDequeueProcessFunc = null
                , int sleepInMilliseconds = 1000
                , Func<JToken, TBatchGroupByKey> keySelector = null
                , Action<long, bool, TBatchGroupByKey, IEnumerable<JToken>> onBatchDequeuesProcessAction = null
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
                                    , keySelector
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
        private void DequeueProcess<TBatchGroupByKey>
            (
                Func<long, JToken, bool> onOnceDequeueProcessFunc
                , int sleepInMilliseconds = 100
                , Func<JToken, TBatchGroupByKey> keySelector = null
                , Action<long, bool, TBatchGroupByKey, IEnumerable<JToken>> onBatchDequeuesProcessAction = null
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
            List<JToken> list = null;
            var needBatchGroupBy = false;
            if (keySelector != null)
            {
                needBatchGroupBy = true;
            }
            long i = 0;
            Stopwatch stopwatch = null;
            if (onBatchDequeuesProcessAction != null)
            {
                list = new List<JToken>();
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
                                    Tuple<Stopwatch, JToken> element = null;
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
                                        JToken item = null;
                                        bool needAdd = false; 
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
                                                                    item = element.Item2;
                                                                    needAdd = onOnceDequeueProcessFunc
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
                                            
                                            //var item = element.Item2;
                                            if (needAdd)
                                            {
                                                i++;
                                                list.Add(item);
                                            }
                                            
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

                                                            if (needBatchGroupBy)
                                                            {
                                                                var groups = list
                                                                                .GroupBy
                                                                                    (
                                                                                        (x) =>
                                                                                        {
                                                                                            return keySelector(x);
                                                                                        }
                                                                                    );

                                                                groups
                                                                    .AsParallel()
                                                                    .ForAll
                                                                        (
                                                                            (group) =>
                                                                            {
                                                                                onBatchDequeuesProcessAction
                                                                                    (
                                                                                        i
                                                                                        , needBatchGroupBy
                                                                                        , group.Key
                                                                                        , group.AsEnumerable()
                                                                                    );
                                                                            }
                                                                        );
                                                                //foreach (var group in groups)
                                                                //{
                                                                    
                                                                //}
                                                            }
                                                            else
                                                            {
                                                                onBatchDequeuesProcessAction
                                                                (
                                                                    i
                                                                    , needBatchGroupBy
                                                                    , default(TBatchGroupByKey)
                                                                    , list
                                                                );

                                                            }

                                                            
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

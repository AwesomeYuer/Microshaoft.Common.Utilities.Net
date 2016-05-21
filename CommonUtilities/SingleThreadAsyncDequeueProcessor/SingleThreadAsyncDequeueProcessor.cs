namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessor<T>
                        where T : class
    {
        private Func<bool> _onEnabledCountPerformanceProcessFunc;
        private bool _isAttachedPerformanceCounters = false;
        private readonly QueuedObjectsPool<Stopwatch> _stopwatchsPool = null;
        private ConcurrentQueue<Tuple<Stopwatch, T>>
                            _queue = new ConcurrentQueue<Tuple<Stopwatch, T>>();

        private QueuePerformanceCountersContainer
                    _performanceCountersContainer
                        = new QueuePerformanceCountersContainer();
        //public ConcurrentQueue<T> InternalQueue
        //{
        //    get { return _queue; }
        //}
        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessor<T> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException
                                            , OnEnqueueProcessCaughtException
                                            , OnDequeueProcessCaughtException;



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
            var qpcc = _performanceCountersContainer;
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
                , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
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
                                    , onCaughtExceptionProcessFunc
                                    , onFinallyProcessAction
                                );
                        }
                    ).Start();
        }
        private void DequeueProcess
            (
                Action<long, T> onOnceDequeueProcessAction = null
                , int sleepInMilliseconds = 100
                , Action<long, List<Tuple<long, T>>> onBatchDequeuesProcessAction = null
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
                , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
            )
        {
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
                                            , _performanceCountersContainer
                                                .QueuedWaitAverageTimerPerformanceCounter
                                            , _performanceCountersContainer
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
                                                , _performanceCountersContainer
                                                    .DequeueProcessedAverageTimerPerformanceCounter
                                                , _performanceCountersContainer
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
                                                    if (_onEnabledCountPerformanceProcessFunc != null)
                                                    {
                                                        enabledCountPerformance = _onEnabledCountPerformanceProcessFunc();
                                                    }
                                                }
                                                var qpcc = _performanceCountersContainer;
                                                
                                                var stopwatchDequeue = _stopwatchsPool.Get();
                                                var stopwatchEnqueue = element.Item1;
                                                //timerCounters[0].Item2 = stopwatchEnqueue;
                                                //timerCounters[1].Item2 = stopwatchDequeue;

                                                //var enabledCountPerformance = true;
                                                {
                                                    if (_onEnabledCountPerformanceProcessFunc != null)
                                                    {
                                                        enabledCountPerformance = _onEnabledCountPerformanceProcessFunc();
                                                    }
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
                                                                        if (OnDequeueProcessCaughtException != null)
                                                                        {
                                                                            reThrowException = OnDequeueProcessCaughtException
                                                                                                            (
                                                                                                                this
                                                                                                                , x
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
                                                                    , null          //finally
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
                                                TryCatchFinallyProcessHelper
                                                    .TryProcessCatchFinally
                                                        (
                                                            true
                                                            , () =>
                                                            {
                                                                onBatchDequeuesProcessAction
                                                                    (
                                                                        i
                                                                        , list
                                                                    );
                                                            }
                                                            , false
                                                            , (xx, yy, zz) =>
                                                            {
                                                                var rrr = false;
                                                                if (onCaughtExceptionProcessFunc != null)
                                                                {
                                                                    rrr = onCaughtExceptionProcessFunc
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
                                                                if (onFinallyProcessAction != null)
                                                                {
                                                                    onFinallyProcessAction
                                                                            (
                                                                                xx, yy, zz, ww
                                                                            );
                                                                }
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
                                   if (onCaughtExceptionProcessFunc != null)
                                   {
                                       rr = onCaughtExceptionProcessFunc
                                               (
                                                   x, y, z
                                               );
                                   }
                                   return rr;
                               }
                        );
            }
        }
    }
}
namespace Microshaoft
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessorSlim<TElement>
    {
        [JsonIgnore]
        public readonly Type ElementType;
        [JsonIgnore]
        public readonly Type QueueElementType;

        public SingleThreadAsyncDequeueProcessorSlim()
        {
            ElementType = typeof(TElement);
            QueueElementType = _queue
                                    .GetType()
                                    .GetGenericArguments()[0];
        }

        public int QueueLength
        {
            get
            {
                return
                    _queue.Count;
            }
        }

        public DataTable GenerateEmptyDataTableByElementType(params string[] dataColumnsNames)
        {
            return
                ValueTupleHelper
                        .GenerateEmptyDataTable
                            (
                                ElementType
                                , dataColumnsNames
                            );
        }

        public DataTable GenerateEmptyDataTableByQueueElementType(params string[] dataColumnsNames)
        {
            return
                ValueTupleHelper
                        .GenerateEmptyDataTable
                            (
                                QueueElementType
                                , dataColumnsNames
                            );
        }
        private ConcurrentQueue
                        <
                            (
                                long ID
                                ,
                                    (
                                        long?           EnqueueTimestamp
                                        , DateTime?     EnqueueTime
                                        , DateTime?     DequeueTime
                                        , long?         DequeueTimestamp
                                        , DateTime?     DequeueProcessedTime
                                        , long?         DequeueProcessedTimestamp
                                    ) Timing
                                , TElement Element
                            )
                        >
                            _queue
                                    = new ConcurrentQueue
                                            <
                                                (
                                                    long ID
                                                    ,
                                                        (
                                                            long?           EnqueueTimestamp
                                                            , DateTime?     EnqueueTime
                                                            , DateTime?     DequeueTime
                                                            , long?         DequeueTimestamp
                                                            , DateTime?     DequeueProcessedTime
                                                            , long?         DequeueProcessedTimestamp
                                                        ) Timing
                                                    , TElement Element
                                                )
                                            >();
        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessorSlim<TElement> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                        , DateTime? exceptionTime
                                        , string source
                                        , Guid? traceID
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException;

        private long _enqueued = 0;
        public long Enqueued
        {
            get
            {
                return _enqueued;
            }
        }
        private long _dequeued = 0;
        public long Dequeued
        {
            get
            {
                return _dequeued;
            }
        }
        private long _dequeuedBatches = 0;
        public long DequeuedBatches
        {
            get
            {
                return _dequeuedBatches;
            }
        }
        public bool Enqueue(TElement element)
        {
            var r = false;
            _queue
                .Enqueue
                    (
                        (
                            Interlocked
                                    .Increment(ref _enqueued)
                            , 
                                (
                                    Stopwatch.GetTimestamp()
                                    , DateTime.Now
                                    , null
                                    , null
                                    , null
                                    , null
                                )
                            , element
                        )
                    );
            return r;
        }
        public void StartRunDequeueThreadProcess
             (
                Action
                        <
                            long            // Dequeued
                            , long          // Dequeued Batch
                            , int           // ID in one Batch
                            ,
                                (
                                    long ID
                                    ,
                                        (
                                            long?           EnqueueTimestamp
                                            , DateTime?     EnqueueTime
                                            , DateTime?     DequeueTime
                                            , long?         DequeueTimestamp
                                            , DateTime?     DequeueProcessedTime
                                            , long?         DequeueProcessedTimestamp
                                        ) Timing
                                    , TElement Element
                                )
                        >
                            onOnceDequeueProcessAction
                , Action
                        <
                            long            // Dequeued
                            , long          // Dequeued Batch
                            , int           // ID in one Batch
                        >
                            onBatchDequeuesProcessAction
                , int sleepInMilliseconds = 1000
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
            )
        {
            if (_isStartedDequeueProcess)
            {
                return;
            }
            _isStartedDequeueProcess = true;
            new Thread
                    (
                        () =>
                        {
                            DequeueProcess
                                (
                                    onOnceDequeueProcessAction
                                    , onBatchDequeuesProcessAction
                                    , sleepInMilliseconds
                                    , waitOneBatchTimeOutInMilliseconds
                                    , waitOneBatchMaxDequeuedTimes
                                );
                        }
                    ).Start();
        }

        private bool _isStartedDequeueProcess = false;
        private void DequeueProcess
            (
               Action
                        <
                            long            // Dequeued
                            , long          // Dequeued Batch
                            , int           // ID in one Batch
                            ,
                                (
                                    long ID
                                    ,
                                        (
                                            long?           EnqueueTimestamp
                                            , DateTime?     EnqueueTime
                                            , DateTime?     DequeueTime
                                            , long?         DequeueTimestamp
                                            , DateTime?     DequeueProcessedTime
                                            , long?         DequeueProcessedTimestamp
                                        ) Timing
                                    , TElement Element
                                )
                        >
                            onOnceDequeueProcessAction
                , Action
                        <
                            long            // Dequeued
                            , long          // Dequeued Batch
                            , int           // ID in one Batch
                        >
                            onBatchDequeuesProcessAction
                , int sleepInMilliseconds = 100
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
            )
        {
            int i = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                var exceptionSource = string.Empty;
                TryCatchFinallyProcessHelper
                    .TryProcessCatchFinally
                        (
                            true
                            , () =>
                            {
                                if (!_queue.IsEmpty)
                                {
                                    if
                                        (
                                            _queue
                                                .TryDequeue
                                                    (
                                                        out var queueElement
                                                    )
                                        )
                                    {
                                        Interlocked
                                                .Increment(ref _dequeued);
                                        i ++;
                                        queueElement
                                            .Timing
                                            .DequeueTime = DateTime.Now;
                                        queueElement
                                            .Timing
                                            .DequeueTimestamp = Stopwatch.GetTimestamp();
                                        exceptionSource = $"{nameof(onOnceDequeueProcessAction)}";
                                        onOnceDequeueProcessAction
                                                        (
                                                            _dequeued
                                                            , _dequeuedBatches
                                                            , i
                                                            , queueElement      //.QueueElement
                                                        );
                                        queueElement
                                            .Timing
                                            .DequeueProcessedTime = DateTime.Now;
                                        queueElement
                                            .Timing
                                            .DequeueProcessedTimestamp = Stopwatch.GetTimestamp();
                                    }
                                }
                                else
                                {
                                    if (sleepInMilliseconds > 0)
                                    {
                                        Thread
                                            .Sleep(sleepInMilliseconds);
                                    }
                                }
                                if (onBatchDequeuesProcessAction != null)
                                {
                                    if
                                        (
                                            i >=
                                                waitOneBatchMaxDequeuedTimes
                                            ||
                                            stopwatch
                                                .ElapsedMilliseconds >
                                                        waitOneBatchTimeOutInMilliseconds
                                        )
                                    {
                                        if (i > 0)
                                        {
                                            if (stopwatch != null)
                                            {
                                                stopwatch
                                                        .Stop();
                                            }
                                            try
                                            {
                                                Interlocked
                                                        .Increment(ref _dequeuedBatches);
                                                exceptionSource = $"{nameof(onBatchDequeuesProcessAction)}";
                                                onBatchDequeuesProcessAction
                                                                    (
                                                                        _dequeued
                                                                        , _dequeuedBatches
                                                                        , i
                                                                    );
                                            }
                                            finally
                                            {
                                                i = 0;
                                                if (stopwatch != null)
                                                {
                                                    stopwatch
                                                            .Restart();
                                                }
                                            }
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
                                                        this
                                                        , x
                                                        , y
                                                        , z
                                                        , DateTime.Now
                                                        , exceptionSource
                                                        , null
                                                    );
                               }
                               return rr;
                           }
                        );
            }
        }
    }
}
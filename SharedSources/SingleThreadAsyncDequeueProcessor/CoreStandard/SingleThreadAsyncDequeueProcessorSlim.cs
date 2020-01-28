namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessorSlim<TQueueElement>
    {
        public int QueueLength
        {
            get
            {
                return
                    _queue.Count;
            }
        }
        private ConcurrentQueue
                        <
                            (
                                long ID
                                , DateTime? EnqueueTime
                                , TQueueElement QueueElement
                                , DateTime? DequeueTime
                                , DateTime? DequeueProcessedTime
                            )
                        >
                            _queue
                                    = new ConcurrentQueue
                                                <
                                                    (
                                                        long ID
                                                        , DateTime? EnqueueTime
                                                        , TQueueElement QueueElement
                                                        , DateTime? DequeueTime
                                                        , DateTime? DequeueProcessedTime
                                                    )
                                                >();
        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessorSlim<TQueueElement> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
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
        public bool Enqueue(TQueueElement element)
        {
            var r = false;

            (
                long ID
                , DateTime? EnqueueTime
                , TQueueElement QueueElement
                , DateTime? DequeueTime
                , DateTime? DequeueProcessedTime
            )
                item =
                    (
                        Interlocked.Increment(ref _enqueued)
                        , DateTime.Now
                        , element
                        , null
                        , null
                    );
            _queue
                .Enqueue
                    (
                        item
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
                            , TQueueElement
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
                            , TQueueElement
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
                                                        out var item
                                                    )
                                        )
                                    {
                                        Interlocked
                                                .Increment(ref _dequeued);
                                        i ++;
                                        item
                                            .DequeueTime = DateTime.Now;
                                        onOnceDequeueProcessAction
                                                        (
                                                            _dequeued
                                                            , _dequeuedBatches
                                                            , i
                                                            , item.QueueElement
                                                        );
                                        item
                                            .DequeueProcessedTime = DateTime.Now;
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
                                                    );
                               }
                               return rr;
                           }
                        );
            }
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    public class SingleThreadAsyncDequeueProcessor<TQueueElement>
    {
        public int QueueLength
        {
            get
            {
                return
                    _queue.Count;
            }
        }
        private ConcurrentQueue<TQueueElement>
                            _queue
                                    = new ConcurrentQueue<TQueueElement>();
        public delegate bool CaughtExceptionEventHandler
                                    (
                                        SingleThreadAsyncDequeueProcessor<TQueueElement> sender
                                        , Exception exception
                                        , Exception newException
                                        , string innerExceptionMessage
                                    );
        public event CaughtExceptionEventHandler
                                            OnCaughtException;
        public bool Enqueue(TQueueElement item)
        {
            var r = false;
            _queue.Enqueue(item);
            return r;
        }
        public void StartRunDequeueThreadProcess
             (
                 Action<long, List<TQueueElement>> onBatchDequeuesProcessAction
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
                                    sleepInMilliseconds
                                    , onBatchDequeuesProcessAction
                                    , waitOneBatchTimeOutInMilliseconds
                                    , waitOneBatchMaxDequeuedTimes
                                );
                        }
                    ).Start();
        }

        private bool _isStartedDequeueProcess = false;
        private void DequeueProcess
            (
                int sleepInMilliseconds = 100
                , Action<long, List<TQueueElement>> onBatchDequeuesProcessAction = null
                , int waitOneBatchTimeOutInMilliseconds = 1000
                , int waitOneBatchMaxDequeuedTimes = 100
            )
        {
            List<TQueueElement> list = null;
            long i = 0;
            Stopwatch stopwatch = null;
            if (onBatchDequeuesProcessAction != null)
            {
                list = new List<TQueueElement>();
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
                                    TQueueElement element = default(TQueueElement);
                                    if (_queue.TryDequeue(out element))
                                    {
                                        if (onBatchDequeuesProcessAction != null)
                                        {
                                            i++;
                                            list.Add(element);
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
                                            try
                                            {
                                                onBatchDequeuesProcessAction
                                                    (
                                                        i
                                                        , list
                                                    );
                                            }
                                            finally
                                            {
                                                if (stopwatch != null)
                                                {
                                                    i = 0;
                                                    list.Clear();
                                                    stopwatch.Restart();
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

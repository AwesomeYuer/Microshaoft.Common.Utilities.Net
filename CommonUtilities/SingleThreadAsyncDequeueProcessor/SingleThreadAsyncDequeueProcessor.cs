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
        private ConcurrentQueue<T> _concurrentQueue
            = new ConcurrentQueue<T>();
        public ConcurrentQueue<T> InternalQueue
        {
            get { return _concurrentQueue; }
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
                                    if (!_concurrentQueue.IsEmpty)
                                    {
                                        T element = null;
                                        if (_concurrentQueue.TryDequeue(out element))
                                        {
                                            if (onOnceDequeueProcessAction != null)
                                            {
                                                TryCatchFinallyProcessHelper
                                                    .TryProcessCatchFinally
                                                        (
                                                            true
                                                            , () =>
                                                                {
                                                                    onOnceDequeueProcessAction(i, element);
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
                                            if (onBatchDequeuesProcessAction != null)
                                            {
                                                i++;
                                                var tuple
                                                        = Tuple
                                                            .Create<long, T>
                                                                  (
                                                                    i
                                                                    , element
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
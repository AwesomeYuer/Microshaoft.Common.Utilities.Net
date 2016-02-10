namespace Microshaoft
{
    using System;
    using System.Threading;
    public static class ReaderWriterLockSlimExtensionsMethodsManager
    {
        public static bool TryEnterWriterLockSlimWrite<T>
                                (
                                    this ReaderWriterLockSlim writerLockSlim
                                    , ref T target
                                    , T newTarget
                                    , int enterTimeOutInSeconds
                                )
                                    where T : class
        {
            return
                ReaderWriterLockSlimHelper
                    .TryEnterWriterLockSlimWrite<T>
                        (
                            writerLockSlim
                            , ref target
                            , newTarget
                            , enterTimeOutInSeconds
                        );
        }


        public static T TryEnterReadLockSlimRead<T>
                        (
                            this ReaderWriterLockSlim readerLockSlim
                            , Func<ReaderWriterLockSlim, T> onReadedProcessFunc
                            , int enterTimeOutInSeconds
                        )
        {
            return
                ReaderWriterLockSlimHelper
                    .TryEnterReadLockSlimRead<T>
                        (
                            readerLockSlim
                            , onReadedProcessFunc
                            , enterTimeOutInSeconds
                        );
        }
        public static bool TryEnterLockSlim
                                (
                                    this ReaderWriterLockSlim lockSlim
                                    , Func<ReaderWriterLockSlim, bool> onEnterProcessFunc
                                    , Action action
                                    , Action<ReaderWriterLockSlim> onExitProcessAction
                                )
        {
            return
                ReaderWriterLockSlimHelper
                    .TryEnterLockSlim
                        (
                            lockSlim
                            , onEnterProcessFunc
                            , action
                            , onExitProcessAction
                        );
        }
    }
    public static class ReaderWriterLockSlimHelper
    {
        public static bool TryEnterWriterLockSlimWrite<T>
                                                (
                                                    ReaderWriterLockSlim writerLockSlim
                                                    , ref T target
                                                    , T newTarget
                                                    , int enterTimeOutInSeconds
                                                )
                                                    where T : class
        {
            bool r = false;
            //var rwls = new ReaderWriterLockSlim();
            int timeOut = Timeout.Infinite;
            if (enterTimeOutInSeconds >= 0)
            {
                timeOut = enterTimeOutInSeconds * 1000;
            }
            try
            {
                r = (writerLockSlim.TryEnterWriteLock(timeOut));
                if (r)
                {
                    Interlocked.Exchange<T>(ref target, newTarget);
                    r = true;
                }
            }
            finally
            {
                if (r)
                {
                    writerLockSlim.ExitWriteLock();
                }
            }
            return r;
        }

        public static T TryEnterReadLockSlimRead<T>
                                (
                                    ReaderWriterLockSlim readerLockSlim
                                    , Func<ReaderWriterLockSlim, T> onReadedProcessFunc
                                    , int enterTimeOutInSeconds
                                )
        {
            T r = default(T);
            var rr = false;
            //var rwls = new ReaderWriterLockSlim();
            int timeOut = Timeout.Infinite;
            if (enterTimeOutInSeconds >= 0)
            {
                timeOut = enterTimeOutInSeconds * 1000;
            }
            try
            {
                rr = (readerLockSlim.TryEnterReadLock(timeOut));
                if (rr)
                {
                    r = onReadedProcessFunc(readerLockSlim);
                    rr = true;
                }
            }
            finally
            {
                if (rr)
                {
                    readerLockSlim.ExitReadLock();
                }
            }
            return r;
        }
        public static bool TryEnterLockSlim
                                (
                                    ReaderWriterLockSlim lockSlim
                                    , Func<ReaderWriterLockSlim, bool> onEnterProcessFunc
                                    , Action action
                                    , Action<ReaderWriterLockSlim> onExitProcessAction
                                )
        {
            bool r = false;
            if (action != null)
            {
                try
                {
                    r = onEnterProcessFunc(lockSlim);
                    if (r)
                    {
                        action();
                        r = true;
                    }
                }
                finally
                {
                    if (r)
                    {
                        onExitProcessAction(lockSlim);
                    }
                }
            }
            return r;
        }
    }
}
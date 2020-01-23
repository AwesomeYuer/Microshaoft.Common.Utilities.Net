namespace Microshaoft
{
    using System;
    using System.Threading;
    //public static class ReaderWriterLockSlimExtensionsMethodsManager
    public static class ReaderWriterLockSlimHelper
    {
        public static bool TryEnterWriterLockSlimWrite<T>
                                                (
                                                    this ReaderWriterLockSlim instance
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
                r = (instance.TryEnterWriteLock(timeOut));
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
                    instance.ExitWriteLock();
                }
            }
            return r;
        }

        public static T TryEnterReadLockSlimRead<T>
                                (
                                    this ReaderWriterLockSlim target
                                    , Func<ReaderWriterLockSlim, T> onReadedProcessFunc
                                    , int enterTimeOutInSeconds
                                )
        {
            T r = default;
            var rr = false;
            //var rwls = new ReaderWriterLockSlim();
            int timeOut = Timeout.Infinite;
            if (enterTimeOutInSeconds >= 0)
            {
                timeOut = enterTimeOutInSeconds * 1000;
            }
            try
            {
                rr = (target.TryEnterReadLock(timeOut));
                if (rr)
                {
                    r = onReadedProcessFunc(target);
                    rr = true;
                }
            }
            finally
            {
                if (rr)
                {
                    target.ExitReadLock();
                }
            }
            return r;
        }
        public static bool TryEnterLockSlim
                                (
                                    this ReaderWriterLockSlim target
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
                    r = onEnterProcessFunc(target);
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
                        onExitProcessAction(target);
                    }
                }
            }
            return r;
        }
    }
}
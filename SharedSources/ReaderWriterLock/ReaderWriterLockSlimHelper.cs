namespace Microshaoft
{
    using System;
    using System.Threading;
    //public static class ReaderWriterLockSlimExtensionsMethodsManager
    public static class ReaderWriterLockSlimHelper
    {
        public static bool TryEnterWriterLockSlimWrite<T>
                                                (
                                                    this ReaderWriterLockSlim @this
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
                r = (@this.TryEnterWriteLock(timeOut));
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
                    @this.ExitWriteLock();
                }
            }
            return r;
        }

        public static T TryEnterReadLockSlimRead<T>
                                (
                                    this ReaderWriterLockSlim @this
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
                rr = (@this.TryEnterReadLock(timeOut));
                if (rr)
                {
                    r = onReadedProcessFunc(@this);
                    rr = true;
                }
            }
            finally
            {
                if (rr)
                {
                    @this.ExitReadLock();
                }
            }
            return r;
        }
        public static bool TryEnterLockSlim
                                (
                                    this ReaderWriterLockSlim @this
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
                    r = onEnterProcessFunc(@this);
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
                        onExitProcessAction(@this);
                    }
                }
            }
            return r;
        }
    }
}
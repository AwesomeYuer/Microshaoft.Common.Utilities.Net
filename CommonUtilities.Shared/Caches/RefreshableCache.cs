#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.Runtime.Caching;
    public class RefreshableCache<TData>
    {
        private TData _cachedData = default(TData);
        public void Refresh()
        {
            MemoryCache
                .Default
                .Remove(CacheKey);
        }
        public TData CachedData
        {
            get
            {
                TData r = _cachedData;
                return r;
            }
        }
        public string CacheKey
        {
            set;
            get;
        }
        public DateTime LastRefreshedTime
        {
            set;
            get;
        }
        public RefreshableCache()
        {

        }
        public RefreshableCache
                    (
                        string cacheKey
                        , uint refreshIntervalInSeconds
                        , Func<TData> onRefreshingCacheDataProcessFunc
                        , bool needTryProcess = true
                        , bool reThrowException = false
                        , Func<Exception, Exception, string, bool> onCaughtExceptionProcessFunc = null
                        , Action<bool, Exception, Exception, string> onFinallyProcessAction = null
                    )
        {
            CacheKey = cacheKey.Trim().ToLower();
            _cachedData = onRefreshingCacheDataProcessFunc();
            LastRefreshedTime = DateTime.Now;
            var cacheItemEntryRemovedNotifier
                    = new CacheItemEntryRemovedNotifier
                            (
                                    CacheKey
                                    , refreshIntervalInSeconds
                                    , (x) =>
                                    {
                                        TData refreshedData = default(TData);
                                        if
                                            (
                                                TryGetRefreshedCacheData
                                                    (
                                                        onRefreshingCacheDataProcessFunc
                                                        , out refreshedData
                                                        , needTryProcess
                                                        , reThrowException
                                                        , onCaughtExceptionProcessFunc
                                                        , onFinallyProcessAction
                                                    )
                                            )
                                        {
                                            _cachedData = refreshedData;
                                            LastRefreshedTime = DateTime.Now;
                                        }
                                        return true;
                                    }
                            );
        }
        private static bool TryGetRefreshedCacheData
                            (
                                Func<TData> onRefreshingCacheDataProcessFunc
                                , out TData refreshedData
                                , bool needTryProcess = true
                                , bool reThrowException = false
                                , Func<Exception, Exception, string, bool>
                                                    onCaughtExceptionProcessFunc = null
                                , Action<bool, Exception, Exception, string>
                                                    onFinallyProcessAction = null
                            )
        {
            var r = false;
            TData refreshingData = default(TData);
            refreshedData = default(TData);
            if (needTryProcess)
            {
                TryCatchFinallyProcessHelper
                        .TryProcessCatchFinally
                            (
                                needTryProcess
                                , () =>
                                {
                                    refreshingData
                                        = onRefreshingCacheDataProcessFunc();
                                    r = true;
                                }
                                , reThrowException
                                , (xx, yy, zz) =>
                                {
                                    return
                                        onCaughtExceptionProcessFunc(xx, yy, zz);
                                }
                            );
                if (r)
                {
                    refreshedData = refreshingData;
                }
            }
            else
            {
                refreshedData = onRefreshingCacheDataProcessFunc();
                r = true;
            }
            return r;
        }
    }
}
#endif
#if NETFRAMEWORK4_X
//#define cs4 //C# 4.0+
//#define cs2 //C# 2.0+
// /r:C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.Caching.dll
namespace Test
{
    using System;
    using System.Threading;
    
    using System.Runtime.Caching;
    using Microshaoft;
    public class Class1
    {
        static void Main(string[] args)
        {
 
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
            Console.ReadLine();
        }
        static void x_CacheItemEntryRemoved(CacheItemEntryRemovedNotifier sender, CacheEntryRemovedReason reason)
        {

            sender.ExpireSeconds = 10;
        }
    }
}
namespace Microshaoft
{
    using System;
    using System.Runtime.Caching;
    public class CacheItemEntryRemovedNotifier
    {

        private MemoryCache _cache = null;
        private string _key;
        public string Key
        {
            get
            {
                return _key;
            }
        }
        private uint _expireSeconds = 0;
        public uint ExpireSeconds
        {
            get
            {
                return _expireSeconds;
            }
            set
            {
                _expireSeconds = value;
            }
        }
        public CacheItemEntryRemovedNotifier
                            (
                                string key
                                , uint expireSeconds
                                , Func<CacheEntryRemovedArguments, bool> onCacheEntryRemovedCallbackProcessFunc
                            )
        {
            _key = key;
            _cache = MemoryCache.Default;
            Add
                (
                    key
                    , expireSeconds
                    , onCacheEntryRemovedCallbackProcessFunc
                );
        }
        private void Add
                    (
                        string key
                         , uint expireSeconds
                         , Func<CacheEntryRemovedArguments, bool>
                                    onCacheEntryRemovedCallbackProcessFunc
                    )
        {
            CacheItem item = null;
            CacheItemPolicy cip = null;
            CacheEntryRemovedCallback removedCallback = null;
            _expireSeconds = expireSeconds;
            if (!_cache.Contains(key))
            {
                //实例化一个CacheItem缓存项
                item = new CacheItem(key, new object());
                //实例化CacheItemPolicy 并关联缓存项的一组逐出和过期详细信息
                cip = new CacheItemPolicy();
                removedCallback =
                                    (
                                        (x) =>
                                        {
                                            var r = onCacheEntryRemovedCallbackProcessFunc(x);
                                            if (r)
                                            {
                                                if (_expireSeconds > 0)
                                                {
                                                    Add
                                                        (
                                                            _key
                                                            , _expireSeconds
                                                            , onCacheEntryRemovedCallbackProcessFunc
                                                        );
                                                }
                                            }
                                        }
                                    );
                cip.RemovedCallback = removedCallback;
                DateTime expire = DateTime.Now.AddSeconds(_expireSeconds);
                cip.AbsoluteExpiration = new DateTimeOffset(expire);
                //将缓存实例添加到系统缓存
                _cache.Add(item, cip);
            }
        }
        public void Remove()
        {
            _cache.Remove(_key);
        }
        public void Stop()
        {
            _cache.Remove(_key);
            _expireSeconds = 0;
        }
    }
}
#endif
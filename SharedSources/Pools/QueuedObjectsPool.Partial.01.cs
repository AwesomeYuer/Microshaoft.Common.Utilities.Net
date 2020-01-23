namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    public partial class QueuedObjectsPool<T> where T : new()
    {
        private readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        public ConcurrentQueue<T> Pool
        {
            get
            {
                return _pool;
            }
        }

        private long _pooledObjectsGotCount = 0;
        public long PooledObjectsGotCount
        {
            get
            {
                return _pooledObjectsGotCount;
            }
        }

        private long _nonPooledObjectsGotCount = 0;
        public long NonPooledObjectsGotCount
        {
            get
            {
                return _nonPooledObjectsGotCount;
            }
        }
        private long _pooledObjectsReturnCount = 0;
        public long PooledObjectsReturnCount
        {
            get
            {
                return _pooledObjectsReturnCount;
            }
        }
        private long _nonPooledObjectsReleaseCount = 0;
        public long NonPooledObjectsReleaseCount
        {
            get
            {
                return _nonPooledObjectsReleaseCount;
            }
        }
        public long MaxCapacity
        {
            get
            {
                return _maxCapacity;
            }
        }
        private readonly long _maxCapacity = 0;
        public QueuedObjectsPool
                    (
                        long maxCapacity
                        , bool needInitializePooledObjects = false
                    )
        {
            _pool = new ConcurrentQueue<T>();
            _maxCapacity = maxCapacity;
            if (needInitializePooledObjects)
            {
                for (var i = 0; i < _maxCapacity; i++)
                {
                    TryPutNew();
                }
            }
        }
        //public Func<bool> onEnablePerformanceCountersProcessFunc
        //{
        //    get;
        //    set;
        //}
        public bool TryPutNew()
        {
            T e = new T();
            bool r = TryPut(e);
            return r;
        }
        public bool TryPut(T item)
        {
            var r = false;
            if (item != null)
            {
                if (_pool.Count < _maxCapacity)
                {
                    _pool.Enqueue(item);
                    Interlocked.Increment(ref _pooledObjectsReturnCount);
                    r = true;
                }
                else
                {
                    Interlocked.Increment(ref _nonPooledObjectsReleaseCount);
                }
            }
            return r;
        }
        public bool TryGet(out T item)
        {
            bool r = _pool.TryDequeue(out item);
            if (r)
            {
                Interlocked.Increment(ref _pooledObjectsGotCount);
            }
            else
            {
                item = new T();
                Interlocked.Increment(ref _nonPooledObjectsGotCount);
            }
            return r;
        }
    }
}

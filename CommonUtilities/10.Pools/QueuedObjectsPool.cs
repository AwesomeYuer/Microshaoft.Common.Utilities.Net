namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Diagnostics;
    public class QueuedObjectsPool<T> where T: new()
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




        private long _maxCapacity = 0;

        private CommonPerformanceCountersContainer _performanceCountersContainer = null;
        public QueuedObjectsPool(long maxCapacity)
        {
            _pool = new ConcurrentQueue<T>();
            _maxCapacity = maxCapacity;

        }
        private bool _isAttachPerformanceCounters = false;
        public void AttachPerformanceCountersCategoryInstance
                                (
                                    string performanceCountersCategoryName
                                    , string performanceCountersCategoryInstanceNamePrefix
                                    , MultiPerformanceCountersTypeFlags enablePerformanceCounters = MultiPerformanceCountersTypeFlags.ProcessNonTimeBasedCounters
                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Process
                                )
        {
            if (!_isAttachPerformanceCounters)
            {
                _isAttachPerformanceCounters = true;
            }
            else
            {
                CommonPerformanceCountersContainer container = null;
                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                        .AttachPerformanceCountersCategoryInstance
                            (
                                performanceCountersCategoryName
                                , string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , "Non-Pooled Objects"
                                                , performanceCountersCategoryInstanceNamePrefix
                                            ) 
                                , out container
                                , PerformanceCounterInstanceLifetime.Process
                                , initializePerformanceCounterInstanceRawValue: 1009
                            );
                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                        .AttachPerformanceCountersCategoryInstance
                            (
                                performanceCountersCategoryName
                                , string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , "Pooled Objects"
                                                , performanceCountersCategoryInstanceNamePrefix
                                            )
                                , out container
                                , PerformanceCounterInstanceLifetime.Process
                                , initializePerformanceCounterInstanceRawValue: 1009
                            );

            }
        }

        public Func<bool> onEnablePerformanceCountersProcessFunc
        {
            get;
            set;
        }
        public void PutNew()
        {
            var e = default(T);
            e = new T();
            Put(e);
        }
        public bool Put(T target)
        {
            var r = false;
            if (target != null)
            {
                if (_pool.Count < _maxCapacity)
                {
                    _pool.Enqueue(target);
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
        public T Get()
        { 
            T r;
            if (!_pool.TryDequeue(out r))
            {
                r = new T();
                Interlocked.Increment(ref _nonPooledObjectsGotCount);
            }
            else
            {
                Interlocked.Increment(ref _pooledObjectsGotCount);
            }
            return r;
        }
    }
}

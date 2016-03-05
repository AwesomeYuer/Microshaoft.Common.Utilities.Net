//namespace Microshaoft
//{
//    using System.Collections.Generic;
//    using System.Diagnostics;
//    public class QueuedObjectsPoolPerformanceCountersContainer :
//                                                        AbstractPerformanceCountersContainer
//                                                        , IPerformanceCountersContainer
//                                                        //, ICommonPerformanceCountersContainer
//                                                        , IPerformanceCountersValuesClearable
//    {
//        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
//        {
//            return
//                GetPerformanceCountersByLevel<QueuedObjectsPoolPerformanceCountersContainer>(this, level);
//        }
//        public override IEnumerable<PerformanceCounter> PerformanceCounters
//        {
//            get
//            {
//                //yield return _pooledObjectsProcessPerformanceCounter;
//                //yield return _pooledObjectsProcessingPerformanceCounter;
//                //yield return _pooledObjectsProcessedPerformanceCounter;
//                //yield return _objectsProcessedRateOfCountsPerSecondPerformanceCounter;
//                //yield return _pooledObjectsProcessedAverageTimerPerformanceCounter;
//                yield return _caughtExceptionsPerformanceCounter;
//                //return
//                //    GetPropertiesPerformanceCounters<CommonPerformanceCountersContainer>
//                //        (this);
//            }
//        }
//        // indexer declaration
//        public override PerformanceCounter this[string name]
//        {
//            get
//            {
//                return
//                    GetPerformanceCounterByName<QueuedObjectsPoolPerformanceCountersContainer>
//                        (
//                            this
//                            , name
//                        );
//            }
//        }
//        public override void AttachPerformanceCountersToProperties
//                                (
//                                    string categoryName
//                                    , string instanceName
//                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
//                                    , long? initializePerformanceCounterInstanceRawValue = null
//                                )
//        {
//            AttachPerformanceCountersToProperties<QueuedObjectsPoolPerformanceCountersContainer>
//                    (
//                        categoryName
//                        , instanceName
//                        , this
//                        , performanceCounterInstanceLifetime
//                        , initializePerformanceCounterInstanceRawValue
//                    );
//        }


//        private PerformanceCounter _caughtExceptionsPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "99.捕获异常次数(次)"
//                    , Level = 10
//                )
//        ]
//        public PerformanceCounter CaughtExceptionsPerformanceCounter
//        {
//            private set
//            {
//                _caughtExceptionsPerformanceCounter = value;
//            }
//            get
//            {
//                return _caughtExceptionsPerformanceCounter;
//            }
//        }

//        #region Pooled PerformanceCounters
//        private PerformanceCounter _pooledObjectsProcessPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "01.获取池化对象累计次数(次)"
//                )
//        ]
//        public PerformanceCounter PooledObjectsPrcocessPerformanceCounter
//        {
//            private set
//            {
//                _pooledObjectsProcessPerformanceCounter = value;
//            }
//            get
//            {
//                return _pooledObjectsProcessPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _objectsProcessingPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "02.对象正在使用个数(个)"
//                    , Level = 10

//                )
//        ]
//        public PerformanceCounter ObjectsProcessingPerformanceCounter
//        {
//            private set
//            {
//                _objectsProcessingPerformanceCounter = value;
//            }
//            get
//            {
//                return _objectsProcessingPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _pooledObjectsProcessedPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "03.退回池化对象累计次数(次)"
//                )
//        ]
//        public PerformanceCounter PooledObjectsProcessedPerformanceCounter
//        {
//            private set
//            {
//                _pooledObjectsProcessedPerformanceCounter = value;
//            }
//            get
//            {
//                return _pooledObjectsProcessedPerformanceCounter;
//            }
//        }
        
//        #endregion


//        #region Non-Pooled PerformanceCounters

//        private PerformanceCounter _nonPooledObjectsProcessPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "01.获取非池化对象累计个数(个)"
//                )
//        ]
//        public PerformanceCounter NonPooledObjectsPrcocessPerformanceCounter
//        {
//            private set
//            {
//                _nonPooledObjectsProcessPerformanceCounter = value;
//            }
//            get
//            {
//                return _nonPooledObjectsProcessPerformanceCounter;
//            }
//        }
        
//        private PerformanceCounter _nonPooledObjectsProcessedPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "03.释放非池化对象累计个数(个)"
//                )
//        ]
//        public PerformanceCounter NonPooledObjectsProcessedPerformanceCounter
//        {
//            private set
//            {
//                _nonPooledObjectsProcessedPerformanceCounter = value;
//            }
//            get
//            {
//                return _nonPooledObjectsProcessedPerformanceCounter;
//            }
//        }
        
//        #endregion


//    }
//}
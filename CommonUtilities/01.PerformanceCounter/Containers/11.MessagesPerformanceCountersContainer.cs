namespace Microsoft.Boc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    /// <summary>
    /// 关于Session数的性能计数器
    /// </summary>
    public class MessagesPerformanceCountersContainer :
                                                        AbstractPerformanceCountersContainer
                                                        , IPerformanceCountersContainer
                                                        , ICommonPerformanceCountersContainer
                                                        //, IPerformanceCountersValuesClearable
    {

        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            return
                GetPerformanceCountersByLevel<MessagesPerformanceCountersContainer>
                    (
                        this
                        , level
                    );
            
        }
        public override IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                //yield return _processPerformanceCounter;
                //yield return _processingPerformanceCounter;
                //yield return _processedPerformanceCounter;
                //yield return _processedRateOfCountsPerSecondPerformanceCounter;
                //yield return _processedAverageTimerPerformanceCounter;
                //yield return _caughtExceptionsPerformanceCounter;
                return
                    GetPropertiesPerformanceCounters<MessagesPerformanceCountersContainer>
                        (this);
            }
        }
        // indexer declaration
        public override PerformanceCounter this[string name]
        {
            get
            {
                return
                    GetPerformanceCounterByName<MessagesPerformanceCountersContainer>
                        (
                            this
                            , name
                        );
            }
        }

        public override void AttachPerformanceCountersToProperties
                                (
                                    string categoryName
                                    , string instanceName
                                )
        {
            AttachPerformanceCountersToProperties<MessagesPerformanceCountersContainer>
                    (
                        categoryName
                        , instanceName
                        , this
                    );
        }

        #region PerformanceCounters
        private PerformanceCounter _caughtExceptionsPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "99.捕获异常次数(次)"
                    , Level = 10
                )
        ]
        public PerformanceCounter CaughtExceptionsPerformanceCounter
        {
            private set
            {
                _caughtExceptionsPerformanceCounter = value;
            }
            get
            {
                return _caughtExceptionsPerformanceCounter;
            }
        }
        private PerformanceCounter _processPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "01.开始处理数(笔)"
                )
        ]
        public PerformanceCounter PrcocessPerformanceCounter
        {
            private set
            {
                _processPerformanceCounter = value;
            }
            get
            {
                return _processPerformanceCounter;
            }
        }
        private PerformanceCounter _processingPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "02.正在处理数(笔)"
                    , Level = 10
                )
        ]
        public PerformanceCounter ProcessingPerformanceCounter
        {
            private set
            {
                _processingPerformanceCounter = value;
            }
            get
            {
                return _processingPerformanceCounter;
            }
        }
        private PerformanceCounter _processedPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "03.完成处理数(笔)"
                )
        ]
        public PerformanceCounter ProcessedPerformanceCounter
        {
            private set
            {
                _processedPerformanceCounter = value;
            }
            get
            {
                return _processedPerformanceCounter;
            }
        }
        private PerformanceCounter _processedRateOfCountsPerSecondPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
                    , CounterName = "04.每秒完成处理笔数(笔/秒)"
                )
        ]
        public PerformanceCounter ProcessedRateOfCountsPerSecondPerformanceCounter
        {
            private set
            {
                _processedRateOfCountsPerSecondPerformanceCounter = value;
            }
            get
            {
                return _processedRateOfCountsPerSecondPerformanceCounter;
            }
        }
        private PerformanceCounter _processedAverageTimerPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "05.平均每笔处理耗时秒数(秒/笔)"
                )
        ]
        public PerformanceCounter ProcessedAverageTimerPerformanceCounter
        {
            private set
            {
                _processedAverageTimerPerformanceCounter = value;
            }
            get
            {
                return _processedAverageTimerPerformanceCounter;
            }
        }
        private PerformanceCounter _processedAverageBasePerformanceCounter;
        [PerformanceCounterDefinitionAttribute(CounterType = PerformanceCounterType.AverageBase)]
        public PerformanceCounter ProcessedAverageBasePerformanceCounter
        {
            private set
            {
                _processedAverageBasePerformanceCounter = value;
            }
            get
            {
                return _processedAverageBasePerformanceCounter;
            }
        }
        #endregion
    }
}
//namespace Microsoft.Boc
//{
//    using System;
//    using System.Diagnostics;
//    /// <summary>
//    /// 关于Session数的性能计数器
//    /// </summary>
//    public class ClientResponsedMessagesPerformanceCountersContainer : IPerformanceCountersContainer
//    {
//        #region PerformanceCounters
//        private PerformanceCounter _caughtExceptionsPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "99.捕获异常次数(次)"
//                )
//        ]
//        public PerformanceCounter CaughtExceptionsPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper
//                    .TryEnterWriterLockSlimWrite<PerformanceCounter>
//                        (
//                            ref _caughtExceptionsPerformanceCounter
//                            , value
//                            , 2
//                        );
//            }
//            get
//            {
//                return _caughtExceptionsPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _processPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "01.发送请求数(笔)"
//                )
//        ]
//        public PerformanceCounter PrcocessPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _processPerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _processPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _processingPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "02.正在发送请求数(笔)"
//                )
//        ]
//        public PerformanceCounter ProcessingPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _processingPerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _processingPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _processedPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.NumberOfItems64
//                    , CounterName = "03.接收应答数(笔)"
//                )
//        ]
//        public PerformanceCounter ProcessedPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _processedPerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _processedPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _processedRateOfCountsPerSecondPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
//                    , CounterName = "04.每秒完成处理笔数(笔/秒)"
//                )
//        ]
//        public PerformanceCounter ProcessedRateOfCountsPerSecondPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _processedRateOfCountsPerSecondPerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _processedRateOfCountsPerSecondPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _ProcessedAverageTimerPerformanceCounter;
//        [
//            PerformanceCounterDefinitionAttribute
//                (
//                    CounterType = PerformanceCounterType.AverageTimer32
//                    , CounterName = "05.平均每笔处理耗时秒数(秒/笔)"
//                )
//        ]
//        public PerformanceCounter ProcessedAverageTimerPerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _ProcessedAverageTimerPerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _ProcessedAverageTimerPerformanceCounter;
//            }
//        }
//        private PerformanceCounter _processedAverageBasePerformanceCounter;
//        [PerformanceCounterDefinitionAttribute(CounterType = PerformanceCounterType.AverageBase)]
//        public PerformanceCounter ProcessedAverageBasePerformanceCounter
//        {
//            private set
//            {
//                ReaderWriterLockSlimHelper.TryEnterWriterLockSlimWrite<PerformanceCounter>(ref _processedAverageBasePerformanceCounter, value, 2);
//            }
//            get
//            {
//                return _processedAverageBasePerformanceCounter;
//            }
//        }
//        #endregion
//        // indexer declaration
//        public PerformanceCounter this[string name]
//        {
//            get
//            {
//                throw new NotImplementedException();
//                //return null;
//            }
//        }
//        //private bool _isAttachedPerformanceCounters = false;
//        public void AttachPerformanceCountersToProperties
//                            (
//                                string instanceName
//                                , string categoryName
//                            )
//        {
//            var type = this.GetType();
//            PerformanceCountersHelper.AttachPerformanceCountersToProperties<ClientResponsedMessagesPerformanceCountersContainer>(instanceName, categoryName, this);
//        }
//    }
//}

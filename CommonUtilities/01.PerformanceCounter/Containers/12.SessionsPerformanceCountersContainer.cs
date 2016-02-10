namespace Microsoft.Boc
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    /// <summary>
    /// 关于Session数的性能计数器
    /// </summary>
    public class SessionsPerformanceCountersContainer :
                                                        AbstractPerformanceCountersContainer
                                                        , IPerformanceCountersContainer
                                                        , ICommonPerformanceCountersContainer
                                                        //, IPerformanceCountersValuesClearable
    {
        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            return
                GetPerformanceCountersByLevel<SessionsPerformanceCountersContainer>
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
                    GetPropertiesPerformanceCounters<SessionsPerformanceCountersContainer>
                        (this);
            }
        }
        // indexer declaration
        public override PerformanceCounter this[string name]
        {
            get
            {
                return
                    GetPerformanceCounterByName<SessionsPerformanceCountersContainer>
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
            AttachPerformanceCountersToProperties<SessionsPerformanceCountersContainer>
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
                    , CounterName = "01.新建次数(次)"
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
                    , CounterName = "02.当前活动数(个)"
                    , Level = 100
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
                    , CounterName = "03.删除次数(次)"
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
                    , CounterName = "04.每秒完成处理笔数(笔/秒)[不适用]"
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
                    , CounterName = "05.平均每笔处理耗时秒数(秒/笔)[不适用]"
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

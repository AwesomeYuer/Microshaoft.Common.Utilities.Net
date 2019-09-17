#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Diagnostics;
    public class CommonPerformanceCountersContainer :
                                                        AbstractPerformanceCountersContainer
                                                        , IPerformanceCountersContainer
                                                        , ICommonPerformanceCountersContainer
                                                        , IPerformanceCountersValuesClearable
    {
        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            return
                GetPerformanceCountersByLevel<CommonPerformanceCountersContainer>(this, level);
        }
        public override IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                yield return _processPerformanceCounter;
                yield return _processingPerformanceCounter;
                yield return _processedPerformanceCounter;
                yield return _processedRateOfCountsPerSecondPerformanceCounter;
                yield return _processedAverageTimerPerformanceCounter;
                yield return _caughtExceptionsPerformanceCounter;
                //return
                //    GetPropertiesPerformanceCounters<CommonPerformanceCountersContainer>
                //        (this);
            }
        }
        // indexer declaration
        public override PerformanceCounter this[string name]
        {
            get
            {
                return
                    GetPerformanceCounterByName<CommonPerformanceCountersContainer>
                        (
                            this
                            , name
                        );
            }
        }
        public override PerformanceCounter[] IncrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] DecrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] IncrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] DecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] TimeBasedOnBeginOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] IncrementOnBeginDecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCountersPair[] TimeBasedOnBeginOnEndPerformanceCountersPairs
        {
            get;
            set;
        }
        public override void AttachPerformanceCountersToMembers
                                (
                                    string categoryName
                                    , string instanceName
                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                    , long? initializePerformanceCounterInstanceRawValue = null
                                )
        {
            AttachPerformanceCountersToMembers
                    //<CommonPerformanceCountersContainer>
                        (
                            categoryName
                            , instanceName
                            , this
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue
                        );
            InitializeProcessingTypedPerformanceCounters
                    //<CommonPerformanceCountersContainer>
                        (
                            this
                            , PerformanceCounterProcessingFlagsType
                                    .IncrementOnBegin
                        );
         
            InitializeProcessingTypedPerformanceCounters
                    //<CommonPerformanceCountersContainer>
                        (
                            this
                            , PerformanceCounterProcessingFlagsType
                                    .IncrementOnEnd
                        );
            InitializeProcessingTypedPerformanceCounters
                    <CommonPerformanceCountersContainer>
                        (
                            this
                            , PerformanceCounterProcessingFlagsType
                                    .DecrementOnEnd
                        );
            InitializeProcessingTypedPerformanceCounters
                    <CommonPerformanceCountersContainer>
                        (
                            this
                            , PerformanceCounterProcessingFlagsType
                                    .TimeBasedOnBeginOnEnd
                        );
            InitializeProcessingTypedPerformanceCountersPairs
                    <CommonPerformanceCountersContainer>
                            (
                                this
                                , PerformanceCounterProcessingFlagsType
                                        .TimeBasedOnBeginOnEnd
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
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.Increment
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
                    , CounterName = "01.接收处理笔数(笔)"
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.IncrementOnBegin
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
                    , CounterName = "02.正在处理笔数(笔)"
                    , Level = 10
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.IncrementOnBeginDecrementOnEnd
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
                    , CounterName = "03.完成处理笔数(笔)"
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.IncrementOnEnd
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
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.IncrementOnEnd
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
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.TimeBasedOnBeginOnEnd
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
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageBase
                )
        ]
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



        private PerformanceCountersPair _processedPerformanceCountersPair;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "06.平均每笔处理耗时秒数(秒/笔)"
                    , BaseCounterType = PerformanceCounterType.AverageBase
                    
                    , CounterProcessingType = PerformanceCounterProcessingFlagsType.TimeBasedOnBeginOnEnd
                )
        ]
        public PerformanceCountersPair ProcessedPerformanceCountersPair
        {
            private set
            {
                _processedPerformanceCountersPair = value;
            }
            get
            {
                return _processedPerformanceCountersPair;
            }
        }


#endregion

    }
}
#endif

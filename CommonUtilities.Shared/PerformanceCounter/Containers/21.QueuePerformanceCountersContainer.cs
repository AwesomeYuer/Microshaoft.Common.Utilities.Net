#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Diagnostics;
    public class QueuePerformanceCountersContainer :
                                                        AbstractPerformanceCountersContainer
                                                        , IPerformanceCountersContainer
                                                        , ICommonPerformanceCountersContainer
                                                        , IPerformanceCountersValuesClearable
    {
        public void RegisterCountersUsage()
        {
            IncrementCountersBeforeCountPerformanceForEnqueue
                                = new PerformanceCounter[]
                                        {
                                            EnqueuePerformanceCounter
                                            , EnqueueRateOfCountsPerSecondPerformanceCounter
                                            , QueueLengthPerformanceCounter
                                        };

            IncrementCountersBeforeCountPerformanceInThread
                                = new PerformanceCounter[]
                                        {
                                            DequeueThreadStartPerformanceCounter
                                            , DequeueThreadsCountPerformanceCounter
                                        };
            DecrementCountersAfterCountPerformanceInThread
                            = new PerformanceCounter[]
                                    {
                                         DequeueThreadsCountPerformanceCounter
                                    };
            IncrementCountersAfterCountPerformanceInThread
                            = new PerformanceCounter[]
                                    {
                                         DequeueThreadEndPerformanceCounter
                                    };
            IncrementCountersBeforeCountPerformanceForDequeue
                            = new PerformanceCounter[]
                                    {
                                        DequeuePerformanceCounter
                                    };
            DecrementCountersBeforeCountPerformanceForDequeue
                            = new PerformanceCounter[]
                                    {
                                        QueueLengthPerformanceCounter
                                    };
           
            IncrementCountersAfterCountPerformanceForDequeue
                            = new PerformanceCounter[]
                                            {
                                                    DequeueProcessedPerformanceCounter
                                                    , DequeueProcessedRateOfCountsPerSecondPerformanceCounter
                                            };
        }

        public PerformanceCounter[] IncrementCountersBeforeCountPerformanceForEnqueue
        {
            get;
            private set;
        }

        public PerformanceCounter[] IncrementCountersBeforeCountPerformanceInThread
        {
            get;
            private set;
        }
        public PerformanceCounter[] IncrementCountersBeforeCountPerformanceForDequeue
        {
            get;
            private set;
        }
        public PerformanceCounter[] DecrementCountersBeforeCountPerformanceForDequeue
        {
            get;
            private set;
        }

        public PerformanceCounter[] DecrementCountersAfterCountPerformanceInThread
        {
            get;
            private set;
        }
        public PerformanceCounter[] IncrementCountersAfterCountPerformanceInThread
        {
            get;
            private set;
        }
        public PerformanceCounter[] IncrementCountersAfterCountPerformanceForDequeue
        {
            get;
            private set;
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
        private PerformanceCounter _enqueuePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "01.入队列累计总数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter EnqueuePerformanceCounter
        {
            private set
            {
                _enqueuePerformanceCounter = value;
            }
            get
            {
                return _enqueuePerformanceCounter;
            }
        }
        private PerformanceCounter _enqueueRateOfCountsPerSecondPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
                    , CounterName = "02.每秒入队列笔数(笔/秒)"
                )
        ]
        public PerformanceCounter EnqueueRateOfCountsPerSecondPerformanceCounter
        {
            private set
            {
                _enqueueRateOfCountsPerSecondPerformanceCounter = value;
            }
            get
            {
                return _enqueueRateOfCountsPerSecondPerformanceCounter;
            }
        }
        private PerformanceCounter _queueLengthPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "03.队列当前长度(笔)"
                    , Level = 10
                )
        ]
        public PerformanceCounter QueueLengthPerformanceCounter
        {
            private set
            {
                _queueLengthPerformanceCounter = value;
            }
            get
            {
                return _queueLengthPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeuePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "04.出队列累计总数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter DequeuePerformanceCounter
        {
            private set
            {
                _dequeuePerformanceCounter = value;
            }
            get
            {
                return _dequeuePerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedRateOfCountsPerSecondPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.RateOfCountsPerSecond64
                    , CounterName = "05.每秒出队列并完成处理笔数(笔/秒)"
                )
        ]
        public PerformanceCounter DequeueProcessedRateOfCountsPerSecondPerformanceCounter
        {
            private set
            {
                _dequeueProcessedRateOfCountsPerSecondPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedRateOfCountsPerSecondPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "06.已出队列并完成处理累计总笔数(笔)"
                    , Level = 1000
                )
        ]
        public PerformanceCounter DequeueProcessedPerformanceCounter
        {
            private set
            {
                _dequeueProcessedPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedAverageTimerPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "07.每笔已出队列并完成处理平均耗时秒数(秒/笔)"
                )
        ]
        public PerformanceCounter DequeueProcessedAverageTimerPerformanceCounter
        {
            private set
            {
                _dequeueProcessedAverageTimerPerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedAverageTimerPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueProcessedAverageBasePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageBase
                )
        ]
        public PerformanceCounter DequeueProcessedAverageBasePerformanceCounter
        {
            private set
            {
                _dequeueProcessedAverageBasePerformanceCounter = value;
            }
            get
            {
                return _dequeueProcessedAverageBasePerformanceCounter;
            }
        }
        private PerformanceCounter _queuedWaitAverageTimerPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageTimer32
                    , CounterName = "08.每笔入出队列并完成处理平均耗时秒数(秒/笔)"
                )
        ]
        public PerformanceCounter QueuedWaitAverageTimerPerformanceCounter
        {
            private set
            {
                _queuedWaitAverageTimerPerformanceCounter = value;
            }
            get
            {
                return _queuedWaitAverageTimerPerformanceCounter;
            }
        }
        private PerformanceCounter _queuedWaitAverageBasePerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.AverageBase
                )
        ]
        public PerformanceCounter QueuedWaitAverageBasePerformanceCounter
        {
            private set
            {
                _queuedWaitAverageBasePerformanceCounter = value;
            }
            get
            {
                return _queuedWaitAverageBasePerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadStartPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "09.新建出队列处理线程启动次数(次)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadStartPerformanceCounter
        {
            private set
            {
                _dequeueThreadStartPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadStartPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadsCountPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "10.当前出队列并发处理线程数(个)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadsCountPerformanceCounter
        {
            private set
            {
                _dequeueThreadsCountPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadsCountPerformanceCounter;
            }
        }
        private PerformanceCounter _dequeueThreadEndPerformanceCounter;
        [
            PerformanceCounterDefinitionAttribute
                (
                    CounterType = PerformanceCounterType.NumberOfItems64
                    , CounterName = "11.出队列处理线程退出次数(次)"
                    , Level = 100
                )
        ]
        public PerformanceCounter DequeueThreadEndPerformanceCounter
        {
            private set
            {
                _dequeueThreadEndPerformanceCounter = value;
            }
            get
            {
                return _dequeueThreadEndPerformanceCounter;
            }
        }
#endregion
        // indexer declaration
        public override PerformanceCounter this[string name]
        {
            get
            {
                return
                    GetPerformanceCounterByName//<QueuePerformanceCountersContainer>
                        (
                            this
                            , name
                        );
            }
        }
        public override IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get
            {
                //throw new NotImplementedException();
                return
                    (
                        GetMembersPerformanceCounters//<QueuePerformanceCountersContainer>
                            (
                                this
                            )
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
        public override PerformanceCounter[] IncrementOnBeginDecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public override PerformanceCounter[] TimeBasedOnBeginOnEndPerformanceCounters
        {
            get;
            set;
        }

        public override PerformanceCountersPair[] TimeBasedOnBeginOnEndPerformanceCountersPairs
        {
            get;
            set;
        }

        public PerformanceCounter PrcocessPerformanceCounter
        {
            get;
            set;
        }

        public PerformanceCounter ProcessingPerformanceCounter
        {
            get;
            set;
        }

        public PerformanceCounter ProcessedPerformanceCounter
        {
            get;
            set;
        }

        public PerformanceCounter ProcessedRateOfCountsPerSecondPerformanceCounter
        {
            get;
            set;
        }

        public PerformanceCounter ProcessedAverageTimerPerformanceCounter
        {
            get;
            set;
        }

        public PerformanceCounter ProcessedAverageBasePerformanceCounter
        {
            get;
            set;
        }

        public override IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level)
        {
            //throw new NotImplementedException();
            return
                GetPerformanceCountersByLevel<QueuePerformanceCountersContainer>
                    (
                        this
                        , level
                    );
        }

        private bool _isAttachedPerformanceCounters = false;
        public override void AttachPerformanceCountersToMembers
                                (
                                    string categoryName
                                    , string instanceName
                                    , PerformanceCounterInstanceLifetime
                                            performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                    , long? initializePerformanceCounterInstanceRawValue = null
                                )
        {
            if (!_isAttachedPerformanceCounters)
            {
                //var type = this.GetType();
                AttachPerformanceCountersToMembers<QueuePerformanceCountersContainer>
                        (
                            categoryName
                            , instanceName
                            , this
                            , performanceCounterInstanceLifetime
                            , initializePerformanceCounterInstanceRawValue
                        );
            }
            _isAttachedPerformanceCounters = true;
        }
    }
}
#endif

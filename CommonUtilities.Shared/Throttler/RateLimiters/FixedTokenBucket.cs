namespace Microshaoft
{
    using System;
    using System.Threading;

    public class FixedTokenBucket : TokenBucket
    {
        public FixedTokenBucket
                    (
                        long maxTokens
                        , long refillInterval
                        , long refillIntervalInMilliSeconds
                    )
                        : base
                            (
                                maxTokens
                                , refillInterval
                                , refillIntervalInMilliSeconds
                            )
        {
        }

        protected override void UpdateTokens()
        {
            var currentTime = SystemTime.UtcNow.Ticks;
            if (currentTime >= _nextRefillTime)
            {
                //重新计时采样
                //_currentAvailableTokensCount = _bucketTokensCapacity;
                Interlocked
                        .Exchange
                            (
                                ref _currentAvailableTokensCount
                                , _bucketTokensCapacity
                            );
                //nextRefillTime = currentTime + ticksRefillInterval;
                Interlocked
                        .Exchange
                            (
                                ref _nextRefillTime
                                , Interlocked
                                    .Add
                                        (
                                            ref currentTime
                                            , _ticksRefillInterval
                                        )
                            );
            }
        }
    }
}
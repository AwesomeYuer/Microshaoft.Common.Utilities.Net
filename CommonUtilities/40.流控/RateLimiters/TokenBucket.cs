namespace Microshaoft
{
    using System;
    using System.Threading;
    public abstract class TokenBucket : IThrottleStrategy
    {
        protected long _bucketTokensCapacity;
        //private readonly object _syncRoot = new object();
        protected readonly long _ticksRefillInterval;
        protected long _nextRefillTime;

        //number of tokens in the bucket
        protected long _currentAvailableTokensCount;

        protected TokenBucket(long bucketTokenCapacity, long refillInterval, long refillIntervalInMilliSeconds)
        {
            if (bucketTokenCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException("bucketTokenCapacity", "bucket token capacity can not be negative");
            }
            if (refillInterval < 0)
            {
                throw new ArgumentOutOfRangeException("refillInterval", "Refill interval cannot be negative");
            }
            if (refillIntervalInMilliSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException("refillIntervalInMilliSeconds", "Refill interval in milliseconds cannot be negative");
            }

            _bucketTokensCapacity = bucketTokenCapacity;
            _ticksRefillInterval = TimeSpan.FromMilliseconds(refillInterval * refillIntervalInMilliSeconds).Ticks;
        }

        public bool ShouldThrottle(long n = 1)
        {
            TimeSpan waitTime;
            return ShouldThrottle(n, out waitTime);
        }

        public bool ShouldThrottle(long n, out TimeSpan waitTime)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException("n", "Should be positive integer");
            }

            //lock (_syncRoot)
            {
                UpdateTokens();
                if (_currentAvailableTokensCount < n)
                {
                    var timeToIntervalEnd = _nextRefillTime - SystemTime.UtcNow.Ticks;
                    if (timeToIntervalEnd < 0)
                    {
                        return ShouldThrottle(n, out waitTime);
                    }
                    waitTime = TimeSpan.FromTicks(timeToIntervalEnd);
                    return true;
                }
                //_currentUnusedTokens -= n;
                Interlocked.Add(ref _currentAvailableTokensCount, -n);
                waitTime = TimeSpan.Zero;
                return false;
            }
        }
        protected abstract void UpdateTokens();
        public bool ShouldThrottle(out TimeSpan waitTime)
        {
            return ShouldThrottle(1, out waitTime);
        }
        public long CurrentAvailableTokensCount
        {
            get
            {
                //lock (_syncRoot)
                {
                    //UpdateTokens();
                    return _currentAvailableTokensCount;
                }
            }
        }
        public long CurrentUsingTokensCount
        {
            get 
            {
                //throw new NotImplementedException(); 
                return
                    (_bucketTokensCapacity - _currentAvailableTokensCount);
            }
        }
    }
}
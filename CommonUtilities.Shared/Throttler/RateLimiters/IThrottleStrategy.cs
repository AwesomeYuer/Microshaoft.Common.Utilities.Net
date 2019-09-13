namespace Microshaoft
{
    using System;
    public interface IThrottleStrategy
    {
        bool ShouldThrottle(long n = 1);
        bool ShouldThrottle(long n, out TimeSpan waitTime);
        bool ShouldThrottle(out TimeSpan waitTime);
        long CurrentAvailableTokensCount { get; }
        long CurrentUsingTokensCount { get; }
    }
}
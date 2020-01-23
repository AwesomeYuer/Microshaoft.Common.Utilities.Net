namespace Microshaoft
{
    using System;
    public class Throttler
    {
        public readonly IThrottleStrategy Strategy;

        public Throttler(IThrottleStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException("strategy");
        }

        public bool CanConsume()
        {
            return !Strategy.ShouldThrottle();
        }
    }
}

namespace Microshaoft
{
    using System;
    public class Throttler
    {
        public readonly IThrottleStrategy Strategy;

        public Throttler(IThrottleStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy");
            }
            Strategy = strategy;
        }

        public bool CanConsume()
        {
            return !Strategy.ShouldThrottle();
        }
    }
}

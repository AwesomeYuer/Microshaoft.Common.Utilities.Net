namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

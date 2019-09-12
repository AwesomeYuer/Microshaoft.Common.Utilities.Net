namespace Microshaoft
{
    using System.Threading;
    public static class SequenceSeedHelper
    {
        private static long _seed = 0;
        public static long NewID()
        {
            long r = Interlocked.Increment(ref _seed);
            return r;
        }
    }
}
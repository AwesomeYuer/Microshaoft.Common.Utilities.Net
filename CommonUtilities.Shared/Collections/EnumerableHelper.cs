namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Linq;
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Range<T>(params T[] elements)
        {
            return
                elements.AsEnumerable();
        }

    }
}

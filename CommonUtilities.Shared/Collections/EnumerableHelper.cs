using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microshaoft
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Range<T>(params T[] elements)
        {
            return
                elements.AsEnumerable();
        }

    }
}

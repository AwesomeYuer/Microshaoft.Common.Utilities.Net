

namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class RandomHelper
    {

        // https://mp.weixin.qq.com/s/H_xVwBHzuAVj2T3_uOTRsg
        // https://www.cnblogs.com/sdflysha/p/20191103-shuffle-array-with-dotnet.html
        public static T[] ShuffleCopyToArray<T>(this IEnumerable<T> target, Random r)
        {
            var array = target.ToArray();
            for (var i = array.Length - 1; i > 0; --i)
            {
                int randomIndex = r.Next(i + 1);
                T temp = array[i];
                array[i] = array[randomIndex];
                array[randomIndex] = temp;
            }
            return array;
        }
    }
}

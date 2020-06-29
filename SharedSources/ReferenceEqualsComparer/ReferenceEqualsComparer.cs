namespace Microshaoft
{
    using System.Collections;
    using System.Collections.Generic;

    public class ReferenceEqualsComparer<T> : ReferenceEqualsComparer, IEqualityComparer<T>
    {
        /// <inheritdoc />
        public bool Equals(T x, T y) => ((IEqualityComparer)this).Equals(x, y);

        /// <inheritdoc />
        public int GetHashCode(T obj) => obj.GetHashCode();
    }

    public class ReferenceEqualsComparer : IEqualityComparer
    {
        /// <inheritdoc />
        bool IEqualityComparer.Equals(object x, object y) => ReferenceEquals(x, y);

        /// <inheritdoc />
        public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
    }
/*
    internal class Program
    {
        // 这里需要使用完全相等的判断，对象完全相等
        // 这样所有进行判断的 Contains 或 ContainsKey 都使用对象引用判断，只有传入和内存里面存放相同的对象才能判断存在
        Dictionary<object, string> LindexiShiDoubi { get; } = new Dictionary<object, string>(new ReferenceEqualsComparer<object>());
        private HashSet<object> Lindexi { get; } = new HashSet<object>(new ReferenceEqualsComparer<object>());
        private static void Main()
        {
            

        }
    }
*/
}

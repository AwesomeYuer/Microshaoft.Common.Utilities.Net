namespace Microshaoft
{
    using System.Collections.Concurrent;

    public class KeyedStack<T> //:　ConcurrentStack<T> 
    {

        private readonly ConcurrentDictionary<T, T> _dictionary = new ConcurrentDictionary<T, T>();
        private readonly ConcurrentStack<T> _stack = new ConcurrentStack<T>();

        public int Count
        {
            get
            {
                return _dictionary.Count;
            }
        }

        public bool TryPush(T item)
        {
            var r = false;
            if (_dictionary.TryAdd(item, default))
            {
                _stack.Push(item);
                r = true;
            }
            return r;
        }
        public bool TryPop(out T item)
        {
            var r = false;
            if (_stack.TryPop(out item))
            {
                //T removed = default(T);
                r = _dictionary
                        .TryRemove
                            (
                                item
                                , out _
                            );
                //removed = default(T);
            }
            return r;
        }
    }
}

#if NET35
namespace System.Collections.Concurrent
{
    using System;
    using System.Collections.Generic;
    public class ConcurrentQueue<T> : Queue<T>
    {
        private object _locker = new object();
        public bool IsEmpty
        {
            get
            {
                return (Count <= 0);
            }
        }
        public new void Enqueue(T element)
        {
            lock (_locker)
            {
                base.Enqueue(element);
            }
        }
        public bool TryDequeue(out T element)
        {
            var r = false;
            element = default(T);
            try
            {
                lock (_locker)
                {
                    element = Dequeue();
                    r = true;
                }
            }
            catch
            {

            }
            return r;
        }
    }
}
#endif
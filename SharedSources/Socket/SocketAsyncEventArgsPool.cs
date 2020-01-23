namespace Microshaoft
{
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    // Represents a collection of reusable SocketAsyncEventArgs objects.
    public class SocketAsyncEventArgsPool
    {
        private ConcurrentStack<SocketAsyncEventArgs> _pool;
        public SocketAsyncEventArgsPool(int count)
        {
            _pool = new ConcurrentStack<SocketAsyncEventArgs>();
            for (var i = 0; i < count; i++)
            {
                _pool.Push(new SocketAsyncEventArgs());
            }
        }
        // Add a SocketAsyncEventArg instance to the pool
        //
        //The "item" parameter is the SocketAsyncEventArgs instance
        // to add to the pool
        public void Push(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            _pool.Push(socketAsyncEventArgs);
        }
        // Removes a SocketAsyncEventArgs instance from the pool
        // and returns the object removed from the pool
        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs socketAsyncEventArgs = null;
            if
                (
                    _pool.IsEmpty
                    || !_pool.TryPop(out socketAsyncEventArgs)
                )
            {
                socketAsyncEventArgs = new SocketAsyncEventArgs();
            }
            return socketAsyncEventArgs;
        }
        // The number of SocketAsyncEventArgs instances in the pool
        public int Count
        {
            get
            {
                return _pool.Count;
            }
        }
    }
}

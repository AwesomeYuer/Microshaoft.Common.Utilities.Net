namespace Microshaoft
{
    using System.Collections.Concurrent;
    using System.Net.Sockets;
    public class BufferManager
    {
        // This class creates a single large buffer which can be divided up
        // and assigned to SocketAsyncEventArgs objects for use with each
        // socket I/O operation.
        // This enables buffers to be easily reused and guards against
        // fragmenting heap memory.
        //
        //This buffer is a byte array which the Windows TCP buffer can copy its data to.
        // the total number of bytes controlled by the buffer pool
        int _totalBytesInBufferBlock;
        // Byte array maintained by the Buffer Manager.
        byte[] _bufferBlock;
        ConcurrentStack<int> _freeIndexPool;
        int _currentIndex;
        int _bufferBytesAllocatedForEachSaea;
        public BufferManager(int totalBytes, int totalBufferBytesInEachSaeaObject)
        {
            _totalBytesInBufferBlock = totalBytes;
            _currentIndex = 0;
            _bufferBytesAllocatedForEachSaea = totalBufferBytesInEachSaeaObject;
            _freeIndexPool = new ConcurrentStack<int>();
        }
        // Allocates buffer space used by the buffer pool
        public void InitBuffer()
        {
            // Create one large buffer block.
            _bufferBlock = new byte[_totalBytesInBufferBlock];
        }
        // Divide that one large buffer block out to each SocketAsyncEventArg object.
        // Assign a buffer space from the buffer block to the
        // specified SocketAsyncEventArgs object.
        //
        // returns true if the buffer was successfully set, else false
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            int index = -1;
            if (_freeIndexPool.TryPop(out index))
            {
                //This if-statement is only true if you have called the FreeBuffer
                //method previously, which would put an offset for a buffer space
                //back into this stack.
                args.SetBuffer(_bufferBlock, index, _bufferBytesAllocatedForEachSaea);
            }
            else
            {
                //Inside this else-statement is the code that is used to set the
                //buffer for each SAEA object when the pool of SAEA objects is built
                //in the Init method.
                if ((_totalBytesInBufferBlock - _bufferBytesAllocatedForEachSaea) < _currentIndex)
                {
                    return false;
                }
                args.SetBuffer(_bufferBlock, _currentIndex, _bufferBytesAllocatedForEachSaea);
                _currentIndex += _bufferBytesAllocatedForEachSaea;
            }
            return true;
        }
        // Removes the buffer from a SocketAsyncEventArg object.   This frees the
        // buffer back to the buffer pool. Try NOT to use the FreeBuffer method,
        // unless you need to destroy the SAEA object, or maybe in the case
        // of some exception handling. Instead, on the server
        // keep the same buffer space assigned to one SAEA object for the duration of
        // this app's running.
        private void FreeBuffer(SocketAsyncEventArgs args)
        {
            _freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}

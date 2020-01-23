//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;

    // This is the base object pool class which manages objects in a FIFO queue. The objects are 
    // created through the provided Func<T> createObjectFunc. The main purpose for this class is
    // to get better memory usage for Garbage Collection (GC) when part or all of an object is
    // regularly pinned. Constantly creating such objects can cause large Gen0 Heap fragmentation
    // and thus high memory usage pressure. The pooled objects are first created in Gen0 heaps and
    // would be eventually moved to a more stable segment which would prevent the fragmentation
    // to happen.
    //
    // The objects are created in batches for better localization of the objects. Here are the
    // parameters that control the behavior of creation/removal:
    // 
    // batchAllocCount: number of objects to be created at the same time when new objects are needed
    //
    // createObjectFunc: func delegate that is used to create objects by sub-classes.
    //
    // maxFreeCount: max number of free objects the queue can store. This is to make sure the memory
    //     usage is bounded.
    //
    abstract class QueuedObjectPool<T>
    {
        Queue<T> objectQueue;
        bool isClosed;
        int batchAllocCount;
        int maxFreeCount;

        protected void Initialize(int batchAllocCount, int maxFreeCount)
        {
            if (batchAllocCount <= 0)
            {
                //throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("batchAllocCount"));
            }

            //Fx.Assert(batchAllocCount <= maxFreeCount, "batchAllocCount cannot be greater than maxFreeCount");
            this.batchAllocCount = batchAllocCount;
            this.maxFreeCount = maxFreeCount;
            this.objectQueue = new Queue<T>(batchAllocCount);
        }

        object ThisLock
        {
            get
            {
                return this.objectQueue;
            }
        }

        public virtual bool Return(T value)
        {
            lock (ThisLock)
            {
                if (this.objectQueue.Count < this.maxFreeCount && !this.isClosed)
                {
                    this.objectQueue.Enqueue(value);
                    return true;
                }

                return false;
            }
        }

        public T Take()
        {
            lock (ThisLock)
            {
                //Fx.Assert(!this.isClosed, "Cannot take an item from closed QueuedObjectPool");

                if (this.objectQueue.Count == 0)
                {
                    AllocObjects();
                }

                return this.objectQueue.Dequeue();
            }
        }

        public void Close()
        {
            lock (ThisLock)
            {
                foreach (T item in this.objectQueue)
                {
                    if (item != null)
                    {
                        this.CleanupItem(item);
                    }
                }

                this.objectQueue.Clear();
                this.isClosed = true;
            }
        }

        protected virtual void CleanupItem(T item)
        {
        }

        protected abstract T Create();

        void AllocObjects()
        {
            //Fx.Assert(this.objectQueue.Count == 0, "The object queue must be empty for new allocations");
            for (int i = 0; i < batchAllocCount; i++)
            {
                this.objectQueue.Enqueue(Create());
            }
        }
    }
}
//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Runtime;

    class SocketAsyncEventArgsPool : QueuedObjectPool<SocketAsyncEventArgs>
    {
        const int SingleBatchSize = 128 * 1024;
        const int MaxBatchCount = 16;
        const int MaxFreeCountFactor = 4;
        int acceptBufferSize;

        public SocketAsyncEventArgsPool(int acceptBufferSize)
        {
            if (acceptBufferSize <= 0)
            {
                //throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("acceptBufferSize"));
            }

            this.acceptBufferSize = acceptBufferSize;
            int batchCount = (SingleBatchSize + acceptBufferSize - 1) / acceptBufferSize;
            if (batchCount > MaxBatchCount)
            {
                batchCount = MaxBatchCount;
            }

            Initialize(batchCount, batchCount * MaxFreeCountFactor);
        }

        public override bool Return(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            CleanupAcceptSocket(socketAsyncEventArgs);

            if (!base.Return(socketAsyncEventArgs))
            {
                this.CleanupItem(socketAsyncEventArgs);
                return false;
            }

            return true;
        }

        internal static void CleanupAcceptSocket(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            //Fx.Assert(socketAsyncEventArgs != null, "socketAsyncEventArgs should not be null.");

            Socket socket = socketAsyncEventArgs.AcceptSocket;
            if (socket != null)
            {
                socketAsyncEventArgs.AcceptSocket = null;

                try
                {
                    socket.Close(0);
                }
                catch (SocketException ex)
                {
                    //FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                    //Microshaoft
                    Console.WriteLine(ex);
                }
                catch (ObjectDisposedException ex)
                {
                    //FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                    //Microshaoft
                    Console.WriteLine(ex);
                }
            }
        }

        protected override void CleanupItem(SocketAsyncEventArgs item)
        {
            item.Dispose();
        }

        protected override SocketAsyncEventArgs Create()
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            //Microshaoft
            byte[] acceptBuffer = new byte[this.acceptBufferSize];// DiagnosticUtility.Utility.AllocateByteArray(this.acceptBufferSize);
            eventArgs.SetBuffer(acceptBuffer, 0, this.acceptBufferSize);
            return eventArgs;
        }
    }
}
namespace Test
{
	using System;
    using System.ServiceModel.Channels;
		/// <summary>
		/// Class1 的摘要说明。
		/// </summary>
	public class Program111
	{
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		//[STAThread]
		static void Main117(string[] args)
		{
			//
			// TODO: 在此处添加代码以启动应用程序
			//

            SocketAsyncEventArgsPool x = new SocketAsyncEventArgsPool(1024 * 64);
            x.Take();
            
			Console.WriteLine("Hello World");
			Console.WriteLine(Environment.Version.ToString());
		}
	}

}


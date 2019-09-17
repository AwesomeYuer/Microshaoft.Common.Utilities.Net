
// Licensed to the .NET Foundation under one or more agreements.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Microshaoft.Buffers
{
    using System.Runtime.CompilerServices;
    using System;

    internal static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SelectBucketIndex(int bufferSize)
        {
            uint bitsRemaining = ((uint)bufferSize - 1) >> 4;

            int poolIndex = 0;
            if (bitsRemaining > 0xFFFF) { bitsRemaining >>= 16; poolIndex = 16; }
            if (bitsRemaining > 0xFF) { bitsRemaining >>= 8; poolIndex += 8; }
            if (bitsRemaining > 0xF) { bitsRemaining >>= 4; poolIndex += 4; }
            if (bitsRemaining > 0x3) { bitsRemaining >>= 2; poolIndex += 2; }
            if (bitsRemaining > 0x1) { bitsRemaining >>= 1; poolIndex += 1; }

            return poolIndex + (int)bitsRemaining;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxSizeForBucket(int binIndex)
        {
            checked
            {
                int result = 2;
                int shifts = binIndex + 3;
                result <<= shifts;
                return result;
            }
        }

        internal static int GetPoolId<T>(ArrayPool<T> pool)
        {
            return pool.GetHashCode();
        }

        internal static int GetBufferId<T>(T[] buffer)
        {
            return buffer.GetHashCode();
        }

        internal static int GetBucketId<T>(DefaultArrayPoolBucket<T> bucket)
        {
            return bucket.GetHashCode();
        }
    }
}

// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Microshaoft.Buffers
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provides a thread-safe bucket containing buffers that can be Rented and Returned as part 
    /// of a buffer pool; it should not be used independent of the pool.
    /// </summary>
    internal sealed class DefaultArrayPoolBucket<T>
    {
        private int _index;
        private readonly T[][] _data;
        internal readonly int _bufferLength;
        private SpinLock _lock;
        private bool _exhaustedEventSent;
        private readonly int _poolId;

        /// <summary>
        /// Creates the pool with numberOfBuffers arrays where each buffer is of bufferLength length.
        /// </summary>
        internal DefaultArrayPoolBucket(int bufferLength, int numberOfBuffers, int poolId)
        {
            _lock = new SpinLock(Debugger.IsAttached); // only enable thread tracking if debugger is attached; it adds non-trivial overheads to Enter/Exit
            _data = new T[numberOfBuffers][];
            _bufferLength = bufferLength;
            _exhaustedEventSent = false;
            _poolId = poolId;
        }

        /// <summary>
        /// Returns an array from the Bucket sized according to the Bucket size.
        /// If the Bucket is empty, null is returned.
        /// </summary>
        /// <returns>Returns a valid buffer when the bucket has free buffers; otherwise, returns null</returns>
        internal T[] Rent()
        {
            T[] buffer = null;

            // Use a SpinLock since it is super lightweight
            // and our lock is very short lived. Wrap in try-finally
            // to protect against thread-aborts
            bool taken = false;
            try
            {
                _lock.Enter(ref taken);

                // Check if all of our buffers have been rented
                if (_index < _data.Length)
                {
                    buffer = _data[_index];
                    if (buffer == null)
                    {
                        buffer = new T[_bufferLength];
                        if (ArrayPoolEventSource.Log.IsEnabled())
                            ArrayPoolEventSource.Log.BufferAllocated(
                                Utilities.GetBufferId(buffer),
                                _bufferLength,
                                _poolId,
                                Utilities.GetBucketId(this),
                                ArrayPoolEventSource.BufferAllocationReason.Pooled);
                    }
                    _data[_index++] = null;
                }
                else if (_exhaustedEventSent == false)
                {
                    if (ArrayPoolEventSource.Log.IsEnabled())
                        ArrayPoolEventSource.Log.BucketExhausted(Utilities.GetBucketId(this), _bufferLength, _data.Length, _poolId);
                    _exhaustedEventSent = true;
                }
            }
            finally
            {
                if (taken) _lock.Exit(false);
            }

            return buffer;
        }

        /// <summary>
        /// Attempts to return a Buffer to the bucket. This can fail
        /// if the buffer being returned was allocated and we don't have
        /// room for it in the bucket.
        /// </summary>
        internal void Return(T[] buffer)
        {
            // Check to see if the buffer is the correct size for this bucket
            if (buffer.Length != _bufferLength)
                throw new ArgumentException("SR.ArgumentException_BufferNotFromPool", "buffer");

            // Use a SpinLock since it is super lightweight
            // and our lock is very short lived. Wrap in try-finally
            // to protect against thread-aborts
            bool taken = false;
            try
            {
                _lock.Enter(ref taken);

                // If we have space to put the buffer back, do it. If we don't
                // then there was a buffer alloc'd that was returned instead so
                // we can just drop this buffer
                if (_index != 0)
                {
                    _data[--_index] = buffer;
                    _exhaustedEventSent = false; // always setting this should be cheaper than a branch
                }
            }
            finally
            {
                if (taken) _lock.Exit(false);
            }
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microshaoft.Buffers
{
    using System;
    internal sealed class DefaultArrayPool<T> : ArrayPool<T>
    {
        /// <summary>The default maximum number of arrays per bucket that are available for rent.</summary>
        private const int DefaultMaxNumberOfArraysPerBucket = 50;
        /// <summary>The default maximum length of each array in the pool (2^20).</summary>
        private const int DefaultMaxArrayLength = 1024 * 1024;
        /// <summary>The minimum length of an array in the pool.</summary>
        private const int MinimumArrayLength = 16;

        private readonly DefaultArrayPoolBucket<T>[] _buckets;

        internal DefaultArrayPool() : this(DefaultMaxArrayLength, DefaultMaxNumberOfArraysPerBucket)
        {
        }

        internal DefaultArrayPool(int maxLength, int arraysPerBucket)
        {
            if (maxLength <= 0)
                throw new ArgumentOutOfRangeException("maxLength");
            if (arraysPerBucket <= 0)
                throw new ArgumentOutOfRangeException("arraysPerBucket");

            // Our bucketing algorithm has a minimum length of 16
            if (maxLength < MinimumArrayLength)
                maxLength = MinimumArrayLength;

            int maxBuckets = Utilities.SelectBucketIndex(maxLength);
            _buckets = new DefaultArrayPoolBucket<T>[maxBuckets + 1];
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = new DefaultArrayPoolBucket<T>(Utilities.GetMaxSizeForBucket(i), arraysPerBucket, Utilities.GetPoolId(this));
        }

        public override T[] Rent(int minimumLength)
        {
            if (minimumLength <= 0)
                throw new ArgumentOutOfRangeException("minimumLength");

            var log = ArrayPoolEventSource.Log;

            T[] buffer = null;
            int index = Utilities.SelectBucketIndex(minimumLength);
            if (index < _buckets.Length)
            {
                // Search for an array starting at the 'index' bucket. If the bucket
                // is empty, bump up to the next higher bucket and try that one.
                for (int i = index; i < _buckets.Length; i++)
                {
                    buffer = _buckets[i].Rent();

                    // If the bucket has an array left and returned it, give it to the caller
                    if (buffer != null)
                    {
                        if (log.IsEnabled())
                        {
                            log.BufferRented(Utilities.GetBufferId(buffer), buffer.Length, Utilities.GetBucketId(_buckets[i]), Utilities.GetPoolId(this));
                        }
                        return buffer;
                    }
                }

                // The pool was exhausted.  Allocate a new buffer with a size corresponding to the appropriate bucket.
                buffer = new T[_buckets[index]._bufferLength];
            }
            else
            {
                // The request was for a size too large for the pool.  Allocate an array of exactly the requested length.
                // When it's returned to the pool, we'll simply throw it away.
                buffer = new T[minimumLength];
            }

            // We had to allocate a buffer, so log that fact.
            if (log.IsEnabled())
            {
                log.BufferAllocated(
                    Utilities.GetBufferId(buffer),
                    buffer.Length,
                    Utilities.GetPoolId(this),
                    -1, // no bucket for an on-demand allocated buffer,
                    index >= _buckets.Length ?
                        ArrayPoolEventSource.BufferAllocationReason.OverMaximumSize :
                        ArrayPoolEventSource.BufferAllocationReason.PoolExhausted);
            }

            return buffer;
        }

        public override void Return(T[] buffer, bool clearArray = false)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            // If we can tell that the buffer was allocated, drop it. Otherwise, check if we have space in the pool
            int bucket = Utilities.SelectBucketIndex(buffer.Length);
            if (bucket < _buckets.Length)
            {
                // Clear the array if the user requests
                if (clearArray) Array.Clear(buffer, 0, buffer.Length);

                _buckets[bucket].Return(buffer);
            }

            var log = ArrayPoolEventSource.Log;
            if (log.IsEnabled())
                log.BufferReturned(Utilities.GetBufferId(buffer), Utilities.GetPoolId(this));
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Microshaoft.Buffers
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(
        Guid = "20b30044-b729-457e-8dda-8b41b5dd02e6",
        Name = "Microshaoft.Buffers.BufferPoolEventSource",
        LocalizationResources = "FxResources.Microshaoft.Buffers.SR")]
    internal sealed class ArrayPoolEventSource : EventSource
    {
        internal readonly static ArrayPoolEventSource Log = new ArrayPoolEventSource();

        internal enum BufferAllocationReason : int
        {
            Pooled,
            OverMaximumSize,
            PoolExhausted
        }

        [Event(1, Level = EventLevel.Informational)]
        internal void BufferRented(int bufferId, int bufferSize, int poolId, int bucketId) { WriteEventHelper(1, bufferId, bufferSize, poolId, bucketId); }

        [Event(2, Level = EventLevel.Informational)]
        internal void BufferAllocated(int bufferId, int bufferSize, int poolId, int bucketId, BufferAllocationReason reason)
        {
            unsafe
            {
                EventData* payload = stackalloc EventData[5];
                payload[0].Size = sizeof(int);
                payload[0].DataPointer = ((IntPtr)(&bufferId));
                payload[1].Size = sizeof(int);
                payload[1].DataPointer = ((IntPtr)(&bufferSize));
                payload[2].Size = sizeof(int);
                payload[2].DataPointer = ((IntPtr)(&poolId));
                payload[3].Size = sizeof(int);
                payload[3].DataPointer = ((IntPtr)(&bucketId));
                payload[4].Size = sizeof(BufferAllocationReason);
                payload[4].DataPointer = ((IntPtr)(&reason));
                WriteEventCore(2, 5, payload);
            }
        }

        [Event(3, Level = EventLevel.Informational)]
        internal void BufferReturned(int bufferId, int poolId) { WriteEvent(3, bufferId, poolId); }

        [Event(4, Level = EventLevel.Warning)]
        internal void BucketExhausted(int bucketId, int bucketSize, int buffersInBucket, int poolId) { WriteEventHelper(4, bucketId, bucketSize, buffersInBucket, poolId); }

        [NonEvent]
        private unsafe void WriteEventHelper(int eventId, int arg0, int arg1, int arg2, int arg3)
        {
            if (IsEnabled())
            {
                unsafe
                {
                    EventData* payload = stackalloc EventData[4];
                    payload[0].Size = sizeof(int);
                    payload[0].DataPointer = ((IntPtr)(&arg0));
                    payload[1].Size = sizeof(int);
                    payload[1].DataPointer = ((IntPtr)(&arg1));
                    payload[2].Size = sizeof(int);
                    payload[2].DataPointer = ((IntPtr)(&arg2));
                    payload[3].Size = sizeof(int);
                    payload[3].DataPointer = ((IntPtr)(&arg3));
                    WriteEventCore(eventId, 4, payload);
                }
            }
        }
    }
}


// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace Microshaoft.Buffers
{

    using System.Threading;
    /// <summary>
    /// Provides a resource pool that enables reusing instances of type <see cref="T:T[]"/>. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Renting and returning buffers with an <see cref="ArrayPool{T}"/> can increase performance
    /// in situations where arrays are created and destroyed frequently, resulting in significant
    /// memory pressure on the garbage collector.
    /// </para>
    /// <para>
    /// This class is thread-safe.  All members may be used by multiple threads concurrently.
    /// </para>
    /// </remarks>
    public abstract class ArrayPool<T>
    {
        /// <summary>The lazily-initialized shared pool instance.</summary>
        private static ArrayPool<T> s_sharedInstance = null;

        /// <summary>
        /// Retrieves a shared <see cref="ArrayPool{T}"/> instance.
        /// </summary>
        /// <remarks>
        /// The shared pool provides a default implementation of <see cref="ArrayPool{T}"/>
        /// that's intended for general applicability.  It maintains arrays of multiple sizes, and 
        /// may hand back a larger array than was actually requested, but will never hand back a smaller 
        /// array than was requested. Renting a buffer from it with <see cref="Rent"/> will result in an 
        /// existing buffer being taken from the pool if an appropriate buffer is available or in a new 
        /// buffer being allocated if one is not available.
        /// </remarks>
        public static ArrayPool<T> Shared
        {
            get
            {
                ArrayPool<T> instance = Volatile.Read(ref s_sharedInstance);
                if (instance == null)
                {
                    Interlocked.CompareExchange(ref s_sharedInstance, Create(), null);
                    instance = s_sharedInstance;
                }

                return instance;
            }
        }

        /// <summary>
        /// Creates a new <see cref="ArrayPool{T}"/> instance using default configuration options.
        /// </summary>
        /// <returns>A new <see cref="ArrayPool{T}"/> instance.</returns>
        public static ArrayPool<T> Create()
        {
            return new DefaultArrayPool<T>();
        }

        /// <summary>
        /// Creates a new <see cref="ArrayPool{T}"/> instance using custom configuration options.
        /// </summary>
        /// <param name="maxArrayLength">The maximum length of array instances that may be stored in the pool.</param>
        /// <param name="maxArraysPerBucket">
        /// The maximum number of array instances that may be stored in each bucket in the pool.  The pool
        /// groups arrays of similar lengths into buckets for faster access.
        /// </param>
        /// <returns>A new <see cref="ArrayPool{T}"/> instance with the specified configuration options.</returns>
        /// <remarks>
        /// The created pool will group arrays into buckets, with no more than <paramref name="maxArraysPerBucket"/>
        /// in each bucket and with those arrays not exceeding <paramref name="maxArrayLength"/> in length.
        /// </remarks>
        public static ArrayPool<T> Create(int maxArrayLength, int maxArraysPerBucket)
        {
            return new DefaultArrayPool<T>(maxArrayLength, maxArraysPerBucket);
        }

        /// <summary>
        /// Retrieves a buffer that is at least the requested length.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array needed.</param>
        /// <returns>
        /// An <see cref="T:T[]"/> that is at least <paramref name="minimumLength"/> in length.
        /// </returns>
        /// <remarks>
        /// This buffer is loaned to the caller and should be returned to the same pool via 
        /// <see cref="Return"/> so that it may be reused in subsequent usage of <see cref="Rent"/>.  
        /// It is not a fatal error to not return a rented buffer, but failure to do so may lead to 
        /// decreased application performance, as the pool may need to create a new buffer to replace
        /// the one lost.
        /// </remarks>
        public abstract T[] Rent(int minimumLength);

        /// <summary>
        /// Returns to the pool an array that was previously obtained via <see cref="Rent"/> on the same 
        /// <see cref="ArrayPool{T}"/> instance.
        /// </summary>
        /// <param name="buffer">
        /// The buffer previously obtained from <see cref="Rent"/> to return to the pool.
        /// </param>
        /// <param name="clearArray">
        /// If <c>true</c> and if the pool will store the buffer to enable subsequent reuse, <see cref="Return"/>
        /// will clear <paramref name="buffer"/> of its contents so that a subsequent consumer via <see cref="Rent"/> 
        /// will not see the previous consumer's content.  If <c>false</c> or if the pool will release the buffer,
        /// the array's contents are left unchanged.
        /// </param>
        /// <remarks>
        /// Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer 
        /// and must not use it. The reference returned from a given call to <see cref="Rent"/> must only be
        /// returned via <see cref="Return"/> once.  The default <see cref="ArrayPool{T}"/>
        /// may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
        /// if it's determined that the pool already has enough buffers stored.
        /// </remarks>
        public abstract void Return(T[] buffer, bool clearArray = false);
    }
}

#if NETCOREAPP3_X
namespace Microshaoft.AspNetCore.ConcurrencyLimiters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.ConcurrencyLimiter;
    public class LIFOQueuePolicy : IQueuePolicy
    {
        private readonly List<TaskCompletionSource<bool>> _buffer;
        private readonly int _maxQueueCapacity;
        private readonly int _maxConcurrentRequests;
        private bool _hasReachedCapacity;
        private int _head;
        private int _queueLength;

        private static readonly Task<bool> _trueTask = Task.FromResult(true);

        private readonly object _bufferLock = new Object();

        private int _freeServerSpots;

        public LIFOQueuePolicy(QueuePolicyOptions options)
        {
            _buffer = new List<TaskCompletionSource<bool>>();
            _maxQueueCapacity = options.RequestQueueLimit;
            _maxConcurrentRequests = options.MaxConcurrentRequests;
            _freeServerSpots = options.MaxConcurrentRequests;
        }

        public ValueTask<bool> TryEnterAsync()
        {
            
            lock (_bufferLock)
            {
                if (_freeServerSpots > 0)
                {
                    _freeServerSpots--;
                    return new ValueTask<bool>(_trueTask);
                }

                // if queue is full, cancel oldest request
                if (_queueLength == _maxQueueCapacity)
                {
                    _hasReachedCapacity = true;
                    _buffer[_head].SetResult(false);
                    _queueLength--;
                }

                // enqueue request with a tcs
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (_hasReachedCapacity || _queueLength < _buffer.Count)
                {
                    _buffer[_head] = tcs;
                }
                else
                {
                    _buffer.Add(tcs);
                }
                _queueLength++;

                // increment _head for next time
                _head++;
                if (_head == _maxQueueCapacity)
                {
                    _head = 0;
                }
                return new ValueTask<bool>( tcs.Task);
            }
        }

        public void OnExit()
        {
            lock (_bufferLock)
            {
                if (_queueLength == 0)
                {
                    _freeServerSpots++;

                    if (_freeServerSpots > _maxConcurrentRequests)
                    {
                        _freeServerSpots--;
                        throw new InvalidOperationException("OnExit must only be called once per successful call to TryEnterAsync");
                    }

                    return;
                }

                // step backwards and launch a new task
                if (_head == 0)
                {
                    _head = _maxQueueCapacity - 1;
                }
                else
                {
                    _head--;
                }

                _buffer[_head].SetResult(true);
                _buffer[_head] = null;
                _queueLength--;
            }
        }

        
    }
}
#endif
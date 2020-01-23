#if NETCOREAPP3_X
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
namespace Microshaoft.AspNetCore.ConcurrencyLimiters
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.ConcurrencyLimiter;
    public class StackPolicy : IQueuePolicy
    {
        private readonly List<ResettableBooleanCompletionSource> _buffer;
        
        private ResettableBooleanCompletionSource _cachedResettableTCS;
        public ResettableBooleanCompletionSource CachedResettableTCS { get => _cachedResettableTCS; set => _cachedResettableTCS = value; }

        private readonly int _maxQueueCapacity;
        private readonly int _maxConcurrentRequests;
        private bool _hasReachedCapacity;
        private int _head;
        private int _queueLength;

        private readonly object _bufferLock = new Object();

        //private readonly static ValueTask<bool> _trueTask = new ValueTask<bool>(true);
        private readonly /*static*/ ValueTask<bool> _trueTask = new ValueTask<bool>(true);

        private int _freeServerSpots;

        

        public StackPolicy(IOptions<QueuePolicyOptions> options)
        {
            _buffer = new List<ResettableBooleanCompletionSource>();
            _maxQueueCapacity = options.Value.RequestQueueLimit;
            _maxConcurrentRequests = options.Value.MaxConcurrentRequests;
            _freeServerSpots = options.Value.MaxConcurrentRequests;
        }

        public ValueTask<bool> TryEnterAsync()
        {
            lock (_bufferLock)
            {
                if (_freeServerSpots > 0)
                {
                    _freeServerSpots--;
                    return _trueTask;
                }
                // if queue is full, cancel oldest request
                if (_queueLength == _maxQueueCapacity)
                {
                    _hasReachedCapacity = true;
                    _buffer[_head].Complete(false);
                    _queueLength--;
                }
                var tcs = CachedResettableTCS ??= new ResettableBooleanCompletionSource(this);
                CachedResettableTCS = null;
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
                return tcs.GetValueTask();
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
                _buffer[_head].Complete(true);
                _queueLength--;
            }
        }
    }
}
#endif
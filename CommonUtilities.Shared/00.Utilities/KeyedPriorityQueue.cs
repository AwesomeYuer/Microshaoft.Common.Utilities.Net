// <copyright file="KeyedPriorityQueue.cs" company="Microsoft">Copyright (c) Microsoft Corporation.  All rights reserved.</copyright>

namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    //internal sealed class KeyedPriorityQueueHeadChangedEventArgs<T> : EventArgs where T : class
    //{
    //    private T oldFirstElement;
    //    private T newFirstElement;
    //    public KeyedPriorityQueueHeadChangedEventArgs(T oldFirstElement, T newFirstElement)
    //    {
    //        this.oldFirstElement = oldFirstElement;
    //        this.newFirstElement = newFirstElement;
    //    }
    //    public T OldFirstElement { get { return oldFirstElement; } }
    //    public T NewFirstElement { get { return newFirstElement; } }
    //}
    ///// <summary> Combines the functionality of a dictionary and a heap-sorted priority queue.
    ///// Enqueue and Dequeue operations are O(log n), Peek is O(1) and Remove is O(n).
    ///// Used by the SchedulerService classes to maintain an ordered list of running timers, etc.
    ///// Lesser priority values are higher priority.</summary>
    ///// <typeparam name="K">Key</typeparam>
    ///// <typeparam name="V">Value</typeparam>
    ///// <typeparam name="P">Priority</typeparam>
    [Serializable]
    public class KeyedPriorityQueue<TKey, TValue, TPriority>
                            where TValue : class
    {
        //private ConcurrentBag<HeapNode<TKey, TValue, TPriority>> _heap = new ConcurrentBag<HeapNode<TKey, TValue, TPriority>>();
        private List<HeapNode<TKey, TValue, TPriority>> _heap = new List<HeapNode<TKey, TValue, TPriority>>();
        private int _size;
        private Comparer<TPriority> _priorityComparer = Comparer<TPriority>.Default;
        private HeapNode<TKey, TValue, TPriority> _placeHolder = default(HeapNode<TKey, TValue, TPriority>);
        //public event EventHandler<KeyedPriorityQueueHeadChangedEventArgs<V>> FirstElementChanged;
        private Action<TValue, TValue> _onKeyedPriorityQueueHeadChangedProcessAction = null;
        private Func<HeapNode<TKey, TValue, TPriority>, HeapNode<TKey, TValue, TPriority>, bool> _onPriorityComparerProcessFunction = null;
        public KeyedPriorityQueue
                    (
                        Action<TValue, TValue> onKeyedPriorityQueueHeadChangedProcessAction = null
                        , Func<HeapNode<TKey, TValue, TPriority>, HeapNode<TKey, TValue, TPriority>, bool> onPriorityComparerProcessFunction = null
                    )
        {
            _onKeyedPriorityQueueHeadChangedProcessAction = onKeyedPriorityQueueHeadChangedProcessAction;
            _onPriorityComparerProcessFunction = onPriorityComparerProcessFunction;
            _heap.Add(default(HeapNode<TKey, TValue, TPriority>));       // Dummy zeroth element, heap is 1-based
        }
        public void Enqueue
                        (
                            TKey key
                            , TValue value
                            , TPriority priority
                        )
        {
            TValue oldHead = _size > 0 ? _heap[1].Value : null;
            int i = ++_size;
            int parent = i / 2;
            if (i == _heap.Count)
            {
                _heap.Add(_placeHolder);
            }
            var heapNode = new HeapNode<TKey, TValue, TPriority>(key, value, priority);
            bool isHigher = IsHigher(heapNode, _heap[parent]);
            while (i > 1 && isHigher)
            {
                _heap[i] = _heap[parent];
                i = parent;
                parent = i / 2;
            }
            _heap[i] = heapNode;
            TValue newHead = _heap[1].Value;
            if (!newHead.Equals(oldHead))
            {
                RaiseHeadChangedEvent(oldHead, newHead);
            }
        }
        public TValue Dequeue()
        {
            TValue oldHead = (_size < 1) ? null : DequeueImpl();
            TValue newHead = (_size < 1) ? null : _heap[1].Value;
            RaiseHeadChangedEvent(null, newHead);
            return oldHead;
        }
        private TValue DequeueImpl()
        {
            Debug.Assert(_size > 0, "Queue Underflow");
            TValue oldHead = _heap[1].Value;
            _heap[1] = _heap[_size];
            _heap[_size--] = _placeHolder;
            Heapify(1);
            return oldHead;
        }
        public TValue Remove(TKey key)
        {
            if (_size < 1)
            {
                return null;
            }
            TValue oldHead = _heap[1].Value;
            for (int i = 1; i <= _size; i++)
            {
                if (_heap[i].Key.Equals(key))
                {
                    TValue retval = _heap[i].Value;
                    Swap(i, _size);
                    _heap[_size--] = _placeHolder;
                    Heapify(i);
                    TValue newHead = _heap[1].Value;
                    if (!oldHead.Equals(newHead))
                    {
                        RaiseHeadChangedEvent(oldHead, newHead);
                    }
                    return retval;
                }
            }
            return null;
        }
        public TValue Peek()
        {
            return (_size < 1) ? null : _heap[1].Value;
        }
        public int Count
        {
            get
            {
                return _size;
            }
        }
        public TValue FindByPriority(TPriority priority, Predicate<TValue> match)
        {
            return _size < 1 ? null : Search(priority, 1, match);
        }
        public ReadOnlyCollection<TValue> Values
        {
            get
            {
                List<TValue> values = new List<TValue>();
                for (int i = 1; i <= _size; i++)
                {
                    values.Add(_heap[i].Value);
                }
                return new ReadOnlyCollection<TValue>(values);
            }
        }
        public ReadOnlyCollection<TKey> Keys
        {
            get
            {
                List<TKey> keys = new List<TKey>();
                for (int i = 1; i <= _size; i++)
                {
                    keys.Add(_heap[i].Key);
                }
                return new ReadOnlyCollection<TKey>(keys);
            }
        }
        public void Clear()
        {
            _heap.Clear();
            _size = 0;
        }
        private void RaiseHeadChangedEvent(TValue oldHead, TValue newHead)
        {
            if (oldHead != newHead)
            {
                //EventHandler<KeyedPriorityQueueHeadChangedEventArgs<V>> fec = FirstElementChanged;
                if (_onKeyedPriorityQueueHeadChangedProcessAction != null)
                {
                    _onKeyedPriorityQueueHeadChangedProcessAction(oldHead, newHead);
                }
            }
        }
        private TValue Search(TPriority priority, int i, Predicate<TValue> match)
        {
            Debug.Assert(i >= 1 || i <= _size, "Index out of range: i = " + i + ", size = " + _size);
            TValue value = null;
            var isHigher = IsHigher(_heap[i], priority);
            if (isHigher)
            {
                if (match(_heap[i].Value))
                {
                    value = _heap[i].Value;
                }
                int left = 2 * i;
                int right = left + 1;
                if (value == null && left <= _size)
                {
                    value = Search(priority, left, match);
                }
                if (value == null && right <= _size)
                {
                    value = Search(priority, right, match);
                }
            }
            return value;
        }
        private void Heapify(int i)
        {
            Debug.Assert(i >= 1 || i <= _size, "Index out of range: i = " + i + ", size = " + _size);
            int left = 2 * i;
            int right = left + 1;
            int highest = i;
            if (left <= _size && IsHigher(_heap[left], _heap[i]))
            {
                highest = left;
            }
            if (right <= _size && IsHigher(_heap[right], _heap[highest]))
            {
                highest = right;
            }
            if (highest != i)
            {
                Swap(i, highest);
                Heapify(highest);
            }
        }
        private void Swap(int i, int j)
        {
            Debug.Assert(i >= 1 || j >= 1 || i <= _size || j <= _size, "Index out of range: i = " + i + ", j = " + j + ", size = " + _size);
            HeapNode<TKey, TValue, TPriority> temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;
        }
        private bool IsHigher
                        (
                            HeapNode<TKey, TValue, TPriority> compare
                            , TPriority compareWithPriority
                        )
        {
            var r = false;
            if (_onPriorityComparerProcessFunction != null)
            {
                var compareWithHeapNode
                        = new HeapNode<TKey, TValue, TPriority>
                            (default(TKey), default(TValue), compareWithPriority);
                r = _onPriorityComparerProcessFunction(compare, compareWithHeapNode);
            }
            else
            {
                r = _priorityComparer.Compare(compare.Priority, compareWithPriority) < 1;
            }
            return r;
        }
        private bool IsHigher
                        (
                            HeapNode<TKey, TValue, TPriority> compare
                            , HeapNode<TKey, TValue, TPriority> compareWith
                        )
        {
            var r = false;
            if (_onPriorityComparerProcessFunction != null)
            {
                r = _onPriorityComparerProcessFunction(compare, compareWith);
            }
            else
            {
                r = _priorityComparer.Compare(compare.Priority, compareWith.Priority) < 1;
            }
            return r;
        }
        [Serializable]
        public struct HeapNode<KeyT, ValueT, PriorityT>
        {
            public KeyT Key;
            public ValueT Value;
            public PriorityT Priority;
            public HeapNode(KeyT key, ValueT value, PriorityT priority)
            {
                Key = key;
                Value = value;
                Priority = priority;
            }
        }
    }
}

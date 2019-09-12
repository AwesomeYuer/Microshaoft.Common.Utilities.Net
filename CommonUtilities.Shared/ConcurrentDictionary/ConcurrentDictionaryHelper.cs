namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    public static partial class ConcurrentDictionaryHelper
    {
        public static TValue Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, TKey key, TValue @value)
        {
            TValue result = target.AddOrUpdate(key, @value, (k, v) => { return @value; });
            return result;
        }

        public static TValue Update<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, TKey key, TValue @value)
        {
            TValue result = target.AddOrUpdate(key, @value, (k, v) => { return @value; });
            return result;
        }

        public static TValue Get<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, TKey key)
        {
            target.TryGetValue(key, out TValue @value);
            return @value;
        }

        public static TValue Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> target, TKey key)
        {
            target.TryRemove(key, out TValue @value);
            return @value;
        }
        public static void ForEach<TKey, TValue>
                                (
                                    this ConcurrentDictionary<TKey, TValue> target
                                    , Func<TKey, TValue, bool> processFunc
                                )
        {
            foreach (KeyValuePair<TKey, TValue> kvp in target)
            {
                if 
                    (
                        processFunc(kvp.Key, kvp.Value)
                    )
                {
                   break;
                }
            }
        }
    }
}

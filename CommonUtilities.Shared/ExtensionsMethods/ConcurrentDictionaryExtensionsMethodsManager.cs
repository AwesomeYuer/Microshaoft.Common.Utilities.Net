namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    public static partial class ExtensionsMethodsManager
    {


        public static void ForEach<TKey, TValue>
                                (
                                    this ConcurrentDictionary<TKey, TValue> instance
                                    , Func<TKey, TValue, bool> processFunc
                                )
        {
            foreach (KeyValuePair<TKey, TValue> kvp in instance)
            {
                bool r = processFunc(kvp.Key, kvp.Value);
                if (r)
                {
                   break;
                }
            }
        }
    }
}

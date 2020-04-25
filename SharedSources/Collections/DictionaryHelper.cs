﻿
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Collections.Generic;
    /// <summary>
    /// Extension methods for <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    //[EditorBrowsable(EditorBrowsableState.Never)]
    public static class DictionaryHelper
    {
        /// <summary>
        /// Remove entries from dictionary that match the removeCondition.
        /// </summary>
        public static void RemoveFromDictionary<TKey, TValue>
                            (
                                this IDictionary<TKey, TValue> @this
                                , Func<KeyValuePair<TKey, TValue>, bool> removeCondition
                            )
        {
            // Pass the delegate as the state to avoid a delegate and closure
            @this.RemoveFromDictionary((entry, innerCondition) =>
            {
                return innerCondition(entry);
            },
                removeCondition);
        }

        /// <summary>
        /// Remove entries from dictionary that match the removeCondition.
        /// </summary>
        public static void RemoveFromDictionary<TKey, TValue, TState>(this IDictionary<TKey, TValue> @this, Func<KeyValuePair<TKey, TValue>, TState, bool> removeCondition, TState state)
        {
            Contract.Assert(@this != null);
            Contract.Assert(removeCondition != null);

            // Because it is not possible to delete while enumerating, a copy of the keys must be taken. Use the size of the dictionary as an upper bound
            // to avoid creating more than one copy of the keys.
            int removeCount = 0;
            TKey[] keys = new TKey[@this.Count];
            foreach (var entry in @this)
            {
                if (removeCondition(entry, state))
                {
                    keys[removeCount] = entry.Key;
                    removeCount++;
                }
            }
            for (int i = 0; i < removeCount; i++)
            {
                @this.Remove(keys[i]);
            }
        }

        /// <summary>
        /// Gets the value of <typeparamref name="T"/> associated with the specified key or <c>default</c> value if
        /// either the key is not present or the value is not of type <typeparamref name="T"/>. 
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the specified key.</typeparam>
        /// <param name="@this">The <see cref="IDictionary{TKey,TValue}"/> instance where <c>TValue</c> is <c>object</c>.</param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns><c>true</c> if key was found, value is non-null, and value is of type <typeparamref name="T"/>; otherwise false.</returns>
        public static bool TryGetValue<T>(this IDictionary<string, object> @this, string key, out T value)
        {
            Contract.Assert(@this != null);

            object valueObj;
            if (@this.TryGetValue(key, out valueObj))
            {
                if (valueObj is T)
                {
                    value = (T)valueObj;
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        internal static IEnumerable<KeyValuePair<string, TValue>> FindKeysWithPrefix<TValue>(this IDictionary<string, TValue> @this, string prefix)
        {
            Contract.Assert(@this != null);
            Contract.Assert(prefix != null);

            TValue exactMatchValue;
            if (@this.TryGetValue(prefix, out exactMatchValue))
            {
                yield return new KeyValuePair<string, TValue>(prefix, exactMatchValue);
            }

            foreach (var entry in @this)
            {
                string key = entry.Key;

                if (key.Length <= prefix.Length)
                {
                    continue;
                }

                if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Everything is prefixed by the empty string
                if (prefix.Length == 0)
                {
                    yield return entry;
                }
                else
                {
                    char charAfterPrefix = key[prefix.Length];
                    switch (charAfterPrefix)
                    {
                        case '[':
                        case '.':
                            yield return entry;
                            break;
                    }
                }
            }
        }
    }
}

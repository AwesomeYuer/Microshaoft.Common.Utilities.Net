
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Helper extension methods for fast use of collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Return a new array with the value added to the end. Slow and best suited to long lived arrays with few writes relative to reads.
        /// </summary>
        public static T[] AppendAndReallocate<T>(this T[] target, T value)
        {
            Contract.Assert(target != null);

            int originalLength = target.Length;
            T[] newArray = new T[originalLength + 1];
            target.CopyTo(newArray, 0);
            newArray[originalLength] = value;
            return newArray;
        }

        /// <summary>
        /// Return the enumerable as an Array, copying if required. Optimized for common case where it is an Array. 
        /// Avoid mutating the return value.
        /// </summary>
        public static T[] AsArray<T>(this IEnumerable<T> target)
        {
            Contract.Assert(target != null);

            T[] array = target as T[];
            if (array == null)
            {
                array = target.ToArray();
            }
            return array;
        }

        /// <summary>
        /// Return the enumerable as a Collection of T, copying if required. Optimized for the common case where it is 
        /// a Collection of T and avoiding a copy if it implements IList of T. Avoid mutating the return value.
        /// </summary>
        public static Collection<T> AsCollection<T>(this IEnumerable<T> target)
        {
            Contract.Assert(target != null);

            Collection<T> collection = target as Collection<T>;
            if (collection != null)
            {
                return collection;
            }
            // Check for IList so that collection can wrap it instead of copying
            IList<T> list = target as IList<T>;
            if (list == null)
            {
                list = new List<T>(target);
            }
            return new Collection<T>(list);
        }

        /// <summary>
        /// Return the enumerable as a IList of T, copying if required. Avoid mutating the return value.
        /// </summary>
        public static IList<T> AsIList<T>(this IEnumerable<T> target)
        {
            Contract.Assert(target != null);

            IList<T> list = target as IList<T>;
            if (list != null)
            {
                return list;
            }
            return new List<T>(target);
        }

        /// <summary>
        /// Return the enumerable as a List of T, copying if required. Optimized for common case where it is an List of T 
        /// or a ListWrapperCollection of T. Avoid mutating the return value.
        /// </summary>
        public static List<T> AsList<T>(this IEnumerable<T> target)
        {
            Contract.Assert(target != null);

            List<T> list = target as List<T>;
            if (list != null)
            {
                return list;
            }
            ListWrapperCollection<T> listWrapper = target as ListWrapperCollection<T>;
            if (listWrapper != null)
            {
                return listWrapper.ItemsList;
            }
            return new List<T>(target);
        }

        /// <summary>
        /// Remove values from the list starting at the index start.
        /// </summary>
        public static void RemoveFrom<T>(this List<T> target, int start)
        {
            Contract.Assert(target != null);
            Contract.Assert(start >= 0 && start <= target.Count);

            target.RemoveRange(start, target.Count - start);
        }

        /// <summary>
        /// Return the only value from list, the type's default value if empty, or call the errorAction for 2 or more.
        /// </summary>
        public static T SingleDefaultOrError<T, TArg1>(this IList<T> target, Action<TArg1> errorAction, TArg1 errorArg1)
        {
            Contract.Assert(target != null);
            Contract.Assert(errorAction != null);

            switch (target.Count)
            {
                case 0:
                    return default(T);

                case 1:
                    T value = target[0];
                    return value;

                default:
                    errorAction(errorArg1);
                    return default(T);
            }
        }

        /// <summary>
        /// Returns a single value in list matching type TMatch if there is only one, null if there are none of type TMatch or calls the
        /// errorAction with errorArg1 if there is more than one.
        /// </summary>
        public static TMatch SingleOfTypeDefaultOrError<TInput, TMatch, TArg1>(this IList<TInput> target, Action<TArg1> errorAction, TArg1 errorArg1) where TMatch : class
        {
            Contract.Assert(target != null);
            Contract.Assert(errorAction != null);

            TMatch result = null;
            for (int i = 0; i < target.Count; i++)
            {
                TMatch typedValue = target[i] as TMatch;
                if (typedValue != null)
                {
                    if (result == null)
                    {
                        result = typedValue;
                    }
                    else
                    {
                        errorAction(errorArg1);
                        return null;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Convert an ICollection to an array, removing null values. Fast path for case where there are no null values.
        /// </summary>
        public static T[] ToArrayWithoutNulls<T>(this ICollection<T> target) where T : class
        {
            Contract.Assert(target != null);

            T[] result = new T[target.Count];
            int count = 0;
            foreach (T value in target)
            {
                if (value != null)
                {
                    result[count] = value;
                    count++;
                }
            }
            if (count == target.Count)
            {
                return result;
            }
            else
            {
                T[] trimmedResult = new T[count];
                Array.Copy(result, trimmedResult, count);
                return trimmedResult;
            }
        }

        /// <summary>
        /// Convert the array to a Dictionary using the keySelector to extract keys from values and the specified comparer. Optimized for array input.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this TValue[] target, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Assert(target != null);
            Contract.Assert(keySelector != null);

            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(target.Length, comparer);
            for (int i = 0; i < target.Length; i++)
            {
                TValue value = target[i];
                dictionary.Add(keySelector(value), value);
            }
            return dictionary;
        }

        /// <summary>
        /// Convert the list to a Dictionary using the keySelector to extract keys from values and the specified comparer. Optimized for IList of T input with fast path for array.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this IList<TValue> target, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Assert(target != null);
            Contract.Assert(keySelector != null);

            TValue[] array = target as TValue[];
            if (array != null)
            {
                return ToDictionaryFast(array, keySelector, comparer);
            }
            return ToDictionaryFastNoCheck(target, keySelector, comparer);
        }

        /// <summary>
        /// Convert the enumerable to a Dictionary using the keySelector to extract keys from values and the specified comparer. Fast paths for array and IList of T.
        /// </summary>
        public static Dictionary<TKey, TValue> ToDictionaryFast<TKey, TValue>(this IEnumerable<TValue> target, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Assert(target != null);
            Contract.Assert(keySelector != null);

            TValue[] array = target as TValue[];
            if (array != null)
            {
                return ToDictionaryFast(array, keySelector, comparer);
            }
            IList<TValue> list = target as IList<TValue>;
            if (list != null)
            {
                return ToDictionaryFastNoCheck(list, keySelector, comparer);
            }
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (TValue value in target)
            {
                dictionary.Add(keySelector(value), value);
            }
            return dictionary;
        }

        /// <summary>
        /// Convert the list to a Dictionary using the keySelector to extract keys from values and the specified comparer. Optimized for IList of T input. No checking for other types.
        /// </summary>
        private static Dictionary<TKey, TValue> ToDictionaryFastNoCheck<TKey, TValue>(IList<TValue> target, Func<TValue, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            Contract.Assert(target != null);
            Contract.Assert(keySelector != null);

            int listCount = target.Count;
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(listCount, comparer);
            for (int i = 0; i < listCount; i++)
            {
                TValue value = target[i];
                dictionary.Add(keySelector(value), value);
            }
            return dictionary;
        }
    }
}


// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    /// <summary>
    /// A class that inherits from Collection of T but also exposes its underlying data as List of T for performance.
    /// </summary>
    public sealed class ListWrapperCollection<T> : Collection<T>
    {
        private readonly List<T> _items;

        public ListWrapperCollection()
            : this(new List<T>())
        {
        }

        public ListWrapperCollection(List<T> list)
            : base(list)
        {
            _items = list;
        }

        public List<T> ItemsList
        {
            get { return _items; }
        }
    }
}

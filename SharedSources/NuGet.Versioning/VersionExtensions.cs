﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Microshaoft.Versioning
{
    public static class VersionExtensions
    {
        /// <summary>
        /// Find the version that best matches the VersionRange and the floating behavior.
        /// </summary>
        public static T FindBestMatch<T>(this IEnumerable<T> items,
                                         VersionRange ideal,
                                         Func<T, NuGetVersion> selector) where T : class
        {
            if (ideal == null)
            {
                // TODO: Disallow null versions for nuget packages
                return items.FirstOrDefault();
            }

            T bestMatch = null;

            foreach (var item in items)
            {
                if (ideal.IsBetter(
                    current: selector(bestMatch),
                    considering: selector(item)))
                {
                    bestMatch = item;
                }
            }

            if (bestMatch == null)
            {
                return null;
            }

            return bestMatch;
        }

        /// <summary>
        /// Find the version that best matches the VersionRange and the floating behavior.
        /// </summary>
        public static INuGetVersionable FindBestMatch(this IEnumerable<INuGetVersionable> items, VersionRange ideal)
        {
            return FindBestMatch<INuGetVersionable>(items, ideal, (e => e.Version));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Postica.Common
{
    /// <summary>
    /// This class contains extension methods for the <see cref="Type"/> class.
    /// </summary>
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Return true if the collection has the same elements of the other collection.
        /// </summary>
        /// <remarks>
        /// Repetitions are not considered.
        /// </remarks>
        /// <param name="collection"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsSimilarTo<T>(this IEnumerable<T> collection, IEnumerable<T> other)
        {
            if (collection == null && other == null)
            {
                return true;
            }

            if (collection == null || other == null)
            {
                return false;
            }

            return collection.ToHashSet().SetEquals(other);
        }
    }
}

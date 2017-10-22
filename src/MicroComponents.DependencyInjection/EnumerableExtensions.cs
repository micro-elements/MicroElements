using System;
using System.Collections.Generic;

namespace MicroComponents.DependencyInjection
{
    public static class EnumerableExtensions
    {
        public static void Iter<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
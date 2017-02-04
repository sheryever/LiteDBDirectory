using System;
using System.Collections.Generic;

namespace Lucene.Net.Store.LiteDbDirectory.Helpers
{
    static class HigherOrderFunctions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
using System.Collections.Generic;

namespace MachtAuthenticate.Shared.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> list, IEnumerable<T> range)
        {
            foreach (var item in range)
                list.Add(item);
        }
    }
}
using System.Collections.Generic;

namespace YnabApi.Extensions
{
    public enum HashSetDuplicateOptions
    {
        KeepFirst,
        Override
    }

    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, HashSetDuplicateOptions options)
        {
            var set = new HashSet<T>();

            foreach (var item in items)
            {
                if (set.Contains(item) && options == HashSetDuplicateOptions.Override)
                {
                    set.Remove(item);
                }

                set.Add(item);
            }

            return set;
        }

        public static void Add<T>(this HashSet<T> hashSet, T item, HashSetDuplicateOptions options)
        {
            if (hashSet.Add(item) == false && options == HashSetDuplicateOptions.Override)
            {
                hashSet.Remove(item);
                hashSet.Add(item);
            }
        }
    }
}
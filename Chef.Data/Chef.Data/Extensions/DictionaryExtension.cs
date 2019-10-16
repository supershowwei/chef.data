using System.Collections.Generic;

namespace Chef.Data.Extensions
{
    internal static class DictionaryExtension
    {
        public static void AddRange<TKey, TValue>(
            this Dictionary<TKey, TValue> me,
            IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var pair in collection)
            {
                me.Add(pair.Key, pair.Value);
            }
        }
    }
}
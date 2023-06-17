using System.Collections.Generic;

namespace LLama.Extensions
{
    public static class DictionaryExtension
    {
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 first, out T2 second)
        {
            first = pair.Key;
            second = pair.Value;
        }

        public static void Update<T1, T2>(this Dictionary<T1, T2> dic, IDictionary<T1, T2> other)
        {
            foreach (var (key, value) in other)
            {
                dic[key] = value;
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key,
            TValue defaultValue)
        {
            return dic.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}

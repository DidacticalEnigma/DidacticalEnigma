using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Utils
{
    static class EnumerableExt
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> input, int n)
        {
            var list = new List<T>();
            int i = 0;
            foreach (var element in input)
            {
                list.Add(element);
                i++;
                if (i == n)
                {
                    yield return list;
                    list = new List<T>();
                    i = 0;
                }
            }
            if (i != 0)
                yield return list;
        }

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> Valuefactory)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                value = Valuefactory();
                dict[key] = value;
                return value;
            }
        }

        private static IEnumerable<T> PrependIfNonEmpty<T>(T element, IEnumerable<T> sequence)
        {
            bool first = true;
            var previous = element;
            foreach(var e in sequence)
            {
                yield return previous;
                previous = e;
                first = false;
            }
            if (!first)
                yield return previous;
        }

        public static IEnumerable<T> IntersperseSequencesWith<T>(IEnumerable<IEnumerable<T>> sequences, T element)
        {
            bool firstSequence = true;
            foreach(var sequence in sequences)
            {
                var newSequence = firstSequence ? sequence : PrependIfNonEmpty(element, sequence);
                foreach(var e in newSequence)
                {
                    yield return e;
                    firstSequence = false;
                }
            }
        }
    }
}

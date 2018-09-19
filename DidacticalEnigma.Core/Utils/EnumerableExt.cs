using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Utils
{
    public static class EnumerableExt
    {
        public static IEnumerable<T> OfSingle<T>(T element)
        {
            yield return element;
        }

        public static IEnumerable<T> Greedy<T>(this IEnumerable<T> input, Func<IReadOnlyList<T>, bool> acceptor, int backOffCountStart = 5)
        {
            int backOffCount = backOffCountStart;
            var sequence = new List<T>();
            int count = 0;
            int lastAcceptedCount = 0;
            foreach(var word in input)
            {
                sequence.Add(word);
                count++;
                bool isOk = acceptor(sequence);
                if(!isOk)
                {
                    backOffCount--;
                    if(backOffCount == 0)
                        break;
                }
                else
                {
                    lastAcceptedCount = count;
                    backOffCount = backOffCountStart;
                }
            }

            return sequence.GetRange(0, lastAcceptedCount);
        }

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

        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> input, T element)
        {
            bool first = true;
            foreach (var e in input)
            {
                if (!first)
                    yield return element;
                yield return e;
                first = false;
            }
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

        public static bool IsIntersectionNonEmpty<T>(this ISet<T> set, IEnumerable<T> otherSet)
        {
            return otherSet.Any(element => set.Contains(element));
        }
    }
}

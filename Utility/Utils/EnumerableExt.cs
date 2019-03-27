using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace Utility.Utils
{
    public static class EnumerableExt
    {
        public static IEnumerable<long> Range(long start, long count)
        {
            for (long i = 0; i < count; ++i)
            {
                yield return start + i;
            }
        }

        public static IEnumerable<T> OfSingle<T>(T element)
        {
            yield return element;
        }

        public static IReadOnlyCollection<TElement> Materialize<TElement>(this IEnumerable<TElement> input)
        {
            if (input is IReadOnlyCollection<TElement> readOnlyCollection)
                return readOnlyCollection;
            return input.ToList();
        }

        public static IReadOnlyDictionary<TValue, TKey> InvertMapping<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> input)
        {
            return input.ToDictionary(i => i.Value, i => i.Key);
        }

        public static IReadOnlyDictionary<TValue, IEnumerable<TKey>> InvertMappingToSequence<TKey, TValue>(
            this IReadOnlyDictionary<TKey, IEnumerable<TValue>> input)
        {
            return input
                .SelectMany(kvp =>
                    kvp.Value.Select(v => new KeyValuePair<TValue, TKey>(v, kvp.Key)))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Value).ToList().AsEnumerable());
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

        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valuefactory)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
            {
                value = valuefactory();
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

        public static Option<TValue> Lookup<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.TryGetValue(key, out var value) ? value.Some() : value.None();
        }

        public static IEnumerable<(TIn1, TIn2)> Zip<TIn1, TIn2>(
            this IEnumerable<TIn1> first,
            IEnumerable<TIn2> second)
        {
            return first.Zip(second, (f, s) => (f, s));
        }

        public static IEnumerable<(TIn1, TIn2, TIn3)> Zip<TIn1, TIn2, TIn3>(
            this IEnumerable<TIn1> first,
            IEnumerable<TIn2> second,
            IEnumerable<TIn3> third)
        {
            return Zip(first, second, third, (in1, in2, in3) => (in1, in2, in3));
        }

        public static IEnumerable<TOut> Zip<TOut, TIn1, TIn2, TIn3>(
            this IEnumerable<TIn1> first,
            IEnumerable<TIn2> second,
            IEnumerable<TIn3> third,
            Func<TIn1, TIn2, TIn3, TOut> selector)
        {
            using (var i1 = first.GetEnumerator())
            using (var i2 = second.GetEnumerator())
            using (var i3 = third.GetEnumerator())
            {
                while (i1.MoveNext() && i2.MoveNext() && i3.MoveNext())
                {
                    yield return selector(i1.Current, i2.Current, i3.Current);
                }
            }
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> entries)
        {
            // manual loop for better debugability
            var dict = new Dictionary<TKey, TValue>();
            foreach (var entry in entries)
            {
                dict.Add(entry.Key, entry.Value);
            }

            return dict;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> entries,
            Func<TKey, TValue, TValue, TValue> duplicateKeyResolver)
        {
            // manual loop for better debugability
            var dict = new Dictionary<TKey, TValue>();
            foreach (var entry in entries)
            {
                if (dict.TryGetValue(entry.Key, out var value))
                {
                    dict[entry.Key] = duplicateKeyResolver(entry.Key, value, entry.Value);
                }
                else
                {
                    dict[entry.Key] = entry.Value;
                }
            }

            return dict;
        }

        public static IEnumerable<(TElement element, int index)> Indexed<TElement>(this IEnumerable<TElement> input)
        {
            return input.Select((element, index) => (element, index));
        }

        public static IEnumerable<TElement> Cycle<TElement>(this IReadOnlyCollection<TElement> elements)
        {
            while (true)
            {
                foreach (var element in elements)
                {
                    yield return element;
                }
            }
            // well duh
            // ReSharper disable once IteratorNeverReturns
        }

        public static IEnumerable<TElement> Repeat<TElement>(TElement element)
        {
            while (true)
            {
                yield return element;
            }
            // well duh
            // ReSharper disable once IteratorNeverReturns
        }

        public static TSource MinBy<TSource, TCompared>(
            this IEnumerable<TSource> elements,
            Func<TSource, TCompared> comparedCriterionSelector,
            IComparer<TCompared> comparer)
        {
            TSource currentMinimum = default(TSource);
            bool isFirst = true;
            foreach (var element in elements)
            {
                if (isFirst)
                {
                    currentMinimum = element;
                }
                else
                {
                    var comparisonResult = comparer.Compare(
                        comparedCriterionSelector(element),
                        comparedCriterionSelector(currentMinimum));
                    currentMinimum = comparisonResult < 0
                        ? element
                        : currentMinimum;
                }

                isFirst = false;
            }

            if (!isFirst)
            {
                return currentMinimum;
            }
            else
            {
                throw new ArgumentException("can't be an empty sequence", nameof(elements));
            }
        }

        public static TSource MinBy<TSource, TCompared>(
            this IEnumerable<TSource> elements,
            Func<TSource, TCompared> comparedCriterionSelector)
        {
            return MinBy(elements, comparedCriterionSelector, Comparer<TCompared>.Default);
        }

        public static TSource MaxBy<TSource, TCompared>(
            this IEnumerable<TSource> elements,
            Func<TSource, TCompared> comparedCriterionSelector,
            IComparer<TCompared> comparer)
        {
            var inverseComparer = Comparer<TCompared>.Create((lhs, rhs) => -Math.Sign(comparer.Compare(lhs, rhs)));
            return MinBy(elements, comparedCriterionSelector, inverseComparer);
        }

        public static TSource MaxBy<TSource, TCompared>(
            this IEnumerable<TSource> elements,
            Func<TSource, TCompared> comparedCriterionSelector)
        {
            return MaxBy(elements, comparedCriterionSelector, Comparer<TCompared>.Default);
        }

        // https://stackoverflow.com/a/489421/1012936
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<IEnumerable<TElement>> GroupConsecutive<TElement, TKey>(
            this IEnumerable<TElement> input,
            Func<TElement, TKey> keySelector,
            IComparer<TKey> comparer)
        {
            var group = new List<TElement>();
            TKey previousKey = default(TKey);
            bool isFirst = true;
            foreach (var current in input)
            {
                var currentKey = keySelector(current);
                if (isFirst)
                {
                    group.Add(current);
                }
                else
                {
                    if (comparer.Compare(previousKey, currentKey) == 0)
                    {
                        group.Add(current);
                    }
                    else
                    {
                        yield return group;
                        group = new List<TElement> {current};
                    }
                }

                previousKey = currentKey;
                isFirst = false;
            }

            if (!isFirst && group.Count != 0)
            {
                yield return group;
            }
        }

        public static IEnumerable<IEnumerable<TElement>> GroupConsecutive<TElement, TKey>(
            this IEnumerable<TElement> input,
            Func<TElement, TKey> keySelector)
        {
            return GroupConsecutive(input, keySelector, Comparer<TKey>.Default);
        }
    }
}
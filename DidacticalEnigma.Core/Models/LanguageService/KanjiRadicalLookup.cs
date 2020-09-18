using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using JDict;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class KanjiRadicalLookup : IKanjiRadicalLookup
    {
        public ReadOnlyListWithSelector<IKanjiOrdering> SortingCriteria { get; }

        private static int DivideRoundUp(int x, int y)
        {
            return (x + y - 1) / y;
        }

        private const int ulongBitCount = 8 * sizeof(ulong);
        private static readonly int vectorBitCount = ulongBitCount * Vector<ulong>.Count;

        public KanjiRadicalLookup(IEnumerable<Radkfile.Entry> entries, KanjiDict kanjiDict)
        {
            SortingCriteria = new ReadOnlyListWithSelector<IKanjiOrdering>(new IKanjiOrdering[]
            {
                KanjiOrdering.Create("Sort by stroke count", kanjiDict, x => x.StrokeCount),
                KanjiOrdering.Create("Sort by frequency", kanjiDict, x => x.FrequencyRating)
            });
            SortingCriteria.SelectedIndex = 0;
            var entryList = entries.ToList();
            radicalCount = entryList.Count;
            elementSize = DivideRoundUp(radicalCount, vectorBitCount);
            elementSize = elementSize == 0 ? 1 : elementSize;

            var kradMapping = entryList
                .ToDictionary(entry => entry.Radical.CodePoint, entry => entry.KanjiCodePoints.AsEnumerable())
                .InvertMappingToSequence();

            var kanjiCodePoints = entryList
                .SelectMany(entry => entry.KanjiCodePoints)
                .Distinct()
                .ToArray();
            kanjiCount = kanjiCodePoints.Length;

            indexToKanji = SortingCriteria
                .Select(sortingCriterion => kanjiCodePoints
                    .OrderBy(x => x, Comparer<int>.Create((l, r) => sortingCriterion.Compare(
                        CodePoint.FromInt(l),
                        CodePoint.FromInt(r))))
                    .ToArray())
                .ToArray();

            indexToRadical = entryList
                .Select(entry => entry.Radical.CodePoint)
                .ToArray();

            radicalToIndex = indexToRadical
                .Indexed()
                .ToDictionary(p => p.element, p => p.index);

            var kanjiToIndex = indexToKanji
                .Select(a => a
                    .Indexed()
                    .ToDictionary(p => p.element, p => p.index))
                .ToArray();

            radkinfo = Enumerable.Range(0, SortingCriteria.Count)
                .Select(CreateRadkInfo)
                .ToArray();

            Vector<ulong>[] CreateRadkInfo(int x)
            {
                var r = new Vector<ulong>[kanjiCount * elementSize];
                foreach (var kanji in kanjiCodePoints)
                {
                    var v = new Vector<ulong>[elementSize];
                    var kanjiIndex = kanjiToIndex[x][kanji];
                    var vec = AsScalarSpan(v);
                    foreach (var radical in kradMapping[kanji])
                    {
                        var radicalIndex = radicalToIndex[radical];
                        vec[radicalIndex / ulongBitCount] |= (ulong)(1UL << radicalIndex);
                    }

                    for (int i = 0; i < elementSize; ++i)
                    {
                        r[kanjiIndex * elementSize + i] = v[i];
                    }
                }

                return r;
            }
        }

        public class Result
        {
            public IReadOnlyCollection<CodePoint> Kanji { get; }

            public IReadOnlyList<KeyValuePair<CodePoint, bool>> PossibleRadicals { get; }

            public Result(IReadOnlyCollection<CodePoint> kanji, IReadOnlyList<KeyValuePair<CodePoint, bool>> possibleRadicals)
            {
                Kanji = kanji;
                PossibleRadicals = possibleRadicals;
            }
        }

        private Span<ulong> AsScalarSpan(Span<Vector<ulong>> input)
        {
            return MemoryMarshal.Cast<Vector<ulong>, ulong>(input);
        }

        

        public Result SelectRadical(IEnumerable<CodePoint> radicals)
        {
            return SelectRadical(radicals, SortingCriteria.SelectedIndex);
        }

        public Result SelectRadical(IEnumerable<CodePoint> radicals, int sortingCriteriaIndex)
        {
            var result = new List<CodePoint>();
            var possibleRadicals = new KeyValuePair<CodePoint, bool>[radicalCount];
            var key = new Vector<ulong>[elementSize].AsSpan();
            var vec = AsScalarSpan(key);
            foreach (var radical in radicals)
            {
                var radicalIndex = radicalToIndex[radical.Utf32];
                vec[radicalIndex / ulongBitCount] |= (ulong)(1UL << radicalIndex);
            }

            var s = sortingCriteriaIndex;
            var radk = radkinfo[s];
            var target = new Vector<ulong>[radk.Length];
            for (int i = 0; i < kanjiCount; ++i)
            {
                for (int j = 0; j < elementSize; ++j)
                {
                    target[i * elementSize + j] = radk[i * elementSize + j] & key[j];
                }
            }

            var possible = new Vector<ulong>[elementSize].AsSpan();
            for (int i = 0; i < kanjiCount; ++i)
            {
                bool isPresent = true;
                for (int j = 0; j < elementSize; ++j)
                {
                    if (target[i * elementSize + j] != key[j])
                        isPresent = false;
                }

                if (isPresent)
                {
                    result.Add(CodePoint.FromInt(indexToKanji[s][i]));
                    for (int j = 0; j < elementSize; ++j)
                    {
                        possible[j] |= radk[i * elementSize + j];
                    }
                }
            }

            var possibleUlong = AsScalarSpan(possible);
            for (int radicalIndex = 0; radicalIndex < radicalCount; ++radicalIndex)
            {
                var mask = (ulong)(1UL << radicalIndex);
                bool isPresent = (possibleUlong[radicalIndex / ulongBitCount] & mask) == mask;
                possibleRadicals[radicalIndex] = new KeyValuePair<CodePoint, bool>(CodePoint.FromInt(indexToRadical[radicalIndex]), isPresent);
            }
            return new Result(result, possibleRadicals);
        }

        public IEnumerable<CodePoint> AllKanji => indexToKanji[0].Select(CodePoint.FromInt);

        public IEnumerable<CodePoint> AllRadicals => indexToRadical.Select(CodePoint.FromInt);

        private readonly Vector<ulong>[][] radkinfo;
        private readonly int[][] indexToKanji;
        private readonly int[] indexToRadical;
        private readonly int radicalCount;
        private readonly int elementSize;
        private readonly int kanjiCount;
        private readonly IReadOnlyDictionary<int, int> radicalToIndex;

        private class KanjiOrdering : IKanjiOrdering
        {
            public string Description { get; }

            private IComparer<CodePoint> comparer;

            public int Compare(CodePoint x, CodePoint y)
            {
                return comparer.Compare(x, y);
            }

            public override string ToString()
            {
                return Description;
            }

            private KanjiOrdering(string description)
            {
                Description = description;
            }

            public static KanjiOrdering Create<T>(string description, KanjiDict kanjiDict, Func<KanjiEntry, T> f)
            {
                var k = new KanjiOrdering(description)
                {
                    comparer = Comparer<CodePoint>.Create((l, r) =>
                    {
                        var left = kanjiDict.Lookup(l.ToString()).Map(f);
                        var right = kanjiDict.Lookup(r.ToString()).Map(f);
                        return left.CompareTo(right);
                    })
                };
                return k;
            }
        }
    }
}

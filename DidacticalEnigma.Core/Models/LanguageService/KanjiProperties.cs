using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class KanjiProperties : IKanjiProperties
    {
        private readonly KanjiDict kanjidict;

        private readonly RadicalRemapper remapper;

        private readonly Radkfile radkfile;

        private readonly Kradfile kradfile;

        public KanjiProperties(
            KanjiDict kanjiDict,
            Kradfile kradfile,
            Radkfile radkfile,
            RadicalRemapper remapper)
        {
            this.kanjidict = kanjiDict;
            this.kradfile = kradfile;
            this.radkfile = radkfile;
            this.remapper = remapper;
            KanjiOrderings =  new ObservableBatchCollection<KanjiOrdering>
            {
                KanjiOrdering.Create("Sort by stroke count", kanjiDict, x => x.StrokeCount),
                KanjiOrdering.Create("Sort by frequency", kanjiDict, x => x.FrequencyRating)
            };
        }

        public Option<IEnumerable<CodePoint>> LookupRadicalsByKanji(Kanji kanji)
        {
            if (remapper == null)
            {
                return kradfile.LookupRadicals(kanji.ToString())
                    .Map(radicals => radicals.Select(cp => CodePoint.FromString(cp)));
            }

            return remapper
                .LookupRadicals(kanji.ToString())
                .Map(radicals => radicals.Select(cp => CodePoint.FromString(cp)));
        }

        public IEnumerable<CodePoint> LookupKanjiByRadicals(IEnumerable<CodePoint> radicals, IKanjiOrdering ordering)
        {
            if (remapper == null)
            {
                radkfile
                    .LookupMatching(radicals.Select(s => s.ToString()))
                    .Select(r => CodePoint.FromString(r))
                    .OrderBy(x => x, ordering)
                    .ToList();
            }

            return remapper
                .LookupKanji(radicals.Select(s => s.ToString()))
                .Select(r => CodePoint.FromString(r))
                .OrderBy(x => x, ordering)
                .ToList();
        }

        public IEnumerable<JDict.Radical> Radicals
        {
            get
            {
                if (remapper == null)
                    return radkfile.Radicals;

                return remapper.Radicals;
            }
        }

        public IEnumerable<IKanjiOrdering> KanjiOrderings { get; }

        public IEqualityComparer<string> RadicalComparer => remapper.Comparer;

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

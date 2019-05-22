using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Xam.Models
{
    public class MockKanjiProperties : IKanjiProperties
    {
        public Option<IEnumerable<CodePoint>> LookupRadicalsByKanji(Kanji kanji)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IKanjiOrdering> KanjiOrderings { get; }
        public IEnumerable<JDict.Radical> Radicals { get; }
        public IEqualityComparer<string> RadicalComparer { get; }

        public IEnumerable<CodePoint> LookupKanjiByRadicals(IEnumerable<CodePoint> radicals, IKanjiOrdering ordering)
        {
            throw new NotImplementedException();
        }
    }

    public class MockKanjiRadicalLookup : IKanjiRadicalLookup
    {
        public ReadOnlyListWithSelector<IKanjiOrdering> SortingCriteria { get; } = new ReadOnlyListWithSelector<IKanjiOrdering>(new[]
        {
            new KanjiOrdering("huh")
        });

        public class KanjiOrdering : IKanjiOrdering
        {
            public KanjiOrdering(string description)
            {
                Description = description;
            }

            public string Description { get; }
            public int Compare(CodePoint x, CodePoint y)
            {
                return x.Utf32.CompareTo(y.Utf32);
            }
        }

        public IEnumerable<CodePoint> AllKanji => new[]
            {CodePoint.FromInt('a'), CodePoint.FromInt('b'), CodePoint.FromInt('c') };
        public IEnumerable<CodePoint> AllRadicals => new[] { CodePoint.FromInt('1'), CodePoint.FromInt('2') };
        public KanjiRadicalLookup.Result SelectRadical(IEnumerable<CodePoint> radicals)
        {
            return new KanjiRadicalLookup.Result(new[] { CodePoint.FromInt('a') }, new Dictionary<CodePoint, bool> { }.ToList());
        }
    }

    public class MockRadicalSearcher : IRadicalSearcher
    {
        public IReadOnlyList<RadicalSearcherResult> Search(string text)
        {
            return new List<RadicalSearcherResult>();
        }
    }
}

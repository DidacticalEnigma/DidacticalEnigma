using System.Collections.Generic;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IKanjiRadicalLookup
    {
        ReadOnlyListWithSelector<IKanjiOrdering> SortingCriteria { get; }
        IEnumerable<CodePoint> AllKanji { get; }
        IEnumerable<CodePoint> AllRadicals { get; }
        KanjiRadicalLookup.Result SelectRadical(IEnumerable<CodePoint> radicals);
        KanjiRadicalLookup.Result SelectRadical(IEnumerable<CodePoint> radicals, int sortingCriteriaIndex);
    }
}
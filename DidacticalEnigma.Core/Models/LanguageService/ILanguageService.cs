using System.Collections.Generic;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IKanjiProperties
    {
        Option<IEnumerable<CodePoint>> LookupRadicalsByKanji(Kanji kanji);

        IEnumerable<IKanjiOrdering> KanjiOrderings { get; }

        IEnumerable<CodePoint> LookupKanjiByRadicals(IEnumerable<CodePoint> radicals, IKanjiOrdering ordering);

        IEnumerable<JDict.Radical> Radicals { get; }

        IEqualityComparer<string> RadicalComparer { get; }
    }

    public interface IKanjiOrdering : IComparer<CodePoint>
    {
        string Description { get; }
    }
}

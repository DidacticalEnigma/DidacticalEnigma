using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.Xam.Services
{
    public class KanjiRadicalLookupControlVM
    {
        private readonly IKanjiRadicalLookup kanjiRadicalLookup;
        private readonly IKanjiProperties kanjiProperties;
        private readonly IRadicalSearcher radicalSearcher;
        private readonly IReadOnlyDictionary<CodePoint, string> radicalMappings;

        public KanjiRadicalLookupControlVM(IKanjiRadicalLookup kanjiRadicalLookup, IKanjiProperties kanjiProperties, IRadicalSearcher radicalSearcher, IReadOnlyDictionary<CodePoint, string> radicalMappings)
        {
            this.kanjiRadicalLookup = kanjiRadicalLookup;
            this.kanjiProperties = kanjiProperties;
            this.radicalSearcher = radicalSearcher;
            this.radicalMappings = radicalMappings;
            Radicals = new ObservableBatchCollection<CodePoint>(kanjiRadicalLookup.AllRadicals);
        }

        public ObservableBatchCollection<CodePoint> Radicals { get; }
    }
}
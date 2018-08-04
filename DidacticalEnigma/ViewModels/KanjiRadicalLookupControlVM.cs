using DidacticalEnigma.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.ViewModels
{
    public class KanjiRadicalLookupControlVM
    {
        private readonly ILanguageService service;

        public void SelectRadicals(IEnumerable<CodePoint> codePoints)
        {
            var lookup = service.LookupByRadicals(codePoints);
            Kanji.Clear();
            Kanji.AddRange(lookup);
        }

        public ObservableBatchCollection<CodePoint> Kanji { get; } = new ObservableBatchCollection<CodePoint>();

        public ObservableBatchCollection<CodePoint> Radicals { get; } = new ObservableBatchCollection<CodePoint>();

        public KanjiRadicalLookupControlVM(ILanguageService service)
        {
            this.service = service;
            Radicals.AddRange(service.AllRadicals());
        }
    }
}

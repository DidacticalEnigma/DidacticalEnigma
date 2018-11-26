using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DidacticalEnigma.Core.Models.LanguageService;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{

    public class WordVM : INotifyPropertyChanged
    {
        public WordInfo WordInfo { get; }

        public string StringForm { get; }

        private readonly ObservableBatchCollection<CodePointVM> codePoints = new ObservableBatchCollection<CodePointVM>();
        public IEnumerable<CodePointVM> CodePoints => codePoints;

        public Func<IEnumerable<WordVM>> SubsequentWords { get; set; }

        public WordVM(WordInfo wordInfo, IKanjiProperties lang, IKanaProperties kanaProperties, IRelated related)
        {
            var s = wordInfo.RawWord;
            StringForm = s;
            codePoints.AddRange(s.AsCodePoints().Select(rawCp =>
            {
                var cp = CodePoint.FromInt(rawCp);
                var vm = new CodePointVM(
                    cp,
                    related.FindRelated(cp).SelectMany(g => g),
                    cp is Kanji k
                        ? lang.LookupRadicalsByKanji(k).ValueOr(Enumerable.Empty<CodePoint>())
                        : Enumerable.Empty<CodePoint>(),
                    cp is Kana kana ? kanaProperties.LookupRomaji(kana.ToString()) : null);
                return vm;
            }));
            WordInfo = wordInfo;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

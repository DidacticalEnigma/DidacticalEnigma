using JDict;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DidacticalEnigma.Models
{

    public class WordVM : INotifyPropertyChanged
    {
        public string DictionaryStringBlurb
        {
            get
            {
                var a = lang.LookupWord(StringForm).DictionaryDefinition;
                return a;
            }
        }

        private string stringForm;
        private readonly ILanguageService lang;

        public string StringForm
        {
            get => stringForm;
            set
            {
                if (stringForm == value)
                    return;
                stringForm = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DictionaryStringBlurb));
            }
        }

        private readonly ObservableBatchCollection<CodePointVM> codePoints = new ObservableBatchCollection<CodePointVM>();
        public IEnumerable<CodePointVM> CodePoints => codePoints;

        public WordVM(string s, ILanguageService lang)
        {
            StringForm = s;
            codePoints.AddRange(s.AsCodePoints().Select(rawCp =>
            {
                var cp = CodePoint.FromInt(rawCp);
                var vm = new CodePointVM(
                    cp,
                    lang.LookupRelatedCharacters(cp),
                    cp is Kanji k ? lang.LookupRadicals(k) : Enumerable.Empty<CodePoint>());
                return vm;
            }));
            this.lang = lang;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

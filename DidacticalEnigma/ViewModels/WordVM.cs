using JDict;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DidacticalEnigma.Models
{
    public class LineVM
    {
        public ObservableBatchCollection<WordVM> Words { get; }

        public LineVM(IEnumerable<WordVM> words)
        {
            Words = new ObservableBatchCollection<WordVM>(words);
        }
    }

    public class WordVM : INotifyPropertyChanged
    {
        public string DictionaryStringBlurb => lang.LookupWord(StringForm).DictionaryDefinition;

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

        public WordVM(string s, ILanguageService lang)
        {
            StringForm = s;
            this.lang = lang;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using DidacticalEnigma.Core.Models.Project;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    class TranslationVM : INotifyPropertyChanged
    {
        private ITranslation model;

        private string originalText = "";
        public string OriginalText
        {
            get => originalText;
            set
            {
                if (originalText == value)
                    return;

                originalText = value;
                OnPropertyChanged();
            }
        }

        private string translatedText;
        public string TranslatedText
        {
            get => translatedText;
            set
            {
                if(translatedText == value)
                    return;

                translatedText = value;
                OnPropertyChanged();
            }
        }

        public ObservableBatchCollection<GlossNoteVM> Glosses { get; } = new ObservableBatchCollection<GlossNoteVM>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class GlossNoteVM
    {

    }
}

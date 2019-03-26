using System.ComponentModel;
using System.Runtime.CompilerServices;
using DidacticalEnigma.Core.Models.Project;
using Utility.Utils;

namespace DidacticalEnigma.ViewModels
{
    class TranslationVM : INotifyPropertyChanged
    {
        private Translation model;

        public string OriginalText
        {
            get => model.OriginalText;
            set
            {
                if (model.OriginalText == value)
                    return;

                model.OriginalText = value;
                OnPropertyChanged();
            }
        }

        public string TranslatedText
        {
            get => model.TranslatedText;
            set
            {
                if(model.TranslatedText == value)
                    return;

                model.TranslatedText = value;
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

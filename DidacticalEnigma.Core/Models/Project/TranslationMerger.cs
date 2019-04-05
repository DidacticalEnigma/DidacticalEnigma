using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslationMerger
    {
        public TranslationEditor Editor { get; } = new TranslationEditor();

        public TranslationMerger()
        {
            
        }
    }

    public class TranslationEditor : INotifyPropertyChanged
    {
        private string originalText = "";
        public string OriginalText
        {
            get => originalText;
            set
            {
                if (value.Equals(originalText)) return;
                originalText = value;
                OnPropertyChanged();
            }
        }

        private string translatedText = "";
        public string TranslatedText
        {
            get => translatedText;
            set
            {
                if (value.Equals(translatedText)) return;
                translatedText = value;
                OnPropertyChanged();
            }
        }

        public IList<GlossNote> Glosses => glosses;

        public IList<TranslatorNote> Notes => notes;

        public IList<TranslatedText> AlternativeTranslations => alternativeTranslations;

        private Option<ITranslation> modifiedTranslation;
        
        private readonly ObservableBatchCollection<TranslatorNote> notes = new ObservableBatchCollection<TranslatorNote>();
        private readonly ObservableBatchCollection<GlossNote> glosses = new ObservableBatchCollection<GlossNote>();
        private readonly ObservableBatchCollection<TranslatedText> alternativeTranslations = new ObservableBatchCollection<TranslatedText>();

        public Option<ITranslation> ModifiedTranslation
        {
            get => modifiedTranslation;
            set
            {
                if (Equals(value, modifiedTranslation))
                    return;
                modifiedTranslation = value;

                OriginalText = modifiedTranslation
                    .Map(t => t.OriginalText)
                    .ValueOr("");
                TranslatedText = modifiedTranslation
                    .Map(t => t.TranslatedText)
                    .ValueOr("");
                glosses.Clear();
                glosses.AddRange(modifiedTranslation
                    .Map(t => t.Glosses)
                    .ValueOr(Enumerable.Empty<GlossNote>()));
                alternativeTranslations.Clear();
                alternativeTranslations.AddRange(modifiedTranslation
                    .Map(t => t.AlternativeTranslations)
                    .ValueOr(Enumerable.Empty<TranslatedText>()));
                notes.Clear();
                notes.AddRange(modifiedTranslation
                    .Map(t => t.Notes)
                    .ValueOr(Enumerable.Empty<TranslatorNote>()));
                OnPropertyChanged();
            }
        }

        public TranslationEditor()
        {

        }

        public Option<ITranslation> Create()
        {
            return ModifiedTranslation.Map(translation =>
                translation.With(originalText, translatedText, Glosses, Notes, AlternativeTranslations));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

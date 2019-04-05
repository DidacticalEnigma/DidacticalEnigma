using System;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Core.Models.Project
{
    public class TranslationChangedEventArgs : EventArgs
    {
        public ITranslationContext Context { get; }

        public Translation Translation { get; }

        public TranslationChangedReason Reason { get; }

        public TranslationChangedEventArgs(ITranslationContext context, Translation translation, TranslationChangedReason reason)
        {
            Context = context;
            Translation = translation;
            Reason = reason;
        }
    }

    public enum TranslationChangedReason
    {
        // there was no change to the translation
        // or the project is unable to determine if
        // the translation was modified
        Unknown,
        // the translation with this id got modified
        InPlaceModification,
        // a new translation was created
        New,
        // this translation was removed
        Removed
    }
}
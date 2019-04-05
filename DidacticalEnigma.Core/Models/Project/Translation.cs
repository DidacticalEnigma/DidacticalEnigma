using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace DidacticalEnigma.Core.Models.Project
{
    public abstract class Translation
    {
        public abstract Option<Guid> Guid { get; }
        public abstract string OriginalText { get; }
        public abstract string TranslatedText { get; }
        public abstract IEnumerable<GlossNote> Glosses { get; }
        public abstract IEnumerable<TranslatorNote> Notes { get; }
        public abstract IEnumerable<TranslatedText> AlternativeTranslations { get; }

        public abstract Translation With(
            string originalText = null,
            string translatedText = null,
            IEnumerable<GlossNote> glosses = null,
            IEnumerable<TranslatorNote> notes = null,
            IEnumerable<TranslatedText> alternativeTranslations = null);
    }
}
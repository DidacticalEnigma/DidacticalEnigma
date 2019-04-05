using System;
using System.Collections.Generic;
using System.Linq;
using Optional;

namespace DidacticalEnigma.Core.Models.Project
{
    public interface ITranslation
    {
        Option<Guid> Guid { get; }
        string OriginalText { get; }
        string TranslatedText { get; }
        IEnumerable<GlossNote> Glosses { get; }
        IEnumerable<TranslatorNote> Notes { get; }
        IEnumerable<TranslatedText> AlternativeTranslations { get; }

        ITranslation With(
            string originalText = null,
            string translatedText = null,
            IEnumerable<GlossNote> glosses = null,
            IEnumerable<TranslatorNote> notes = null,
            IEnumerable<TranslatedText> alternativeTranslations = null,
            Option<Option<Guid>> guid = default);
    }
}
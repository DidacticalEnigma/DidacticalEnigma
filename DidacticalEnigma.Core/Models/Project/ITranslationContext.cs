using System.Collections.Generic;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.Core.Models.Project
{
    public interface ITranslationContext
    {
        bool IsAddSupported(ITranslation translation);

        IEnumerable<ITranslationContext> Children { get; }

        ModificationResult Modify(ITranslation translation);

        IEnumerable<ITranslation> Translations { get; }

        void Add(ITranslation translation);

        RichFormatting Render();

        RichFormatting Render(ITranslation translation);
    }

    public interface ITranslationContext<out TContext> : ITranslationContext
        where TContext : ITranslationContext
    {
        new IEnumerable<TContext> Children { get; }
    }
}
using System;
using System.Collections.Generic;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.Core.Models.Project
{
    public interface ITranslationContext
    {
        IEnumerable<ITranslationContext> Children { get; }
    }

    public interface ITranslationContext<out TContext> : ITranslationContext
        where TContext : ITranslationContext
    {
        new IEnumerable<TContext> Children { get; }
    }

    public interface IModifiableTranslationContext : ITranslationContext
    {
        new IReadOnlyList<ITranslationContext> Children { get; }

        ITranslationContext AppendEmpty();

        bool Remove(Guid guid);

        void Reorder(Guid translationId, Guid moveAt);
    }

    public interface IModifiableTranslationContext<out TContext> :
        ITranslationContext<TContext>,
        IModifiableTranslationContext
        where TContext : ITranslationContext
    {
        new IReadOnlyList<TContext> Children { get; }

        new TContext AppendEmpty();
    }

    public interface IEditableTranslation : ITranslationContext
    {
        Translation Translation { get; }

        ModificationResult Modify(Translation translation);
    }

    public interface IEditableTranslation<out TContext> : ITranslationContext<TContext>, IEditableTranslation
        where TContext : ITranslationContext
    {
        
    }
}
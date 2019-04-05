using System;

namespace DidacticalEnigma.Core.Models.Project
{
    public interface IProject : IDisposable
    {
        ITranslationContext Root { get; }

        void Refresh();

        event EventHandler<TranslationChangedEventArgs> TranslationChanged;
    }
}
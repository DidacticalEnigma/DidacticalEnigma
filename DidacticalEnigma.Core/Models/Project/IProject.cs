using System;
using DidacticalEnigma.Core.Models.Formatting;

namespace DidacticalEnigma.Core.Models.Project
{
    public interface IProject : IDisposable
    {
        ITranslationContext Root { get; }

        void Refresh(bool fullRefresh = false);

        event EventHandler<TranslationChangedEventArgs> TranslationChanged;

        RichFormatting Render(ITranslationContext context, RenderingVerbosity verbosity);
    }

    public enum RenderingVerbosity
    {
        Minimal = 10000,
        Normal = 50000,
        Full = 1000000
    }

    public static class RenderingVerbosityExt
    {
        public static bool IsAtLeast(this RenderingVerbosity @checked, RenderingVerbosity required)
        {
            return (int) @checked >= (int) required;
        }
    }
}
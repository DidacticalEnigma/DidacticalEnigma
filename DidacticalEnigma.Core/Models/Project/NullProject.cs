using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DidacticalEnigma.Core.Models.Formatting;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.Project
{
    public class NullProject : IProject
    {
        public static NullProject Instance { get; } = new NullProject();

        public void Dispose()
        {
            // do nothing
        }

        public ITranslationContext Root { get; } = new Context();

        public void Refresh(bool fullRefresh = false)
        {
            // do nothing
        }

        public event EventHandler<TranslationChangedEventArgs> TranslationChanged;

        public RichFormatting Render(ITranslationContext context, RenderingVerbosity verbosity)
        {
            throw new ArgumentException(nameof(context));
        }

        private class Context : ITranslationContext
        {
            public IEnumerable<ITranslationContext> Children => Enumerable.Empty<ITranslationContext>();
        }
    }
}

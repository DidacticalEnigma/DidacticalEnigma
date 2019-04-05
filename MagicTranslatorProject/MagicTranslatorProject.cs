using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using Optional;

namespace MagicTranslatorProject
{
    public class MagicTranslatorProject : IProject
    {
        private class MagicTranslatorProjectRegistration : ProjectFormatHandlerRegistration
        {
            public MagicTranslatorProjectRegistration() :
                base("Magic Translator", "*", true)
            {
            }

            public override bool IsValid(string path)
            {
                // TODO: actually check
                return true;
            }

            public override IProject Open(string path)
            {
                return new MagicTranslatorProject(path);
            }
        }

        public static ProjectFormatHandlerRegistration Registration { get; } = new MagicTranslatorProjectRegistration();

        public void Dispose()
        {
            
        }

        ITranslationContext IProject.Root => Root;

        public MangaContext Root { get; }

        public void Refresh(bool fullRefresh = false)
        {
            
        }

        public event EventHandler<TranslationChangedEventArgs> TranslationChanged;

        public RichFormatting Render(ITranslationContext context, RenderingVerbosity verbosity)
        {
            switch (context)
            {
                case MangaContext m:
                    break;
                case VolumeContext v:
                    break;
                case ChapterContext c:
                    break;
                case PageContext p:
                    break;
                case CaptureContext cap:
                    break;
            }
            return new RichFormatting();
        }

        public MagicTranslatorProject(string path)
        {
            if(!Registration.IsValid(path))
                throw new ArgumentException(nameof(path));

            Root = new MangaContext(path);
        }
    }
}
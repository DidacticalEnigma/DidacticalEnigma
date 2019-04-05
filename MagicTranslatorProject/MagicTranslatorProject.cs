using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Project;

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

        public void Refresh()
        {
            
        }

        public event EventHandler<TranslationChangedEventArgs> TranslationChanged;

        private static readonly Regex volumeNumberMatcher = new Regex("^vol([0-9]{2})$");

        public MagicTranslatorProject(string path)
        {
            if(!Registration.IsValid(path))
                throw new ArgumentException(nameof(path));

            var dirs = new DirectoryInfo(path)
                .EnumerateDirectories();

            var chapterNumbers = dirs
                .OrderBy(dir => dir.Name)
                .Select(dir => volumeNumberMatcher.Match(dir.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value));

            Root = new MangaContext(path, chapterNumbers);
        }
    }
}
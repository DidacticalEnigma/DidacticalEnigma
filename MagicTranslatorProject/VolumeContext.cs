using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class VolumeContext : ITranslationContext<ChapterContext>
    {
        private static readonly Regex pageNumberMatcher = new Regex("^([0-9]{4}).png$");

        internal VolumeContext(MangaContext root, int volumeNumber, IEnumerable<int> chapterNumbers)
        {
            Children = chapterNumbers.Select(ch =>
            {
                var pageNumbers = new DirectoryInfo(Path.Combine(root.RootPath, $"vol{volumeNumber:D2}", $"ch{ch:D3}", "raw"))
                    .EnumerateFiles()
                    .OrderBy(f => f.Name)
                    .Select(f => pageNumberMatcher.Match(f.Name))
                    .Where(match => match.Success)
                    .Select(match => int.Parse(match.Groups[1].Value));
                return new ChapterContext(root, volumeNumber, ch, pageNumbers);
            });
        }

        public bool IsAddSupported(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return false;
        }

        public IEnumerable<ChapterContext> Children { get; }

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return ModificationResult.WithUnsupported("translations are only available on page level");
        }

        public IEnumerable<DidacticalEnigma.Core.Models.Project.ITranslation> Translations => Enumerable.Empty<DidacticalEnigma.Core.Models.Project.ITranslation>();

        public void Add(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            throw new InvalidOperationException();
        }

        public RichFormatting Render()
        {
            return new RichFormatting(new Paragraph[]
            {
                new TextParagraph(EnumerableExt.OfSingle(new Text("Manga Name (chapter count: 65)")))
            });
        }

        public RichFormatting Render(ITranslation translation)
        {
            throw new InvalidOperationException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class ChapterContext : ITranslationContext<PageContext>
    {
        internal ChapterContext(MangaContext root, int volumeNumber, int chapterNumber, IEnumerable<int> pageNumbers)
        {
            Children = pageNumbers
                .Select(page => new PageContext(root, volumeNumber, chapterNumber, page))
                .ToList();
        }

        public bool IsAddSupported(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return false;
        }

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;

        public IEnumerable<PageContext> Children { get; }

        public IEnumerable<DidacticalEnigma.Core.Models.Project.ITranslation> Translations => Enumerable.Empty<DidacticalEnigma.Core.Models.Project.ITranslation>();

        public void Add(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            throw new InvalidOperationException();
        }

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return ModificationResult.WithUnsupported("translations are only available on page level");
        }

        public RichFormatting Render()
        {
            return new RichFormatting(new Paragraph[]
            {
                new TextParagraph(EnumerableExt.OfSingle(new Text("Chapter 23"))),
                new ImageParagraph(), 
            });
        }

        public RichFormatting Render(ITranslation translation)
        {
            throw new InvalidOperationException();
        }
    }
}
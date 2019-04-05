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
    public class ChapterContext : ITranslationContext<PageContext>
    {
        private static readonly Regex pageNumberMatcher = new Regex("^([0-9]{4}).png$");
        private readonly int chapterNumber;
        private readonly MangaContext root;
        private readonly int volumeNumber;

        internal ChapterContext(MangaContext root, int volumeNumber, int chapterNumber)
        {
            this.root = root;
            this.volumeNumber = volumeNumber;
            this.chapterNumber = chapterNumber;
        }

        private IReadOnlyCollection<PageContext> Load()
        {
            return new DirectoryInfo(Path.Combine(
                    root.RootPath,
                    $"vol{volumeNumber:D2}",
                    $"ch{chapterNumber:D3}",
                    "raw"))
                .EnumerateFiles()
                .OrderBy(f => f.Name)
                .Select(f => pageNumberMatcher.Match(f.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .Select(page => new PageContext(root, volumeNumber, chapterNumber, page))
                .ToList();
        }

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;
        public RichFormatting Render(RenderingVerbosity verbosity)
        {
            throw new NotImplementedException();
        }

        private readonly WeakReference<IReadOnlyCollection<PageContext>> children = new WeakReference<IReadOnlyCollection<PageContext>>(null);

        public IEnumerable<PageContext> Children
        {
            get
            {
                if (children.TryGetTarget(out var v))
                {
                    return v;
                }
                else
                {
                    v = Load();
                    children.SetTarget(v);
                    return v;
                }
            }
        }

        public RichFormatting Render()
        {
            return new RichFormatting(new Paragraph[]
            {
                new TextParagraph(EnumerableExt.OfSingle(new Text("Chapter 23"))),
                new ImageParagraph(), 
            });
        }
    }
}
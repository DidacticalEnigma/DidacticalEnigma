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
        private static readonly Regex chapterNumberMatcher = new Regex("^ch([0-9]{3})$");
        private MangaContext root;
        private int volumeNumber;

        internal VolumeContext(MangaContext root, int volumeNumber)
        {
            this.volumeNumber = volumeNumber;
            this.root = root;
        }

        private IReadOnlyCollection<ChapterContext> Load()
        {
            return new DirectoryInfo(Path.Combine(root.RootPath, $"vol{volumeNumber:D2}"))
                .EnumerateDirectories()
                .Select(dir => chapterNumberMatcher.Match(dir.Name))
                .Where(match => match.Success)
                .Select(match => int.Parse(match.Groups[1].Value))
                .OrderBy(ch => ch)
                .Select(ch =>
                {
                    return new ChapterContext(root, volumeNumber, ch);
                })
                .ToList();
        }

        private readonly WeakReference<IReadOnlyCollection<ChapterContext>> children = new WeakReference<IReadOnlyCollection<ChapterContext>>(null);

        public IEnumerable<ChapterContext> Children
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

        IEnumerable<ITranslationContext> ITranslationContext.Children => Children;
    }
}
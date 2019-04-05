using System.Collections.Generic;
using System.IO;
using System.Linq;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using Newtonsoft.Json;
using Optional;
using Utility;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class PageContext : ITranslationContext<CaptureContext>
    {
        IEnumerable<ITranslationContext> ITranslationContext.Children => captures;

        public RichFormatting Render()
        {
            return new RichFormatting(new Paragraph[]
            {
                new TextParagraph(EnumerableExt.OfSingle(new Text($"Volume {volumeNumber}, Chapter {chapterNumber}, Page {pageNumber}"))),
                new ImageParagraph(),
            });
        }

        internal PageContext(MangaContext root, int volumeNumber, int chapterNumber, int pageNumber)
        {
            this.volumeNumber = volumeNumber;
            this.chapterNumber = chapterNumber;
            this.pageNumber = pageNumber;
            pageJson = JsonConvert.DeserializeObject<PageJson>(File.ReadAllText(Path.Combine(
                root.RootPath,
                $"vol{volumeNumber:D2}",
                $"ch{chapterNumber:D3}",
                "capture",
                $"{pageNumber:D4}.json")), new CharacterTypeConverter(root.IdNameMapping));
            captures = pageJson.Captures
                .Select((c, i) =>
                {
                    var guid = root.Map(volumeNumber, chapterNumber, pageNumber, c.Id).Some();
                    return new CaptureContext(c, j =>
                    {
                        pageJson.Captures[i] = j;
                        return ModificationResult.WithSuccess(new Translation(c, guid));
                    }, new Translation(c, guid));
                })
                .ToList();
        }

        private readonly PageJson pageJson;

        private readonly int pageNumber;

        private readonly int chapterNumber;

        private readonly int volumeNumber;

        private readonly IList<CaptureContext> captures;

        public IEnumerable<CaptureContext> Children => captures;
    }
}

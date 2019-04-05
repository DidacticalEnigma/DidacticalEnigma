using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.Project;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optional;
using Utility;
using Utility.Utils;

namespace MagicTranslatorProject
{
    public class PageContext : ITranslationContext
    {
        public bool IsAddSupported(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            return false;
        }

        public IEnumerable<ITranslationContext> Children => Enumerable.Empty<ITranslationContext>();

        public IEnumerable<DidacticalEnigma.Core.Models.Project.ITranslation> Translations => translations;

        public void Add(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            throw new InvalidOperationException();
        }

        public ModificationResult Modify(DidacticalEnigma.Core.Models.Project.ITranslation translation)
        {
            // TODO
            return ModificationResult.WithUnsupported("TODO");
        }

        public RichFormatting Render()
        {
            return new RichFormatting(new Paragraph[]
            {
                new TextParagraph(EnumerableExt.OfSingle(new Text($"Volume {volumeNumber}, Chapter {chapterNumber}, Page {pageNumber}"))),
                new ImageParagraph(),
            });
        }

        public RichFormatting Render(ITranslation translation)
        {
            if (translation is Translation t)
            {
                return new RichFormatting(new Paragraph[]
                {
                    new TextParagraph(
                        EnumerableExt.OfSingle(new Text($"Character: {t.Character}"))),
                    new ImageParagraph(),
                });
            }
            else
            {
                throw new ArgumentException(nameof(translation));
            }
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
            translations = pageJson.Captures
                .Select(c => new Translation(c, Guid.NewGuid().Some()))
                .ToList();
        }

        private readonly PageJson pageJson;

        private readonly List<Translation> translations;

        private int pageNumber;

        private int chapterNumber;

        private int volumeNumber;
    }

    internal class PageJson
    {
        [JsonProperty("captureId")]
        public int CaptureId { get; set; }

        [JsonProperty("captures")]
        public IEnumerable<CaptureJson> Captures { get; set; }
    }

    internal class CaptureJson
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("translation")]
        public string Translation { get; set; }

        [JsonProperty("notes")]
        public IEnumerable<IReadOnlyList<string>> Notes { get; set; }

        [JsonProperty("character")]
        public CharacterType Character { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [UsedImplicitly]
        [JsonExtensionData]
        private readonly Dictionary<string, JToken> extensionData = new Dictionary<string, JToken>();
    }

    internal class Translation : ITranslation
    {
        private readonly CaptureJson capture;

        public long Id => capture.Id;

        public string Character => capture.Character.ToString();

        public Option<Guid> Guid { get; }

        public string OriginalText => capture.Text;

        public string TranslatedText => capture.Translation;

        public IEnumerable<GlossNote> Glosses => capture.Notes.Select(c => new GlossNote(c[0], c[1]));

        public IEnumerable<TranslatorNote> Notes => Enumerable.Empty<TranslatorNote>();

        public IEnumerable<TranslatedText> AlternativeTranslations => Enumerable.Empty<TranslatedText>();

        public Translation(
            CaptureJson capture,
            Option<Guid> guid = default)
        {
            Guid = guid;
            this.capture = capture ?? throw new ArgumentNullException(nameof(capture));
        }

        public ITranslation With(
            string originalText = null,
            string translatedText = null,
            IEnumerable<GlossNote> glosses = null,
            IEnumerable<TranslatorNote> notes = null,
            IEnumerable<TranslatedText> alternativeTranslations = null,
            Option<Option<Guid>> guid = default)
        {
            var other = new Translation(JsonConvert.DeserializeObject<CaptureJson>(ToJson()), guid.ValueOr(this.Guid));
            other.capture.Text = originalText ?? this.OriginalText;
            other.capture.Translation = translatedText ?? this.TranslatedText;
            other.capture.Notes = (glosses ?? this.Glosses).Select(g => new[] {g.Foreign, g.Text}).ToList();
            return other;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this.capture);
        }
    }
}

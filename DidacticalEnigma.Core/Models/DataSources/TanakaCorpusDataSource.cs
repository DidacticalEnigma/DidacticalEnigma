using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using JDict;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class TanakaCorpusDataSource : IDataSource
    {
        private Tanaka tanaka;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("E034C3A3-FF93-40EA-A77C-EB071F53CE48"),
            "Tanaka Corpus",
            "These sentences are from Tanaka Corpus",
            new Uri("http://www.edrdg.org/wiki/index.php/Tanaka_Corpus"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var rich = new RichFormatting();
                var sentences = tanaka.SearchByJapaneseText(request.QueryText);
                foreach (var sentence in sentences.Take(100).OrderBy(s => s.JapaneseSentence.Length).Take(20))
                {
                    var paragraph = new TextParagraph();
                    foreach (var part in StringExt.HighlightWords(sentence.JapaneseSentence, request.QueryText))
                    {
                        paragraph.Content.Add(new Text(part.text, part.highlight));
                    }
                    paragraph.Content.Add(new Text(sentence.EnglishSentence));
                    rich.Paragraphs.Add(paragraph);
                };
                if (rich.Paragraphs.Count != 0)
                {
                    await yield.ReturnAsync(rich).ConfigureAwait(false);
                }
            });
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public TanakaCorpusDataSource(string dataDirectory)
        {
            this.tanaka = new Tanaka(Path.Combine(dataDirectory, "examples.utf"), Encoding.UTF8);
        }
    }

    public class AutoGlosserDataSource : IDataSource
    {
        private readonly AutoGlosser autoglosser;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("76EAA6CD-2FD7-4691-9B07-E9FE907F89E9"),
            "AutoGlosser",
            "NOTE: This functionality is completely untested and may result in horribly broken glosses",
            null);

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var rich = new RichFormatting();
                var glosses = autoglosser.Gloss(request.AllText());
                rich.Paragraphs.Add(new TextParagraph(EnumerableExt.OfSingle(new Text(Descriptor.AcknowledgementText))));
                var s = string.Join("\n", glosses.Select(gloss => $"- {gloss}"));
                rich.Paragraphs.Add(new TextParagraph(EnumerableExt.OfSingle(new Text(s))));
                await yield.ReturnAsync(rich).ConfigureAwait(false);
            });
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public AutoGlosserDataSource(ILanguageService lang, JMDict jdict)
        {
            this.autoglosser = new AutoGlosser(lang, jdict);
        }
    }
}
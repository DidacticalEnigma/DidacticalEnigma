using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class PartialExpressionJMDictDataSource : IDataSource
    {
        private readonly IdiomDetector idiomDetector;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("C04648BA-68B2-467B-87C4-33DA5B9CA070"),
            "Partial JMDict expression lookup",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            var lookup = LookAhead(request);
            foreach (var l in lookup)
            {
                var textParagraph = new TextParagraph(
                        l.RenderedHighlights
                            .Select(t => new Text(t.fragment, emphasis: t.highlight)));
                textParagraph.Content.Add(new Text("\n"));
                textParagraph.Content.Add(new Text(string.Join("\n", l.DictionaryEntry.Senses
                    .Where(s => s.PartOfSpeechInfo.Contains(EdictPartOfSpeech.exp))
                    .Select(s => string.Join("/", s.Glosses)))));

                rich.Paragraphs.Add(textParagraph);
            }

            return Task.FromResult(Option.Some(rich));
        }

        private IEnumerable<IdiomDetector.Result> LookAhead(Request request, int ahead = 5)
        {
            return idiomDetector.Detect(Impl(request, ahead));

            string Impl(Request r, int a)
            {
                return string.Join("", r.SubsequentWords.Take(a));
            }
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public PartialExpressionJMDictDataSource(IdiomDetector idiomDetector)
        {
            this.idiomDetector = idiomDetector;
        }
    }
}

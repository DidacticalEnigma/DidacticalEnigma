using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class TanakaCorpusFastDataSource : IDataSource
    {
        private Corpus corpus;

        public void Dispose()
        {

        }

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("4C939423-463D-46E1-A8F4-685B1875FDFD"),
            "Tanaka Corpus (fast)",
            "These sentences are from Tanaka Corpus",
            new Uri("http://www.edrdg.org/wiki/index.php/Tanaka_Corpus"));

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
                textParagraph.Content.Add(new Text(l.Sentence.EnglishSentence));
                rich.Paragraphs.Add(textParagraph);
            }

            return Task.FromResult(Option.Some(rich));
        }

        private IEnumerable<Corpus.Result> LookAhead(Request request, int ahead = 5)
        {
            return corpus.GetSentences(Impl(request, ahead));

            string Impl(Request r, int a)
            {
                return string.Join("", r.SubsequentWords.Take(a));
            }
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public TanakaCorpusFastDataSource(Corpus corpus)
        {
            this.corpus = corpus;
        }
    }
}

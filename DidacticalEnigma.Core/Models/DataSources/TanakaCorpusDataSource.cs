using System;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;
using Optional;
using Utility.Utils;

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

        public async Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            var rich = new RichFormatting();
            var sentences = tanaka.SearchByJapaneseTextAsync(request.QueryText);
            foreach (var sentence in (await sentences.Take(100).ToListAsync(token)).OrderBy(s => s.JapaneseSentence.Length).Take(20))
            {
                var paragraph = new TextParagraph();
                foreach (var (text, highlight) in StringExt.HighlightWords(sentence.JapaneseSentence, request.QueryText))
                {
                    paragraph.Content.Add(new Text(text, highlight));
                }
                paragraph.Content.Add(new Text(sentence.EnglishSentence));
                rich.Paragraphs.Add(paragraph);
            }

            if (rich.Paragraphs.Count != 0)
            {
                return Option.Some(rich);
            }

            return Option.None<RichFormatting>();
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public TanakaCorpusDataSource(Tanaka tanaka)
        {
            this.tanaka = tanaka;
        }
    }
}
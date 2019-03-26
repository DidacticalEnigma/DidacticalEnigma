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
    public class JESCDataSource : IDataSource
    {
        private readonly JESC jesc;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("C296FA4B-6EE5-4138-A628-69F056489ACA"),
            "Japanese-English Subtitle Corpus",
            "This data sources uses sentences from JESC Corpus",
            new Uri("https://nlp.stanford.edu/projects/jesc/"));

        public void Dispose()
        {

        }

        public async Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            var sentences = jesc.SearchByJapaneseTextAsync(request.QueryText);
            foreach (var sentence in (await sentences.Take(100).ToListAsync()).OrderByDescending(s => s.JapaneseSentence.Length).Take(20))
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

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JESCDataSource(JESC jesc)
        {
            this.jesc = jesc;
        }
    }
}
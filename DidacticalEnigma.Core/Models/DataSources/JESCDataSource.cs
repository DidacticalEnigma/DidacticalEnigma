using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JDict;

namespace DidacticalEnigma.Models.DataSources
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

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var rich = new RichFormatting();
                var sentences = jesc.SearchByJapaneseText(request.QueryText);
                foreach (var sentence in sentences.Take(100).OrderByDescending(s => s.JapaneseSentence.Length).Take(20))
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

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JESCDataSource(string dataDirectory)
        {
            this.jesc = new JESC(Path.Combine(dataDirectory, "jesc_raw"), Encoding.UTF8);
        }
    }
}
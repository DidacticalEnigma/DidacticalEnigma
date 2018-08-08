using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Utils;
using JDict;

namespace DidacticalEnigma.Models
{
    public class TanakaCorpusDataSource : IDataSource
    {
        private Tanaka tanaka;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
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
                    foreach (var part in HighlightWords(sentence.JapaneseSentence, request.QueryText))
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

        private IEnumerable<(string text, bool highlight)> HighlightWords(string input, string word)
        {
            return input.Split(new string[] { word }, StringSplitOptions.None).Select(part => (part, false)).Intersperse((word, true));
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
}
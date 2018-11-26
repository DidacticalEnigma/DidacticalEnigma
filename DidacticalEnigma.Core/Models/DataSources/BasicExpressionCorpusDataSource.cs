using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class BasicExpressionCorpusDataSource : IDataSource
    {
        private readonly BasicExpressionsCorpus be;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("6ECF8F92-E97B-4D27-8BBF-1438E987C230"),
            "Basic Expressions Corpus",
            "This data sources uses sentences from Basic Expressions corpus",
            new Uri("http://nlp.ist.i.kyoto-u.ac.jp/index.php?%E6%97%A5%E8%8B%B1%E4%B8%AD%E5%9F%BA%E6%9C%AC%E6%96%87%E3%83%87%E3%83%BC%E3%82%BF"));

        public void Dispose()
        {

        }

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            var sentences = be.SearchByJapaneseText(request.QueryText);
            foreach (var sentence in sentences.OrderBy(s => s.JapaneseSentence.Length).Take(20))
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
                return Task.FromResult(Option.Some(rich));
            }

            return Task.FromResult(Option.None<RichFormatting>());
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public BasicExpressionCorpusDataSource(BasicExpressionsCorpus corpus)
        {
            this.be = corpus;
        }
    }
}

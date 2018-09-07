﻿using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Utils;
using JDict;

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

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
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
                    await yield.ReturnAsync(rich).ConfigureAwait(false);
                }
            });
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public BasicExpressionCorpusDataSource(string dataDirectory)
        {
            this.be = new BasicExpressionsCorpus(Path.Combine(dataDirectory, "JEC_basic_sentence_v1-2.csv"), Encoding.UTF8);
        }
    }
}
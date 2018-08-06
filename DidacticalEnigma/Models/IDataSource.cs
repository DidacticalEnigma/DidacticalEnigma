using DidacticalEnigma.Utils;
using JDict;
using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace DidacticalEnigma.Models
{
    public interface IDataSource : IDisposable
    {
        IAsyncEnumerable<RichFormatting> Answer(Request request);

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
    public class Request
    {
        public string Word { get; }

        public string QueryText { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public Request(string word, string queryText, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown)
        {
            Word = word;
            QueryText = queryText;
            PartOfSpeech = partOfSpeech;
        }
    }

    public class DataSourceDescriptor
    {
        public string Name { get; }

        public string AcknowledgementText { get; }

        public Uri DataSourceUrl { get; }

        public DataSourceDescriptor(string name, string acknowledgementText, Uri dataSourceUrl)
        {
            Name = name;
            AcknowledgementText = acknowledgementText;
            DataSourceUrl = dataSourceUrl;
        }
    }

    public enum UpdateResult
    {
        NoUpdateNeeded,
        Updated,
        Failure,
        Cancelled,
        NotSupported
    }

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
                foreach (var sentence in sentences.Take(20))
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

    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jdict.Lookup(request.Word.Trim());
                if (entry == null)
                    return;
                var rich = new RichFormatting();
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                rich.Paragraphs.Add(p);
                await yield.ReturnAsync(rich);
            });
        }

        public void Dispose()
        {
            jdict.Dispose();
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JMDictDataSource(string dataDirectory)
        {
            this.jdict = JDict.JMDict.Create(Path.Combine(dataDirectory, "JMdict_e"));
        }
    }
}

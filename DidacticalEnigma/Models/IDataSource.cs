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
        IAsyncEnumerable<FlowDocument> Answer(Request request);

        Task<UpdateResult> UpdateLocalDataSource(
            CancellationToken cancellation = default(CancellationToken));
    }
    public class Request
    {
        public string QueryText { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public Request(string queryText, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown)
        {
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

        public IAsyncEnumerable<FlowDocument> Answer(Request request)
        {
            return new AsyncEnumerable<FlowDocument>(async yield =>
            {
                var flow = new FlowDocument();
                var sentences = tanaka.SearchByJapaneseTextAsync(request.QueryText);
                await sentences.ForEachAsync(sentence =>
                {
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(sentence.JapaneseSentence));
                    paragraph.Inlines.Add(new Run(sentence.EnglishSentence));
                    flow.Blocks.Add(paragraph);
                }).ConfigureAwait(false);
                await yield.ReturnAsync(flow).ConfigureAwait(false);
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

    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<FlowDocument> Answer(Request request)
        {
            return new AsyncEnumerable<FlowDocument>(async yield =>
            {
                foreach (var entry in jdict.Lookup(request.QueryText) ?? Enumerable.Empty<JMDictEntry>())
                {
                    var flow = new FlowDocument();
                    flow.Blocks.Add(new Paragraph(new Run(entry.ToString())));
                    await yield.ReturnAsync(flow);
                }
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

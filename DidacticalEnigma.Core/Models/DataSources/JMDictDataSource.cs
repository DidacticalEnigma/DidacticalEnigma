using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("ED1B840C-B2A8-4018-87B0-D5FC64A1ABC8"),
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jdict.Lookup(request.Word.Trim());
                var rich = new RichFormatting();

                var (greedyEntry, greedyWord) = GreedyLookup(request);
                if(greedyEntry != null && greedyWord != request.Word)
                {
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        new Text("The entries below are a result of the greedy lookup: "),
                        new Text(greedyWord, emphasis: true)
                    }));
                    var p = new TextParagraph();
                    p.Content.Add(new Text(string.Join("\n\n", greedyEntry.Select(e => e.ToString()))));
                    rich.Paragraphs.Add(p);
                }

                if (entry != null)
                {
                    if (greedyEntry != null && greedyWord != request.Word)
                    {
                        rich.Paragraphs.Add(new TextParagraph(new[]
                        {
                            new Text("The entries below are a result of the regular lookup: "),
                            new Text(request.Word, emphasis: true)
                        }));
                    }
                    var p = new TextParagraph();
                    p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                    rich.Paragraphs.Add(p);
                }

                if (request.NotInflected != null && request.NotInflected != request.Word)
                {
                    entry = jdict.Lookup(request.NotInflected);
                    if (entry != null)
                    {
                        rich.Paragraphs.Add(new TextParagraph(new[]
                        {
                            new Text("The entries below are a result of lookup on the base form: "),
                            new Text(request.NotInflected, emphasis: true)
                        }));
                        var p = new TextParagraph();
                        p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                        rich.Paragraphs.Add(p);
                    }
                }

                if (rich.Paragraphs.Count == 0)
                    return;

                await yield.ReturnAsync(rich);
            });
        }

        private (IEnumerable<JMDictEntry> entry, string word) GreedyLookup(Request request, int backOffCountStart = 5)
        {
            if (request.SubsequentWords == null)
                return (null, null);

            int backOffCount = backOffCountStart;
            IEnumerable<JMDictEntry> found = null;
            string foundWord = null;
            string concatenatedWord = "";
            foreach (var word in request.SubsequentWords)
            {
                concatenatedWord += word;
                var newEntry = jdict.Lookup(concatenatedWord);
                if (newEntry == null)
                {
                    backOffCount--;
                    if (backOffCount == 0)
                        break;
                }
                else
                {
                    found = newEntry;
                    foundWord = concatenatedWord;
                    backOffCount = backOffCountStart;
                }
            }

            return (found, foundWord);
        }

        public void Dispose()
        {
            
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JMDictDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}
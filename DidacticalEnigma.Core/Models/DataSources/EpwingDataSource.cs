using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class EpwingDataSource : IDataSource
    {
        private readonly YomichanTermDictionary dict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("0C0999F9-F361-46D3-8E24-BA3A7CA669E7"),
            "Epwing",
            "...",
            null);

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var entry = dict.Lookup(request.Word.RawWord.Trim());
            var rich = new RichFormatting();

            var (greedyEntry, greedyWord) = GreedyLookup(request);
            if (greedyEntry != null && greedyWord != request.Word.RawWord)
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
                if (greedyEntry != null && greedyWord != request.Word.RawWord)
                {
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        new Text("The entries below are a result of the regular lookup: "),
                        new Text(request.Word.RawWord, emphasis: true)
                    }));
                }
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                rich.Paragraphs.Add(p);
            }

            if (request.NotInflected != null && request.NotInflected != request.Word.RawWord)
            {
                entry = dict.Lookup(request.NotInflected);
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
                return Task.FromResult(Option.None<RichFormatting>());

            return Task.FromResult(Option.Some(rich));
        }

        private (IEnumerable<YomichanTermDictionary.Entry> entry, string word) GreedyLookup(Request request, int backOffCountStart = 5)
        {
            if (request.SubsequentWords == null)
                return (null, null);

            return DictUtils.GreedyLookup(s => dict.Lookup(s), request.SubsequentWords, backOffCountStart);
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public EpwingDataSource(YomichanTermDictionary dict)
        {
            this.dict = dict;
        }
    }
}

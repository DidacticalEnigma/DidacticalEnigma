using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class WordFrequencyRatingDataSource : IDataSource
    {
        private readonly Dictionary<string, double> frequencies;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("B9DC52B9-B081-4898-88E3-53EEA8C097EF"),
            "Word Frequency Rating",
            "",
            new Uri("http://corpus.leeds.ac.uk/list.html"));

        public Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            var rich = new RichFormatting();
            var p = new TextParagraph();
            var word = request.NotInflected ?? request.Word.RawWord;

            if (frequencies.TryGetValue(word, out var d))
            {
                p.Content.Add(new Text($"{word} is rated {d:F3} (10-grade scale)"));
            }
            else
            {
                p.Content.Add(new Text($"{word} is unrated (not enough data)"));
            }
            
            rich.Paragraphs.Add(p);
            return Task.FromResult(Option.Some(rich));
        }

        public void Dispose()
        {
            // purposefully not disposing language service
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public WordFrequencyRatingDataSource(FrequencyList frequencyList)
        {
            var words = frequencyList.GetAllWords().Materialize();
            var count = words.Count;
            frequencies = words.Select((kvp, i) => new KeyValuePair<string, double>(kvp.Key, (double)(count - i + 1) / (count + 1) * 10)).ToDictionary();
        }
    }
}

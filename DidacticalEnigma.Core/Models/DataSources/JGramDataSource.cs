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
    public class JGramDataSource : IDataSource
    {
        private readonly IKanjiProperties kanji;
        private readonly IKanaProperties kana;
        private IJGramLookup lookup;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("AF65046C-35CB-4856-9774-943203E26979"),
            "JGram",
            "JGram: The Japanese Grammar Database",
            new Uri("http://www.jgram.org/"));

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();

            var key = string.Join("", request.SubsequentWords.Take(10));
            var results = lookup.Lookup(key);

            foreach (var paragraph in results.SelectMany(Render))
            {
                rich.Paragraphs.Add(paragraph);
            }

            return Task.FromResult(Option.Some(rich));
        }

        private IEnumerable<Paragraph> Render(JGram.Entry entry)
        {
            yield return new TextParagraph(new []
            {
                new Text( $"{entry.Key} [{entry.Reading}]\n"),
                new Text(entry.Translation + "\n"),
                new Text(entry.Example, fontSize: FontSize.Small)
            });
            yield return new LinkParagraph(new Uri("http://www.jgram.org/pages/viewOne.php?id=" + entry.Id), "more info");
        }

        public void Dispose()
        {
            // purposefully not disposing language service
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JGramDataSource(IJGramLookup lookup)
        {
            this.lookup = lookup;
        }
    }
}

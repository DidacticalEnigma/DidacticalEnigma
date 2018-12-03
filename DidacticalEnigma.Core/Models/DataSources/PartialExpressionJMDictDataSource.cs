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
    public class PartialExpressionJMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("C04648BA-68B2-467B-87C4-33DA5B9CA070"),
            "Partial JMDict expression lookup",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            var lookup = LookAhead(request);
            foreach (var (l, word) in lookup)
            {
                var textParagraph = new TextParagraph(
                        l.Readings
                            .Concat(l.Kanji)
                            .Where(r => r.Contains(word))
                            .SelectMany(w => StringExt.HighlightWords(w, word).Concat(EnumerableExt.OfSingle((text: "; ", highlight: false))))
                            .Select(t => new Text(t.text, emphasis: t.highlight)));
                textParagraph.Content.Add(new Text("\n"));
                textParagraph.Content.Add(new Text(string.Join("\n", l.Senses
                    .Where(s => s.PartOfSpeechInfo.Contains(EdictPartOfSpeech.exp))
                    .Select(s => s.Description))));

                rich.Paragraphs.Add(textParagraph);
            }

            return Task.FromResult(Option.Some(rich));
        }

        private IEnumerable<(JMDictEntry entry, string word)> LookAhead(Request request, int ahead = 5)
        {
            return Impl(request, ahead)
                .Reverse()
                .SelectMany(key => jdict.PartialExpressionLookup(key, 60).Select(e => (entry: e, word: key)))
                .DistinctBy(k => k.entry.SequenceNumber)
                .Take(60);

            IEnumerable<string> Impl(Request r, int a)
            {
                string k = "";
                foreach(var word in r.SubsequentWords)
                {
                    k += word;
                    if (jdict.PartialExpressionLookup(k, 1).Any())
                        yield return k;
                }
            }
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public PartialExpressionJMDictDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}

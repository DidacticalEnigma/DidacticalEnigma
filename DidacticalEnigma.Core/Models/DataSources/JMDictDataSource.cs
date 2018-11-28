using System;
using System.Collections.Async;
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
    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;
        private readonly IKanaProperties kana;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("ED1B840C-B2A8-4018-87B0-D5FC64A1ABC8"),
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            return DictUtils.Lookup(
                request,
                t => jdict.Lookup(t),
                r => GreedyLookup(r),
                kana,
                Render);
        }

        private (IEnumerable<JMDictEntry> entry, string word) GreedyLookup(Request request, int backOffCountStart = 5)
        {
            if (request.SubsequentWords == null)
                return (null, null);

            return DictUtils.GreedyLookup(s => jdict.Lookup(s), request.SubsequentWords, backOffCountStart);
        }

        private IEnumerable<Paragraph> Render(IEnumerable<JMDictEntry> entries)
        {
            foreach (var entry in entries)
            {
                var l = new List<Text>();
                bool first;
                {
                    first = true;
                    foreach (var kanji in entry.Kanji)
                    {
                        if (!first)
                            l.Add(new Text(";  "));
                        first = false;
                        l.Add(new Text(kanji));
                    }

                    l.Add(new Text("\n"));
                }
                {
                    first = true;
                    foreach (var reading in entry.Readings)
                    {
                        if (!first)
                            l.Add(new Text("\n"));
                        first = false;
                        l.Add(new Text(reading));
                    }

                    l.Add(new Text("\n"));
                }
                {
                    foreach (var sense in entry.Senses)
                    {
                        l.Add(new Text(sense.PartOfSpeechString, fontSize: FontSize.Small));
                        l.Add(new Text("\n"));
                        l.Add(new Text(sense.Description));
                        l.Add(new Text("\n"));
                    }
                }
                yield return new TextParagraph(l);
            }
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JMDictDataSource(JMDict jdict, IKanaProperties kana)
        {
            this.jdict = jdict;
            this.kana = kana;
        }
    }
}
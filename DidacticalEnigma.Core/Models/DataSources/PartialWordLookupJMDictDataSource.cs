using System;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class PartialWordLookupJMDictDataSource : IDataSource
    {
        private readonly JMDict jmdict;
        private readonly FrequencyList list;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("1C91B1EE-FD02-413F-B007-58FEF2B998FB"),
            "Partial word search (JMDict)",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public PartialWordLookupJMDictDataSource(JMDict jmdict, FrequencyList list)
        {
            this.jmdict = jmdict;
            this.list = list;
        }

        public void Dispose()
        {
            
        }

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jmdict.PartialWordLookup(request.Word.RawWord.Trim());
                var rich = new RichFormatting();
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n", entry.Select(e => e.match).OrderByDescending(m => list.RateFrequency(m)).Distinct()), fontSize: FontSize.Large));
                rich.Paragraphs.Add(p);
                await yield.ReturnAsync(rich);
            });
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }
    }
}
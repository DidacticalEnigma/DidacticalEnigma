using System;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDict;

namespace DidacticalEnigma.Models.DataSources
{
    public class PartialWordLookupJMDictDataSource : IDataSource
    {
        private readonly JMDict jmdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "Partial word search (JMDict)",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public PartialWordLookupJMDictDataSource(JMDict jmdict)
        {
            this.jmdict = jmdict;
        }

        public void Dispose()
        {
            
        }

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jmdict.PartialWordLookup(request.Word.Trim());
                var rich = new RichFormatting();
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n", entry.SelectMany(e => e.Kanji.Select(kanji => kanji.ToString()))), fontSize: FontSize.Large));
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
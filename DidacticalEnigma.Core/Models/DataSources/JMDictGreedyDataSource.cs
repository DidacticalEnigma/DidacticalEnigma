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
    public class JMDictGreedyDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("13637B69-80C2-40B9-8967-7D83D7724C61"),
            "JMDict (greedy search)",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jdict.Lookup(request.Word.Trim());
                var rich = new RichFormatting();

                if(entry != null)
                {
                    var p = new TextParagraph();
                    p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                    rich.Paragraphs.Add(p);
                }

                if(rich.Paragraphs.Count == 0)
                    return;

                await yield.ReturnAsync(rich);
            });
        }

        public IEnumerable<KeyValuePair<string, IEnumerable<JMDictEntry>>> GreedyLookup(string text)
        {
            yield break;
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JMDictGreedyDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}
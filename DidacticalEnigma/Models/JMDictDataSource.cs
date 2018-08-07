using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JDict;

namespace DidacticalEnigma.Models
{
    public class JMDictDataSource : IDataSource
    {
        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "JMDict",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var entry = jdict.Lookup(request.Word.Trim());
                if (entry == null)
                    return;
                var rich = new RichFormatting();
                var p = new TextParagraph();
                p.Content.Add(new Text(string.Join("\n\n", entry.Select(e => e.ToString()))));
                rich.Paragraphs.Add(p);
                await yield.ReturnAsync(rich);
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
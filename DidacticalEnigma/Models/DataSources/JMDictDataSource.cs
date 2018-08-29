using System;
using System.Collections.Async;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Utils;
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
                var rich = new RichFormatting();

                if (entry != null)
                {
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
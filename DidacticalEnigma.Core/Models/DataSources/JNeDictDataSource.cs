using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class JNeDictDataSource : IDataSource
    {
        private readonly Jnedict dict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("12808B42-047E-4711-84A3-7699F74F7E5B"),
            "JMnedict",
            "The data JMnedict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/enamdict/enamdict_doc.html"));

        public Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            var entries = dict.Lookup(request.Word.RawWord.Trim()) ?? Enumerable.Empty<JnedictEntry>();
            var rich = new RichFormatting();

            foreach (var e in entries)
            {
                rich.Paragraphs.Add(new TextParagraph(new []
                {
                    new Text(string.Join("; ", e.Kanji)), 
                    new Text("\n"), 
                    new Text(string.Join(" ", e.Reading)), 
                }.Concat(e.Translation.SelectMany(t =>
                {
                    return new[]
                    {
                        new Text("\n"),
                        new Text(string.Join("/", t.Type.Select(type => type.ToLongString()))),
                        new Text("\n"),
                        new Text(string.Join("/", t.Translation)),
                    };
                }))));
            }

            if (rich.Paragraphs.Count == 0)
                return Task.FromResult(Option.None<RichFormatting>());

            return Task.FromResult(Option.Some(rich));
        }

        public void Dispose()
        {

        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public JNeDictDataSource(Jnedict dict)
        {
            this.dict = dict;
        }
    }
}
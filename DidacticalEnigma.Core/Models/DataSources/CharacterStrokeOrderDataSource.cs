using System;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class CharacterStrokeOrderDataSource : IDataSource
    {
        public void Dispose()
        {
            
        }

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("E0B9D0E5-BE75-4D1E-9F19-7795FD602836"),
            "Stroke Order Info",
            "Provides the stroke order information from the Kanji stroke order font",
            new Uri("http://www.nihilist.org.uk/"));

        public Task<Option<RichFormatting>> Answer(Request request, CancellationToken token)
        {
            var ch = request.Character;
            var rich = new RichFormatting();
            rich.Paragraphs.Add(new TextParagraph(new[] { new Text(ch, fontName: "kanji", fontSize: FontSize.Humonguous) }));
            return Task.FromResult(Option.Some(rich));
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }
    }
}
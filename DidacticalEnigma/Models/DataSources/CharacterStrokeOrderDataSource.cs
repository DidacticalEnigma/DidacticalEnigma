using System;
using System.Collections.Async;
using System.Threading;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    public class CharacterStrokeOrderDataSource : IDataSource
    {
        public void Dispose()
        {
            
        }

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "Stroke Order Info",
            "Provides the stroke order information from the Kanji stroke order font",
            new Uri("http://www.nihilist.org.uk/"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var ch = request.Character;
                var rich = new RichFormatting();
                rich.Paragraphs.Add(new TextParagraph(new[] { new Text(ch, fontName: "kanji", fontSize: FontSize.Humonguous) }));
                await yield.ReturnAsync(rich);
            });
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public CharacterStrokeOrderDataSource(string dataPath)
        {

        }
    }
}
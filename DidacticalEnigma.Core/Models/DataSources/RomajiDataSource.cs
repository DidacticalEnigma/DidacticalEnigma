using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class RomajiDataSource : IDataSource
    {
        private readonly IRomaji romaji;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("685F1A9E-D8F5-4537-A85E-CED1D9397979"),
            "Romaji",
            "Transcription of Japanese text to romaji",
            null);

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            var p = new TextParagraph();

            var text = romaji.ToRomaji(request.AllText());

            p.Content.Add(new Text(text));
            rich.Paragraphs.Add(p);
            return Task.FromResult(Option.Some(rich));
        }

        public void Dispose()
        {
            
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public RomajiDataSource(IRomaji romaji)
        {
            this.romaji = romaji;
        }
    }
}

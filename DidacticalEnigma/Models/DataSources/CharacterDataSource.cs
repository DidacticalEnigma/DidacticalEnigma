using System;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    public class CharacterDataSource : IDataSource
    {
        private ILanguageService lang;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            "Character Information",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var ch = request.Character;
                var cp = CodePoint.FromString(ch);
                var rich = new RichFormatting();
                var p = new TextParagraph();

                var radicals = cp is Kanji k ? lang.LookupRadicals(k) : Enumerable.Empty<CodePoint>();
                var romaji = cp is Kana kana ? lang.LookupRomaji(kana) : null;
                var text = cp.ToDescriptionString() + "\n" +
                           (romaji != null ? romaji + "\n" : "") +
                           string.Join(" ; ", radicals);

                p.Content.Add(new Text(text));
                rich.Paragraphs.Add(p);
                await yield.ReturnAsync(rich);
            });
        }

        public void Dispose()
        {
            // purposefully not disposing language service
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public CharacterDataSource(ILanguageService languageService)
        {
            this.lang = languageService;
        }
    }
}
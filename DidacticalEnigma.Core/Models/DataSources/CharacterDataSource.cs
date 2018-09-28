using System;
using System.Collections.Async;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class CharacterDataSource : IDataSource
    {
        private ILanguageService lang;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("9EAF9B28-6ABC-40B1-86D1-14967E0FA4DA"),
            "Character Information",
            "The data JMdict by Electronic Dictionary Research and Development Group",
            new Uri("http://www.edrdg.org/jmdict/j_jmdict.html"));

        public Task<Option<RichFormatting>> Answer(Request request)
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
            return Task.FromResult(Option.Some(rich));
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
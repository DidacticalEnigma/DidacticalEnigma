using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Formatting;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Core.Utils;
using JDict;
using LibJpConjSharp;
using Optional;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class VerbConjugationDataSource : IDataSource
    {
        private readonly string path;

        private readonly JMDict jdict;

        public static DataSourceDescriptor Descriptor { get; } = new DataSourceDescriptor(
            new Guid("4D25D667-D7FC-4F37-9FF5-364DFAD46028"),
            "Verb Conjugation",
            "The conjugation is done using the LibJpConjSharp library.",
            new Uri("https://github.com/milleniumbug/LibJpConjSharp/"));

        public void Dispose()
        {

        }

        public Task<Option<RichFormatting>> Answer(Request request)
        {
            var rich = new RichFormatting();
            if(!(request.PartOfSpeech == PartOfSpeech.Verb || request.PartOfSpeech == PartOfSpeech.Unknown))
                rich.Paragraphs.Add(
                    new TextParagraph(
                        EnumerableExt.OfSingle(
                            new Text("The program estimates this word is not a verb. The results below may be garbage.", emphasis: false))));

            var verb = request.NotInflected ?? request.Word.RawWord;
            var entries = jdict.Lookup(verb);
            if (entries == null)
            {
                rich.Paragraphs.Add(
                    new TextParagraph(
                        EnumerableExt.OfSingle(
                            new Text("No word found.", emphasis: false))));
                return Task.FromResult(Option.Some(rich));
            }

            var entry = entries.FirstOrDefault(e =>
            {
                if(!e.Readings.Any())
                    return false;
                var et = GetEdictType(e);
                return et != null;
            });
            if (entry == null)
            {
                rich.Paragraphs.Add(
                    new TextParagraph(
                        EnumerableExt.OfSingle(
                            new Text("No verb found.", emphasis: false))));
                return Task.FromResult(Option.Some(rich));
            }

            var edictType = GetEdictType(entry).Value;
            rich.Paragraphs.Add(new TextParagraph(new []
            {
                Form("=", CForm.Present),
                Form("<", CForm.Past),
                Form("?", CForm.Potential),
                Form("->", CForm.Causative),
                Form("Te", CForm.TeForm),
                Form("!", CForm.Imperative),
                Form(":D", CForm.Volitional),
            }));

            rich.Paragraphs.Add(new TextParagraph(EnumerableExt.OfSingle(new Text(
@"= - Present
< - Past
? - Potential
-> - Causative
Te - Te Form
! - Imperative
:D - Volitional
~ - Negative form of any of those
@ - Polite form
"))));

            return Task.FromResult(Option.Some(rich));

            Text Form(string name, CForm form)
            {
                return new Text(
                    $"{name}: {JpConj.Conjugate(verb, edictType, form, Politeness.Plain, Polarity.Affirmative).Replace("|", "")}\n~{name}: {JpConj.Conjugate(verb, edictType, form, Politeness.Plain, Polarity.Negative).Replace("|", "")}\n");
            }
        }

        private LibJpConjSharp.EdictType? GetEdictType(JMDictEntry entry)
        {
            var sense = entry.Senses.FirstOrDefault(s => LibJpConjSharp.EdictTypeUtils.FromDescriptionOrNull(s.PartOfSpeech.Split('/')[0]) != null);
            if (sense == null)
                return null;
            return LibJpConjSharp.EdictTypeUtils.FromDescription(sense.PartOfSpeech.Split('/')[0]);
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default(CancellationToken))
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public VerbConjugationDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}

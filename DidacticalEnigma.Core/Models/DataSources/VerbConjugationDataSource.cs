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
using JDict;
using LibJpConjSharp;
using Optional;
using Optional.Collections;
using Utility.Utils;
using EdictTypeUtils = JDict.EdictTypeUtils;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class VerbConjugationDataSource : IDataSource
    {
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

            var verbTypes = entries.Select(e =>
            {
                if(!e.Readings.Any())
                    return Option.None<LibJpConjSharp.EdictType>();
                return GetEdictVerbType(e);
            })
                .OfNonNone()
                .Distinct()
                .OrderByDescending(e => Option.Some((int)e) == request.Word.Type.Map(t => (int)t) ? 1 : 0)
                .ToList();
            if (verbTypes.Count == 0)
            {
                rich.Paragraphs.Add(
                    new TextParagraph(
                        EnumerableExt.OfSingle(
                            new Text("No verb found.", emphasis: false))));
                return Task.FromResult(Option.Some(rich));
            }
            else
            {
                foreach (var verbType in verbTypes)
                {
                    if(verbTypes.Count > 1)
                        rich.Paragraphs.Add(new TextParagraph(
                            EnumerableExt.OfSingle(new Text(verb + ": " + LibJpConjSharp.EdictTypeUtils.ToLongString(verbType)))));
                    rich.Paragraphs.Add(new TextParagraph(new[]
                    {
                        Form("=", CForm.Present, verbType),
                        Form("@=", CForm.Present, verbType, Politeness.Polite),
                        Form("<", CForm.Past, verbType),
                        Form("?", CForm.Potential, verbType),
                        Form("#", CForm.Passive, verbType),
                        Form("->", CForm.Causative, verbType),
                        Form("if", CForm.Condition, verbType),
                        Form("Te", CForm.TeForm, verbType),
                        Form("!", CForm.Imperative, verbType),
                        Form(":D", CForm.Volitional, verbType),
                    }));
                }
            }

            rich.Paragraphs.Add(new TextParagraph(EnumerableExt.OfSingle(new Text(
@"= - Present
< - Past
? - Potential
# - Passive
-> - Causative
if - Conditional
Te - Te Form
! - Imperative
:D - Volitional
~ - Negative form of any of those
@ - Polite form
"))));

            return Task.FromResult(Option.Some(rich));

            Text Form(string name, CForm form, LibJpConjSharp.EdictType type, Politeness politeness = Politeness.Plain)
            {
                return new Text(
                    $"{name}: {JpConj.Conjugate(verb, type, form, politeness, Polarity.Affirmative).Replace("|", "")}\n~{name}: {JpConj.Conjugate(verb, type, form, politeness, Polarity.Negative).Replace("|", "")}\n");
            }
        }

        private Option<LibJpConjSharp.EdictType> GetEdictVerbType(JMDictEntry entry)
        {
            var senseType = entry.Senses
                .Select(s => s.Type)
                .OfNonNone()
                .Where(t => (int)t <= (int)EdictType.vs_s)
                .FirstOrNone();
            return senseType.Map(s => (LibJpConjSharp.EdictType)(int)s);
        }

        public Task<UpdateResult> UpdateLocalDataSource(CancellationToken cancellation = default)
        {
            return Task.FromResult(UpdateResult.NotSupported);
        }

        public VerbConjugationDataSource(JMDict jdict)
        {
            this.jdict = jdict;
        }
    }
}

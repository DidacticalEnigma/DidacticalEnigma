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

        public IAsyncEnumerable<RichFormatting> Answer(Request request)
        {
            return new AsyncEnumerable<RichFormatting>(async yield =>
            {
                var rich = new RichFormatting();
                if(!(request.PartOfSpeech == PartOfSpeech.Verb || request.PartOfSpeech == PartOfSpeech.Unknown))
                    rich.Paragraphs.Add(
                        new TextParagraph(
                            EnumerableExt.OfSingle(
                                new Text("The program estimates this word is not a verb. The results below may be garbage.", emphasis: false))));

                var verb = request.NotInflected ?? request.Word;
                var entries = jdict.Lookup(verb);
                if (entries == null)
                {
                    rich.Paragraphs.Add(
                        new TextParagraph(
                            EnumerableExt.OfSingle(
                                new Text("No word found.", emphasis: false))));
                    await yield.ReturnAsync(rich);
                    return;
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
                    await yield.ReturnAsync(rich);
                    return;
                }

                var edictType = GetEdictType(entry).Value;
                var reading = entry.Readings.First();
                rich.Paragraphs.Add(
                    new TextParagraph(
                        EnumerableExt.OfSingle(
                            new Text($"Plain form: {reading}", emphasis: false))));
                PrintForm("Present", CForm.Present);
                PrintForm("Present", CForm.Past);
                PrintForm("Potential", CForm.Potential);
                PrintForm("Te form", CForm.TeForm);
                PrintForm("Imperative", CForm.Imperative);
                PrintForm("Volitional", CForm.Volitional);

                await yield.ReturnAsync(rich);
                return;

                void PrintForm(string name, CForm form)
                {
                    rich.Paragraphs.Add(
                        new TextParagraph(
                            EnumerableExt.OfSingle(
                                new Text($"Plain {name}: {JpConj.Conjugate(reading, edictType, form, Politeness.Plain, Polarity.Affirmative).Replace("|", "")}"))));
                    rich.Paragraphs.Add(
                        new TextParagraph(
                            EnumerableExt.OfSingle(
                                new Text($"Negative Plain {name}: {JpConj.Conjugate(reading, edictType, form, Politeness.Plain, Polarity.Negative).Replace("|", "")}"))));
                }
            });

            EdictType? GetEdictType(JMDictEntry entry)
            {
                var sense = entry.Senses.FirstOrDefault(s =>
                {
                    try
                    {
                        var et = EdictTypeUtils.FromDescription(s.PartOfSpeech.Split('/')[0]);
                        return true;
                    }
                    catch(Exception exception)
                    {
                        return false;
                    }
                });
                if(sense == null)
                    return null;
                return EdictTypeUtils.FromDescription(sense.PartOfSpeech.Split('/')[0]);
            }
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

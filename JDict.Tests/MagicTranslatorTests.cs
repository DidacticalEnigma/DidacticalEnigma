using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.Project;
using JDict.Internal.XmlModels;
using NUnit.Framework;
using Optional;
using Utility.Utils;

namespace JDict.Tests
{
    [TestFixture]
    class MagicTranslatorTests
    {
        [Test]
        public void Basic()
        {
            using (var project =
                new MagicTranslatorProject.MagicTranslatorProject(Path.Combine(
                    TestDataPaths.MagicTranslatorTestDir,
                    "Basic")))
            {
                var manga = project.Root;
                var volume = manga.Children.First();
                var chapter = volume.Children.First();
                var page = chapter.Children.First();
                var note = page.Children.Single().Translation;
                Assert.AreEqual(note.OriginalText, "考えて");
                Assert.AreEqual(note.TranslatedText, "Think");

                var actual = new List<Translation>();
                project.TranslationChanged += (sender, args) => { actual.Add(args.Translation); };

                project.Refresh(fullRefresh: true);
                var expected = new List<SimpleProject.Translation>
                {
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "考えて", 
                        "Think",
                        new GlossNote[]{ new GlossNote("", "") },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "無敵なパワー!!",
                        "UNLIMITED POWER!!",
                        new GlossNote[]{
                            new GlossNote("無敵", "invincible/unrivaled/unrivalled"),
                            new GlossNote("な","applies the na-adjective 無敵 on パワー"), 
                            new GlossNote("パワー", "power"), 
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "ゴゴゴゴ",
                        "menacing",
                        new GlossNote[]{
                            new GlossNote(  "", "obvious reference to more famous works"),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "必ず助けるわ!!!",
                        "",
                        new GlossNote[]{
                            new GlossNote(  "", ""),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "絶対!!!!!",
                        "",
                        new GlossNote[]{
                            new GlossNote(  "", ""),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "???",
                        "",
                        new GlossNote[]{
                            new GlossNote(  "", ""),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "",
                        "???",
                        new GlossNote[]{
                            new GlossNote(  "", ""),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),
                    new SimpleProject.Translation(
                        Option.None<Guid>(),
                        "終わり",
                        "The end",
                        new GlossNote[]{
                            new GlossNote(  "終わり", "the end"),
                        },
                        Enumerable.Empty<TranslatorNote>(),
                        Enumerable.Empty<TranslatedText>()),

                };
                var comp = Comparer<GlossNote>.Create((l, r) =>
                {
                    return l.Foreign.CompareTo(r.Foreign) * 16 +
                           l.Text.CompareTo(r.Text) * 4;
                });
                var comparer = Comparer<Translation>.Create((l, r) =>
                {
                    return
                        l.OriginalText.CompareTo(r.OriginalText) * 64 +
                        l.TranslatedText.CompareTo(r.TranslatedText) * 16 +
                        l.Glosses.LexicographicalCompare(r.Glosses, comp) * 4;
                });
                actual.Sort(comparer);
                expected.Sort(comparer);
                CollectionAssert.AreEqual(expected, actual, comparer);
            }
        }
    }
}

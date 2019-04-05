using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JDict.Tests
{
    [TestFixture]
    class MagicTranslatorTests
    {
        [Test]
        public void Basic()
        {
            using (var project =
                new MagicTranslatorProject.MagicTranslatorProject(Path.Combine(TestDataPaths.MagicTranslatorTestDir,
                    "Basic")))
            {
                var manga = project.Root;
                var volume = manga.Children.First();
                var chapter = volume.Children.First();
                var page = chapter.Children.First();
                var note = page.Translations.Single();
                Assert.AreEqual(note.OriginalText, "考えて");
                Assert.AreEqual(note.TranslatedText, "Think");
            }
        }
    }
}

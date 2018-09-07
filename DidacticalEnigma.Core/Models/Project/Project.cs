using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models.Project
{
    class Project
    {
        public string RootPath { get; }

        public static Project Open(string path)
        {
            if(Directory.Exists(path))
                throw new ArgumentException("The specified path doesn't point to a directory", nameof(path));

            return new Project(path);
        }

        private Project(string path)
        {
            RootPath = path;
        }
    }

    class Translation
    {
        public string OriginalText { get; }

        // Can be null
        public string TranslatedText { get; }

        public IEnumerable<TranslatorNote> Notes { get; }
    }

    abstract class TranslatorNote
    {
        
    }

    class GeneralTranslatorNote : TranslatorNote
    {
        public string Text { get; }

        public GeneralTranslatorNote(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return $"{Text}";
        }
    }

    class GlossTranslatorNote : TranslatorNote
    {
        public string Foreign { get; }

        public string Text { get; }

        public override string ToString()
        {
            return $"{Foreign}: {Text}";
        }
    }
}

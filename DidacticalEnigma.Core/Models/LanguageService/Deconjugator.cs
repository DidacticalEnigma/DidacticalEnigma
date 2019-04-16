using System;
using System.Collections.Generic;
using System.Text;
using LibJpConjSharp;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IDeconjugator
    {
        
    }

    public class DeconjugationResult
    {
        public Word Word { get; }

        public IEnumerable<ConjugationRule> Rules { get; }
    }

    public class ConjugationRule
    {
        public CForm Form { get; }

        public Polarity Polarity { get; }    

        public Politeness Politeness { get; }
    }

    public class Word
    {
        public EdictType Type { get; }

        public string Text { get; }

        public Word(string text, EdictType type)
        {
            Text = text;
            Type = type;
        }
    }

    class Deconjugator
    {
    }
}

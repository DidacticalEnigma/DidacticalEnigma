using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidacticalEnigma.Models
{
    public class Word
    {
        public enum WordKind
        {
            Unknown,
            Particle,
            Noun,
            Verb,
        }

        public WordKind Kind { get; }
    }

    // To check: do I need normalization forms?
    public abstract class CodePoint
    {
        
    }
    
    public class Kanji : CodePoint
    {

    }

    public class Hiragana : CodePoint
    {

    }
}

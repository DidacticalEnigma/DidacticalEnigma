using JDict;

namespace DidacticalEnigma.Models
{
    public class WordVM
    {
        public enum WordKind
        {
            Unknown,
            Particle,
            Noun,
            Verb,
        }

        public string DictionaryStringBlurb { get; }

        public EDictEntry DictionaryEntry { get; }

        public string StringForm { get; }

        public WordKind Kind { get; }

        public WordVM(string s, EDictEntry entry)
        {
            StringForm = s;
            DictionaryEntry = entry;
            DictionaryStringBlurb = entry?.ToString();
        }
    }
}

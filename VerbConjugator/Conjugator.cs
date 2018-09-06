using System;

namespace VerbConjugator
{
    // the verb
    // precondition: the verb must be a valid verb in the base form
    // this will be detected at best effort basis
    // https://www.sljfaq.org/afaq/verb-conjugation.html
    public class Conjugator
    {
        private string Normalize(string verb)
        {
            // attempt at detecting precondition violations
            return verb.Trim();
        }

        private string ReplaceEnd(string verb, string end, string newEnd)
        {
            return verb.Remove(verb.Length - end.Length - 1) + newEnd;
        }

        private bool IsConsonantStem(string verb)
        {
            throw new NotImplementedException();
        }

        private bool IsVowelStem(string verb)
        {
            return !IsConsonantStem(verb);
        }

        private string Causative(string verb)
        {
            if (verb == "くる")
                return "こさせる";
            if (verb == "する")
                return "させる";
            if (verb.EndsWith("う"))
                return ReplaceEnd(verb, "う", "わせる");
            if (verb.EndsWith("く"))
                return ReplaceEnd(verb, "く", "かせる");
            if (verb.EndsWith("ぐ"))
                return ReplaceEnd(verb, "ぐ", "がせる");
            if (verb.EndsWith("す"))
                return ReplaceEnd(verb, "す", "させる");
            if (verb.EndsWith("つ"))
                return ReplaceEnd(verb, "つ", "たせる");
            if(verb.EndsWith("ぬ"))
                return ReplaceEnd(verb, "ぬ", "なせる");
            if(verb.EndsWith("ぶ"))
                return ReplaceEnd(verb, "ぶ", "ばせる");
            if(verb.EndsWith("む"))
                return ReplaceEnd(verb, "む", "ませる");
            if(verb.EndsWith("る") && IsConsonantStem(verb))
                return ReplaceEnd(verb, "る", "らせる");
            if(verb.EndsWith("いる"))
                return ReplaceEnd(verb, "いる", "いさせる");
            if(verb.EndsWith("える"))
                return ReplaceEnd(verb, "える", "えさせる");
            throw new ArgumentException(nameof(verb));
        }

        private string PastForm(string verb)
        {
            throw new ArgumentException();
        }

        private string Conditional(string verb)
        {
            if (verb == "だ")
                return "なら";
            return PastForm(verb) + "ら";
        }

        private string Negative(string verb)
        {
            if(verb == "くる")
                return "こない";
            if(verb == "する")
                return "しない";
            if (verb == "だ")
                return "でわない";
            if(verb.EndsWith("ます"))
                return ReplaceEnd(verb, "ます", "ません");
            if(verb.EndsWith("う"))
                return ReplaceEnd(verb, "う", "わない");
            if(verb.EndsWith("く"))
                return ReplaceEnd(verb, "く", "かない");
            if(verb.EndsWith("ぐ"))
                return ReplaceEnd(verb, "ぐ", "がない");
            if(verb.EndsWith("す"))
                return ReplaceEnd(verb, "す", "さない");
            if(verb.EndsWith("つ"))
                return ReplaceEnd(verb, "つ", "たない");
            if(verb.EndsWith("ぬ"))
                return ReplaceEnd(verb, "ぬ", "なない");
            if(verb.EndsWith("ぶ"))
                return ReplaceEnd(verb, "ぶ", "ばない");
            if(verb.EndsWith("む"))
                return ReplaceEnd(verb, "む", "まない");
            if(verb.EndsWith("る") && IsConsonantStem(verb))
                return ReplaceEnd(verb, "る", "らない");
            if(verb.EndsWith("いる"))
                return ReplaceEnd(verb, "いる", "いない");
            if(verb.EndsWith("える"))
                return ReplaceEnd(verb, "える", "えない");
            throw new ArgumentException(nameof(verb));
        }

        public Conjugation Conjugate(string verb)
        {
            verb = Normalize(verb);
            return new Conjugation(
                Negative(verb),
                Causative(verb));
        }
    }

    public class Conjugation
    {
        public string Plain { get; }

        public string NegativePlain { get; }

        public string PastPlain { get; }

        public string NegativePastPlain { get; }

        public string Polite { get; }

        public string NegativePolite { get; }

        public string PastPolite { get; }

        public string NegativePastPolite { get; }

        public string Potential { get; }

        public string TeForm { get; }

        public string NegativeTeForm { get; }

        public string Imperative { get; }

        public string NegativeImperative { get; }

        public string Volitional { get; }

        // more to come

        public string Causative { get; }

        public Conjugation(string negativePlain, string causative)
        {
            NegativePlain = negativePlain;
            Causative = causative;
        }
    }
}

namespace JDict
{
    public class Sentence
    {
        public string JapaneseSentence { get; }

        public string EnglishSentence { get; }

        public Sentence(string japaneseSentence, string englishSentence)
        {
            JapaneseSentence = japaneseSentence;
            EnglishSentence = englishSentence;
        }
    }
}
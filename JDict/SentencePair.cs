namespace JDict
{
    public class SentencePair
    {
        public string JapaneseSentence { get; }

        public string EnglishSentence { get; }

        public SentencePair(string japaneseSentence, string englishSentence)
        {
            JapaneseSentence = japaneseSentence;
            EnglishSentence = englishSentence;
        }
    }
}
using DidacticalEnigma.Core.Models.LanguageService;

namespace DidacticalEnigma.Core.Models.DataSources
{
    public class Request
    {
        public string Character { get; }

        public string Word { get; }

        public string QueryText { get; }

        public PartOfSpeech PartOfSpeech { get; }

        public string NotInflected { get; }

        public Request(string character, string word, string queryText, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown, string notInflected = null)
        {
            Character = character;
            Word = word;
            QueryText = queryText;
            PartOfSpeech = partOfSpeech;
            NotInflected = notInflected;
        }
    }
}
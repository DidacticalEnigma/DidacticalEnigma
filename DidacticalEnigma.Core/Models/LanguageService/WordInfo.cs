using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class WordInfo
    {
        public string DictionaryDefinition { get; }

        public PartOfSpeech EstimatedPartOfSpeech { get; }

        public string NotInflected { get; }

        public string RawWord { get; }

        public IEnumerable<CodePoint> CodePoints { get; }

        public WordInfo(string word, string dictionaryEntry, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown, string notInflected = null)
        {
            RawWord = word;
            CodePoints = new List<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            DictionaryDefinition = dictionaryEntry;
            EstimatedPartOfSpeech = partOfSpeech;
            NotInflected = notInflected;
        }
    }
}
using DidacticalEnigma.Utils;
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Models
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
            CodePoints = new ObservableBatchCollection<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            DictionaryDefinition = dictionaryEntry;
            EstimatedPartOfSpeech = partOfSpeech;
            NotInflected = notInflected;
        }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Models
{
    public class WordInfo
    {
        public string DictionaryDefinition { get; }

        public string RawWord { get; }

        public IEnumerable<CodePoint> CodePoints { get; }

        public WordInfo(string word, string dictionaryEntry)
        {
            RawWord = word;
            CodePoints = new ObservableBatchCollection<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            DictionaryDefinition = dictionaryEntry;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using JDict;
using Optional;
using Utility.Utils;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class WordInfo
    {
        public PartOfSpeech EstimatedPartOfSpeech { get; }

        public string DictionaryForm { get; }

        public string RawWord { get; }

        public IEnumerable<CodePoint> CodePoints { get; }

        public bool? Independent { get; }

        public Option<EdictPartOfSpeech> Type { get; }

        public string Reading { get; }

        public WordInfo(
            string word,
            PartOfSpeech partOfSpeech = PartOfSpeech.Unknown,
            string dictionaryForm = null,
            bool? isIndependent = null,
            Option<EdictPartOfSpeech> type = default,
            string reading = null)
        {
            RawWord = word;
            CodePoints = new List<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            EstimatedPartOfSpeech = partOfSpeech;
            DictionaryForm = dictionaryForm;
            Independent = isIndependent;
            Type = type;
            Reading = reading;
        }
    }
}
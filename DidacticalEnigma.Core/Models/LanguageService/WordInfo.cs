using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;
using JDict;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class WordInfo
    {
        public PartOfSpeech EstimatedPartOfSpeech { get; }

        public string NotInflected { get; }

        public string RawWord { get; }

        public IEnumerable<CodePoint> CodePoints { get; }

        public bool? Independent { get; }

        public EdictType? Type { get; }

        public WordInfo(string word, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown, string notInflected = null, bool? isIndependent = null, EdictType? type = null)
        {
            RawWord = word;
            CodePoints = new List<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            EstimatedPartOfSpeech = partOfSpeech;
            NotInflected = notInflected;
            Independent = isIndependent;
            Type = type;
        }
    }
}
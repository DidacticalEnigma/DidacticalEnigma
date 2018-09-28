using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Utils;
using JDict;
using Optional;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public class WordInfo
    {
        public PartOfSpeech EstimatedPartOfSpeech { get; }

        public string NotInflected { get; }

        public string RawWord { get; }

        public IEnumerable<CodePoint> CodePoints { get; }

        public bool? Independent { get; }

        public Option<EdictType> Type { get; }

        public IEnumerable<PartOfSpeechInfo> PartOfSpeechInfo { get; }

        public WordInfo(string word, PartOfSpeech partOfSpeech = PartOfSpeech.Unknown, string notInflected = null, bool? isIndependent = null, Option<EdictType> type = default(Option<EdictType>), IEnumerable<PartOfSpeechInfo> posInfo = null)
        {
            RawWord = word;
            CodePoints = new List<CodePoint>(
                word.AsCodePoints().Select(cp => CodePoint.FromInt(cp)));
            EstimatedPartOfSpeech = partOfSpeech;
            NotInflected = notInflected;
            Independent = isIndependent;
            Type = type;
            this.PartOfSpeechInfo = posInfo?.ToList() ?? Enumerable.Empty<PartOfSpeechInfo>();
        }
    }
}
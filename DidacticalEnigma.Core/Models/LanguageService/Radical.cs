using System;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    [Obsolete]
    public class Radical
    {
        public Radical(CodePoint cp, int strokeCount)
        {
            CodePoint = cp;
            StrokeCount = strokeCount;
        }

        public CodePoint CodePoint { get; }

        public int StrokeCount { get; }
    }
}
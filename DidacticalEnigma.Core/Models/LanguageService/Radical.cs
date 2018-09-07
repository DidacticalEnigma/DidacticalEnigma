using DidacticalEnigma.Models;

namespace DidacticalEnigma
{
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
using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Models
{
    public class CodePointVM
    {
        public CodePoint CodePoint { get; }

        public string FullName => CodePoint.ToLongString();

        public string StringForm => CodePoint.ToString();

        public IEnumerable<CodePoint> Similar { get; }

        public bool HasSimilar => Similar.Any();

        public CodePointVM(CodePoint cp, WordVM word, IEnumerable<CodePoint> similar)
        {
            CodePoint = cp;
            Similar = similar;
        }
    }
}

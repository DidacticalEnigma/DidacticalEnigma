using System.Collections.Generic;
using System.Linq;

namespace DidacticalEnigma.Models
{
    public class CodePointVM
    {
        public CodePoint CodePoint { get; }

        public string Description
        {
            get
            {
                return CodePoint.ToDescriptionString() + "\n" +
                string.Join(" ; ", radicals);
            }
        }

        public string StringForm => CodePoint.ToString();

        public IEnumerable<CodePoint> Similar { get; }

        private readonly IEnumerable<string> radicals;

        public bool HasSimilar => Similar.Any();

        public CodePointVM(CodePoint cp, IEnumerable<CodePoint> similar, IEnumerable<CodePoint> radicals)
        {
            CodePoint = cp;
            Similar = similar;
            this.radicals = radicals?.Select(r => r.ToString()).ToList() ?? Enumerable.Empty<string>();
        }
    }
}

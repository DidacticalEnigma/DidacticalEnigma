using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.Models;

namespace DidacticalEnigma.ViewModels
{
    public class CodePointVM
    {
        public CodePoint CodePoint { get; }

        public string Description
        {
            get
            {
                return CodePoint.ToDescriptionString() + "\n" +
                    (romaji != null ? romaji + "\n" : "") +
                    string.Join(" ; ", radicals);
            }
        }

        public string StringForm => CodePoint.ToString();

        public IEnumerable<CodePoint> Similar { get; }

        private readonly IEnumerable<string> radicals;

        private readonly string romaji;

        public bool HasSimilar => Similar.Any();

        public CodePointVM(CodePoint cp, IEnumerable<CodePoint> similar, IEnumerable<CodePoint> radicals, string romaji)
        {
            CodePoint = cp;
            Similar = similar;
            this.radicals = radicals?.Select(r => r.ToString()).ToList() ?? Enumerable.Empty<string>();
            this.romaji = romaji;
        }
    }
}

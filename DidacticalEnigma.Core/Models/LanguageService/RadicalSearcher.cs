using System.Collections.Generic;

namespace DidacticalEnigma.Core.Models.LanguageService
{
    public interface IRadicalSearcher
    {
        IReadOnlyList<KeyValuePair<string, CodePoint>> Search(string text);
    }

    public class RadicalSearcher : IRadicalSearcher
    {
        public IReadOnlyList<KeyValuePair<string, CodePoint>> Search(string text)
        {
            return null;
        }

        private IEnumerable<string> Split(string text)
        {
            yield return text.Trim();
        }

        public RadicalSearcher(IEnumerable<CodePoint> radicals)
        {

        }
    }
}

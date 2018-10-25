using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DidacticalEnigma.Core.Models.LanguageService;

namespace DidacticalEnigma.Core.Utils
{
    class CategoryGrouping : IGrouping<string, CodePoint>
    {
        private readonly IEnumerable<CodePoint> values;

        public IEnumerator<CodePoint> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Key { get; }

        public CategoryGrouping(string key, IEnumerable<CodePoint> values)
        {
            Key = key;
            this.values = values;
        }
    }
}

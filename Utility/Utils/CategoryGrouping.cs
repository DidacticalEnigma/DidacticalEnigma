using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility.Utils
{
    public class CategoryGrouping<T> : IGrouping<string, T>
    {
        private readonly IEnumerable<T> values;

        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Key { get; }

        public CategoryGrouping(string key, IEnumerable<T> values)
        {
            Key = key;
            this.values = values;
        }
    }
}

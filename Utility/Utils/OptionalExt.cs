using System.Collections.Generic;
using System.Linq;
using Optional;

namespace Utility.Utils
{
    public static class OptionalExt
    {
        public static IEnumerable<T> OfNonNone<T>(this IEnumerable<Option<T>> input)
        {
            return input.SelectMany(i => i.ToEnumerable());
        }
    }
}

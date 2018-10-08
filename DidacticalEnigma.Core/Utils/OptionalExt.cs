using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Optional;

namespace DidacticalEnigma.Core.Utils
{
    internal static class OptionalExt
    {
        public static IEnumerable<T> OfNonNone<T>(this IEnumerable<Option<T>> input)
        {
            return input.SelectMany(i => i.ToEnumerable());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Utility
{
    public static class ComparerExt
    {
        private class ProjectingEqualityComparer<T, TKey> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return comparer.Equals(selector(x), selector(y));
            }

            public int GetHashCode(T obj)
            {
                return comparer.GetHashCode(selector(obj));
            }

            private IEqualityComparer<TKey> comparer;

            private Func<T, TKey> selector;

            public ProjectingEqualityComparer(Func<T, TKey> selector, IEqualityComparer<TKey> comparer)
            {
                this.selector = selector;
                this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            }
        }

        public static IEqualityComparer<T> Projecting<T, TKey>(Func<T, TKey> selector, IEqualityComparer<TKey> comparer = null)
        {
            if(selector == null)
                throw new ArgumentNullException(nameof(selector));
            return new ProjectingEqualityComparer<T,TKey>(selector, comparer);
        }
    }
}

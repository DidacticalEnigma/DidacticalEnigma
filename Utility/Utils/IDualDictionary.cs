using System.Collections.Generic;

namespace Utility.Utils
{
    public interface IDualDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        IDualDictionary<TValue, TKey> Inverse { get; }
    }
}
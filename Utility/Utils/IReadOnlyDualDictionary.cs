using System.Collections.Generic;

namespace Utility.Utils
{
    public interface IReadOnlyDualDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        IReadOnlyDualDictionary<TValue, TKey> Inverse { get; }
    }
}
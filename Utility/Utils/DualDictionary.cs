using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utility.Utils
{
    public class DualDictionary<TKey, TValue> : IReadOnlyDualDictionary<TKey, TValue>, IDualDictionary<TKey, TValue>
    {
        public IEqualityComparer<TKey> KeyComparer { get; }
        public IEqualityComparer<TValue> ValueComparer { get; }
        private readonly Dictionary<TKey, TValue> keyToValue;
        private readonly Dictionary<TValue, TKey> valueToKey;

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => keyToValue[key];

        private DualDictionary(
            IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TValue> valueComparer) :
            this(
                new Dictionary<TKey, TValue>(),
                new Dictionary<TValue, TKey>(),
                keyComparer,
                valueComparer)
        {
            
        }

        private DualDictionary(
            Dictionary<TKey, TValue> keyToValue,
            Dictionary<TValue, TKey> valueToKey,
            IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TValue> valueComparer)
        {
            this.KeyComparer = keyComparer;
            this.ValueComparer = valueComparer;
            this.keyToValue = keyToValue;
            this.valueToKey = valueToKey;
            Value = new ValueProxy(this);
            Key = new KeyProxy(this);
        }

        private DualDictionary(
            Dictionary<TKey, TValue> keyToValue,
            Dictionary<TValue, TKey> valueToKey,
            IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TValue> valueComparer,
            DualDictionary<TValue, TKey> inverse) :
            this(
                keyToValue,
                valueToKey,
                keyComparer,
                valueComparer)
        {
            Inverse = inverse;
        }

        public DualDictionary(
            IEnumerable<KeyValuePair<TKey, TValue>> source,
            IEqualityComparer<TKey> keyComparer,
            IEqualityComparer<TValue> valueComparer) :
            this(
                keyComparer,
                valueComparer)
        {
            foreach (var kvp in source)
            {
                keyToValue.Add(kvp.Key, kvp.Value);
                valueToKey.Add(kvp.Value, kvp.Key);
            }
            Inverse = new DualDictionary<TValue, TKey>(valueToKey, keyToValue, valueComparer, this.KeyComparer, this);
        }

        public DualDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source) :
            this(source, EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default)
        {

        }

        public DualDictionary() :
            this(Enumerable.Empty<KeyValuePair<TKey, TValue>>())
        {

        }

        IDualDictionary<TValue, TKey> IDualDictionary<TKey, TValue>.Inverse => Inverse;

        IReadOnlyDualDictionary<TValue, TKey> IReadOnlyDualDictionary<TKey, TValue>.Inverse => Inverse;

        public DualDictionary<TValue, TKey> Inverse { get; }

        public class KeyProxy
        {
            internal readonly DualDictionary<TKey, TValue> dict;

            internal KeyProxy(DualDictionary<TKey, TValue> dict)
            {
                this.dict = dict;
            }

            public TValue this[TKey key]
            {
                get => dict.keyToValue[key];
                set => dict.Set(key, value);
            }
        }

        public class ValueProxy
        {
            internal readonly DualDictionary<TKey, TValue> dict;

            internal ValueProxy(DualDictionary<TKey, TValue> dict)
            {
                this.dict = dict;
            }

            public TKey this[TValue v]
            {
                get => dict.valueToKey[v];
                set => dict.Set(value, v);
            }
        }

        public ValueProxy Value { get; }

        public KeyProxy Key { get; }

        public IEnumerable<TKey> Keys => keyToValue.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => keyToValue.Values;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => keyToValue.Keys;

        public IEnumerable<TValue> Values => valueToKey.Keys;

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            keyToValue.Clear();
            valueToKey.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)keyToValue).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)keyToValue).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (keyToValue.TryGetValue(item.Key, out var value))
            {
                if (ValueComparer.Equals(value, item.Value))
                {
                    keyToValue.Remove(item.Key);
                    valueToKey.Remove(value);
                    return true;
                }
            }

            return false;
        }

        public int Count => keyToValue.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (keyToValue.ContainsKey(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (valueToKey.ContainsKey(value))
            {
                throw new ArgumentException(nameof(value));
            }

            keyToValue.Add(key, value);
            valueToKey.Add(value, key);
        }

        public bool ContainsKey(TKey key)
        {
            return keyToValue.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (keyToValue.TryGetValue(key, out var value))
            {
                keyToValue.Remove(key);
                valueToKey.Remove(value);
                return true;
            }

            return false;
        }

        public bool ContainsValue(TValue value)
        {
            return valueToKey.ContainsKey(value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keyToValue.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return keyToValue.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => keyToValue[key];
            set => Set(key, value);
        }

        private void Set(TKey key, TValue value)
        {
            TKey k;
            TValue v;
            if (keyToValue.TryGetValue(key, out v) && valueToKey.TryGetValue(value, out k))
            {
                keyToValue.Remove(key);
                valueToKey.Remove(v);
            }
            keyToValue[key] = value;
            valueToKey[value] = key;
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return valueToKey.TryGetValue(value, out key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

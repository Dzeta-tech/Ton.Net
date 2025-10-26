using System.Collections;
using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     TON blockchain dictionary (Hashmap) implementation.
///     Dictionaries are stored as binary Patricia trees in BOC format.
/// </summary>
public class Dictionary<K, V> : IEnumerable<KeyValuePair<K, V>> where K : IDictionaryKeyType
{
    readonly IDictionaryKey<K>? _key;
    readonly IDictionaryValue<V>? _value;
    readonly System.Collections.Generic.Dictionary<string, V> _map;

    /// <summary>
    ///     Number of entries in the dictionary.
    /// </summary>
    public int Size => _map.Count;

    Dictionary(System.Collections.Generic.Dictionary<string, V> values, IDictionaryKey<K>? key, IDictionaryValue<V>? value)
    {
        _key = key;
        _value = value;
        _map = values;
    }

    /// <summary>
    ///     Create an empty dictionary.
    /// </summary>
    public static Dictionary<K, V> Empty(IDictionaryKey<K>? key = null, IDictionaryValue<V>? value = null)
    {
        return new Dictionary<K, V>(new System.Collections.Generic.Dictionary<string, V>(), key, value);
    }

    /// <summary>
    ///     Load dictionary from slice (reads ref to dictionary root).
    /// </summary>
    public static Dictionary<K, V> Load(IDictionaryKey<K> key, IDictionaryValue<V> value, Slice slice)
    {
        var cell = slice.LoadMaybeRef();
        if (cell != null && !cell.IsExotic)
        {
            return LoadDirect(key, value, cell.BeginParse());
        }

        return Empty(key, value);
    }

    /// <summary>
    ///     Load dictionary from slice (reads ref to dictionary root).
    /// </summary>
    public static Dictionary<K, V> Load(IDictionaryKey<K> key, IDictionaryValue<V> value, Cell cell)
    {
        if (cell.IsExotic)
            return Empty(key, value);

        var slice = cell.BeginParse();
        var dictCell = slice.LoadMaybeRef();
        if (dictCell != null && !dictCell.IsExotic)
        {
            return LoadDirect(key, value, dictCell.BeginParse());
        }

        return Empty(key, value);
    }

    /// <summary>
    ///     Load dictionary directly from slice (no ref indirection).
    ///     Low-level method for rare dictionaries from system contracts.
    /// </summary>
    public static Dictionary<K, V> LoadDirect(IDictionaryKey<K> key, IDictionaryValue<V> value, Slice? slice)
    {
        if (slice == null)
            return Empty(key, value);

        var parsed = DictParser.ParseDict(slice, key.Bits, value.Parse);
        var map = new System.Collections.Generic.Dictionary<string, V>();

        foreach (var (k, v) in parsed)
        {
            var parsedKey = key.Parse(k);
            var serializedKey = SerializeInternalKey(parsedKey);
            map[serializedKey] = v;
        }

        return new Dictionary<K, V>(map, key, value);
    }

    /// <summary>
    ///     Get value by key.
    /// </summary>
    public V? Get(K key)
    {
        var serialized = SerializeInternalKey(key);
        return _map.TryGetValue(serialized, out var value) ? value : default;
    }

    /// <summary>
    ///     Check if key exists in dictionary.
    /// </summary>
    public bool Has(K key)
    {
        var serialized = SerializeInternalKey(key);
        return _map.ContainsKey(serialized);
    }

    /// <summary>
    ///     Set value for key.
    /// </summary>
    public Dictionary<K, V> Set(K key, V value)
    {
        var serialized = SerializeInternalKey(key);
        _map[serialized] = value;
        return this;
    }

    /// <summary>
    ///     Delete key from dictionary.
    /// </summary>
    public bool Delete(K key)
    {
        var serialized = SerializeInternalKey(key);
        return _map.Remove(serialized);
    }

    /// <summary>
    ///     Clear all entries.
    /// </summary>
    public void Clear() => _map.Clear();

    /// <summary>
    ///     Get all keys.
    /// </summary>
    public K[] Keys()
    {
        return _map.Keys.Select(DeserializeInternalKey).ToArray();
    }

    /// <summary>
    ///     Get all values.
    /// </summary>
    public V[] Values()
    {
        return _map.Values.ToArray();
    }

    /// <summary>
    ///     Store dictionary to builder (stores ref to dictionary root).
    /// </summary>
    public void Store(Builder builder, IDictionaryKey<K>? key = null, IDictionaryValue<V>? value = null)
    {
        if (_map.Count == 0)
        {
            builder.StoreBit(false); // null ref
        }
        else
        {
            // Resolve serializers
            var resolvedKey = key ?? _key ?? throw new InvalidOperationException("Key serializer is not defined");
            var resolvedValue = value ?? _value ?? throw new InvalidOperationException("Value serializer is not defined");

            // Serialize dictionary
            var prepared = new System.Collections.Generic.Dictionary<BigInteger, V>();
            foreach (var (k, v) in _map)
            {
                var deserializedKey = DeserializeInternalKey(k);
                prepared[resolvedKey.Serialize(deserializedKey)] = v;
            }

            var dictCell = DictSerializer.SerializeDict(prepared, resolvedKey.Bits, resolvedValue.Serialize);
            builder.StoreMaybeRef(dictCell);
        }
    }

    /// <summary>
    ///     Store dictionary directly to builder (no ref indirection).
    ///     Low-level method for rare dictionaries in system contracts.
    /// </summary>
    public void StoreDirect(Builder builder, IDictionaryKey<K>? key = null, IDictionaryValue<V>? value = null)
    {
        if (_map.Count == 0)
        {
            // Empty dictionary - do nothing
        }
        else
        {
            // Resolve serializers
            var resolvedKey = key ?? _key ?? throw new InvalidOperationException("Key serializer is not defined");
            var resolvedValue = value ?? _value ?? throw new InvalidOperationException("Value serializer is not defined");

            // Serialize dictionary
            var prepared = new System.Collections.Generic.Dictionary<BigInteger, V>();
            foreach (var (k, v) in _map)
            {
                var deserializedKey = DeserializeInternalKey(k);
                prepared[resolvedKey.Serialize(deserializedKey)] = v;
            }

            var dictCell = DictSerializer.SerializeDict(prepared, resolvedKey.Bits, resolvedValue.Serialize);
            if (dictCell != null)
            {
                builder.StoreSlice(dictCell.BeginParse());
            }
        }
    }

    /// <summary>
    ///     Enumerate all key-value pairs.
    /// </summary>
    public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        foreach (var (k, v) in _map)
        {
            yield return new KeyValuePair<K, V>(DeserializeInternalKey(k), v);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #region Internal Key Serialization

    static string SerializeInternalKey(K key)
    {
        return key switch
        {
            DictKeyInt i => i.Value.ToString(),
            DictKeyUint u => u.Value.ToString(),
            DictKeyBigInt bi => bi.Value.ToString(),
            DictKeyBuffer buf => Convert.ToBase64String(buf.Value),
            DictKeyBitString bits => bits.Value.ToString(),
            DictKeyAddress addr => addr.Value.ToString(),
            _ => throw new ArgumentException($"Unsupported key type: {typeof(K)}")
        };
    }

    static K DeserializeInternalKey(string serialized)
    {
        return typeof(K).Name switch
        {
            nameof(DictKeyInt) => (K)(object)new DictKeyInt(int.Parse(serialized)),
            nameof(DictKeyUint) => (K)(object)new DictKeyUint(uint.Parse(serialized)),
            nameof(DictKeyBigInt) => (K)(object)new DictKeyBigInt(BigInteger.Parse(serialized)),
            nameof(DictKeyBuffer) => (K)(object)new DictKeyBuffer(Convert.FromBase64String(serialized)),
            nameof(DictKeyBitString) => (K)(object)new DictKeyBitString(BitString.Parse(serialized)),
            nameof(DictKeyAddress) => (K)(object)new DictKeyAddress(Addresses.Address.Parse(serialized)),
            _ => throw new ArgumentException($"Unsupported key type: {typeof(K)}")
        };
    }

    #endregion
}


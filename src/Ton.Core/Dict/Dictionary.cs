using System.Collections;
using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     TON blockchain dictionary (Hashmap) implementation.
///     Dictionaries are stored as binary Patricia trees in BOC format.
/// </summary>
public class Dictionary<TK, TV> : IEnumerable<KeyValuePair<TK, TV>> where TK : IDictionaryKeyType
{
    readonly IDictionaryKey<TK>? key;
    readonly System.Collections.Generic.Dictionary<string, TV> map;
    readonly IDictionaryValue<TV>? value;

    Dictionary(System.Collections.Generic.Dictionary<string, TV> values, IDictionaryKey<TK>? key,
        IDictionaryValue<TV>? value)
    {
        this.key = key;
        this.value = value;
        map = values;
    }

    /// <summary>
    ///     Number of entries in the dictionary.
    /// </summary>
    public int Size => map.Count;

    /// <summary>
    ///     Enumerate all key-value pairs.
    /// </summary>
    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
        foreach ((string k, TV v) in map) yield return new KeyValuePair<TK, TV>(DeserializeInternalKey(k), v);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Create an empty dictionary.
    /// </summary>
    public static Dictionary<TK, TV> Empty(IDictionaryKey<TK>? key = null, IDictionaryValue<TV>? value = null)
    {
        return new Dictionary<TK, TV>(new System.Collections.Generic.Dictionary<string, TV>(), key, value);
    }

    /// <summary>
    ///     Load dictionary from slice (reads ref to dictionary root).
    /// </summary>
    public static Dictionary<TK, TV> Load(IDictionaryKey<TK> key, IDictionaryValue<TV> value, Slice slice)
    {
        Cell? cell = slice.LoadMaybeRef();
        if (cell != null && !cell.IsExotic) return LoadDirect(key, value, cell.BeginParse());

        return Empty(key, value);
    }

    /// <summary>
    ///     Load dictionary from slice (reads ref to dictionary root).
    /// </summary>
    public static Dictionary<TK, TV> Load(IDictionaryKey<TK> key, IDictionaryValue<TV> value, Cell cell)
    {
        if (cell.IsExotic)
            return Empty(key, value);

        Slice slice = cell.BeginParse();
        Cell? dictCell = slice.LoadMaybeRef();
        if (dictCell != null && !dictCell.IsExotic) return LoadDirect(key, value, dictCell.BeginParse());

        return Empty(key, value);
    }

    /// <summary>
    ///     Load dictionary directly from slice (no ref indirection).
    ///     Low-level method for rare dictionaries from system contracts.
    /// </summary>
    public static Dictionary<TK, TV> LoadDirect(IDictionaryKey<TK> key, IDictionaryValue<TV> value, Slice? slice)
    {
        if (slice == null)
            return Empty(key, value);

        System.Collections.Generic.Dictionary<BigInteger, TV>
            parsed = DictParser.ParseDict(slice, key.Bits, value.Parse);
        System.Collections.Generic.Dictionary<string, TV> map = new();

        foreach ((BigInteger k, TV v) in parsed)
        {
            TK parsedKey = key.Parse(k);
            string serializedKey = SerializeInternalKey(parsedKey);
            map[serializedKey] = v;
        }

        return new Dictionary<TK, TV>(map, key, value);
    }

    /// <summary>
    ///     Get value by key.
    /// </summary>
    public TV? Get(TK key)
    {
        string serialized = SerializeInternalKey(key);
        return map.TryGetValue(serialized, out TV? value) ? value : default;
    }

    /// <summary>
    ///     Check if key exists in dictionary.
    /// </summary>
    public bool Has(TK key)
    {
        string serialized = SerializeInternalKey(key);
        return map.ContainsKey(serialized);
    }

    /// <summary>
    ///     Set value for key.
    /// </summary>
    public Dictionary<TK, TV> Set(TK key, TV value)
    {
        string serialized = SerializeInternalKey(key);
        map[serialized] = value;
        return this;
    }

    /// <summary>
    ///     Delete key from dictionary.
    /// </summary>
    public bool Delete(TK key)
    {
        string serialized = SerializeInternalKey(key);
        return map.Remove(serialized);
    }

    /// <summary>
    ///     Clear all entries.
    /// </summary>
    public void Clear()
    {
        map.Clear();
    }

    /// <summary>
    ///     Get all keys.
    /// </summary>
    public TK[] Keys()
    {
        return map.Keys.Select(DeserializeInternalKey).ToArray();
    }

    /// <summary>
    ///     Get all values.
    /// </summary>
    public TV[] Values()
    {
        return map.Values.ToArray();
    }

    /// <summary>
    ///     Store dictionary to builder (stores ref to dictionary root).
    /// </summary>
    public void Store(Builder builder, IDictionaryKey<TK>? key = null, IDictionaryValue<TV>? value = null)
    {
        if (map.Count == 0)
        {
            builder.StoreBit(false); // null ref
        }
        else
        {
            // Resolve serializers
            IDictionaryKey<TK> resolvedKey =
                key ?? this.key ?? throw new InvalidOperationException("Key serializer is not defined");
            IDictionaryValue<TV> resolvedValue =
                value ?? this.value ?? throw new InvalidOperationException("Value serializer is not defined");

            // Serialize dictionary
            System.Collections.Generic.Dictionary<BigInteger, TV> prepared = new();
            foreach ((string k, TV v) in map)
            {
                TK deserializedKey = DeserializeInternalKey(k);
                prepared[resolvedKey.Serialize(deserializedKey)] = v;
            }

            Cell? dictCell = DictSerializer.SerializeDictToCell(prepared, resolvedKey.Bits, resolvedValue.Serialize);
            builder.StoreMaybeRef(dictCell);
        }
    }

    /// <summary>
    ///     Store dictionary directly to builder (no ref indirection).
    ///     Low-level method for rare dictionaries in system contracts.
    /// </summary>
    public void StoreDirect(Builder builder, IDictionaryKey<TK>? key = null, IDictionaryValue<TV>? value = null)
    {
        if (map.Count == 0)
        {
            // Empty dictionary - do nothing
        }
        else
        {
            // Resolve serializers
            IDictionaryKey<TK> resolvedKey =
                key ?? this.key ?? throw new InvalidOperationException("Key serializer is not defined");
            IDictionaryValue<TV> resolvedValue =
                value ?? this.value ?? throw new InvalidOperationException("Value serializer is not defined");

            // Serialize dictionary
            System.Collections.Generic.Dictionary<BigInteger, TV> prepared = new();
            foreach ((string k, TV v) in map)
            {
                TK deserializedKey = DeserializeInternalKey(k);
                prepared[resolvedKey.Serialize(deserializedKey)] = v;
            }

            DictSerializer.SerializeDict(prepared, resolvedKey.Bits, resolvedValue.Serialize, builder);
        }
    }

    #region Internal Key Serialization

    static string SerializeInternalKey(TK key)
    {
        return key switch
        {
            DictKeyInt i => i.Value.ToString(),
            DictKeyUint u => u.Value.ToString(),
            DictKeyBigInt bi => bi.Value.ToString(),
            DictKeyBuffer buf => Convert.ToBase64String(buf.Value),
            DictKeyBitString bits => bits.Value.ToString(),
            DictKeyAddress addr => addr.Value.ToString(),
            _ => throw new ArgumentException($"Unsupported key type: {typeof(TK)}")
        };
    }

    static TK DeserializeInternalKey(string serialized)
    {
        return typeof(TK).Name switch
        {
            nameof(DictKeyInt) => (TK)(object)new DictKeyInt(int.Parse(serialized)),
            nameof(DictKeyUint) => (TK)(object)new DictKeyUint(uint.Parse(serialized)),
            nameof(DictKeyBigInt) => (TK)(object)new DictKeyBigInt(BigInteger.Parse(serialized)),
            nameof(DictKeyBuffer) => (TK)(object)new DictKeyBuffer(Convert.FromBase64String(serialized)),
            nameof(DictKeyBitString) => (TK)(object)new DictKeyBitString(BitString.Parse(serialized)),
            nameof(DictKeyAddress) => (TK)(object)new DictKeyAddress(Address.Parse(serialized)),
            _ => throw new ArgumentException($"Unsupported key type: {typeof(TK)}")
        };
    }

    #endregion
}
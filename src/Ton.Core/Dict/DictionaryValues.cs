using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Factory methods for creating dictionary value serializers.
/// </summary>
public static class DictionaryValues
{
    /// <summary>
    ///     Create signed integer value serializer.
    /// </summary>
    public static IDictionaryValue<long> Int(int bits) => new IntValue(bits);

    /// <summary>
    ///     Create signed BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigInt(int bits) => new BigIntValue(bits);

    /// <summary>
    ///     Create var-length signed BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigVarInt(int headerBits) => new BigVarIntValue(headerBits);

    /// <summary>
    ///     Create unsigned integer value serializer.
    /// </summary>
    public static IDictionaryValue<ulong> Uint(int bits) => new UintValue(bits);

    /// <summary>
    ///     Create unsigned BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigUint(int bits) => new BigUintValue(bits);

    /// <summary>
    ///     Create var-length unsigned BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigVarUint(int headerBits) => new BigVarUintValue(headerBits);

    /// <summary>
    ///     Create boolean value serializer.
    /// </summary>
    public static IDictionaryValue<bool> Bool() => new BoolValue();

    /// <summary>
    ///     Create Address value serializer.
    /// </summary>
    public static IDictionaryValue<Address?> Address() => new AddressValue();

    /// <summary>
    ///     Create Cell value serializer.
    /// </summary>
    public static IDictionaryValue<Cell> Cell() => new CellValue();

    /// <summary>
    ///     Create Buffer value serializer.
    /// </summary>
    public static IDictionaryValue<byte[]> Buffer(int bytes) => new BufferValue(bytes);

    /// <summary>
    ///     Create BitString value serializer.
    /// </summary>
    public static IDictionaryValue<BitString> BitString(int bits) => new BitStringValue(bits);

    /// <summary>
    ///     Create nested Dictionary value serializer.
    /// </summary>
    public static IDictionaryValue<Dictionary<K, V>> Dictionary<K, V>(IDictionaryKey<K> key, IDictionaryValue<V> value)
        where K : IDictionaryKeyType
        => new DictionaryValue<K, V>(key, value);

    #region Value Implementations

    class IntValue : IDictionaryValue<long>
    {
        readonly int _bits;

        public IntValue(int bits) => _bits = bits;

        public void Serialize(long value, Builder builder) => builder.StoreInt(value, _bits);

        public long Parse(Slice slice)
        {
            var value = slice.LoadInt(_bits);
            slice.EndParse();
            return value;
        }
    }

    class BigIntValue : IDictionaryValue<BigInteger>
    {
        readonly int _bits;

        public BigIntValue(int bits) => _bits = bits;

        public void Serialize(BigInteger value, Builder builder) => builder.StoreInt(value, _bits);

        public BigInteger Parse(Slice slice)
        {
            var value = slice.LoadIntBig(_bits);
            slice.EndParse();
            return value;
        }
    }

    class BigVarIntValue : IDictionaryValue<BigInteger>
    {
        readonly int _headerBits;

        public BigVarIntValue(int headerBits) => _headerBits = headerBits;

        public void Serialize(BigInteger value, Builder builder) => builder.StoreVarInt(value, _headerBits);

        public BigInteger Parse(Slice slice)
        {
            var value = slice.LoadVarIntBig(_headerBits);
            slice.EndParse();
            return value;
        }
    }

    class UintValue : IDictionaryValue<ulong>
    {
        readonly int _bits;

        public UintValue(int bits) => _bits = bits;

        public void Serialize(ulong value, Builder builder) => builder.StoreUint(value, _bits);

        public ulong Parse(Slice slice)
        {
            var value = (ulong)slice.LoadUint(_bits);
            slice.EndParse();
            return value;
        }
    }

    class BigUintValue : IDictionaryValue<BigInteger>
    {
        readonly int _bits;

        public BigUintValue(int bits) => _bits = bits;

        public void Serialize(BigInteger value, Builder builder) => builder.StoreUint(value, _bits);

        public BigInteger Parse(Slice slice)
        {
            var value = slice.LoadUintBig(_bits);
            slice.EndParse();
            return value;
        }
    }

    class BigVarUintValue : IDictionaryValue<BigInteger>
    {
        readonly int _headerBits;

        public BigVarUintValue(int headerBits) => _headerBits = headerBits;

        public void Serialize(BigInteger value, Builder builder) => builder.StoreVarUint(value, _headerBits);

        public BigInteger Parse(Slice slice)
        {
            var value = slice.LoadVarUintBig(_headerBits);
            slice.EndParse();
            return value;
        }
    }

    class BoolValue : IDictionaryValue<bool>
    {
        public void Serialize(bool value, Builder builder) => builder.StoreBit(value);

        public bool Parse(Slice slice)
        {
            var value = slice.LoadBit();
            slice.EndParse();
            return value;
        }
    }

    class AddressValue : IDictionaryValue<Address?>
    {
        public void Serialize(Address? value, Builder builder) => builder.StoreAddress(value);

        public Address? Parse(Slice slice)
        {
            var addr = slice.LoadAddress();
            slice.EndParse();
            return addr;
        }
    }

    class CellValue : IDictionaryValue<Cell>
    {
        public void Serialize(Cell value, Builder builder) => builder.StoreRef(value);

        public Cell Parse(Slice slice)
        {
            var cell = slice.LoadRef();
            slice.EndParse();
            return cell;
        }
    }

    class BufferValue : IDictionaryValue<byte[]>
    {
        readonly int _bytes;

        public BufferValue(int bytes) => _bytes = bytes;

        public void Serialize(byte[] value, Builder builder) => builder.StoreBuffer(value);

        public byte[] Parse(Slice slice)
        {
            var buffer = slice.LoadBuffer(_bytes);
            slice.EndParse();
            return buffer;
        }
    }

    class BitStringValue : IDictionaryValue<BitString>
    {
        readonly int _bits;

        public BitStringValue(int bits) => _bits = bits;

        public void Serialize(BitString value, Builder builder) => builder.StoreBits(value);

        public BitString Parse(Slice slice)
        {
            var bitString = slice.LoadBits(_bits);
            slice.EndParse();
            return bitString;
        }
    }

    class DictionaryValue<K, V> : IDictionaryValue<Dictionary<K, V>> where K : IDictionaryKeyType
    {
        readonly IDictionaryKey<K> _key;
        readonly IDictionaryValue<V> _value;

        public DictionaryValue(IDictionaryKey<K> key, IDictionaryValue<V> value)
        {
            _key = key;
            _value = value;
        }

        public void Serialize(Dictionary<K, V> value, Builder builder) => builder.StoreDict(value);

        public Dictionary<K, V> Parse(Slice slice) => slice.LoadDict(_key, _value);
    }

    #endregion
}


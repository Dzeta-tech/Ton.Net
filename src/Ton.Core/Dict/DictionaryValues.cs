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
    public static IDictionaryValue<long> Int(int bits)
    {
        return new IntValue(bits);
    }

    /// <summary>
    ///     Create signed BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigInt(int bits)
    {
        return new BigIntValue(bits);
    }

    /// <summary>
    ///     Create var-length signed BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigVarInt(int headerBits)
    {
        return new BigVarIntValue(headerBits);
    }

    /// <summary>
    ///     Create unsigned integer value serializer.
    /// </summary>
    public static IDictionaryValue<ulong> Uint(int bits)
    {
        return new UintValue(bits);
    }

    /// <summary>
    ///     Create unsigned BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigUint(int bits)
    {
        return new BigUintValue(bits);
    }

    /// <summary>
    ///     Create var-length unsigned BigInteger value serializer.
    /// </summary>
    public static IDictionaryValue<BigInteger> BigVarUint(int headerBits)
    {
        return new BigVarUintValue(headerBits);
    }

    /// <summary>
    ///     Create boolean value serializer.
    /// </summary>
    public static IDictionaryValue<bool> Bool()
    {
        return new BoolValue();
    }

    /// <summary>
    ///     Create Address value serializer.
    /// </summary>
    public static IDictionaryValue<Address?> Address()
    {
        return new AddressValue();
    }

    /// <summary>
    ///     Create Cell value serializer.
    /// </summary>
    public static IDictionaryValue<Cell> Cell()
    {
        return new CellValue();
    }

    /// <summary>
    ///     Create Buffer value serializer.
    /// </summary>
    public static IDictionaryValue<byte[]> Buffer(int bytes)
    {
        return new BufferValue(bytes);
    }

    /// <summary>
    ///     Create BitString value serializer.
    /// </summary>
    public static IDictionaryValue<BitString> BitString(int bits)
    {
        return new BitStringValue(bits);
    }

    /// <summary>
    ///     Create nested Dictionary value serializer.
    /// </summary>
    public static IDictionaryValue<Dictionary<TK, TV>> Dictionary<TK, TV>(IDictionaryKey<TK> key,
        IDictionaryValue<TV> value)
        where TK : IDictionaryKeyType
    {
        return new DictionaryValue<TK, TV>(key, value);
    }

    #region Value Implementations

    class IntValue(int bits) : IDictionaryValue<long>
    {
        public void Serialize(long value, Builder builder)
        {
            builder.StoreInt(value, bits);
        }

        public long Parse(Slice slice)
        {
            long value = slice.LoadInt(bits);
            slice.EndParse();
            return value;
        }
    }

    class BigIntValue(int bits) : IDictionaryValue<BigInteger>
    {
        public void Serialize(BigInteger value, Builder builder)
        {
            builder.StoreInt(value, bits);
        }

        public BigInteger Parse(Slice slice)
        {
            BigInteger value = slice.LoadIntBig(bits);
            slice.EndParse();
            return value;
        }
    }

    class BigVarIntValue(int headerBits) : IDictionaryValue<BigInteger>
    {
        public void Serialize(BigInteger value, Builder builder)
        {
            builder.StoreVarInt(value, headerBits);
        }

        public BigInteger Parse(Slice slice)
        {
            BigInteger value = slice.LoadVarIntBig(headerBits);
            slice.EndParse();
            return value;
        }
    }

    class UintValue(int bits) : IDictionaryValue<ulong>
    {
        public void Serialize(ulong value, Builder builder)
        {
            builder.StoreUint(value, bits);
        }

        public ulong Parse(Slice slice)
        {
            ulong value = (ulong)slice.LoadUint(bits);
            slice.EndParse();
            return value;
        }
    }

    class BigUintValue(int bits) : IDictionaryValue<BigInteger>
    {
        public void Serialize(BigInteger value, Builder builder)
        {
            builder.StoreUint(value, bits);
        }

        public BigInteger Parse(Slice slice)
        {
            BigInteger value = slice.LoadUintBig(bits);
            slice.EndParse();
            return value;
        }
    }

    class BigVarUintValue(int headerBits) : IDictionaryValue<BigInteger>
    {
        public void Serialize(BigInteger value, Builder builder)
        {
            builder.StoreVarUint(value, headerBits);
        }

        public BigInteger Parse(Slice slice)
        {
            BigInteger value = slice.LoadVarUintBig(headerBits);
            slice.EndParse();
            return value;
        }
    }

    class BoolValue : IDictionaryValue<bool>
    {
        public void Serialize(bool value, Builder builder)
        {
            builder.StoreBit(value);
        }

        public bool Parse(Slice slice)
        {
            bool value = slice.LoadBit();
            slice.EndParse();
            return value;
        }
    }

    class AddressValue : IDictionaryValue<Address?>
    {
        public void Serialize(Address? value, Builder builder)
        {
            builder.StoreAddress(value);
        }

        public Address? Parse(Slice slice)
        {
            Address? addr = slice.LoadAddress();
            slice.EndParse();
            return addr;
        }
    }

    class CellValue : IDictionaryValue<Cell>
    {
        public void Serialize(Cell value, Builder builder)
        {
            builder.StoreRef(value);
        }

        public Cell Parse(Slice slice)
        {
            Cell cell = slice.LoadRef();
            slice.EndParse();
            return cell;
        }
    }

    class BufferValue(int bytes) : IDictionaryValue<byte[]>
    {
        public void Serialize(byte[] value, Builder builder)
        {
            builder.StoreBuffer(value);
        }

        public byte[] Parse(Slice slice)
        {
            byte[] buffer = slice.LoadBuffer(bytes);
            slice.EndParse();
            return buffer;
        }
    }

    class BitStringValue(int bits) : IDictionaryValue<BitString>
    {
        public void Serialize(BitString value, Builder builder)
        {
            builder.StoreBits(value);
        }

        public BitString Parse(Slice slice)
        {
            BitString bitString = slice.LoadBits(bits);
            slice.EndParse();
            return bitString;
        }
    }

    class DictionaryValue<TK, TV>(IDictionaryKey<TK> key, IDictionaryValue<TV> value)
        : IDictionaryValue<Dictionary<TK, TV>>
        where TK : IDictionaryKeyType
    {
        public void Serialize(Dictionary<TK, TV> value, Builder builder)
        {
            builder.StoreDict(value);
        }

        public Dictionary<TK, TV> Parse(Slice slice)
        {
            return slice.LoadDict(key, value);
        }
    }

    #endregion
}
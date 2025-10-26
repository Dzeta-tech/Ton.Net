using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

/// <summary>
///     Factory methods for creating dictionary key serializers.
/// </summary>
public static class DictionaryKeys
{
    /// <summary>
    ///     Create Address key serializer (267 bits).
    /// </summary>
    public static IDictionaryKey<DictKeyAddress> Address()
    {
        return new AddressKey();
    }

    /// <summary>
    ///     Create signed integer key serializer.
    /// </summary>
    public static IDictionaryKey<DictKeyBigInt> BigInt(int bits)
    {
        return new BigIntKey(bits);
    }

    /// <summary>
    ///     Create signed 32-bit integer key serializer.
    /// </summary>
    public static IDictionaryKey<DictKeyInt> Int(int bits)
    {
        return new IntKey(bits);
    }

    /// <summary>
    ///     Create unsigned bigint key serializer.
    /// </summary>
    public static IDictionaryKey<DictKeyBigInt> BigUint(int bits)
    {
        return new BigUintKey(bits);
    }

    /// <summary>
    ///     Create unsigned 32-bit integer key serializer.
    /// </summary>
    public static IDictionaryKey<DictKeyUint> Uint(int bits)
    {
        return new UintKey(bits);
    }

    /// <summary>
    ///     Create buffer key serializer.
    /// </summary>
    public static IDictionaryKey<DictKeyBuffer> Buffer(int bytes)
    {
        return new BufferKey(bytes);
    }

    /// <summary>
    ///     Create BitString key serializer (for non-8-bit-aligned keys).
    /// </summary>
    public static IDictionaryKey<DictKeyBitString> BitString(int bits)
    {
        return new BitStringKey(bits);
    }

    #region Key Implementations

    class AddressKey : IDictionaryKey<DictKeyAddress>
    {
        public int Bits => 267;

        public BigInteger Serialize(DictKeyAddress key)
        {
            Builder builder = new();
            builder.StoreAddress(key.Value);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(267);
        }

        public DictKeyAddress Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, 267);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyAddress(slice.LoadAddress()!);
        }
    }

    class BigIntKey(int bits) : IDictionaryKey<DictKeyBigInt>
    {
        public int Bits { get; } = bits;

        public BigInteger Serialize(DictKeyBigInt key)
        {
            Builder builder = new();
            builder.StoreInt(key.Value, Bits);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBigInt Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyBigInt(slice.LoadIntBig(Bits));
        }
    }

    class IntKey(int bits) : IDictionaryKey<DictKeyInt>
    {
        public int Bits { get; } = bits;

        public BigInteger Serialize(DictKeyInt key)
        {
            Builder builder = new();
            builder.StoreInt(key.Value, Bits);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyInt Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyInt((int)slice.LoadInt(Bits));
        }
    }

    class BigUintKey(int bits) : IDictionaryKey<DictKeyBigInt>
    {
        public int Bits { get; } = bits;

        public BigInteger Serialize(DictKeyBigInt key)
        {
            if (key.Value < 0)
                throw new ArgumentException($"Key is negative: {key.Value}");

            Builder builder = new();
            builder.StoreUint(key.Value, Bits);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBigInt Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyBigInt(slice.LoadUintBig(Bits));
        }
    }

    class UintKey(int bits) : IDictionaryKey<DictKeyUint>
    {
        public int Bits { get; } = bits;

        public BigInteger Serialize(DictKeyUint key)
        {
            Builder builder = new();
            builder.StoreUint(key.Value, Bits);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyUint Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyUint((uint)slice.LoadUint(Bits));
        }
    }

    class BufferKey(int bytes) : IDictionaryKey<DictKeyBuffer>
    {
        public int Bits { get; } = bytes * 8;

        public BigInteger Serialize(DictKeyBuffer key)
        {
            Builder builder = new();
            builder.StoreBuffer(key.Value);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBuffer Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            Slice slice = cell.BeginParse();
            return new DictKeyBuffer(slice.LoadBuffer(Bits / 8));
        }
    }

    class BitStringKey(int bits) : IDictionaryKey<DictKeyBitString>
    {
        public int Bits { get; } = bits;

        public BigInteger Serialize(DictKeyBitString key)
        {
            if (key.Value.Length != Bits)
                throw new ArgumentException($"BitString length mismatch: expected {Bits}, got {key.Value.Length}");

            Builder builder = new();
            builder.StoreBits(key.Value);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBitString Parse(BigInteger value)
        {
            Builder builder = new();
            builder.StoreUint(value, Bits);
            Cell cell = builder.EndCell();
            BitReader reader = new(cell.Bits);
            return new DictKeyBitString(reader.LoadBits(Bits));
        }
    }

    #endregion
}
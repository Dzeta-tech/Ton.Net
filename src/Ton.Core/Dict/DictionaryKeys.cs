using System.Numerics;
using Ton.Core.Addresses;
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
            var builder = new Builder();
            builder.StoreAddress(key.Value);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(267);
        }

        public DictKeyAddress Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, 267);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyAddress(slice.LoadAddress()!);
        }
    }

    class BigIntKey : IDictionaryKey<DictKeyBigInt>
    {
        public int Bits { get; }

        public BigIntKey(int bits) => Bits = bits;

        public BigInteger Serialize(DictKeyBigInt key)
        {
            var builder = new Builder();
            builder.StoreInt(key.Value, Bits);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBigInt Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyBigInt(slice.LoadIntBig(Bits));
        }
    }

    class IntKey : IDictionaryKey<DictKeyInt>
    {
        public int Bits { get; }

        public IntKey(int bits) => Bits = bits;

        public BigInteger Serialize(DictKeyInt key)
        {
            var builder = new Builder();
            builder.StoreInt(key.Value, Bits);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyInt Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyInt((int)slice.LoadInt(Bits));
        }
    }

    class BigUintKey : IDictionaryKey<DictKeyBigInt>
    {
        public int Bits { get; }

        public BigUintKey(int bits) => Bits = bits;

        public BigInteger Serialize(DictKeyBigInt key)
        {
            if (key.Value < 0)
                throw new ArgumentException($"Key is negative: {key.Value}");

            var builder = new Builder();
            builder.StoreUint(key.Value, Bits);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBigInt Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyBigInt(slice.LoadUintBig(Bits));
        }
    }

    class UintKey : IDictionaryKey<DictKeyUint>
    {
        public int Bits { get; }

        public UintKey(int bits) => Bits = bits;

        public BigInteger Serialize(DictKeyUint key)
        {
            var builder = new Builder();
            builder.StoreUint(key.Value, Bits);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyUint Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyUint((uint)slice.LoadUint(Bits));
        }
    }

    class BufferKey : IDictionaryKey<DictKeyBuffer>
    {
        public int Bits { get; }

        public BufferKey(int bytes) => Bits = bytes * 8;

        public BigInteger Serialize(DictKeyBuffer key)
        {
            var builder = new Builder();
            builder.StoreBuffer(key.Value);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBuffer Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var slice = cell.BeginParse();
            return new DictKeyBuffer(slice.LoadBuffer(Bits / 8));
        }
    }

    class BitStringKey : IDictionaryKey<DictKeyBitString>
    {
        public int Bits { get; }

        public BitStringKey(int bits) => Bits = bits;

        public BigInteger Serialize(DictKeyBitString key)
        {
            if (key.Value.Length != Bits)
                throw new ArgumentException($"BitString length mismatch: expected {Bits}, got {key.Value.Length}");

            var builder = new Builder();
            builder.StoreBits(key.Value);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return reader.PreloadUintBig(Bits);
        }

        public DictKeyBitString Parse(BigInteger value)
        {
            var builder = new Builder();
            builder.StoreUint(value, Bits);
            var cell = builder.EndCell();
            var reader = new BitReader(cell.Bits);
            return new DictKeyBitString(reader.LoadBits(Bits));
        }
    }

    #endregion
}


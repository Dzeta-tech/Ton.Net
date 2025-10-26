using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

// Marker types for dictionary keys
public readonly struct DictKeyInt(int value) : IDictionaryKeyType
{
    public int Value { get; } = value;

    public static implicit operator int(DictKeyInt key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyInt(int value)
    {
        return new DictKeyInt(value);
    }
}

public readonly record struct DictKeyUint(uint Value) : IDictionaryKeyType
{
    public uint Value { get; } = Value;

    public static implicit operator uint(DictKeyUint key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyUint(uint value)
    {
        return new DictKeyUint(value);
    }
}

public readonly struct DictKeyBigInt(BigInteger value) : IDictionaryKeyType
{
    public BigInteger Value { get; } = value;

    public static implicit operator BigInteger(DictKeyBigInt key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyBigInt(BigInteger value)
    {
        return new DictKeyBigInt(value);
    }
}

public readonly struct DictKeyBuffer(byte[] value) : IDictionaryKeyType
{
    public byte[] Value { get; } = value;

    public static implicit operator byte[](DictKeyBuffer key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyBuffer(byte[] value)
    {
        return new DictKeyBuffer(value);
    }
}

public readonly struct DictKeyBitString(BitString value) : IDictionaryKeyType
{
    public BitString Value { get; } = value;

    public static implicit operator BitString(DictKeyBitString key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyBitString(BitString value)
    {
        return new DictKeyBitString(value);
    }
}

public readonly struct DictKeyAddress(Address value) : IDictionaryKeyType
{
    public Address Value { get; } = value;

    public static implicit operator Address(DictKeyAddress key)
    {
        return key.Value;
    }

    public static implicit operator DictKeyAddress(Address value)
    {
        return new DictKeyAddress(value);
    }
}
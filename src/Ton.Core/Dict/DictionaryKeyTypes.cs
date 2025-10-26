using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Dict;

// Marker types for dictionary keys
public readonly struct DictKeyInt : IDictionaryKeyType
{
    public int Value { get; }

    public DictKeyInt(int value) => Value = value;

    public static implicit operator int(DictKeyInt key) => key.Value;
    public static implicit operator DictKeyInt(int value) => new(value);
}

public readonly struct DictKeyUint : IDictionaryKeyType
{
    public uint Value { get; }

    public DictKeyUint(uint value) => Value = value;

    public static implicit operator uint(DictKeyUint key) => key.Value;
    public static implicit operator DictKeyUint(uint value) => new(value);
}

public readonly struct DictKeyBigInt : IDictionaryKeyType
{
    public BigInteger Value { get; }

    public DictKeyBigInt(BigInteger value) => Value = value;

    public static implicit operator BigInteger(DictKeyBigInt key) => key.Value;
    public static implicit operator DictKeyBigInt(BigInteger value) => new(value);
}

public readonly struct DictKeyBuffer : IDictionaryKeyType
{
    public byte[] Value { get; }

    public DictKeyBuffer(byte[] value) => Value = value;

    public static implicit operator byte[](DictKeyBuffer key) => key.Value;
    public static implicit operator DictKeyBuffer(byte[] value) => new(value);
}

public readonly struct DictKeyBitString : IDictionaryKeyType
{
    public BitString Value { get; }

    public DictKeyBitString(BitString value) => Value = value;

    public static implicit operator BitString(DictKeyBitString key) => key.Value;
    public static implicit operator DictKeyBitString(BitString value) => new(value);
}

public readonly struct DictKeyAddress : IDictionaryKeyType
{
    public Address Value { get; }

    public DictKeyAddress(Address value) => Value = value;

    public static implicit operator Address(DictKeyAddress key) => key.Value;
    public static implicit operator DictKeyAddress(Address value) => new(value);
}


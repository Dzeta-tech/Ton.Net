using System.Numerics;
using Ton.Core.Addresses;

namespace Ton.Core.Boc;

/// <summary>
/// Reader for sequentially reading bits from a BitString.
/// Supports reading bits, integers, addresses, and other TON-specific types.
/// </summary>
public class BitReader
{
    readonly BitString _bits;
    int _offset;
    readonly Stack<int> _checkpoints = new();

    /// <summary>
    /// Creates a new BitReader for the given BitString.
    /// </summary>
    /// <param name="bits">The bit string to read from.</param>
    /// <param name="offset">Initial offset in bits (default: 0).</param>
    public BitReader(BitString bits, int offset = 0)
    {
        _bits = bits;
        _offset = offset;
    }

    /// <summary>
    /// Gets the current read offset in bits.
    /// </summary>
    public int Offset => _offset;

    /// <summary>
    /// Gets the number of bits remaining to read.
    /// </summary>
    public int Remaining => _bits.Length - _offset;

    /// <summary>
    /// Skips the specified number of bits.
    /// </summary>
    /// <param name="bits">Number of bits to skip.</param>
    public void Skip(int bits)
    {
        if (bits < 0 || _offset + bits > _bits.Length)
            throw new ArgumentOutOfRangeException(nameof(bits), $"Index {_offset + bits} is out of bounds");
        _offset += bits;
    }

    /// <summary>
    /// Resets the reader to the beginning or last checkpoint.
    /// </summary>
    public void Reset()
    {
        if (_checkpoints.Count > 0)
            _offset = _checkpoints.Pop();
        else
            _offset = 0;
    }

    /// <summary>
    /// Saves the current position as a checkpoint.
    /// </summary>
    public void Save()
    {
        _checkpoints.Push(_offset);
    }

    /// <summary>
    /// Loads a single bit and advances the offset.
    /// </summary>
    /// <returns>True if bit is set, false otherwise.</returns>
    public bool LoadBit()
    {
        bool result = _bits.At(_offset);
        _offset++;
        return result;
    }

    /// <summary>
    /// Reads a single bit without advancing the offset.
    /// </summary>
    /// <returns>True if bit is set, false otherwise.</returns>
    public bool PreloadBit()
    {
        return _bits.At(_offset);
    }

    /// <summary>
    /// Loads a bit string and advances the offset.
    /// </summary>
    /// <param name="bits">Number of bits to load.</param>
    /// <returns>New BitString.</returns>
    public BitString LoadBits(int bits)
    {
        BitString result = _bits.Substring(_offset, bits);
        _offset += bits;
        return result;
    }

    /// <summary>
    /// Reads a bit string without advancing the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read.</param>
    /// <returns>New BitString.</returns>
    public BitString PreloadBits(int bits)
    {
        return _bits.Substring(_offset, bits);
    }

    /// <summary>
    /// Loads a byte buffer and advances the offset.
    /// </summary>
    /// <param name="bytes">Number of bytes to load.</param>
    /// <returns>Byte array.</returns>
    public byte[] LoadBuffer(int bytes)
    {
        byte[] result = PreloadBuffer(bytes);
        _offset += bytes * 8;
        return result;
    }

    /// <summary>
    /// Reads a byte buffer without advancing the offset.
    /// </summary>
    /// <param name="bytes">Number of bytes to read.</param>
    /// <returns>Byte array.</returns>
    public byte[] PreloadBuffer(int bytes)
    {
        byte[] result = new byte[bytes];
        for (int i = 0; i < bytes; i++)
        {
            result[i] = (byte)PreloadUint(8, _offset + i * 8);
        }
        return result;
    }

    /// <summary>
    /// Loads an unsigned integer and advances the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read.</param>
    /// <returns>Value as long.</returns>
    public long LoadUint(int bits)
    {
        BigInteger result = LoadUintBig(bits);
        if (result > long.MaxValue)
            throw new OverflowException($"Value {result} doesn't fit in long");
        return (long)result;
    }

    /// <summary>
    /// Loads an unsigned integer as BigInteger and advances the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read.</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger LoadUintBig(int bits)
    {
        BigInteger result = PreloadUintBig(bits);
        _offset += bits;
        return result;
    }

    /// <summary>
    /// Reads an unsigned integer without advancing the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read.</param>
    /// <returns>Value as long.</returns>
    public long PreloadUint(int bits)
    {
        return PreloadUint(bits, _offset);
    }

    /// <summary>
    /// Reads an unsigned integer as BigInteger without advancing the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read.</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger PreloadUintBig(int bits)
    {
        return PreloadUintBig(bits, _offset);
    }

    long PreloadUint(int bits, int offset)
    {
        BigInteger result = PreloadUintBig(bits, offset);
        if (result > long.MaxValue)
            throw new OverflowException($"Value {result} doesn't fit in long");
        return (long)result;
    }

    BigInteger PreloadUintBig(int bits, int offset)
    {
        if (bits == 0)
            return BigInteger.Zero;

        BigInteger result = BigInteger.Zero;
        for (int i = 0; i < bits; i++)
        {
            if (_bits.At(offset + i))
            {
                result |= BigInteger.One << (bits - i - 1);
            }
        }
        return result;
    }

    /// <summary>
    /// Loads a signed integer and advances the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read (includes sign bit).</param>
    /// <returns>Value as long.</returns>
    public long LoadInt(int bits)
    {
        BigInteger result = LoadIntBig(bits);
        if (result > long.MaxValue || result < long.MinValue)
            throw new OverflowException($"Value {result} doesn't fit in long");
        return (long)result;
    }

    /// <summary>
    /// Loads a signed integer as BigInteger and advances the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read (includes sign bit).</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger LoadIntBig(int bits)
    {
        BigInteger result = PreloadIntBig(bits);
        _offset += bits;
        return result;
    }

    /// <summary>
    /// Reads a signed integer without advancing the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read (includes sign bit).</param>
    /// <returns>Value as long.</returns>
    public long PreloadInt(int bits)
    {
        BigInteger result = PreloadIntBig(bits, _offset);
        if (result > long.MaxValue || result < long.MinValue)
            throw new OverflowException($"Value {result} doesn't fit in long");
        return (long)result;
    }

    /// <summary>
    /// Reads a signed integer as BigInteger without advancing the offset.
    /// </summary>
    /// <param name="bits">Number of bits to read (includes sign bit).</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger PreloadIntBig(int bits)
    {
        return PreloadIntBig(bits, _offset);
    }

    BigInteger PreloadIntBig(int bits, int offset)
    {
        if (bits == 0)
            return BigInteger.Zero;
        if (bits == 1)
            return _bits.At(offset) ? -BigInteger.One : BigInteger.Zero;

        // Read sign bit
        bool sign = _bits.At(offset);
        BigInteger value = PreloadUintBig(bits - 1, offset + 1);

        if (sign)
        {
            return value - (BigInteger.One << (bits - 1));
        }
        return value;
    }

    /// <summary>
    /// Loads a variable-length unsigned integer and advances the offset.
    /// </summary>
    /// <param name="headerBits">Number of bits in the length header.</param>
    /// <returns>Value as long.</returns>
    public long LoadVarUint(int headerBits)
    {
        int size = (int)LoadUint(headerBits);
        if (size == 0)
            return 0;
        return LoadUint(size * 8);
    }

    /// <summary>
    /// Loads a variable-length unsigned integer as BigInteger and advances the offset.
    /// </summary>
    /// <param name="headerBits">Number of bits in the length header.</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger LoadVarUintBig(int headerBits)
    {
        int size = (int)LoadUint(headerBits);
        if (size == 0)
            return BigInteger.Zero;
        return LoadUintBig(size * 8);
    }

    /// <summary>
    /// Loads a variable-length signed integer and advances the offset.
    /// </summary>
    /// <param name="headerBits">Number of bits in the length header.</param>
    /// <returns>Value as long.</returns>
    public long LoadVarInt(int headerBits)
    {
        int size = (int)LoadUint(headerBits);
        if (size == 0)
            return 0;
        return LoadInt(size * 8);
    }

    /// <summary>
    /// Loads a variable-length signed integer as BigInteger and advances the offset.
    /// </summary>
    /// <param name="headerBits">Number of bits in the length header.</param>
    /// <returns>Value as BigInteger.</returns>
    public BigInteger LoadVarIntBig(int headerBits)
    {
        int size = (int)LoadUint(headerBits);
        if (size == 0)
            return BigInteger.Zero;
        return LoadIntBig(size * 8);
    }

    /// <summary>
    /// Loads a coin amount in nanotons and advances the offset.
    /// </summary>
    /// <returns>Amount in nanotons.</returns>
    public BigInteger LoadCoins()
    {
        return LoadVarUintBig(4);
    }

    /// <summary>
    /// Loads a TON address and advances the offset.
    /// </summary>
    /// <returns>Address or null if empty address.</returns>
    public Address? LoadAddress()
    {
        int type = (int)LoadUint(2);

        if (type == 0)
            return null; // Empty address

        if (type == 2)
        {
            // Internal address
            Skip(1); // anycast (not supported, must be 0)
            int workchain = (int)LoadInt(8);
            byte[] hash = LoadBuffer(32);
            return new Address(workchain, hash);
        }

        throw new InvalidOperationException($"Unsupported address type: {type}");
    }

    /// <summary>
    /// Preload address (read without advancing offset).
    /// </summary>
    /// <returns>Address or null if no address.</returns>
    public Address? PreloadAddress()
    {
        int savedOffset = _offset;
        var address = LoadAddress();
        _offset = savedOffset;
        return address;
    }

    /// <summary>
    /// Loads maybe address (with presence bit).
    /// </summary>
    /// <returns>Address or null.</returns>
    public Address? LoadMaybeAddress()
    {
        if (LoadBit())
        {
            return LoadAddress();
        }
        return null;
    }

    /// <summary>
    /// Preload varuint (read without advancing offset).
    /// </summary>
    /// <param name="bits">Number of bits for length encoding.</param>
    /// <returns>Varuint value.</returns>
    public long PreloadVarUint(int bits)
    {
        int savedOffset = _offset;
        var value = LoadVarUint(bits);
        _offset = savedOffset;
        return value;
    }

    /// <summary>
    /// Preload varuint big (read without advancing offset).
    /// </summary>
    /// <param name="bits">Number of bits for length encoding.</param>
    /// <returns>Varuint value as BigInteger.</returns>
    public BigInteger PreloadVarUintBig(int bits)
    {
        int savedOffset = _offset;
        var value = LoadVarUintBig(bits);
        _offset = savedOffset;
        return value;
    }

    /// <summary>
    /// Preload varint (read without advancing offset).
    /// </summary>
    /// <param name="bits">Number of bits for length encoding.</param>
    /// <returns>Varint value.</returns>
    public long PreloadVarInt(int bits)
    {
        int savedOffset = _offset;
        var value = LoadVarInt(bits);
        _offset = savedOffset;
        return value;
    }

    /// <summary>
    /// Preload varint big (read without advancing offset).
    /// </summary>
    /// <param name="bits">Number of bits for length encoding.</param>
    /// <returns>Varint value as BigInteger.</returns>
    public BigInteger PreloadVarIntBig(int bits)
    {
        int savedOffset = _offset;
        var value = LoadVarIntBig(bits);
        _offset = savedOffset;
        return value;
    }

    /// <summary>
    /// Preload coins (read without advancing offset).
    /// </summary>
    /// <returns>Coins value as BigInteger.</returns>
    public BigInteger PreloadCoins()
    {
        int savedOffset = _offset;
        var value = LoadCoins();
        _offset = savedOffset;
        return value;
    }

    /// <summary>
    /// Clone this BitReader (creates a copy at the current offset).
    /// </summary>
    /// <returns>Cloned BitReader.</returns>
    public BitReader Clone()
    {
        var clone = new BitReader(_bits, _offset);
        foreach (var checkpoint in _checkpoints.Reverse())
        {
            clone._checkpoints.Push(checkpoint);
        }
        return clone;
    }
}


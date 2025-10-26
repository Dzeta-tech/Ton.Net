using System.Numerics;
using Ton.Core.Addresses;

namespace Ton.Core.Boc;

/// <summary>
///     Builder for constructing bit strings efficiently.
///     Supports writing bits, integers, addresses, and other TON-specific types.
/// </summary>
public class BitBuilder
{
    readonly byte[] buffer;

    /// <summary>
    ///     Creates a new BitBuilder with the specified maximum size in bits.
    /// </summary>
    /// <param name="size">Maximum size in bits (default: 1023).</param>
    public BitBuilder(int size = 1023)
    {
        buffer = new byte[(int)Math.Ceiling(size / 8.0)];
        Length = 0;
    }

    /// <summary>
    ///     Gets the current number of bits written.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    ///     Writes a single bit.
    /// </summary>
    /// <param name="value">True for 1, false for 0.</param>
    /// <exception cref="InvalidOperationException">Thrown when buffer overflows.</exception>
    public void WriteBit(bool value)
    {
        if (Length >= buffer.Length * 8)
            throw new InvalidOperationException("BitBuilder overflow");

        if (value) buffer[Length / 8] |= (byte)(1 << (7 - Length % 8));

        Length++;
    }

    /// <summary>
    ///     Writes bits from another BitString.
    /// </summary>
    /// <param name="src">Source bit string.</param>
    public void WriteBits(BitString src)
    {
        for (int i = 0; i < src.Length; i++) WriteBit(src.At(i));
    }

    /// <summary>
    ///     Writes bytes from a buffer.
    /// </summary>
    /// <param name="src">Source buffer.</param>
    public void WriteBuffer(byte[] src)
    {
        // Optimized path for byte-aligned writes
        if (Length % 8 == 0)
        {
            if (Length + src.Length * 8 > buffer.Length * 8)
                throw new InvalidOperationException("BitBuilder overflow");

            Array.Copy(src, 0, buffer, Length / 8, src.Length);
            Length += src.Length * 8;
        }
        else
        {
            // Unaligned - write byte by byte as uint
            foreach (byte b in src) WriteUint(b, 8);
        }
    }

    /// <summary>
    ///     Writes an unsigned integer value.
    /// </summary>
    /// <param name="value">Value to write (as BigInteger or long).</param>
    /// <param name="bits">Number of bits to use.</param>
    public void WriteUint(BigInteger value, int bits)
    {
        if (bits is < 0 or > int.MaxValue)
            throw new ArgumentException($"Invalid bit length. Got {bits}");

        if (bits == 0)
        {
            if (value != 0)
                throw new ArgumentException($"Value is not zero for {bits} bits. Got {value}");
            return;
        }

        BigInteger maxValue = BigInteger.One << bits;
        if (value < 0 || value >= maxValue)
            throw new ArgumentException($"bitLength is too small for value {value}. Got {bits}");

        if (Length + bits > buffer.Length * 8)
            throw new InvalidOperationException("BitBuilder overflow");

        // Write bits
        int tillByte = 8 - Length % 8;
        if (tillByte > 0)
        {
            int bidx = Length / 8;
            if (bits < tillByte)
            {
                byte wb = (byte)value;
                buffer[bidx] |= (byte)(wb << (tillByte - bits));
                Length += bits;
                return;
            }
            else
            {
                byte wb = (byte)(value >> (bits - tillByte));
                buffer[bidx] |= wb;
                Length += tillByte;
                bits -= tillByte;
            }
        }

        while (bits > 0)
            if (bits >= 8)
            {
                buffer[Length / 8] = (byte)((value >> (bits - 8)) & 0xFF);
                Length += 8;
                bits -= 8;
            }
            else
            {
                buffer[Length / 8] = (byte)((value << (8 - bits)) & 0xFF);
                Length += bits;
                bits = 0;
            }
    }

    /// <summary>
    ///     Writes an unsigned integer value.
    /// </summary>
    public void WriteUint(long value, int bits)
    {
        WriteUint(new BigInteger(value), bits);
    }

    /// <summary>
    ///     Writes a signed integer value.
    /// </summary>
    /// <param name="value">Value to write.</param>
    /// <param name="bits">Number of bits to use (includes sign bit).</param>
    public void WriteInt(BigInteger value, int bits)
    {
        if (bits is < 0 or > int.MaxValue)
            throw new ArgumentException($"Invalid bit length. Got {bits}");

        if (bits == 0)
        {
            if (value != 0)
                throw new ArgumentException($"Value is not zero for {bits} bits. Got {value}");
            return;
        }

        if (bits == 1)
        {
            if (value != -1 && value != 0)
                throw new ArgumentException($"Value is not zero or -1 for {bits} bits. Got {value}");
            WriteBit(value == -1);
            return;
        }

        BigInteger minValue = -(BigInteger.One << (bits - 1));
        BigInteger maxValue = BigInteger.One << (bits - 1);
        if (value < minValue || value >= maxValue)
            throw new ArgumentException($"Value is out of range for {bits} bits. Got {value}");

        // Write sign bit
        if (value < 0)
        {
            WriteBit(true);
            value = (BigInteger.One << (bits - 1)) + value;
        }
        else
        {
            WriteBit(false);
        }

        // Write value
        WriteUint(value, bits - 1);
    }

    /// <summary>
    ///     Writes a signed integer value.
    /// </summary>
    public void WriteInt(long value, int bits)
    {
        WriteInt(new BigInteger(value), bits);
    }

    /// <summary>
    ///     Writes a variable-length unsigned integer (used for coins).
    /// </summary>
    /// <param name="value">Value to write.</param>
    /// <param name="headerBits">Number of bits for the length header.</param>
    public void WriteVarUint(BigInteger value, int headerBits)
    {
        if (headerBits is < 0 or > int.MaxValue)
            throw new ArgumentException($"Invalid bit length. Got {headerBits}");
        if (value < 0)
            throw new ArgumentException($"Value is negative. Got {value}");

        if (value == 0)
        {
            WriteUint(0, headerBits);
            return;
        }

        // Calculate size in bytes
        int sizeBytes = (int)Math.Ceiling(value.GetBitLength() / 8.0);
        int sizeBits = sizeBytes * 8;

        // Write size
        WriteUint(sizeBytes, headerBits);

        // Write number
        WriteUint(value, sizeBits);
    }

    /// <summary>
    ///     Writes a variable-length unsigned integer (used for coins).
    /// </summary>
    public void WriteVarUint(long value, int headerBits)
    {
        WriteVarUint(new BigInteger(value), headerBits);
    }

    /// <summary>
    ///     Writes a variable-length signed integer.
    /// </summary>
    /// <param name="value">Value to write.</param>
    /// <param name="headerBits">Number of bits for the length header.</param>
    public void WriteVarInt(BigInteger value, int headerBits)
    {
        if (headerBits is < 0 or > int.MaxValue)
            throw new ArgumentException($"Invalid bit length. Got {headerBits}");

        if (value == 0)
        {
            WriteUint(0, headerBits);
            return;
        }

        // Calculate size in bytes
        BigInteger absValue = value > 0 ? value : -value;
        int sizeBytes = (int)Math.Ceiling((absValue.GetBitLength() + 1) / 8.0);
        int sizeBits = sizeBytes * 8;

        // Write size
        WriteUint(sizeBytes, headerBits);

        // Write number
        WriteInt(value, sizeBits);
    }

    /// <summary>
    ///     Writes a variable-length signed integer.
    /// </summary>
    public void WriteVarInt(long value, int headerBits)
    {
        WriteVarInt(new BigInteger(value), headerBits);
    }

    /// <summary>
    ///     Writes a coin amount in nanotons (varuint16 format).
    /// </summary>
    /// <param name="amount">Amount in nanotons.</param>
    public void WriteCoins(BigInteger amount)
    {
        WriteVarUint(amount, 4);
    }

    /// <summary>
    ///     Writes a coin amount in nanotons (varuint16 format).
    /// </summary>
    public void WriteCoins(long amount)
    {
        WriteVarUint(amount, 4);
    }

    /// <summary>
    ///     Writes a TON address (internal or external) or null for empty address.
    /// </summary>
    /// <param name="address">Address to write, or null for empty address.</param>
    public void WriteAddress(Address? address)
    {
        if (address == null)
        {
            WriteUint(0, 2); // Empty address
            return;
        }

        // Internal address
        WriteUint(2, 2); // Address type
        WriteUint(0, 1); // No anycast
        WriteInt(address.WorkChain, 8);
        WriteBuffer(address.Hash);
    }

    /// <summary>
    ///     Writes an external address (can be Address or ExternalAddress or null).
    /// </summary>
    /// <param name="address">Address to write.</param>
    public void WriteAddressExt(object? address)
    {
        if (address == null)
        {
            WriteUint(0, 2); // Empty address
            return;
        }

        // Internal address
        if (address is Address internalAddr)
        {
            WriteUint(2, 2); // Internal address type
            WriteUint(0, 1); // No anycast
            WriteInt(internalAddr.WorkChain, 8);
            WriteBuffer(internalAddr.Hash);
            return;
        }

        // External address
        if (address is ExternalAddress externalAddr)
        {
            WriteUint(1, 2); // External address type
            WriteUint(externalAddr.Bits, 9);
            WriteUint(externalAddr.Value, externalAddr.Bits);
            return;
        }

        throw new InvalidOperationException($"Invalid address type: {address.GetType()}");
    }

    /// <summary>
    ///     Builds the final BitString.
    /// </summary>
    /// <returns>Immutable BitString containing the written bits.</returns>
    public BitString Build()
    {
        return new BitString(buffer, 0, Length);
    }

    /// <summary>
    ///     Returns the underlying byte buffer (only valid if byte-aligned).
    /// </summary>
    /// <returns>Byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if not byte-aligned.</exception>
    public byte[] Buffer()
    {
        if (Length % 8 != 0)
            throw new InvalidOperationException("BitBuilder buffer is not byte aligned");

        return buffer[..(Length / 8)];
    }
}
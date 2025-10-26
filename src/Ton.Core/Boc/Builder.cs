using System.Numerics;
using System.Text;
using Ton.Core.Addresses;
using Ton.Core.Dict;

namespace Ton.Core.Boc;

/// <summary>
///     Builder for creating cells.
/// </summary>
public class Builder
{
    readonly BitBuilder bits;
    readonly List<Cell> refs;

    /// <summary>
    ///     Creates a new builder.
    /// </summary>
    public Builder()
    {
        bits = new BitBuilder();
        refs = [];
    }

    /// <summary>
    ///     Gets the number of bits written.
    /// </summary>
    public int Bits => bits.Length;

    /// <summary>
    ///     Gets the number of references written.
    /// </summary>
    public int Refs => refs.Count;

    /// <summary>
    ///     Gets the number of available bits.
    /// </summary>
    public int AvailableBits => 1023 - Bits;

    /// <summary>
    ///     Gets the number of available references.
    /// </summary>
    public int AvailableRefs => 4 - Refs;

    /// <summary>
    ///     Start building a cell.
    /// </summary>
    /// <returns>A new builder.</returns>
    public static Builder BeginCell()
    {
        return new Builder();
    }

    /// <summary>
    ///     Store a single bit.
    /// </summary>
    /// <param name="value">Bit value.</param>
    /// <returns>This builder.</returns>
    public Builder StoreBit(bool value)
    {
        bits.WriteBit(value);
        return this;
    }

    /// <summary>
    ///     Store bits from BitString.
    /// </summary>
    /// <param name="src">Source bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreBits(BitString src)
    {
        bits.WriteBits(src);
        return this;
    }

    /// <summary>
    ///     Store buffer.
    /// </summary>
    /// <param name="src">Source buffer.</param>
    /// <param name="bytes">Optional number of bytes to write.</param>
    /// <returns>This builder.</returns>
    public Builder StoreBuffer(byte[] src, int? bytes = null)
    {
        if (bytes.HasValue && src.Length != bytes.Value)
            throw new ArgumentException($"Buffer length {src.Length} is not equal to {bytes}");
        bits.WriteBuffer(src);
        return this;
    }

    /// <summary>
    ///     Store maybe buffer.
    /// </summary>
    /// <param name="src">Source buffer or null.</param>
    /// <param name="bytes">Optional number of bytes to write.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeBuffer(byte[]? src, int? bytes = null)
    {
        if (src != null)
        {
            StoreBit(true);
            StoreBuffer(src, bytes);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store uint value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreUint(long value, int bits)
    {
        this.bits.WriteUint(value, bits);
        return this;
    }

    /// <summary>
    ///     Store uint value (BigInteger).
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreUint(BigInteger value, int bits)
    {
        this.bits.WriteUint(value, bits);
        return this;
    }

    /// <summary>
    ///     Store maybe uint value.
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeUint(long? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreUint(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe uint value (BigInteger).
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeUint(BigInteger? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreUint(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store int value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreInt(long value, int bits)
    {
        this.bits.WriteInt(value, bits);
        return this;
    }

    /// <summary>
    ///     Store int value (BigInteger).
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreInt(BigInteger value, int bits)
    {
        this.bits.WriteInt(value, bits);
        return this;
    }

    /// <summary>
    ///     Store maybe int value.
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeInt(long? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreInt(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe int value (BigInteger).
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Number of bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeInt(BigInteger? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreInt(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store varuint value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreVarUint(long value, int bits)
    {
        this.bits.WriteVarUint(value, bits);
        return this;
    }

    /// <summary>
    ///     Store varuint value (BigInteger).
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreVarUint(BigInteger value, int bits)
    {
        this.bits.WriteVarUint(value, bits);
        return this;
    }

    /// <summary>
    ///     Store maybe varuint value.
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeVarUint(long? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreVarUint(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe varuint value (BigInteger).
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeVarUint(BigInteger? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreVarUint(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store varint value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreVarInt(long value, int bits)
    {
        this.bits.WriteVarInt(value, bits);
        return this;
    }

    /// <summary>
    ///     Store varint value (BigInteger).
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreVarInt(BigInteger value, int bits)
    {
        this.bits.WriteVarInt(value, bits);
        return this;
    }

    /// <summary>
    ///     Store maybe varint value.
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeVarInt(long? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreVarInt(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe varint value (BigInteger).
    /// </summary>
    /// <param name="value">Value or null.</param>
    /// <param name="bits">Header bits.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeVarInt(BigInteger? value, int bits)
    {
        if (value.HasValue)
        {
            StoreBit(true);
            StoreVarInt(value.Value, bits);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store coins value.
    /// </summary>
    /// <param name="amount">Amount.</param>
    /// <returns>This builder.</returns>
    public Builder StoreCoins(long amount)
    {
        bits.WriteCoins(amount);
        return this;
    }

    /// <summary>
    ///     Store coins value (BigInteger).
    /// </summary>
    /// <param name="amount">Amount.</param>
    /// <returns>This builder.</returns>
    public Builder StoreCoins(BigInteger amount)
    {
        bits.WriteCoins(amount);
        return this;
    }

    /// <summary>
    ///     Store maybe coins value.
    /// </summary>
    /// <param name="amount">Amount or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeCoins(long? amount)
    {
        if (amount.HasValue)
        {
            StoreBit(true);
            StoreCoins(amount.Value);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe coins value (BigInteger).
    /// </summary>
    /// <param name="amount">Amount or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeCoins(BigInteger? amount)
    {
        if (amount.HasValue)
        {
            StoreBit(true);
            StoreCoins(amount.Value);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store address.
    /// </summary>
    /// <param name="address">Address or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreAddress(Address? address)
    {
        bits.WriteAddress(address);
        return this;
    }

    /// <summary>
    ///     Store reference.
    /// </summary>
    /// <param name="cell">Cell or builder.</param>
    /// <returns>This builder.</returns>
    public Builder StoreRef(Cell cell)
    {
        if (refs.Count >= 4) throw new InvalidOperationException("Too many references");
        refs.Add(cell);
        return this;
    }

    /// <summary>
    ///     Store reference.
    /// </summary>
    /// <param name="builder">Builder.</param>
    /// <returns>This builder.</returns>
    public Builder StoreRef(Builder builder)
    {
        return StoreRef(builder.EndCell());
    }

    /// <summary>
    ///     Store maybe reference.
    /// </summary>
    /// <param name="cell">Cell or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeRef(Cell? cell)
    {
        if (cell != null)
        {
            StoreBit(true);
            StoreRef(cell);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store maybe reference.
    /// </summary>
    /// <param name="builder">Builder or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeRef(Builder? builder)
    {
        if (builder != null)
        {
            StoreBit(true);
            StoreRef(builder);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store slice.
    /// </summary>
    /// <param name="src">Source slice.</param>
    /// <returns>This builder.</returns>
    public Builder StoreSlice(Slice src)
    {
        Slice c = src.Clone();
        if (c.RemainingBits > 0) StoreBits(c.LoadBits(c.RemainingBits));
        while (c.RemainingRefs > 0) StoreRef(c.LoadRef());
        return this;
    }

    /// <summary>
    ///     Store maybe slice.
    /// </summary>
    /// <param name="src">Source slice or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeSlice(Slice? src)
    {
        if (src != null)
        {
            StoreBit(true);
            StoreSlice(src);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store builder.
    /// </summary>
    /// <param name="src">Source builder.</param>
    /// <returns>This builder.</returns>
    public Builder StoreBuilder(Builder src)
    {
        return StoreSlice(src.EndCell().BeginParse());
    }

    /// <summary>
    ///     Store maybe builder.
    /// </summary>
    /// <param name="src">Source builder or null.</param>
    /// <returns>This builder.</returns>
    public Builder StoreMaybeBuilder(Builder? src)
    {
        if (src != null)
        {
            StoreBit(true);
            StoreBuilder(src);
        }
        else
        {
            StoreBit(false);
        }

        return this;
    }

    /// <summary>
    ///     Store dictionary (stores ref to dictionary root).
    /// </summary>
    /// <param name="dict">Dictionary to store.</param>
    /// <returns>This builder.</returns>
    public Builder StoreDict<TK, TV>(Dict.Dictionary<TK, TV>? dict) where TK : IDictionaryKeyType
    {
        dict?.Store(this);
        return this;
    }

    /// <summary>
    ///     Store dictionary directly (no ref indirection).
    /// </summary>
    /// <param name="dict">Dictionary to store.</param>
    /// <returns>This builder.</returns>
    public Builder StoreDictDirect<TK, TV>(Dict.Dictionary<TK, TV>? dict) where TK : IDictionaryKeyType
    {
        dict?.StoreDirect(this);
        return this;
    }

    /// <summary>
    ///     Complete building and return the cell.
    /// </summary>
    /// <param name="exotic">Whether to create an exotic cell.</param>
    /// <returns>Built cell.</returns>
    public Cell EndCell(bool exotic = false)
    {
        return new Cell(bits.Build(), [.. refs], exotic);
    }

    /// <summary>
    ///     Convert to cell.
    /// </summary>
    /// <returns>Cell.</returns>
    public Cell AsCell()
    {
        return EndCell();
    }

    /// <summary>
    ///     Store string tail (string that can span multiple cells via refs).
    /// </summary>
    public Builder StoreStringTail(string src)
    {
        StoreBufferTail(Encoding.UTF8.GetBytes(src));
        return this;
    }

    /// <summary>
    ///     Store buffer tail (buffer that can span multiple cells via refs).
    /// </summary>
    void StoreBufferTail(byte[] src)
    {
        if (src.Length > 0)
        {
            int bytes = AvailableBits / 8;
            if (src.Length > bytes)
            {
                byte[] a = src[..bytes];
                byte[] t = src[bytes..];
                StoreBuffer(a);
                Builder bb = BeginCell();
                bb.StoreBufferTail(t);
                StoreRef(bb.EndCell());
            }
            else
            {
                StoreBuffer(src);
            }
        }
    }

    /// <summary>
    ///     Convert to slice.
    /// </summary>
    /// <returns>Slice.</returns>
    public Slice AsSlice()
    {
        return EndCell().BeginParse();
    }
}
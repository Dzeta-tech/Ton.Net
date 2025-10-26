using System.Numerics;
using Ton.Core.Addresses;

namespace Ton.Core.Boc;

/// <summary>
///     Slice allows reading data from cells.
/// </summary>
public class Slice
{
    readonly BitReader reader;
    readonly Cell[] refs;

    /// <summary>
    ///     Creates a new slice.
    /// </summary>
    /// <param name="reader">Bit reader.</param>
    /// <param name="refs">Cell references.</param>
    public Slice(BitReader reader, Cell[] refs)
    {
        this.reader = reader.Clone();
        this.refs = [.. refs];
        OffsetRefs = 0;
    }

    /// <summary>
    ///     Gets remaining bits.
    /// </summary>
    public int RemainingBits => reader.Remaining;

    /// <summary>
    ///     Gets offset bits.
    /// </summary>
    public int OffsetBits => reader.Offset;

    /// <summary>
    ///     Gets remaining refs.
    /// </summary>
    public int RemainingRefs => refs.Length - OffsetRefs;

    /// <summary>
    ///     Gets offset refs.
    /// </summary>
    public int OffsetRefs { get; private set; }

    /// <summary>
    ///     Skip bits.
    /// </summary>
    /// <param name="bits">Number of bits to skip.</param>
    /// <returns>This slice.</returns>
    public Slice Skip(int bits)
    {
        reader.Skip(bits);
        return this;
    }

    /// <summary>
    ///     Load a single bit.
    /// </summary>
    /// <returns>Bit value.</returns>
    public bool LoadBit()
    {
        return reader.LoadBit();
    }

    /// <summary>
    ///     Preload a single bit.
    /// </summary>
    /// <returns>Bit value.</returns>
    public bool PreloadBit()
    {
        return reader.PreloadBit();
    }

    /// <summary>
    ///     Load a boolean.
    /// </summary>
    /// <returns>Boolean value.</returns>
    public bool LoadBoolean()
    {
        return LoadBit();
    }

    /// <summary>
    ///     Load maybe boolean.
    /// </summary>
    /// <returns>Boolean value or null.</returns>
    public bool? LoadMaybeBoolean()
    {
        if (LoadBit()) return LoadBoolean();
        return null;
    }

    /// <summary>
    ///     Load bits as BitString.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>BitString.</returns>
    public BitString LoadBits(int bits)
    {
        return reader.LoadBits(bits);
    }

    /// <summary>
    ///     Preload bits as BitString.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>BitString.</returns>
    public BitString PreloadBits(int bits)
    {
        return reader.PreloadBits(bits);
    }

    /// <summary>
    ///     Load uint.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value.</returns>
    public long LoadUint(int bits)
    {
        return reader.LoadUint(bits);
    }

    /// <summary>
    ///     Load uint big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value as BigInteger.</returns>
    public BigInteger LoadUintBig(int bits)
    {
        return reader.LoadUintBig(bits);
    }

    /// <summary>
    ///     Preload uint.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value.</returns>
    public long PreloadUint(int bits)
    {
        return reader.PreloadUint(bits);
    }

    /// <summary>
    ///     Preload uint big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value as BigInteger.</returns>
    public BigInteger PreloadUintBig(int bits)
    {
        return reader.PreloadUintBig(bits);
    }

    /// <summary>
    ///     Load maybe uint.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value or null.</returns>
    public long? LoadMaybeUint(int bits)
    {
        if (LoadBit()) return LoadUint(bits);
        return null;
    }

    /// <summary>
    ///     Load maybe uint big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Uint value as BigInteger or null.</returns>
    public BigInteger? LoadMaybeUintBig(int bits)
    {
        if (LoadBit()) return LoadUintBig(bits);
        return null;
    }

    /// <summary>
    ///     Load int.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value.</returns>
    public long LoadInt(int bits)
    {
        return reader.LoadInt(bits);
    }

    /// <summary>
    ///     Load int big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value as BigInteger.</returns>
    public BigInteger LoadIntBig(int bits)
    {
        return reader.LoadIntBig(bits);
    }

    /// <summary>
    ///     Preload int.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value.</returns>
    public long PreloadInt(int bits)
    {
        return reader.PreloadInt(bits);
    }

    /// <summary>
    ///     Preload int big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value as BigInteger.</returns>
    public BigInteger PreloadIntBig(int bits)
    {
        return reader.PreloadIntBig(bits);
    }

    /// <summary>
    ///     Load maybe int.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value or null.</returns>
    public long? LoadMaybeInt(int bits)
    {
        if (LoadBit()) return LoadInt(bits);
        return null;
    }

    /// <summary>
    ///     Load maybe int big.
    /// </summary>
    /// <param name="bits">Number of bits.</param>
    /// <returns>Int value as BigInteger or null.</returns>
    public BigInteger? LoadMaybeIntBig(int bits)
    {
        if (LoadBit()) return LoadIntBig(bits);
        return null;
    }

    /// <summary>
    ///     Load varuint.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varuint value.</returns>
    public long LoadVarUint(int bits)
    {
        return reader.LoadVarUint(bits);
    }

    /// <summary>
    ///     Load varuint big.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varuint value as BigInteger.</returns>
    public BigInteger LoadVarUintBig(int bits)
    {
        return reader.LoadVarUintBig(bits);
    }

    /// <summary>
    ///     Preload varuint.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varuint value.</returns>
    public long PreloadVarUint(int bits)
    {
        return reader.PreloadVarUint(bits);
    }

    /// <summary>
    ///     Preload varuint big.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varuint value as BigInteger.</returns>
    public BigInteger PreloadVarUintBig(int bits)
    {
        return reader.PreloadVarUintBig(bits);
    }

    /// <summary>
    ///     Load varint.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varint value.</returns>
    public long LoadVarInt(int bits)
    {
        return reader.LoadVarInt(bits);
    }

    /// <summary>
    ///     Load varint big.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varint value as BigInteger.</returns>
    public BigInteger LoadVarIntBig(int bits)
    {
        return reader.LoadVarIntBig(bits);
    }

    /// <summary>
    ///     Preload varint.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varint value.</returns>
    public long PreloadVarInt(int bits)
    {
        return reader.PreloadVarInt(bits);
    }

    /// <summary>
    ///     Preload varint big.
    /// </summary>
    /// <param name="bits">Header bits.</param>
    /// <returns>Varint value as BigInteger.</returns>
    public BigInteger PreloadVarIntBig(int bits)
    {
        return reader.PreloadVarIntBig(bits);
    }

    /// <summary>
    ///     Load coins.
    /// </summary>
    /// <returns>Coins value.</returns>
    public BigInteger LoadCoins()
    {
        return reader.LoadCoins();
    }

    /// <summary>
    ///     Preload coins.
    /// </summary>
    /// <returns>Coins value.</returns>
    public BigInteger PreloadCoins()
    {
        return reader.PreloadCoins();
    }

    /// <summary>
    ///     Load maybe coins.
    /// </summary>
    /// <returns>Coins value or null.</returns>
    public BigInteger? LoadMaybeCoins()
    {
        if (reader.LoadBit()) return reader.LoadCoins();
        return null;
    }

    /// <summary>
    ///     Load address.
    /// </summary>
    /// <returns>Address or null.</returns>
    public Address? LoadAddress()
    {
        return reader.LoadAddress();
    }

    /// <summary>
    ///     Load maybe address.
    /// </summary>
    /// <returns>Address or null.</returns>
    public Address? LoadMaybeAddress()
    {
        return reader.LoadMaybeAddress();
    }

    /// <summary>
    ///     Preload address.
    /// </summary>
    /// <returns>Address or null.</returns>
    public Address? PreloadAddress()
    {
        return reader.PreloadAddress();
    }

    /// <summary>
    ///     Load reference.
    /// </summary>
    /// <returns>Cell.</returns>
    public Cell LoadRef()
    {
        if (OffsetRefs >= refs.Length) throw new InvalidOperationException("No more references");
        return refs[OffsetRefs++];
    }

    /// <summary>
    ///     Preload reference.
    /// </summary>
    /// <returns>Cell.</returns>
    public Cell PreloadRef()
    {
        if (OffsetRefs >= refs.Length) throw new InvalidOperationException("No more references");
        return refs[OffsetRefs];
    }

    /// <summary>
    ///     Load maybe reference.
    /// </summary>
    /// <returns>Cell or null.</returns>
    public Cell? LoadMaybeRef()
    {
        if (LoadBit()) return LoadRef();
        return null;
    }

    /// <summary>
    ///     Preload maybe reference.
    /// </summary>
    /// <returns>Cell or null.</returns>
    public Cell? PreloadMaybeRef()
    {
        if (PreloadBit()) return PreloadRef();
        return null;
    }

    /// <summary>
    ///     Load buffer.
    /// </summary>
    /// <param name="bytes">Number of bytes.</param>
    /// <returns>Buffer.</returns>
    public byte[] LoadBuffer(int bytes)
    {
        return reader.LoadBuffer(bytes);
    }

    /// <summary>
    ///     Preload buffer.
    /// </summary>
    /// <param name="bytes">Number of bytes.</param>
    /// <returns>Buffer.</returns>
    public byte[] PreloadBuffer(int bytes)
    {
        return reader.PreloadBuffer(bytes);
    }

    /// <summary>
    ///     Check if slice is empty and throw if not.
    /// </summary>
    public void EndParse()
    {
        if (RemainingBits > 0 || RemainingRefs > 0) throw new InvalidOperationException("Slice is not empty");
    }

    /// <summary>
    ///     Convert slice to cell.
    /// </summary>
    /// <returns>Cell.</returns>
    public Cell AsCell()
    {
        return Builder.BeginCell().StoreSlice(this).EndCell();
    }

    /// <summary>
    ///     Convert slice to builder.
    /// </summary>
    /// <returns>Builder.</returns>
    public Builder AsBuilder()
    {
        return Builder.BeginCell().StoreSlice(this);
    }

    /// <summary>
    ///     Clone slice.
    /// </summary>
    /// <param name="fromStart">Whether to clone from start.</param>
    /// <returns>Cloned slice.</returns>
    public Slice Clone(bool fromStart = false)
    {
        if (fromStart)
        {
            BitReader reader = this.reader.Clone();
            reader.Reset();
            return new Slice(reader, refs);
        }

        Slice res = new(reader, refs);
        res.OffsetRefs = OffsetRefs;
        return res;
    }

    /// <summary>
    ///     Print slice as string.
    /// </summary>
    /// <returns>String representation.</returns>
    public override string ToString()
    {
        return AsCell().ToString();
    }
}
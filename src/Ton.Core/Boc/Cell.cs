using System.Security.Cryptography;
using System.Text;

namespace Ton.Core.Boc;

/// <summary>
///     Cell as described in TVM specification.
/// </summary>
public class Cell
{
    /// <summary>
    ///     Empty cell constant.
    /// </summary>
    public static readonly Cell Empty = new();

    readonly int[] depths;

    readonly byte[][] hashes;

    /// <summary>
    ///     Creates a new cell.
    /// </summary>
    /// <param name="bits">Bit string data.</param>
    /// <param name="refs">Cell references.</param>
    /// <param name="exotic">Whether this is an exotic cell.</param>
    public Cell(BitString? bits = null, Cell[]? refs = null, bool exotic = false)
    {
        // Resolve bits
        Bits = bits ?? BitString.Empty;

        // Resolve refs
        Refs = refs != null ? [.. refs] : [];

        // Determine cell type from first byte if exotic
        if (exotic)
        {
            if (Bits.Length < 8)
                throw new ArgumentException("Exotic cell must have at least 8 bits for type byte");

            var reader = new BitReader(Bits);
            var typeValue = (int)reader.LoadUint(8);
            Type = typeValue switch
            {
                1 => CellType.PrunedBranch,
                3 => CellType.MerkleProof,
                4 => CellType.MerkleUpdate,
                _ => throw new ArgumentException($"Unknown exotic cell type: {typeValue}")
            };

            // Validate constraints based on type
            ValidateExoticCell();
        }
        else
        {
            Type = CellType.Ordinary;

            // Validate ordinary cell constraints
            if (Refs.Length > 4)
                throw new ArgumentException("Invalid number of references: maximum 4 references allowed");
            if (Bits.Length > 1023) throw new ArgumentException($"Bits overflow: {Bits.Length} > 1023");
        }

        // Calculate mask
        int mask = 0;
        foreach (Cell r in Refs) mask |= r.Mask.Value;
        Mask = new LevelMask(mask);

        // Calculate hashes and depths
        (hashes, depths) = CalculateHashesAndDepths();
    }

    void ValidateExoticCell()
    {
        switch (Type)
        {
            case CellType.PrunedBranch:
                // Two formats:
                // 1. Special case (config proof): type(8) + hash(256) + depth(16) = 280 bits (no mask, level=1)
                // 2. Standard: type(8) + mask(8) + level * (hash(256) + depth(16)) = 8 + 8 + level*272
                if (Bits.Length != 280 && Bits.Length != 288) // For now, only support level 1
                    throw new ArgumentException(
                        $"PrunedBranch cell must have exactly 280 or 288 bits, got {Bits.Length}");
                if (Refs.Length != 0)
                    throw new ArgumentException($"PrunedBranch cell must have 0 refs, got {Refs.Length}");
                break;

            case CellType.MerkleProof:
                // type(8) + hash(256) + depth(16) = 280 bits
                if (Bits.Length != 280)
                    throw new ArgumentException($"MerkleProof cell must have exactly 280 bits, got {Bits.Length}");
                if (Refs.Length != 1)
                    throw new ArgumentException($"MerkleProof cell must have exactly 1 ref, got {Refs.Length}");
                break;

            case CellType.MerkleUpdate:
                // type(8) + 2 * (hash(256) + depth(16)) = 552 bits
                if (Bits.Length != 552)
                    throw new ArgumentException($"MerkleUpdate cell must have exactly 552 bits, got {Bits.Length}");
                if (Refs.Length != 2)
                    throw new ArgumentException($"MerkleUpdate cell must have exactly 2 refs, got {Refs.Length}");
                break;
        }
    }

    /// <summary>
    ///     Gets the cell type.
    /// </summary>
    public CellType Type { get; }

    /// <summary>
    ///     Gets the bit string data.
    /// </summary>
    public BitString Bits { get; }

    /// <summary>
    ///     Gets the cell references.
    /// </summary>
    public Cell[] Refs { get; }

    /// <summary>
    ///     Gets the level mask.
    /// </summary>
    public LevelMask Mask { get; }

    /// <summary>
    ///     Checks if cell is exotic.
    /// </summary>
    public bool IsExotic => Type != CellType.Ordinary;

    /// <summary>
    ///     Begin parsing the cell.
    /// </summary>
    /// <param name="allowExotic">Whether to allow parsing exotic cells.</param>
    /// <returns>A new slice for reading.</returns>
    public Slice BeginParse(bool allowExotic = false)
    {
        if (IsExotic && !allowExotic) throw new InvalidOperationException("Exotic cells cannot be parsed");
        return new Slice(new BitReader(Bits), Refs);
    }

    /// <summary>
    ///     Get cell hash at specific level.
    /// </summary>
    /// <param name="level">Level (default 3).</param>
    /// <returns>Cell hash.</returns>
    public byte[] Hash(int level = 3)
    {
        return hashes[Math.Min(hashes.Length - 1, level)];
    }

    /// <summary>
    ///     Get cell depth at specific level.
    /// </summary>
    /// <param name="level">Level (default 3).</param>
    /// <returns>Cell depth.</returns>
    public int Depth(int level = 3)
    {
        return depths[Math.Min(depths.Length - 1, level)];
    }

    /// <summary>
    ///     Get cell level.
    /// </summary>
    /// <returns>Cell level.</returns>
    public int Level()
    {
        return Mask.Level;
    }

    /// <summary>
    ///     Check if this cell equals another cell.
    /// </summary>
    /// <param name="other">Other cell.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(Cell other)
    {
        return Hash().SequenceEqual(other.Hash());
    }

    /// <summary>
    ///     Convert cell to slice.
    /// </summary>
    /// <returns>Slice.</returns>
    public Slice AsSlice()
    {
        return BeginParse();
    }

    /// <summary>
    ///     Convert cell to builder.
    /// </summary>
    /// <returns>Builder.</returns>
    public Builder AsBuilder()
    {
        return Builder.BeginCell().StoreSlice(AsSlice());
    }

    /// <summary>
    ///     Format cell to string representation.
    /// </summary>
    /// <param name="indent">Indentation.</param>
    /// <returns>String representation.</returns>
    public string ToString(string indent = "")
    {
        StringBuilder sb = new();
        sb.Append(indent);
        sb.Append(IsExotic ? GetExoticPrefix() : 'x');
        sb.Append('{');
        sb.Append(Bits);
        sb.Append('}');

        foreach (Cell r in Refs)
        {
            sb.AppendLine();
            sb.Append(r.ToString(indent + " "));
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Format cell to string representation.
    /// </summary>
    /// <returns>String representation.</returns>
    public override string ToString()
    {
        return ToString();
    }

    char GetExoticPrefix()
    {
        return Type switch
        {
            CellType.MerkleProof => 'p',
            CellType.MerkleUpdate => 'u',
            CellType.PrunedBranch => 'p',
            _ => 'x'
        };
    }

    (byte[][], int[]) CalculateHashesAndDepths()
    {
        int hashCount = Mask.HashCount;
        byte[][] hashes = new byte[hashCount][];
        int[] depths = new int[hashCount];

        for (int hashI = 0; hashI < hashCount; hashI++)
        {
            // Calculate depth
            int currentDepth = 0;
            foreach (Cell r in Refs)
            {
                int refDepth = r.Depth(hashI);
                if (refDepth > currentDepth) currentDepth = refDepth;
            }

            if (Refs.Length > 0) currentDepth++;
            depths[hashI] = currentDepth;

            // Calculate hash
            byte[] descriptor = GetDescriptor();
            using SHA256 sha = SHA256.Create();
            sha.TransformBlock(descriptor, 0, descriptor.Length, null, 0);

            // Add data
            byte[] bitsData = GetBitsData();
            sha.TransformBlock(bitsData, 0, bitsData.Length, null, 0);

            // Add depths
            foreach (Cell r in Refs)
            {
                int depth = r.Depth(hashI);
                byte[] depthBytes = [(byte)(depth >> 8), (byte)(depth & 0xFF)];
                sha.TransformBlock(depthBytes, 0, 2, null, 0);
            }

            // Add hashes
            foreach (Cell r in Refs)
            {
                byte[] hash = r.Hash(hashI);
                sha.TransformBlock(hash, 0, hash.Length, null, 0);
            }

            sha.TransformFinalBlock([], 0, 0);
            hashes[hashI] = sha.Hash!;
        }

        return (hashes, depths);
    }

    byte[] GetDescriptor()
    {
        // d1 = refs_cnt + 8*is_exotic + 32*level
        int d1 = Refs.Length + (IsExotic ? 8 : 0) + (Mask.Value << 5);

        // d2 = bits_cnt (rounded up to 8) + bits_cnt % 8 (padding flag)
        int d2 = (int)Math.Ceiling(Bits.Length / 8.0) * 2 + (Bits.Length % 8 != 0 ? 1 : 0);

        return [(byte)d1, (byte)d2];
    }

    byte[] GetBitsData()
    {
        if (Bits.Length == 0) return [];

        // Get the underlying buffer
        byte[]? buffer = Bits.Subbuffer(0, Bits.Length);
        if (buffer == null) return [];

        // If bits are not byte-aligned, we need to add padding
        if (Bits.Length % 8 != 0)
        {
            int paddingBits = 8 - Bits.Length % 8;
            BitBuilder builder = new();
            builder.WriteBits(Bits);
            builder.WriteBit(true); // Padding bit
            for (int i = 1; i < paddingBits; i++) builder.WriteBit(false);
            BitString paddedBits = builder.Build();
            return paddedBits.Subbuffer(0, paddedBits.Length) ?? [];
        }

        return buffer;
    }
}
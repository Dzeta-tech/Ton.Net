using System.Security.Cryptography;
using System.Text;

namespace Ton.Core.Boc;

/// <summary>
/// Cell as described in TVM specification.
/// </summary>
public class Cell
{
    /// <summary>
    /// Empty cell constant.
    /// </summary>
    public static readonly Cell Empty = new();

    private readonly byte[][] _hashes;
    private readonly int[] _depths;

    /// <summary>
    /// Creates a new cell.
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

        // For now, only support ordinary cells
        if (exotic)
        {
            throw new NotImplementedException("Exotic cells are not yet supported");
        }

        // Validate ordinary cell constraints
        if (Refs.Length > 4)
        {
            throw new ArgumentException("Invalid number of references: maximum 4 references allowed");
        }
        if (Bits.Length > 1023)
        {
            throw new ArgumentException($"Bits overflow: {Bits.Length} > 1023");
        }

        // Calculate mask for ordinary cell
        int mask = 0;
        foreach (var r in Refs)
        {
            mask |= r.Mask.Value;
        }
        Mask = new LevelMask(mask);

        // Calculate hashes and depths
        (_hashes, _depths) = CalculateHashesAndDepths();

        Type = CellType.Ordinary;
    }

    /// <summary>
    /// Gets the cell type.
    /// </summary>
    public CellType Type { get; }

    /// <summary>
    /// Gets the bit string data.
    /// </summary>
    public BitString Bits { get; }

    /// <summary>
    /// Gets the cell references.
    /// </summary>
    public Cell[] Refs { get; }

    /// <summary>
    /// Gets the level mask.
    /// </summary>
    public LevelMask Mask { get; }

    /// <summary>
    /// Checks if cell is exotic.
    /// </summary>
    public bool IsExotic => Type != CellType.Ordinary;

    /// <summary>
    /// Begin parsing the cell.
    /// </summary>
    /// <param name="allowExotic">Whether to allow parsing exotic cells.</param>
    /// <returns>A new slice for reading.</returns>
    public Slice BeginParse(bool allowExotic = false)
    {
        if (IsExotic && !allowExotic)
        {
            throw new InvalidOperationException("Exotic cells cannot be parsed");
        }
        return new Slice(new BitReader(Bits), Refs);
    }

    /// <summary>
    /// Get cell hash at specific level.
    /// </summary>
    /// <param name="level">Level (default 3).</param>
    /// <returns>Cell hash.</returns>
    public byte[] Hash(int level = 3)
    {
        return _hashes[Math.Min(_hashes.Length - 1, level)];
    }

    /// <summary>
    /// Get cell depth at specific level.
    /// </summary>
    /// <param name="level">Level (default 3).</param>
    /// <returns>Cell depth.</returns>
    public int Depth(int level = 3)
    {
        return _depths[Math.Min(_depths.Length - 1, level)];
    }

    /// <summary>
    /// Get cell level.
    /// </summary>
    /// <returns>Cell level.</returns>
    public int Level()
    {
        return Mask.Level;
    }

    /// <summary>
    /// Check if this cell equals another cell.
    /// </summary>
    /// <param name="other">Other cell.</param>
    /// <returns>True if equal.</returns>
    public bool Equals(Cell other)
    {
        return Hash().SequenceEqual(other.Hash());
    }

    /// <summary>
    /// Convert cell to slice.
    /// </summary>
    /// <returns>Slice.</returns>
    public Slice AsSlice()
    {
        return BeginParse();
    }

    /// <summary>
    /// Convert cell to builder.
    /// </summary>
    /// <returns>Builder.</returns>
    public Builder AsBuilder()
    {
        return Builder.BeginCell().StoreSlice(AsSlice());
    }

    /// <summary>
    /// Format cell to string representation.
    /// </summary>
    /// <param name="indent">Indentation.</param>
    /// <returns>String representation.</returns>
    public string ToString(string indent = "")
    {
        var sb = new StringBuilder();
        sb.Append(indent);
        sb.Append(IsExotic ? GetExoticPrefix() : 'x');
        sb.Append('{');
        sb.Append(Bits.ToString());
        sb.Append('}');

        foreach (var r in Refs)
        {
            sb.AppendLine();
            sb.Append(r.ToString(indent + " "));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format cell to string representation.
    /// </summary>
    /// <returns>String representation.</returns>
    public override string ToString()
    {
        return ToString("");
    }

    private char GetExoticPrefix()
    {
        return Type switch
        {
            CellType.MerkleProof => 'p',
            CellType.MerkleUpdate => 'u',
            CellType.PrunedBranch => 'p',
            _ => 'x'
        };
    }

    private (byte[][], int[]) CalculateHashesAndDepths()
    {
        int hashCount = Mask.HashCount;
        var hashes = new byte[hashCount][];
        var depths = new int[hashCount];

        for (int hashI = 0; hashI < hashCount; hashI++)
        {
            // Calculate depth
            int currentDepth = 0;
            foreach (var r in Refs)
            {
                int refDepth = r.Depth(hashI);
                if (refDepth > currentDepth)
                {
                    currentDepth = refDepth;
                }
            }
            if (Refs.Length > 0)
            {
                currentDepth++;
            }
            depths[hashI] = currentDepth;

            // Calculate hash
            var descriptor = GetDescriptor();
            using var sha = SHA256.Create();
            sha.TransformBlock(descriptor, 0, descriptor.Length, null, 0);

            // Add data
            var bitsData = GetBitsData();
            sha.TransformBlock(bitsData, 0, bitsData.Length, null, 0);

            // Add depths
            foreach (var r in Refs)
            {
                int depth = r.Depth(hashI);
                var depthBytes = new byte[] { (byte)(depth >> 8), (byte)(depth & 0xFF) };
                sha.TransformBlock(depthBytes, 0, 2, null, 0);
            }

            // Add hashes
            foreach (var r in Refs)
            {
                var hash = r.Hash(hashI);
                sha.TransformBlock(hash, 0, hash.Length, null, 0);
            }

            sha.TransformFinalBlock([], 0, 0);
            hashes[hashI] = sha.Hash!;
        }

        return (hashes, depths);
    }

    private byte[] GetDescriptor()
    {
        // d1 = refs_cnt + 8*is_exotic + 32*level
        int d1 = Refs.Length + (IsExotic ? 8 : 0) + (Mask.Value << 5);
        
        // d2 = bits_cnt (rounded up to 8) + bits_cnt % 8 (padding flag)
        int d2 = (int)Math.Ceiling(Bits.Length / 8.0) * 2 + (Bits.Length % 8 != 0 ? 1 : 0);
        
        return [(byte)d1, (byte)d2];
    }

    private byte[] GetBitsData()
    {
        if (Bits.Length == 0)
        {
            return [];
        }

        // Get the underlying buffer
        var buffer = Bits.Subbuffer(0, Bits.Length);
        if (buffer == null)
        {
            return [];
        }

        // If bits are not byte-aligned, we need to add padding
        if (Bits.Length % 8 != 0)
        {
            int paddingBits = 8 - (Bits.Length % 8);
            var builder = new BitBuilder();
            builder.WriteBits(Bits);
            builder.WriteBit(true); // Padding bit
            for (int i = 1; i < paddingBits; i++)
            {
                builder.WriteBit(false);
            }
            var paddedBits = builder.Build();
            return paddedBits.Subbuffer(0, paddedBits.Length) ?? [];
        }

        return buffer;
    }
}


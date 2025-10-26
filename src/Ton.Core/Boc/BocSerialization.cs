using System.Numerics;

namespace Ton.Core.Boc;

/// <summary>
///     BOC (Bag of Cells) serialization and deserialization utilities.
/// </summary>
public static class BocSerialization
{
    /// <summary>
    ///     Serialize a cell to BOC format.
    /// </summary>
    /// <param name="root">Root cell to serialize.</param>
    /// <param name="hasIdx">Whether to include cell index.</param>
    /// <param name="hasCrc32C">Whether to include CRC32-C checksum.</param>
    /// <returns>Serialized BOC bytes.</returns>
    public static byte[] SerializeBoc(Cell root, bool hasIdx = true, bool hasCrc32C = true)
    {
        // Sort cells topologically
        List<CellRef> allCells = TopologicalSort(root);

        // Calculate parameters
        int cellsNum = allCells.Count;
        bool hasCacheBits = false;
        int flags = 0;
        int sizeBytes = Math.Max((int)Math.Ceiling(BitsForNumber(cellsNum) / 8.0), 1);
        int totalCellSize = 0;
        List<int> index = [];

        foreach (CellRef cellRef in allCells)
        {
            int sz = CalcCellSize(cellRef.Cell, sizeBytes);
            totalCellSize += sz;
            index.Add(totalCellSize);
        }

        int offsetBytes = Math.Max((int)Math.Ceiling(BitsForNumber(totalCellSize) / 8.0), 1);
        int totalSize = (
            4 + // magic
            1 + // flags and s_bytes
            1 + // offset_bytes
            3 * sizeBytes + // cells_num, roots, complete
            offsetBytes + // full_size
            1 * sizeBytes + // root_idx
            (hasIdx ? cellsNum * offsetBytes : 0) +
            totalCellSize +
            (hasCrc32C ? 4 : 0)
        ) * 8;

        // Serialize
        BitBuilder builder = new(totalSize);
        builder.WriteUint(0xb5ee9c72, 32); // Magic
        builder.WriteBit(hasIdx); // Has index
        builder.WriteBit(hasCrc32C); // Has crc32c
        builder.WriteBit(hasCacheBits); // Has cache bits
        builder.WriteUint(flags, 2); // Flags
        builder.WriteUint(sizeBytes, 3); // Size bytes
        builder.WriteUint(offsetBytes, 8); // Offset bytes
        builder.WriteUint(cellsNum, sizeBytes * 8); // Cells num
        builder.WriteUint(1, sizeBytes * 8); // Roots num
        builder.WriteUint(0, sizeBytes * 8); // Absent num
        builder.WriteUint(totalCellSize, offsetBytes * 8); // Total cell size
        builder.WriteUint(0, sizeBytes * 8); // Root id == 0

        if (hasIdx) // Index
            for (int i = 0; i < cellsNum; i++)
                builder.WriteUint(index[i], offsetBytes * 8);

        for (int i = 0; i < cellsNum; i++) // Cells
            WriteCellToBuilder(allCells[i].Cell, allCells[i].Refs, sizeBytes, builder);

        if (hasCrc32C)
        {
            byte[] crc32 = Utils.Crc32C.Compute(builder.Buffer());
            builder.WriteBuffer(crc32);
        }

        // Sanity Check
        byte[] res = builder.Buffer();
        if (res.Length != totalSize / 8)
            throw new InvalidOperationException("Internal error in BOC serialization");

        return res;
    }

    /// <summary>
    ///     Deserialize BOC format to cells.
    /// </summary>
    /// <param name="src">BOC bytes.</param>
    /// <returns>Array of root cells.</returns>
    public static Cell[] DeserializeBoc(byte[] src)
    {
        // Parse BOC
        BocHeader boc = ParseBoc(src);
        BitReader reader = new(new BitString(boc.CellData, 0, boc.CellData.Length * 8));

        // Load cells
        List<CellData> cells = [];
        for (int i = 0; i < boc.Cells; i++)
        {
            CellData cll = ReadCell(reader, boc.Size);
            cells.Add(cll);
        }

        // Build cells (bottom-up)
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            if (cells[i].Result != null)
                throw new InvalidOperationException("Impossible");

            List<Cell> refs = [];
            foreach (int r in cells[i].Refs)
            {
                if (cells[r].Result == null)
                    throw new InvalidOperationException("Invalid BOC file");
                refs.Add(cells[r].Result!);
            }

            cells[i].Result = new Cell(cells[i].Bits, refs.ToArray(), cells[i].Exotic);
        }

        // Load roots
        List<Cell> roots = [];
        foreach (int rootIdx in boc.Root)
            roots.Add(cells[rootIdx].Result!);

        return roots.ToArray();
    }

    /// <summary>
    ///     Topological sort of cells for serialization.
    /// </summary>
    static List<CellRef> TopologicalSort(Cell root)
    {
        Dictionary<string, int> hashes = [];
        List<CellRef> allCells = [];
        List<Cell> pending = [root];

        while (pending.Count > 0)
        {
            Cell cell = pending[^1];
            pending.RemoveAt(pending.Count - 1);

            string hash = Convert.ToBase64String(cell.Hash());
            if (hashes.ContainsKey(hash))
                continue;

            bool allRefsFound = true;
            foreach (Cell r in cell.Refs)
            {
                string refHash = Convert.ToBase64String(r.Hash());
                if (!hashes.ContainsKey(refHash))
                {
                    allRefsFound = false;
                    break;
                }
            }

            if (!allRefsFound)
            {
                pending.Add(cell);
                for (int i = cell.Refs.Length - 1; i >= 0; i--)
                    pending.Add(cell.Refs[i]);
                continue;
            }

            List<int> refs = [];
            foreach (Cell r in cell.Refs)
            {
                string refHash = Convert.ToBase64String(r.Hash());
                refs.Add(hashes[refHash]);
            }

            hashes[hash] = allCells.Count;
            allCells.Add(new CellRef(cell, refs.ToArray()));
        }

        // Reverse to match JS behavior: parents before children
        allCells.Reverse();

        // Rebuild ref indices for reversed order
        Dictionary<string, int> newIndices = [];
        for (int i = 0; i < allCells.Count; i++)
        {
            string hash = Convert.ToBase64String(allCells[i].Cell.Hash());
            newIndices[hash] = i;
        }

        List<CellRef> result = [];
        foreach (CellRef cellRef in allCells)
        {
            List<int> newRefs = [];
            foreach (Cell r in cellRef.Cell.Refs)
            {
                string refHash = Convert.ToBase64String(r.Hash());
                newRefs.Add(newIndices[refHash]);
            }

            result.Add(new CellRef(cellRef.Cell, newRefs.ToArray()));
        }

        return result;
    }

    static int BitsForNumber(int n)
    {
        if (n == 0) return 1;
        return (int)Math.Floor(Math.Log2(n)) + 1;
    }

    static int CalcCellSize(Cell cell, int sizeBytes)
    {
        return 2 /* D1+D2 */ + (int)Math.Ceiling(cell.Bits.Length / 8.0) + cell.Refs.Length * sizeBytes;
    }

    static void WriteCellToBuilder(Cell cell, int[] refs, int sizeBytes, BitBuilder to)
    {
        int d1 = GetRefsDescriptor(cell.Refs, cell.Mask.Value, cell.Type);
        int d2 = GetBitsDescriptor(cell.Bits);
        to.WriteUint(d1, 8);
        to.WriteUint(d2, 8);
        to.WriteBuffer(BitsToPaddedBuffer(cell.Bits));
        foreach (int r in refs)
            to.WriteUint(r, sizeBytes * 8);
    }

    static int GetRefsDescriptor(Cell[] refs, int levelMask, CellType cellType)
    {
        int d1 = refs.Length;
        if (cellType != CellType.Ordinary)
            d1 |= 8;
        d1 |= levelMask << 5;
        return d1;
    }

    static int GetBitsDescriptor(BitString bits)
    {
        int len = bits.Length;
        int d2 = (int)Math.Ceiling(len / 8.0) * 2 + (len % 8 != 0 ? 1 : 0);
        return d2;
    }

    static byte[] BitsToPaddedBuffer(BitString bits)
    {
        int len = bits.Length;
        int bytes = (int)Math.Ceiling(len / 8.0);
        byte[] buf = new byte[bytes];

        for (int i = 0; i < len; i++)
        {
            if (bits.At(i))
                buf[i / 8] |= (byte)(1 << (7 - (i % 8)));
        }

        // Add padding bit if needed
        if (len % 8 != 0)
            buf[bytes - 1] |= (byte)(1 << (7 - (len % 8)));

        return buf;
    }

    static BocHeader ParseBoc(byte[] src)
    {
        BitReader reader = new(new BitString(src, 0, src.Length * 8));
        uint magic = (uint)reader.LoadUint(32);

        if (magic == 0x68ff65f3) // Generic magic
        {
            int size = (int)reader.LoadUint(8);
            int offBytes = (int)reader.LoadUint(8);
            int cells = (int)reader.LoadUint(size * 8);
            int roots = (int)reader.LoadUint(size * 8);
            int absent = (int)reader.LoadUint(size * 8);
            int totalCellSize = (int)reader.LoadUint(offBytes * 8);
            byte[] index = reader.LoadBuffer(cells * offBytes);
            byte[] cellData = reader.LoadBuffer(totalCellSize);

            return new BocHeader(size, offBytes, cells, roots, absent, totalCellSize, index, cellData, [0]);
        }

        if (magic == 0xacc3a728) // Generic magic with CRC
        {
            int size = (int)reader.LoadUint(8);
            int offBytes = (int)reader.LoadUint(8);
            int cells = (int)reader.LoadUint(size * 8);
            int roots = (int)reader.LoadUint(size * 8);
            int absent = (int)reader.LoadUint(size * 8);
            int totalCellSize = (int)reader.LoadUint(offBytes * 8);
            byte[] index = reader.LoadBuffer(cells * offBytes);
            byte[] cellData = reader.LoadBuffer(totalCellSize);
            byte[] crc32 = reader.LoadBuffer(4);

            if (!Utils.Crc32C.Compute(src[..^4]).SequenceEqual(crc32))
                throw new InvalidOperationException("Invalid CRC32C");

            return new BocHeader(size, offBytes, cells, roots, absent, totalCellSize, index, cellData, [0]);
        }

        if (magic == 0xb5ee9c72) // Standard magic
        {
            bool hasIdx = reader.LoadBit();
            bool hasCrc32C = reader.LoadBit();
            bool hasCacheBits = reader.LoadBit();
            int flags = (int)reader.LoadUint(2);
            int size = (int)reader.LoadUint(3);
            int offBytes = (int)reader.LoadUint(8);
            int cells = (int)reader.LoadUint(size * 8);
            int roots = (int)reader.LoadUint(size * 8);
            int absent = (int)reader.LoadUint(size * 8);
            int totalCellSize = (int)reader.LoadUint(offBytes * 8);

            List<int> root = [];
            for (int i = 0; i < roots; i++)
                root.Add((int)reader.LoadUint(size * 8));

            byte[]? index = null;
            if (hasIdx)
                index = reader.LoadBuffer(cells * offBytes);

            byte[] cellData = reader.LoadBuffer(totalCellSize);

            if (hasCrc32C)
            {
                byte[] crc32 = reader.LoadBuffer(4);
                if (!Utils.Crc32C.Compute(src[..^4]).SequenceEqual(crc32))
                    throw new InvalidOperationException("Invalid CRC32C");
            }

            return new BocHeader(size, offBytes, cells, roots, absent, totalCellSize, index, cellData,
                root.ToArray());
        }

        throw new InvalidOperationException("Invalid magic");
    }

    static CellData ReadCell(BitReader reader, int sizeBytes)
    {
        // D1
        int d1 = (int)reader.LoadUint(8);
        int refsCount = d1 % 8;
        bool exotic = (d1 & 8) != 0;

        // D2
        int d2 = (int)reader.LoadUint(8);
        int dataBytesize = d2 / 2; // Ceiling is implicit: d2 = ceil(bits/8) * 2 + padding_flag
        bool paddingAdded = (d2 % 2) != 0;

        // In standard BOC format without cache bits, cells don't include hashes/depths
        // They're only included if has_cache_bits flag is set in the BOC header

        // Bits
        BitString bits = BitString.Empty;
        if (dataBytesize > 0)
            bits = paddingAdded
                ? reader.LoadPaddedBits(dataBytesize * 8)
                : reader.LoadBits(dataBytesize * 8);

        // Refs
        List<int> refs = [];
        for (int i = 0; i < refsCount; i++)
            refs.Add((int)reader.LoadUint(sizeBytes * 8));

        return new CellData(bits, refs.ToArray(), exotic);
    }

    static int GetHashesCount(int levelMask)
    {
        return GetHashesCountFromMask(levelMask & 7);
    }

    static int GetHashesCountFromMask(int mask)
    {
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            n += mask & 1;
            mask >>= 1;
        }

        return n + 1; // 1 repr + up to 3 higher hashes
    }

    record BocHeader(
        int Size,
        int OffBytes,
        int Cells,
        int Roots,
        int Absent,
        int TotalCellSize,
        byte[]? Index,
        byte[] CellData,
        int[] Root);

    class CellData(BitString bits, int[] refs, bool exotic)
    {
        public BitString Bits { get; } = bits;
        public int[] Refs { get; } = refs;
        public bool Exotic { get; } = exotic;
        public Cell? Result { get; set; }
    }

    record CellRef(Cell Cell, int[] Refs);
}


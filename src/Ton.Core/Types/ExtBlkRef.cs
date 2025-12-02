using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Extended block reference.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L417
///     ext_blk_ref$_ end_lt:uint64
///     seq_no:uint32 root_hash:bits256 file_hash:bits256 = ExtBlkRef;
/// </summary>
public record ExtBlkRef
{
    /// <summary>
    ///     Creates a new ExtBlkRef.
    /// </summary>
    public ExtBlkRef(ulong endLt, uint seqNo, byte[] rootHash, byte[] fileHash)
    {
        ArgumentNullException.ThrowIfNull(rootHash);
        ArgumentNullException.ThrowIfNull(fileHash);

        if (rootHash.Length != 32)
            throw new ArgumentException("Root hash must be 32 bytes", nameof(rootHash));

        if (fileHash.Length != 32)
            throw new ArgumentException("File hash must be 32 bytes", nameof(fileHash));

        EndLt = endLt;
        SeqNo = seqNo;
        RootHash = rootHash;
        FileHash = fileHash;
    }

    /// <summary>
    ///     End logical time.
    /// </summary>
    public ulong EndLt { get; init; }

    /// <summary>
    ///     Sequence number.
    /// </summary>
    public uint SeqNo { get; init; }

    /// <summary>
    ///     Root hash (32 bytes).
    /// </summary>
    public byte[] RootHash { get; init; }

    /// <summary>
    ///     File hash (32 bytes).
    /// </summary>
    public byte[] FileHash { get; init; }

    /// <summary>
    ///     Loads ExtBlkRef from a Slice.
    /// </summary>
    public static ExtBlkRef Load(Slice slice)
    {
        ulong endLt = (ulong)slice.LoadUintBig(64);
        uint seqNo = (uint)slice.LoadUint(32);
        byte[] rootHash = slice.LoadBits(256).ToBytes();
        byte[] fileHash = slice.LoadBits(256).ToBytes();

        return new ExtBlkRef(endLt, seqNo, rootHash, fileHash);
    }

    /// <summary>
    ///     Stores ExtBlkRef into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(EndLt, 64);
        builder.StoreUint(SeqNo, 32);
        builder.StoreBits(new BitString(RootHash, 0, RootHash.Length * 8));
        builder.StoreBits(new BitString(FileHash, 0, FileHash.Length * 8));
    }
}


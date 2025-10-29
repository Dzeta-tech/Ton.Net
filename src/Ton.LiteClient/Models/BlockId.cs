using Ton.Adnl.Protocol;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents a TON blockchain block identifier with full hash information
/// </summary>
public record BlockId
{
    public BlockId(int workchain, long shard, uint seqno, byte[] rootHash, byte[] fileHash)
    {
        ArgumentNullException.ThrowIfNull(rootHash);
        ArgumentNullException.ThrowIfNull(fileHash);

        if (rootHash.Length != 32)
            throw new ArgumentException("Root hash must be 32 bytes", nameof(rootHash));

        if (fileHash.Length != 32)
            throw new ArgumentException("File hash must be 32 bytes", nameof(fileHash));

        Workchain = workchain;
        Shard = shard;
        Seqno = seqno;
        RootHash = rootHash;
        FileHash = fileHash;
    }

    /// <summary>
    ///     Workchain ID (-1 for masterchain, 0 for basechain)
    /// </summary>
    public int Workchain { get; init; }

    /// <summary>
    ///     Shard identifier
    /// </summary>
    public long Shard { get; init; }

    /// <summary>
    ///     Block sequence number
    /// </summary>
    public uint Seqno { get; init; }

    /// <summary>
    ///     Root hash of the block
    /// </summary>
    public byte[] RootHash { get; init; }

    /// <summary>
    ///     File hash of the block
    /// </summary>
    public byte[] FileHash { get; init; }

    /// <summary>
    ///     Returns true if this is a masterchain block
    /// </summary>
    public bool IsMasterchain => Workchain == -1;

    /// <summary>
    ///     Returns the root hash as a hex string
    /// </summary>
    public string RootHashHex => Convert.ToHexString(RootHash);

    /// <summary>
    ///     Returns the file hash as a hex string
    /// </summary>
    public string FileHashHex => Convert.ToHexString(FileHash);

    /// <summary>
    ///     Creates a BlockId from ADNL protocol's TonNodeBlockIdExt
    /// </summary>
    public static BlockId FromAdnl(TonNodeBlockIdExt adnlBlock)
    {
        return new BlockId(
            adnlBlock.Workchain,
            adnlBlock.Shard,
            unchecked((uint)adnlBlock.Seqno),
            adnlBlock.RootHash,
            adnlBlock.FileHash);
    }

    /// <summary>
    ///     Converts this BlockId to ADNL protocol's TonNodeBlockIdExt
    /// </summary>
    public TonNodeBlockIdExt ToAdnl()
    {
        return new TonNodeBlockIdExt
        {
            Workchain = Workchain,
            Shard = Shard,
            Seqno = unchecked((int)Seqno),
            RootHash = RootHash,
            FileHash = FileHash
        };
    }

    public override string ToString()
    {
        return $"Block(wc:{Workchain}, shard:{Shard:X}, seqno:{Seqno})";
    }
}
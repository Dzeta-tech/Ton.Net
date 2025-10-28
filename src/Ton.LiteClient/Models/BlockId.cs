using System.Numerics;

namespace Ton.LiteClient.Models;

/// <summary>
/// Represents a TON blockchain block identifier with full hash information
/// </summary>
public readonly record struct BlockId
{
    /// <summary>
    /// Workchain ID (-1 for masterchain, 0 for basechain)
    /// </summary>
    public int Workchain { get; init; }

    /// <summary>
    /// Shard identifier
    /// </summary>
    public long Shard { get; init; }

    /// <summary>
    /// Block sequence number
    /// </summary>
    public int Seqno { get; init; }

    /// <summary>
    /// Root hash of the block
    /// </summary>
    public byte[] RootHash { get; init; }

    /// <summary>
    /// File hash of the block
    /// </summary>
    public byte[] FileHash { get; init; }

    public BlockId(int workchain, long shard, int seqno, byte[] rootHash, byte[] fileHash)
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
    /// Returns true if this is a masterchain block
    /// </summary>
    public bool IsMasterchain => Workchain == -1;

    /// <summary>
    /// Returns the root hash as a hex string
    /// </summary>
    public string RootHashHex => Convert.ToHexString(RootHash);

    /// <summary>
    /// Returns the file hash as a hex string
    /// </summary>
    public string FileHashHex => Convert.ToHexString(FileHash);

    public override string ToString() =>
        $"Block(wc:{Workchain}, shard:{Shard:X}, seqno:{Seqno})";
}


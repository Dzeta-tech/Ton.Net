namespace Ton.LiteClient.Models;

/// <summary>
/// Represents masterchain information including the latest block and zero state
/// </summary>
public sealed class MasterchainInfo
{
    /// <summary>
    /// Latest known masterchain block
    /// </summary>
    public required BlockId Last { get; init; }

    /// <summary>
    /// State root hash
    /// </summary>
    public required byte[] StateRootHash { get; init; }

    /// <summary>
    /// Zero state (initial state) block reference
    /// </summary>
    public required ZeroStateId Init { get; init; }

    /// <summary>
    /// Unix timestamp when this info was retrieved
    /// </summary>
    public required int Now { get; init; }

    public override string ToString() =>
        $"MasterchainInfo(seqno:{Last.Seqno}, time:{DateTimeOffset.FromUnixTimeSeconds(Now):yyyy-MM-dd HH:mm:ss} UTC)";
}

/// <summary>
/// Represents zero state (genesis block) identifier
/// </summary>
public readonly record struct ZeroStateId
{
    /// <summary>
    /// Workchain ID (usually -1 for masterchain)
    /// </summary>
    public int Workchain { get; init; }

    /// <summary>
    /// Root hash of the zero state
    /// </summary>
    public byte[] RootHash { get; init; }

    /// <summary>
    /// File hash of the zero state
    /// </summary>
    public byte[] FileHash { get; init; }

    public ZeroStateId(int workchain, byte[] rootHash, byte[] fileHash)
    {
        ArgumentNullException.ThrowIfNull(rootHash);
        ArgumentNullException.ThrowIfNull(fileHash);

        if (rootHash.Length != 32)
            throw new ArgumentException("Root hash must be 32 bytes", nameof(rootHash));

        if (fileHash.Length != 32)
            throw new ArgumentException("File hash must be 32 bytes", nameof(fileHash));

        Workchain = workchain;
        RootHash = rootHash;
        FileHash = fileHash;
    }

    public override string ToString() => $"ZeroState(wc:{Workchain})";
}


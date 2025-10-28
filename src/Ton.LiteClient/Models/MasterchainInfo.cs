namespace Ton.LiteClient.Models;

/// <summary>
///     Represents masterchain information including the latest block and zero state
/// </summary>
public sealed class MasterchainInfo
{
    /// <summary>
    ///     Latest known masterchain block
    /// </summary>
    public required BlockId Last { get; init; }

    /// <summary>
    ///     State root hash
    /// </summary>
    public required byte[] StateRootHash { get; init; }

    /// <summary>
    ///     Zero state (initial state) block reference
    /// </summary>
    public required ZeroStateId Init { get; init; }

    public override string ToString()
    {
        return $"MasterchainInfo(seqno:{Last.Seqno})";
    }
}

/// <summary>
///     Represents zero state (genesis block) identifier
/// </summary>
public record ZeroStateId
{
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

    /// <summary>
    ///     Workchain ID (usually -1 for masterchain)
    /// </summary>
    public int Workchain { get; init; }

    /// <summary>
    ///     Root hash of the zero state
    /// </summary>
    public byte[] RootHash { get; init; }

    /// <summary>
    ///     File hash of the zero state
    /// </summary>
    public byte[] FileHash { get; init; }

    public override string ToString()
    {
        return $"ZeroState(wc:{Workchain})";
    }
}
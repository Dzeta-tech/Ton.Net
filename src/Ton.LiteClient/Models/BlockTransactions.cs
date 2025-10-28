namespace Ton.LiteClient.Models;

/// <summary>
///     Represents a list of transactions in a block
/// </summary>
public sealed class BlockTransactions
{
    /// <summary>
    ///     Block identifier
    /// </summary>
    public required BlockId BlockId { get; init; }

    /// <summary>
    ///     Number of transactions requested
    /// </summary>
    public required uint RequestedCount { get; init; }

    /// <summary>
    ///     Whether the list is incomplete (there are more transactions)
    /// </summary>
    public required bool Incomplete { get; init; }

    /// <summary>
    ///     List of transaction identifiers
    /// </summary>
    public required List<BlockTransaction> Transactions { get; init; }

    /// <summary>
    ///     Proof data (if requested)
    /// </summary>
    public byte[]? Proof { get; init; }

    public override string ToString()
    {
        return $"BlockTransactions(block:{BlockId.Seqno}, count:{Transactions.Count}, incomplete:{Incomplete})";
    }
}

/// <summary>
///     Represents a single transaction identifier in a block
/// </summary>
public readonly record struct BlockTransaction
{
    /// <summary>
    ///     Account address (32 bytes)
    /// </summary>
    public required byte[] Account { get; init; }

    /// <summary>
    ///     Logical time
    /// </summary>
    public required long Lt { get; init; }

    /// <summary>
    ///     Transaction hash (32 bytes)
    /// </summary>
    public required byte[] Hash { get; init; }

    /// <summary>
    ///     Returns the account as a hex string
    /// </summary>
    public string AccountHex => Convert.ToHexString(Account);

    /// <summary>
    ///     Returns the hash as a hex string
    /// </summary>
    public string HashHex => Convert.ToHexString(Hash);

    public override string ToString()
    {
        return $"Tx(account:...{AccountHex[^16..]}, lt:{Lt}, hash:{HashHex[..16]}...)";
    }
}
using Ton.Adnl.Protocol;
using Ton.Core.Addresses;

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

    /// <summary>
    ///     Creates BlockTransactions from ADNL protocol's LiteServerBlockTransactions
    /// </summary>
    public static BlockTransactions FromAdnl(LiteServerBlockTransactions adnlResponse, uint requestedCount)
    {
        BlockId blockId = BlockId.FromAdnl(adnlResponse.Id);

        List<BlockTransaction> transactions = adnlResponse.Ids
            .Select(id => BlockTransaction.FromAdnl(id, blockId.Workchain))
            .ToList();

        return new BlockTransactions
        {
            BlockId = blockId,
            RequestedCount = requestedCount,
            Transactions = transactions,
            Incomplete = adnlResponse.Incomplete
        };
    }


    /// <inheritdoc />
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
    ///     Account address
    /// </summary>
    public required Address Account { get; init; }

    /// <summary>
    ///     Logical time
    /// </summary>
    public required long Lt { get; init; }

    /// <summary>
    ///     Transaction hash (32 bytes)
    /// </summary>
    public required byte[] Hash { get; init; }

    /// <summary>
    ///     Returns the hash as a hex string
    /// </summary>
    public string HashHex => Convert.ToHexString(Hash);

    /// <summary>
    ///     Creates BlockTransaction from ADNL protocol's LiteServerTransactionId
    /// </summary>
    /// <param name="adnlTx">The ADNL transaction identifier</param>
    /// <param name="workchain">The workchain of the block containing this transaction</param>
    public static BlockTransaction FromAdnl(LiteServerTransactionId adnlTx, int workchain)
    {
        return new BlockTransaction
        {
            Account = new Address(workchain, adnlTx.Account),
            Lt = adnlTx.Lt,
            Hash = adnlTx.Hash
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Tx(account:{Account}, lt:{Lt}, hash:{HashHex[..16]}...)";
    }
}
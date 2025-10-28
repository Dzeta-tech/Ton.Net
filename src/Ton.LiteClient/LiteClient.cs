using System.Numerics;
using Ton.Adnl.Protocol;
using Ton.Core.Boc;
using Ton.LiteClient.Engines;
using Ton.LiteClient.Models;
using Ton.LiteClient.Parsers;
using LiteServerTransactionId3 = Ton.Adnl.Protocol.LiteServerTransactionId3;

namespace Ton.LiteClient;

/// <summary>
///     High-level TON Lite Client for interacting with TON blockchain
/// </summary>
public sealed class LiteClient : IDisposable
{
    readonly bool ownsEngine;

    /// <summary>
    ///     Creates a new lite client with the specified engine
    /// </summary>
    /// <param name="engine">Lite engine instance</param>
    /// <param name="ownsEngine">Whether this client should dispose the engine when disposed</param>
    public LiteClient(ILiteEngine engine, bool ownsEngine = true)
    {
        ArgumentNullException.ThrowIfNull(engine);
        Engine = engine;
        this.ownsEngine = ownsEngine;
    }

    /// <summary>
    ///     Gets the underlying engine
    /// </summary>
    public ILiteEngine Engine { get; }

    public void Dispose()
    {
        if (ownsEngine) Engine.Dispose();
    }

    /// <summary>
    ///     Creates a new lite client with a single server connection
    /// </summary>
    /// <param name="host">Server host/IP</param>
    /// <param name="port">Server port</param>
    /// <param name="serverPublicKey">Server's Ed25519 public key (32 bytes or base64 string)</param>
    public static LiteClient Create(string host, int port, byte[] serverPublicKey)
    {
        LiteSingleEngine engine = new(host, port, serverPublicKey);
        return new LiteClient(engine);
    }

    /// <summary>
    ///     Creates a new lite client with a single server connection (base64 public key)
    /// </summary>
    public static LiteClient Create(string host, int port, string serverPublicKeyBase64)
    {
        LiteSingleEngine engine = new(host, port, serverPublicKeyBase64);
        return new LiteClient(engine);
    }

    /// <summary>
    ///     Gets current server time
    /// </summary>
    public async Task<DateTimeOffset> GetTimeAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var request = new GetTimeRequest();
        LiteServerCurrentTime response = await Engine.QueryAsync(
            request,
            static r => LiteServerCurrentTime.ReadFrom(r),
            timeout,
            cancellationToken);

        return DateTimeOffset.FromUnixTimeSeconds(response.Now);
    }

    /// <summary>
    ///     Gets masterchain information including latest block
    /// </summary>
    public async Task<MasterchainInfo> GetMasterchainInfoAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var request = new GetMasterchainInfoRequest();
        LiteServerMasterchainInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerMasterchainInfo.ReadFrom(r),
            timeout,
            cancellationToken);

        return new MasterchainInfo
        {
            Last = new BlockId(
                response.Last.Workchain,
                response.Last.Shard,
                unchecked((uint)response.Last.Seqno),
                response.Last.RootHash,
                response.Last.FileHash),
            StateRootHash = response.StateRootHash,
            Init = new ZeroStateId(
                response.Init.Workchain,
                response.Init.RootHash,
                response.Init.FileHash)
        };
    }

    /// <summary>
    ///     Gets version information from the lite server
    /// </summary>
    public async Task<(int Version, long Capabilities, int Now)> GetVersionAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var request = new GetVersionRequest();
        LiteServerVersion response = await Engine.QueryAsync(
            request,
            static r => LiteServerVersion.ReadFrom(r),
            timeout,
            cancellationToken);

        return (response.Version, response.Capabilities, response.Now);
    }

    /// <summary>
    ///     Looks up a block by workchain, shard, and seqno
    /// </summary>
    public async Task<BlockId> LookupBlockAsync(
        int workchain,
        long shard,
        uint seqno,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockId = new TonNodeBlockId
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = unchecked((int)seqno)
        };
        var request = new LookupBlockRequest(blockId); // Mode flags handled automatically

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        return new BlockId(
            response.Id.Workchain,
            response.Id.Shard,
            unchecked((uint)response.Id.Seqno),
            response.Id.RootHash,
            response.Id.FileHash);
    }

    /// <summary>
    ///     Gets block header information
    /// </summary>
    public async Task<BlockHeader> GetBlockHeaderAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockIdExt = new TonNodeBlockIdExt
        {
            Workchain = blockId.Workchain,
            Shard = blockId.Shard,
            Seqno = unchecked((int)blockId.Seqno),
            RootHash = blockId.RootHash,
            FileHash = blockId.FileHash
        };

        var request = new GetBlockHeaderRequest(blockIdExt); // Mode handled automatically

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        // Parse block header from data
        Cell headerCell = Cell.FromBoc(response.HeaderProof)[0];
        Slice headerSlice = headerCell.BeginParse();

        // Read block header fields (simplified - full parsing would be more complex)
        return new BlockHeader
        {
            Id = new BlockId(
                response.Id.Workchain,
                response.Id.Shard,
                unchecked((uint)response.Id.Seqno),
                response.Id.RootHash,
                response.Id.FileHash),
            GlobalId = 0, // Would need to parse from cell
            Version = 0,
            Flags = 0,
            AfterMerge = false,
            AfterSplit = false,
            BeforeSplit = false,
            WantMerge = false,
            WantSplit = false,
            ValidatorListHashShort = 0,
            CatchainSeqno = 0,
            MinRefMcSeqno = 0,
            IsKeyBlock = false,
            PrevKeyBlockSeqno = 0,
            GenUtime = 0,
            StartLt = BigInteger.Zero,
            EndLt = BigInteger.Zero
        };
    }

    /// <summary>
    ///     Gets all shard block IDs for a given block
    /// </summary>
    public async Task<BlockId[]> GetAllShardsInfoAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockIdExt = new TonNodeBlockIdExt
        {
            Workchain = blockId.Workchain,
            Shard = blockId.Shard,
            Seqno = unchecked((int)blockId.Seqno),
            RootHash = blockId.RootHash,
            FileHash = blockId.FileHash
        };

        var request = new GetAllShardsInfoRequest(blockIdExt);

        LiteServerAllShardsInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerAllShardsInfo.ReadFrom(r),
            timeout,
            cancellationToken);

        // Parse shard data from BOC and return as BlockId array
        return ShardParser.ParseShards(response.Data);
    }

    /// <summary>
    ///     Lists transactions in a specific block.
    /// </summary>
    /// <param name="blockId">The block identifier</param>
    /// <param name="count">Maximum number of transactions to return</param>
    /// <param name="after">Optional transaction ID to start after (for pagination)</param>
    /// <param name="timeout">Optional timeout in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Block transactions information</returns>
    public async Task<BlockTransactions> ListBlockTransactionsAsync(
        BlockId blockId,
        uint count = 40,
        LiteServerTransactionId3? after = null,
        int timeout = 10000,
        CancellationToken cancellationToken = default)
    {
        var blockIdExt = new TonNodeBlockIdExt
        {
            Workchain = blockId.Workchain,
            Shard = blockId.Shard,
            Seqno = unchecked((int)blockId.Seqno),
            RootHash = blockId.RootHash,
            FileHash = blockId.FileHash
        };

        var request = new ListBlockTransactionsRequest(blockIdExt, count, after);

        LiteServerBlockTransactions response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockTransactions.ReadFrom(r),
            timeout,
            cancellationToken);

        // Convert to user-friendly model
        var transactions = new List<BlockTransaction>();
        foreach (var txId in response.Ids)
        {
            transactions.Add(new BlockTransaction
            {
                Account = txId.Account,
                Lt = txId.Lt,
                Hash = txId.Hash
            });
        }

        return new BlockTransactions
        {
            BlockId = new BlockId(
                response.Id.Workchain,
                response.Id.Shard,
                unchecked((uint)response.Id.Seqno),
                response.Id.RootHash,
                response.Id.FileHash),
            RequestedCount = count,
            Transactions = transactions,
            Incomplete = response.Incomplete
        };
    }

}
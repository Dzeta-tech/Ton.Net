using Ton.Adnl.Protocol;
using Ton.Core.Addresses;
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
    ///     Gets current server time
    /// </summary>
    public async Task<DateTimeOffset> GetTimeAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetTimeRequest request = new();
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
        GetMasterchainInfoRequest request = new();
        LiteServerMasterchainInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerMasterchainInfo.ReadFrom(r),
            timeout,
            cancellationToken);

        return MasterchainInfo.FromAdnl(response);
    }

    /// <summary>
    ///     Gets extended masterchain information including version, capabilities, and timestamps
    /// </summary>
    public async Task<MasterchainInfoExt> GetMasterchainInfoExtAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetMasterchainInfoExtRequest request = new();
        LiteServerMasterchainInfoExt response = await Engine.QueryAsync(
            request,
            static r => LiteServerMasterchainInfoExt.ReadFrom(r),
            timeout,
            cancellationToken);

        return MasterchainInfoExt.FromAdnl(response);
    }

    /// <summary>
    ///     Gets version information from the lite server
    /// </summary>
    public async Task<(int Version, long Capabilities, int Now)> GetVersionAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetVersionRequest request = new();
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
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = unchecked((int)seqno)
        };
        LookupBlockRequest request = new(blockId); // Mode flags handled automatically

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        return BlockId.FromAdnl(response.Id);
    }

    /// <summary>
    ///     Looks up a block by workchain, shard, and unix timestamp
    /// </summary>
    public async Task<BlockId> LookupBlockByUtimeAsync(
        int workchain,
        long shard,
        int utime,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = 0
        };
        LookupBlockRequest request = new(blockId, null, utime);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        return BlockId.FromAdnl(response.Id);
    }

    /// <summary>
    ///     Looks up a block by workchain, shard, and logical time
    /// </summary>
    public async Task<BlockId> LookupBlockByLtAsync(
        int workchain,
        long shard,
        long lt,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = 0
        };
        LookupBlockRequest request = new(blockId, lt);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        return BlockId.FromAdnl(response.Id);
    }

    /// <summary>
    ///     Gets block header information
    /// </summary>
    public async Task<BlockHeader> GetBlockHeaderAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetBlockHeaderRequest request = new(blockId.ToAdnl()); // Mode handled automatically

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            timeout,
            cancellationToken);

        return BlockHeader.FromAdnl(response);
    }

    /// <summary>
    ///     Gets all shard block IDs for a given block
    /// </summary>
    public async Task<BlockId[]> GetAllShardsInfoAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetAllShardsInfoRequest request = new(blockId.ToAdnl());

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
        ListBlockTransactionsRequest request = new(blockId.ToAdnl(), count, after);

        LiteServerBlockTransactions response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockTransactions.ReadFrom(r),
            timeout,
            cancellationToken);

        return BlockTransactions.FromAdnl(response, count);
    }

    /// <summary>
    ///     Gets the state of an account at a specific block
    /// </summary>
    /// <param name="address">Account address</param>
    /// <param name="blockId">Block identifier</param>
    /// <param name="timeout">Optional timeout in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account state information</returns>
    public async Task<AccountState> GetAccountStateAsync(
        Address address,
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        LiteServerAccountId accountId = new()
        {
            Workchain = address.Workchain,
            Id = address.Hash
        };

        GetAccountStateRequest request = new(blockId.ToAdnl(), accountId);

        LiteServerAccountState response = await Engine.QueryAsync(
            request,
            static r => LiteServerAccountState.ReadFrom(r),
            timeout,
            cancellationToken);

        return AccountState.FromAdnl(response, address);
    }

    /// <summary>
    ///     Gets blockchain configuration parameters
    /// </summary>
    /// <param name="blockId">Block identifier</param>
    /// <param name="timeout">Optional timeout in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Config info with state proof and config proof</returns>
    public async Task<ConfigInfo> GetConfigAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        GetConfigAllRequest request = new(blockId.ToAdnl());

        LiteServerConfigInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerConfigInfo.ReadFrom(r),
            timeout,
            cancellationToken);

        return ConfigInfo.FromAdnl(response);
    }
}
using System.Numerics;
using Ton.Adnl.Protocol;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Tuple;
using Ton.Core.Types;
using Ton.Core.Utils;
using Ton.LiteClient.Engines;
using Ton.LiteClient.Models;
using Ton.LiteClient.Parsers;
using LiteServerTransactionId3 = Ton.Adnl.Protocol.LiteServerTransactionId3;
using AccountState = Ton.LiteClient.Models.AccountState;
using Tuple = Ton.Core.Tuple.Tuple;

namespace Ton.LiteClient;

/// <summary>
///     High-level TON Lite Client for interacting with TON blockchain
/// </summary>
public sealed class LiteClient : IDisposable
{
    /// <summary>
    ///     Creates a new lite client with the specified engine
    /// </summary>
    /// <param name="engine">Lite engine instance</param>
    public LiteClient(ILiteEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        Engine = engine;
    }

    /// <summary>
    ///     Gets the underlying engine
    /// </summary>
    public ILiteEngine Engine { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Engine.Dispose();
    }

    /// <summary>
    ///     Gets current server time
    /// </summary>
    public async Task<DateTimeOffset> GetTimeAsync(CancellationToken cancellationToken = default)
    {
        GetTimeRequest request = new();
        LiteServerCurrentTime response = await Engine.QueryAsync(
            request,
            static r => LiteServerCurrentTime.ReadFrom(r),
            cancellationToken);

        return DateTimeOffset.FromUnixTimeSeconds(response.Now);
    }

    /// <summary>
    ///     Gets masterchain information including latest block
    /// </summary>
    public async Task<MasterchainInfo> GetMasterchainInfoAsync(CancellationToken cancellationToken = default)
    {
        GetMasterchainInfoRequest request = new();
        LiteServerMasterchainInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerMasterchainInfo.ReadFrom(r),
            cancellationToken);

        return MasterchainInfo.FromAdnl(response);
    }

    /// <summary>
    ///     Gets extended masterchain information including version, capabilities, and timestamps
    /// </summary>
    public async Task<MasterchainInfoExt> GetMasterchainInfoExtAsync(CancellationToken cancellationToken = default)
    {
        // Mode 0: default mode
        GetMasterchainInfoExtRequest request = new(0);
        LiteServerMasterchainInfoExt response = await Engine.QueryAsync(
            request,
            static r => LiteServerMasterchainInfoExt.ReadFrom(r),
            cancellationToken);

        return MasterchainInfoExt.FromAdnl(response);
    }

    /// <summary>
    ///     Gets version information from the lite server
    /// </summary>
    public async Task<(int Version, long Capabilities, int Now)> GetVersionAsync(
        CancellationToken cancellationToken = default)
    {
        GetVersionRequest request = new();
        LiteServerVersion response = await Engine.QueryAsync(
            request,
            static r => LiteServerVersion.ReadFrom(r),
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
        CancellationToken cancellationToken = default)
    {
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = unchecked((int)seqno)
        };
        // Mode 1: lookup by seqno (bit 0)
        LookupBlockRequest request = new(1, blockId);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
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
        CancellationToken cancellationToken = default)
    {
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = 0
        };
        // Mode 4: lookup by utime (bit 2)
        LookupBlockRequest request = new(4, blockId, null, utime);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
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
        CancellationToken cancellationToken = default)
    {
        TonNodeBlockId blockId = new()
        {
            Workchain = workchain,
            Shard = shard,
            Seqno = 0
        };
        // Mode 2: lookup by lt (bit 1)
        LookupBlockRequest request = new(2, blockId, lt);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            cancellationToken);

        return BlockId.FromAdnl(response.Id);
    }

    /// <summary>
    ///     Gets block header information
    /// </summary>
    public async Task<BlockHeader> GetBlockHeaderAsync(
        BlockId blockId,
        CancellationToken cancellationToken = default)
    {
        // Mode 0: default mode
        GetBlockHeaderRequest request = new(blockId.ToAdnl(), 0);

        LiteServerBlockHeader response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockHeader.ReadFrom(r),
            cancellationToken);

        return BlockHeader.FromAdnl(response);
    }

    /// <summary>
    ///     Gets all shard descriptions for a given block.
    /// </summary>
    public async Task<ShardDescr[]> GetAllShardsInfoAsync(
        BlockId blockId,
        CancellationToken cancellationToken = default)
    {
        GetAllShardsInfoRequest request = new(blockId.ToAdnl());

        LiteServerAllShardsInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerAllShardsInfo.ReadFrom(r),
            cancellationToken);

        // Parse shard data from BOC and return as ShardDescr array
        return ShardParser.ParseShards(response.Data);
    }

    /// <summary>
    ///     Lists transactions in a specific block.
    /// </summary>
    /// <param name="blockId">The block identifier</param>
    /// <param name="count">Maximum number of transactions to return</param>
    /// <param name="after">Optional transaction ID to start after (for pagination)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Block transactions information</returns>
    public async Task<BlockTransactions> ListBlockTransactionsAsync(
        BlockId blockId,
        uint count = 256,
        LiteServerTransactionId3? after = null,
        CancellationToken cancellationToken = default)
    {
        // Mode 7: full transaction info (bits 0-2 set)
        ListBlockTransactionsRequest request = new(blockId.ToAdnl(), 7, count, after);

        LiteServerBlockTransactions response = await Engine.QueryAsync(
            request,
            static r => LiteServerBlockTransactions.ReadFrom(r),
            cancellationToken);

        return BlockTransactions.FromAdnl(response, count);
    }

    /// <summary>
    ///     Gets the state of an account at a specific block
    /// </summary>
    /// <param name="address">Account address</param>
    /// <param name="blockId">Block identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account state information</returns>
    public async Task<AccountState> GetAccountStateAsync(
        Address address,
        BlockId blockId,
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
            cancellationToken);

        return AccountState.FromAdnl(response, address);
    }

    /// <summary>
    ///     Gets blockchain configuration parameters
    /// </summary>
    /// <param name="blockId">Block identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Config info with state proof and config proof</returns>
    public async Task<ConfigInfo> GetConfigAsync(
        BlockId blockId,
        CancellationToken cancellationToken = default)
    {
        // Mode 0: default mode
        GetConfigAllRequest request = new(0, blockId.ToAdnl());

        LiteServerConfigInfo response = await Engine.QueryAsync(
            request,
            static r => LiteServerConfigInfo.ReadFrom(r),
            cancellationToken);

        return ConfigInfo.FromAdnl(response);
    }

    /// <summary>
    ///     Runs a smart contract get method
    /// </summary>
    /// <param name="blockId">Block to query at</param>
    /// <param name="address">Contract address</param>
    /// <param name="methodName">Method name</param>
    /// <param name="args">Method arguments as tuple items</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Method execution result</returns>
    public async Task<RunMethodResult> RunMethodAsync(
        BlockId blockId,
        Address address,
        string methodName,
        TupleItem[]? args = null,
        CancellationToken cancellationToken = default)
    {
        long methodId = MethodId.Get(methodName);

        byte[] paramsBytes = args is { Length: > 0 }
            ? Tuple.SerializeTuple(args).ToBoc()
            : [];

        // Mode 4: include stack in response
        RunSmcMethodRequest request = new(
            4,
            blockId.ToAdnl(),
            new LiteServerAccountId { Workchain = address.Workchain, Id = address.Hash },
            methodId,
            paramsBytes
        );

        LiteServerRunMethodResult response = await Engine.QueryAsync(
            request,
            static r => LiteServerRunMethodResult.ReadFrom(r),
            cancellationToken);

        TupleItem[] result = response.Result.Length > 0
            ? Tuple.ParseTuple(Cell.FromBoc(response.Result)[0])
            : [];

        return new RunMethodResult
        {
            ExitCode = response.ExitCode,
            Stack = new TupleReader(result),
            GasUsed = null, // Not provided by lite server
            Block = BlockId.FromAdnl(response.Id),
            ShardBlock = BlockId.FromAdnl(response.Shardblk)
        };
    }


    /// <summary>
    ///     Sends a message to the network
    /// </summary>
    /// <param name="message">Serialized message BOC</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Send status</returns>
    public async Task<int> SendMessageAsync(
        byte[] message,
        CancellationToken cancellationToken = default)
    {
        SendMessageRequest request = new(message);

        LiteServerSendMsgStatus response = await Engine.QueryAsync(
            request,
            static r => LiteServerSendMsgStatus.ReadFrom(r),
            cancellationToken);

        return response.Status;
    }

    /// <summary>
    ///     Gets account transactions (single request)
    /// </summary>
    /// <param name="address">Account address</param>
    /// <param name="count">Number of transactions to retrieve</param>
    /// <param name="lt">Starting logical time</param>
    /// <param name="hash">Starting transaction hash</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Account transactions with deserialized transaction objects</returns>
    public async Task<AccountTransactions> GetAccountTransactionsAsync(
        Address address,
        uint count,
        BigInteger lt,
        byte[] hash,
        CancellationToken cancellationToken = default)
    {
        GetTransactionsRequest request = new(
            count,
            new LiteServerAccountId
            {
                Workchain = address.Workchain,
                Id = address.Hash
            },
            (long)lt,
            hash
        );

        LiteServerTransactionList response = await Engine.QueryAsync(
            request,
            static r => LiteServerTransactionList.ReadFrom(r),
            cancellationToken
        );

        return AccountTransactions.FromAdnl(response);
    }

    /// <summary>
    ///     Opens a contract for interaction using this client
    /// </summary>
    /// <typeparam name="T">Contract type</typeparam>
    /// <param name="contract">Contract instance</param>
    /// <returns>Opened contract</returns>
    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return new OpenedContract<T>(contract, Provider(contract.Address, contract.Init));
    }

    /// <summary>
    ///     Creates a provider for interacting with a contract at a specific address
    /// </summary>
    /// <param name="address">Contract address</param>
    /// <param name="init">Optional state init for deployment</param>
    /// <returns>Contract provider</returns>
    public IContractProvider Provider(Address address, StateInit? init = null)
    {
        return new LiteClientProvider(this, null, address, init);
    }

    /// <summary>
    ///     Creates a provider for interacting with a contract at a specific address and block
    /// </summary>
    /// <param name="blockId">Block ID to query at</param>
    /// <param name="address">Contract address</param>
    /// <param name="init">Optional state init for deployment</param>
    /// <returns>Contract provider</returns>
    public IContractProvider ProviderAt(BlockId blockId, Address address, StateInit? init = null)
    {
        return new LiteClientProvider(this, blockId, address, init);
    }

    /// <summary>
    ///     Opens a contract at a specific block for interaction
    /// </summary>
    /// <typeparam name="T">Contract type</typeparam>
    /// <param name="blockId">Block ID to query at</param>
    /// <param name="contract">Contract instance</param>
    /// <returns>Opened contract</returns>
    public OpenedContract<T> OpenAt<T>(BlockId blockId, T contract) where T : IContract
    {
        return new OpenedContract<T>(contract, ProviderAt(blockId, contract.Address, contract.Init));
    }
}
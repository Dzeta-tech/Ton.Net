using System.Numerics;
using Ton.Core.Tuple;
using Ton.Core.Types;
using Ton.HttpClient.Api;
using Ton.HttpClient.Api.Models;
using Ton.HttpClient.Utils;
using TonDict = Ton.Core.Dict;

namespace Ton.HttpClient;

/// <summary>
///     TON blockchain client using Toncenter v2 API.
///     Implements IContractProvider for contract interaction.
/// </summary>
public class TonClient : IDisposable
{
    readonly HttpApi api;

    public TonClient(TonClientParameters parameters)
    {
        Parameters = parameters;
        api = new HttpApi(
            parameters.Endpoint,
            parameters.Timeout,
            parameters.ApiKey
        );
    }

    public TonClientParameters Parameters { get; }

    public void Dispose()
    {
        api.Dispose();
    }

    /// <summary>
    ///     Get balance of an address.
    /// </summary>
    public async Task<BigInteger> GetBalanceAsync(Address address)
    {
        ContractState state = await GetContractStateAsync(address);
        return state.Balance;
    }

    /// <summary>
    ///     Run a get method on a contract.
    /// </summary>
    public async Task<RunMethodResult> RunMethodAsync(Address address, string name, TupleItem[]? stack = null)
    {
        CallGetMethodResult result = await api.CallGetMethodAsync(address, name, stack ?? []);

        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Unable to execute get method. Got exit_code: {result.ExitCode}");

        TupleReader parsedStack = StackParser.ParseStack(result.Stack);
        return new RunMethodResult(result.GasUsed, parsedStack);
    }

    /// <summary>
    ///     Run a get method that may return an error code.
    /// </summary>
    public async Task<RunMethodErrorResult> RunMethodWithErrorAsync(Address address, string name,
        TupleItem[]? stack = null)
    {
        CallGetMethodResult result = await api.CallGetMethodAsync(address, name, stack ?? []);
        TupleReader parsedStack = StackParser.ParseStack(result.Stack);
        return new RunMethodErrorResult(result.GasUsed, parsedStack, result.ExitCode);
    }

    /// <summary>
    ///     Get transactions for an address.
    /// </summary>
    public async Task<List<Transaction>> GetTransactionsAsync(
        Address address,
        int limit,
        string? lt = null,
        string? hash = null,
        string? toLt = null,
        bool inclusive = false,
        bool archival = false)
    {
        // Adjust limit for inclusive mode
        int actualLimit = limit;
        if (hash != null && lt != null && !inclusive) actualLimit++;

        List<RawTransaction> rawTransactions =
            await api.GetTransactionsAsync(address, actualLimit, lt, hash, toLt, archival);

        List<Transaction> transactions = new();
        foreach (RawTransaction raw in rawTransactions)
        {
            byte[] boc = Convert.FromBase64String(raw.Data);
            Cell cell = Cell.FromBoc(boc)[0];
            Transaction tx = Transaction.Load(cell.BeginParse(), cell);
            transactions.Add(tx);
        }

        // Adjust result for inclusive mode
        if (hash != null && lt != null && !inclusive && transactions.Count > 0) transactions.RemoveAt(0);

        return transactions.Take(limit).ToList();
    }

    /// <summary>
    ///     Get single transaction by logical time and hash.
    /// </summary>
    public async Task<Transaction?> GetTransactionAsync(Address address, string lt, string hash, bool archival = false)
    {
        RawTransaction? raw = await api.GetTransactionAsync(address, lt, hash, archival);
        if (raw == null) return null;

        byte[] boc = Convert.FromBase64String(raw.Data);
        Cell cell = Cell.FromBoc(boc)[0];
        return Transaction.Load(cell.BeginParse(), cell);
    }

    /// <summary>
    ///     Get masterchain information.
    /// </summary>
    public async Task<MasterchainInfoResult> GetMasterchainInfoAsync()
    {
        MasterchainInfo info = await api.GetMasterchainInfoAsync();
        return new MasterchainInfoResult(
            info.Init.Workchain,
            info.Last.Shard,
            info.Init.Seqno,
            info.Last.Seqno
        );
    }

    /// <summary>
    ///     Get workchain shards for a given seqno.
    /// </summary>
    public async Task<List<ShardInfo>> GetWorkchainShardsAsync(int seqno)
    {
        List<BlockIdExt> shards = await api.GetShardsAsync(seqno);
        return shards.Select(s => new ShardInfo(s.Workchain, s.Shard, s.Seqno)).ToList();
    }

    /// <summary>
    ///     Get shard transactions.
    /// </summary>
    public async Task<List<ShardTransactionInfo>> GetShardTransactionsAsync(int workchain, int seqno, string shard)
    {
        BlockTransactions txs = await api.GetBlockTransactionsAsync(workchain, seqno, shard);

        if (txs.Incomplete) throw new NotSupportedException("Incomplete shard transactions are not supported");

        return txs.Transactions.Select(t => new ShardTransactionInfo(
            Address.ParseRaw(t.Account),
            t.Lt,
            t.Hash
        )).ToList();
    }

    /// <summary>
    ///     Send a message to the network.
    /// </summary>
    public async Task SendMessageAsync(Message message)
    {
        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();
        await api.SendBocAsync(cell.ToBoc());
    }

    /// <summary>
    ///     Send a BOC file to the network.
    /// </summary>
    public async Task SendFileAsync(byte[] boc)
    {
        await api.SendBocAsync(boc);
    }

    /// <summary>
    ///     Check if a contract is deployed (in active state).
    /// </summary>
    public async Task<bool> IsContractDeployedAsync(Address address)
    {
        ContractState state = await GetContractStateAsync(address);
        return state.State is ContractState.AccountStateInfo.Active;
    }

    /// <summary>
    ///     Get contract state from the blockchain.
    /// </summary>
    public async Task<ContractState> GetContractStateAsync(Address address)
    {
        AddressInformation info = await api.GetAddressInformationAsync(address);

        BigInteger balance = BigInteger.Parse(info.Balance);

        // Parse extra currencies
        TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? extraCurrencies = null;
        if (info.ExtraCurrencies != null && info.ExtraCurrencies.Count > 0)
        {
            Dictionary<uint, BigInteger> ecDict = new();
            foreach (ExtraCurrencyInfo ec in info.ExtraCurrencies) ecDict[(uint)ec.Id] = BigInteger.Parse(ec.Amount);

            // Convert to TON Dictionary
            TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> dictBuilder =
                TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Empty();
            foreach (KeyValuePair<uint, BigInteger> kvp in ecDict) dictBuilder.Set(kvp.Key, kvp.Value);
            extraCurrencies = dictBuilder;
        }

        // Parse last transaction
        ContractState.LastTransaction? lastTransaction = null;
        if (info.LastTransactionId.Lt != "0")
        {
            byte[] hash = Convert.FromBase64String(info.LastTransactionId.Hash);
            lastTransaction = new ContractState.LastTransaction(
                BigInteger.Parse(info.LastTransactionId.Lt),
                hash
            );
        }

        // Parse state
        ContractState.AccountStateInfo state = info.State switch
        {
            "active" => new ContractState.AccountStateInfo.Active(
                info.Code != "" ? Convert.FromBase64String(info.Code) : null,
                info.Data != "" ? Convert.FromBase64String(info.Data) : null
            ),
            "uninitialized" => new ContractState.AccountStateInfo.Uninit(),
            "frozen" => new ContractState.AccountStateInfo.Frozen(new byte[0]),
            _ => throw new NotSupportedException($"Unknown state: {info.State}")
        };

        return new ContractState
        {
            Balance = balance,
            ExtraCurrency = extraCurrencies,
            Last = lastTransaction,
            State = state
        };
    }

    /// <summary>
    ///     Try to locate result transaction.
    /// </summary>
    public async Task<Transaction> TryLocateResultTxAsync(Address source, Address destination, string createdLt)
    {
        RawTransaction raw = await api.TryLocateResultTxAsync(source, destination, createdLt);
        byte[] boc = Convert.FromBase64String(raw.Data);
        Cell cell = Cell.FromBoc(boc)[0];
        return Transaction.Load(cell.BeginParse(), cell);
    }

    /// <summary>
    ///     Try to locate source transaction.
    /// </summary>
    public async Task<Transaction> TryLocateSourceTxAsync(Address source, Address destination, string createdLt)
    {
        RawTransaction raw = await api.TryLocateSourceTxAsync(source, destination, createdLt);
        byte[] boc = Convert.FromBase64String(raw.Data);
        Cell cell = Cell.FromBoc(boc)[0];
        return Transaction.Load(cell.BeginParse(), cell);
    }

    /// <summary>
    ///     Open a contract for interaction.
    /// </summary>
    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return new OpenedContract<T>(contract, CreateProvider(contract.Address, contract.Init));
    }

    /// <summary>
    ///     Create a provider for an address.
    /// </summary>
    public IContractProvider Provider(Address address, StateInit? init = null)
    {
        return CreateProvider(address, init);
    }

    IContractProvider CreateProvider(Address address, StateInit? init)
    {
        return new TonClientProvider(this, address, init);
    }
}

/// <summary>
///     Result from running a get method.
/// </summary>
public record RunMethodResult(int GasUsed, TupleReader Stack);

/// <summary>
///     Result from running a get method that may fail.
/// </summary>
public record RunMethodErrorResult(int GasUsed, TupleReader Stack, int ExitCode);

/// <summary>
///     Masterchain information.
/// </summary>
public record MasterchainInfoResult(int Workchain, string Shard, int InitSeqno, int LatestSeqno);

/// <summary>
///     Shard information.
/// </summary>
public record ShardInfo(int Workchain, string Shard, int Seqno);

/// <summary>
///     Shard transaction information.
/// </summary>
public record ShardTransactionInfo(Address Account, string Lt, string Hash);
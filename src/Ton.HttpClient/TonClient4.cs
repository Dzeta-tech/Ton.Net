using System.Net.Http.Json;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Web;
using Ton.HttpClient.Api.Models;

namespace Ton.HttpClient;

/// <summary>
///     High-level TON blockchain HTTP client for Toncenter API v4.
///     Uses block seqno for deterministic queries.
/// </summary>
public class TonClient4 : IDisposable
{
    readonly string endpoint;
    readonly System.Net.Http.HttpClient httpClient;

    public TonClient4(TonClient4Parameters parameters)
    {
        endpoint = parameters.Endpoint.TrimEnd('/');
        httpClient = new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(parameters.Timeout)
        };
    }

    public void Dispose()
    {
        httpClient?.Dispose();
    }

    /// <summary>
    ///     Get last block information.
    /// </summary>
    public async Task<LastBlock> GetLastBlockAsync()
    {
        HttpResponseMessage response = await httpClient.GetAsync($"{endpoint}/block/latest");
        response.EnsureSuccessStatusCode();

        LastBlock? lastBlock = await response.Content.ReadFromJsonAsync<LastBlock>();
        if (lastBlock == null)
            throw new InvalidOperationException("Failed to deserialize last block response");

        return lastBlock;
    }

    /// <summary>
    ///     Get block information by sequence number.
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <returns>Block information</returns>
    public async Task<BlockDetails> GetBlockAsync(int seqno)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"{endpoint}/block/{seqno}");
        response.EnsureSuccessStatusCode();

        Block? block = await response.Content.ReadFromJsonAsync<Block>();
        if (block == null)
            throw new InvalidOperationException("Failed to deserialize block response");

        if (!block.Exist)
            throw new InvalidOperationException("Block is out of scope");

        return block.BlockData!;
    }

    /// <summary>
    ///     Get block information by unix timestamp.
    /// </summary>
    /// <param name="timestamp">Unix timestamp</param>
    /// <returns>Block information</returns>
    public async Task<BlockDetails> GetBlockByUtimeAsync(int timestamp)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"{endpoint}/block/utime/{timestamp}");
        response.EnsureSuccessStatusCode();

        Block? block = await response.Content.ReadFromJsonAsync<Block>();
        if (block == null)
            throw new InvalidOperationException("Failed to deserialize block response");

        if (!block.Exist)
            throw new InvalidOperationException("Block is out of scope");

        return block.BlockData!;
    }

    /// <summary>
    ///     Get account information at specified block.
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="address">Account address</param>
    /// <returns>Account information</returns>
    public async Task<AccountInfo> GetAccountAsync(int seqno, Address address)
    {
        string addressStr = address.ToString();
        HttpResponseMessage response = await httpClient.GetAsync($"{endpoint}/block/{seqno}/{addressStr}");
        response.EnsureSuccessStatusCode();

        AccountInfo? account = await response.Content.ReadFromJsonAsync<AccountInfo>();
        if (account == null)
            throw new InvalidOperationException("Failed to deserialize account response");

        return account;
    }

    /// <summary>
    ///     Get account lite information (without code and data).
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="address">Account address</param>
    /// <returns>Account lite information</returns>
    public async Task<AccountLiteInfo> GetAccountLiteAsync(int seqno, Address address)
    {
        string addressStr = address.ToString();
        HttpResponseMessage response = await httpClient.GetAsync($"{endpoint}/block/{seqno}/{addressStr}/lite");
        response.EnsureSuccessStatusCode();

        AccountLiteInfo? account = await response.Content.ReadFromJsonAsync<AccountLiteInfo>();
        if (account == null)
            throw new InvalidOperationException("Failed to deserialize account lite response");

        return account;
    }

    /// <summary>
    ///     Check if contract is deployed (in active state).
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="address">Contract address</param>
    /// <returns>True if contract is active</returns>
    public async Task<bool> IsContractDeployedAsync(int seqno, Address address)
    {
        AccountLiteInfo account = await GetAccountLiteAsync(seqno, address);
        return account.Account.State is AccountStateActive;
    }

    /// <summary>
    ///     Check if account was updated since specified logical time.
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="address">Account address</param>
    /// <param name="lt">Last transaction logical time</param>
    /// <returns>Account change information</returns>
    public async Task<AccountChanged> IsAccountChangedAsync(int seqno, Address address, BigInteger lt)
    {
        string addressStr = address.ToString();
        HttpResponseMessage response =
            await httpClient.GetAsync($"{endpoint}/block/{seqno}/{addressStr}/changed/{lt}");
        response.EnsureSuccessStatusCode();

        AccountChanged? changed = await response.Content.ReadFromJsonAsync<AccountChanged>();
        if (changed == null)
            throw new InvalidOperationException("Failed to deserialize account changed response");

        return changed;
    }

    /// <summary>
    ///     Get account transactions.
    /// </summary>
    /// <param name="address">Account address</param>
    /// <param name="lt">Last transaction logical time</param>
    /// <param name="hash">Last transaction hash</param>
    /// <returns>List of transactions with block information</returns>
    public async Task<List<(BlockRef Block, Transaction Transaction)>> GetAccountTransactionsAsync(
        Address address,
        BigInteger lt,
        byte[] hash)
    {
        string addressStr = address.ToString();
        string hashStr = ToUrlSafeBase64(hash);

        HttpResponseMessage response =
            await httpClient.GetAsync($"{endpoint}/account/{addressStr}/tx/{lt}/{hashStr}");
        response.EnsureSuccessStatusCode();

        TransactionsResponse? txResponse = await response.Content.ReadFromJsonAsync<TransactionsResponse>();
        if (txResponse == null)
            throw new InvalidOperationException("Failed to deserialize transactions response");

        // Decode BOC
        byte[] bocBytes = Convert.FromBase64String(txResponse.Boc);
        Cell[] cells = Cell.FromBoc(bocBytes);

        List<(BlockRef, Transaction)> result = [];
        for (int i = 0; i < txResponse.Blocks.Count; i++)
        {
            Transaction tx = Transaction.Load(cells[i].BeginParse(), cells[i]);
            result.Add((txResponse.Blocks[i], tx));
        }

        return result;
    }

    /// <summary>
    ///     Get network configuration.
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="ids">Optional specific config parameter IDs</param>
    /// <returns>Configuration response</returns>
    public async Task<ConfigResponse> GetConfigAsync(int seqno, int[]? ids = null)
    {
        string url = $"{endpoint}/block/{seqno}/config";

        if (ids is { Length: > 0 })
        {
            string sortedIds = string.Join(",", ids.OrderBy(x => x));
            url += $"/{sortedIds}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        ConfigResponse? config = await response.Content.ReadFromJsonAsync<ConfigResponse>();
        if (config == null)
            throw new InvalidOperationException("Failed to deserialize config response");

        return config;
    }

    /// <summary>
    ///     Run get method on a contract.
    /// </summary>
    /// <param name="seqno">Block sequence number</param>
    /// <param name="address">Contract address</param>
    /// <param name="methodName">Method name</param>
    /// <param name="args">Optional method arguments</param>
    /// <returns>Method execution result</returns>
    public async Task<(int ExitCode, TupleReader Reader, string? ResultRaw, BlockRef Block, BlockRef ShardBlock)>
        RunMethodAsync(int seqno, Address address, string methodName, TupleItem[]? args = null)
    {
        string addressStr = address.ToString();
        string encodedMethod = HttpUtility.UrlEncode(methodName);

        string url = $"{endpoint}/block/{seqno}/{addressStr}/run/{encodedMethod}";

        // TODO: Add serialized arguments if present
        // Tuple serialization needs proper implementation
        if (args is { Length: > 0 })
            throw new NotImplementedException("TonClient4 does not yet support method arguments");

        HttpResponseMessage response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        V4RunMethodResult? result = await response.Content.ReadFromJsonAsync<V4RunMethodResult>();
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize run method response");

        // Parse result tuple using StackParser
        TupleItem[] resultTuple = [];
        if (!string.IsNullOrEmpty(result.ResultRaw))
        {
            // ResultRaw is expected to be a hex-encoded stack representation
            // For now, we'll parse it as a raw tuple - this might need adjustment
            byte[] resultBoc = Convert.FromBase64String(result.ResultRaw);
            Cell[] resultCells = Cell.FromBoc(resultBoc);
            if (resultCells.Length > 0) resultTuple = [new TupleItemCell(resultCells[0])];
        }

        return (result.ExitCode, new TupleReader(resultTuple), result.ResultRaw, result.Block, result.ShardBlock);
    }

    /// <summary>
    ///     Send external message to the blockchain.
    /// </summary>
    /// <param name="message">Serialized message BOC</param>
    /// <returns>Send status</returns>
    public async Task<int> SendMessageAsync(byte[] message)
    {
        string bocBase64 = Convert.ToBase64String(message);
        StringContent content = new(
            JsonSerializer.Serialize(new { boc = bocBase64 }),
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response = await httpClient.PostAsync($"{endpoint}/send", content);
        response.EnsureSuccessStatusCode();

        SendResponse? result = await response.Content.ReadFromJsonAsync<SendResponse>();
        if (result == null)
            throw new InvalidOperationException("Failed to deserialize send response");

        return result.Status;
    }

    /// <summary>
    ///     Create contract provider for specified address.
    /// </summary>
    /// <param name="address">Contract address</param>
    /// <param name="init">Optional state init</param>
    /// <returns>Contract provider</returns>
    public IContractProvider Provider(Address address, StateInit? init = null)
    {
        return new TonClient4Provider(this, null, address, init);
    }

    /// <summary>
    ///     Create contract provider at specified block.
    /// </summary>
    /// <param name="block">Block sequence number</param>
    /// <param name="address">Contract address</param>
    /// <param name="init">Optional state init</param>
    /// <returns>Contract provider</returns>
    public IContractProvider ProviderAt(int block, Address address, StateInit? init = null)
    {
        return new TonClient4Provider(this, block, address, init);
    }

    /// <summary>
    ///     Open contract with this client.
    /// </summary>
    /// <typeparam name="T">Contract type</typeparam>
    /// <param name="contract">Contract instance</param>
    /// <returns>Opened contract</returns>
    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return ContractExtensions.Open(Provider(contract.Address, contract.Init), contract);
    }

    /// <summary>
    ///     Open contract at specified block.
    /// </summary>
    /// <typeparam name="T">Contract type</typeparam>
    /// <param name="block">Block sequence number</param>
    /// <param name="contract">Contract instance</param>
    /// <returns>Opened contract</returns>
    public OpenedContract<T> OpenAt<T>(int block, T contract) where T : IContract
    {
        return ContractExtensions.Open(ProviderAt(block, contract.Address, contract.Init), contract);
    }

    static string ToUrlSafeBase64(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
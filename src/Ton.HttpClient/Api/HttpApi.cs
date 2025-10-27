using System.Text;
using System.Text.Json;
using Ton.HttpClient.Api.Models;

namespace Ton.HttpClient.Api;

/// <summary>
///     Low-level HTTP API client for Toncenter v2 API.
/// </summary>
public class HttpApi : IDisposable
{
    readonly string? apiKey;
    readonly string endpoint;
    readonly System.Net.Http.HttpClient httpClient;

    public HttpApi(string endpoint, int timeout = 30000, string? apiKey = null)
    {
        this.endpoint = endpoint;
        this.apiKey = apiKey;
        httpClient = new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(timeout)
        };
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }

    /// <summary>
    ///     Get address information from the blockchain.
    /// </summary>
    public async Task<AddressInformation> GetAddressInformationAsync(Address address)
    {
        return await DoCallAsync<AddressInformation>("getAddressInformation", new { address = address.ToString() });
    }

    /// <summary>
    ///     Call a get method on a contract.
    /// </summary>
    public async Task<CallGetMethodResult> CallGetMethodAsync(Address address, string method, TupleItem[] stack)
    {
        return await DoCallAsync<CallGetMethodResult>("runGetMethod", new
        {
            address = address.ToString(),
            method,
            stack = SerializeStack(stack)
        });
    }

    /// <summary>
    ///     Send a BOC (Bag of Cells) to the network.
    /// </summary>
    public async Task SendBocAsync(byte[] boc)
    {
        await DoCallAsync<object>("sendBoc", new { boc = Convert.ToBase64String(boc) });
    }

    /// <summary>
    ///     Get masterchain info.
    /// </summary>
    public async Task<MasterchainInfo> GetMasterchainInfoAsync()
    {
        return await DoCallAsync<MasterchainInfo>("getMasterchainInfo", new { });
    }

    /// <summary>
    ///     Get transactions for an address.
    /// </summary>
    public async Task<List<RawTransaction>> GetTransactionsAsync(
        Address address,
        int limit,
        string? lt = null,
        string? hash = null,
        string? toLt = null,
        bool archival = false)
    {
        Dictionary<string, object> parameters = new()
        {
            ["address"] = address.ToString(),
            ["limit"] = limit
        };

        if (lt != null) parameters["lt"] = lt;
        if (hash != null)
        {
            // Convert base64 to hex
            byte[] hashBytes = Convert.FromBase64String(hash);
            parameters["hash"] = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        if (toLt != null) parameters["to_lt"] = toLt;
        // Only include archival if true
        if (archival) parameters["archival"] = archival;

        return await DoCallAsync<List<RawTransaction>>("getTransactions", parameters);
    }

    /// <summary>
    ///     Get single transaction by address, lt, and hash.
    /// </summary>
    public async Task<RawTransaction?> GetTransactionAsync(Address address, string lt, string hash,
        bool archival = false)
    {
        // Convert base64 to hex
        byte[] hashBytes = Convert.FromBase64String(hash);
        string hashHex = Convert.ToHexString(hashBytes).ToLowerInvariant();

        Dictionary<string, object> parameters = new()
        {
            ["address"] = address.ToString(),
            ["lt"] = lt,
            ["hash"] = hashHex,
            ["limit"] = 1
        };

        // Old transactions typically require archival access
        if (archival) parameters["archival"] = archival;

        List<RawTransaction> transactions = await DoCallAsync<List<RawTransaction>>("getTransactions", parameters);

        return transactions.FirstOrDefault(t => t.TransactionId.Lt == lt && t.TransactionId.Hash == hash);
    }

    /// <summary>
    ///     Get shards for a masterchain block.
    /// </summary>
    public async Task<List<BlockIdExt>> GetShardsAsync(int seqno)
    {
        ShardResponse response = await DoCallAsync<ShardResponse>("shards", new { seqno });
        return response.Shards;
    }

    /// <summary>
    ///     Get block transactions.
    /// </summary>
    public async Task<BlockTransactions> GetBlockTransactionsAsync(int workchain, int seqno, string shard)
    {
        return await DoCallAsync<BlockTransactions>("getBlockTransactions", new { workchain, seqno, shard });
    }

    /// <summary>
    ///     Try to locate result transaction.
    /// </summary>
    public async Task<RawTransaction> TryLocateResultTxAsync(Address source, Address destination, string createdLt)
    {
        return await DoCallAsync<RawTransaction>("tryLocateResultTx", new
        {
            source = source.ToString(),
            destination = destination.ToString(),
            created_lt = createdLt
        });
    }

    /// <summary>
    ///     Try to locate source transaction.
    /// </summary>
    public async Task<RawTransaction> TryLocateSourceTxAsync(Address source, Address destination, string createdLt)
    {
        return await DoCallAsync<RawTransaction>("tryLocateSourceTx", new
        {
            source = source.ToString(),
            destination = destination.ToString(),
            created_lt = createdLt
        });
    }

    async Task<T> DoCallAsync<T>(string method, object parameters)
    {
        var request = new
        {
            id = "1",
            jsonrpc = "2.0",
            method,
            @params = parameters
        };

        StringContent content = new(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        if (apiKey != null) content.Headers.Add("X-API-Key", apiKey);

        HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync();
        JsonDocument jsonDoc = JsonDocument.Parse(responseJson);

        if (!jsonDoc.RootElement.TryGetProperty("ok", out JsonElement okElement) || !okElement.GetBoolean())
            throw new InvalidOperationException($"API call failed: {responseJson}");

        JsonElement resultElement = jsonDoc.RootElement.GetProperty("result");
        return JsonSerializer.Deserialize<T>(resultElement.GetRawText())
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    static List<List<object>> SerializeStack(TupleItem[] items)
    {
        List<List<object>> result = [];
        foreach (TupleItem item in items) result.Add(SerializeTupleItem(item));
        return result;
    }

    static List<object> SerializeTupleItem(TupleItem item)
    {
        return item switch
        {
            TupleItemInt intItem => ["num", intItem.Value.ToString()],
            TupleItemCell cellItem => ["cell", Convert.ToBase64String(cellItem.Cell.ToBoc())],
            TupleItemSlice sliceItem => ["slice", Convert.ToBase64String(sliceItem.Cell.ToBoc())],
            TupleItemBuilder builderItem => ["builder", Convert.ToBase64String(builderItem.Cell.ToBoc())],
            TupleItemNaN => ["nan"],
            TupleItemNull => ["null"],
            _ => throw new NotSupportedException($"Tuple item type {item.GetType().Name} not supported")
        };
    }
}
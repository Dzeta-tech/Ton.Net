using System.Numerics;
using System.Text;
using System.Text.Json;
using Ton.Core.Tuple;
using Ton.HttpClient.Api.Models;

namespace Ton.HttpClient.Api;

/// <summary>
/// Low-level HTTP API client for Toncenter v2 API.
/// </summary>
public class HttpApi : IDisposable
{
    private readonly System.Net.Http.HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string? _apiKey;

    public HttpApi(string endpoint, int timeout = 30000, string? apiKey = null)
    {
        _endpoint = endpoint;
        _apiKey = apiKey;
        _httpClient = new System.Net.Http.HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(timeout)
        };
    }

    /// <summary>
    /// Get address information from the blockchain.
    /// </summary>
    public async Task<AddressInformation> GetAddressInformationAsync(Address address)
    {
        return await DoCallAsync<AddressInformation>("getAddressInformation", new { address = address.ToString() });
    }

    /// <summary>
    /// Call a get method on a contract.
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
    /// Send a BOC (Bag of Cells) to the network.
    /// </summary>
    public async Task SendBocAsync(byte[] boc)
    {
        await DoCallAsync<object>("sendBoc", new { boc = Convert.ToBase64String(boc) });
    }

    /// <summary>
    /// Get masterchain info.
    /// </summary>
    public async Task<MasterchainInfo> GetMasterchainInfoAsync()
    {
        return await DoCallAsync<MasterchainInfo>("getMasterchainInfo", new { });
    }

    /// <summary>
    /// Get transactions for an address.
    /// </summary>
    public async Task<List<RawTransaction>> GetTransactionsAsync(
        Address address,
        int limit,
        string? lt = null,
        string? hash = null,
        string? toLt = null)
    {
        var parameters = new Dictionary<string, object>
        {
            ["address"] = address.ToString(),
            ["limit"] = limit
        };

        if (lt != null) parameters["lt"] = lt;
        if (hash != null)
        {
            // Convert base64 to hex
            var hashBytes = Convert.FromBase64String(hash);
            parameters["hash"] = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
        if (toLt != null) parameters["to_lt"] = toLt;

        return await DoCallAsync<List<RawTransaction>>("getTransactions", parameters);
    }

    private async Task<T> DoCallAsync<T>(string method, object parameters)
    {
        var request = new
        {
            id = "1",
            jsonrpc = "2.0",
            method,
            @params = parameters
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        if (_apiKey != null)
        {
            content.Headers.Add("X-API-Key", _apiKey);
        }

        var response = await _httpClient.PostAsync(_endpoint, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);

        if (!jsonDoc.RootElement.TryGetProperty("ok", out var okElement) || !okElement.GetBoolean())
        {
            throw new InvalidOperationException($"API call failed: {responseJson}");
        }

        var resultElement = jsonDoc.RootElement.GetProperty("result");
        return JsonSerializer.Deserialize<T>(resultElement.GetRawText())
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private static List<List<object>> SerializeStack(TupleItem[] items)
    {
        var result = new List<List<object>>();
        foreach (var item in items)
        {
            result.Add(SerializeTupleItem(item));
        }
        return result;
    }

    private static List<object> SerializeTupleItem(TupleItem item)
    {
        return item switch
        {
            TupleItemInt intItem => new List<object> { "num", intItem.Value.ToString() },
            TupleItemCell cellItem => new List<object> { "cell", Convert.ToBase64String(cellItem.Cell.ToBoc()) },
            TupleItemSlice sliceItem => new List<object> { "slice", Convert.ToBase64String(sliceItem.Cell.ToBoc()) },
            TupleItemBuilder builderItem => new List<object> { "builder", Convert.ToBase64String(builderItem.Cell.ToBoc()) },
            TupleItemNaN => new List<object> { "nan" },
            TupleItemNull => new List<object> { "null" },
            _ => throw new NotSupportedException($"Tuple item type {item.GetType().Name} not supported")
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}


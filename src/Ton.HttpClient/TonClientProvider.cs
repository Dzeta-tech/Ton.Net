using System.Numerics;
using Ton.Core.Types;
using Ton.Core.Tuple;

namespace Ton.HttpClient;

/// <summary>
/// Implementation of IContractProvider for TonClient.
/// </summary>
internal class TonClientProvider : IContractProvider
{
    private readonly TonClient _client;
    private readonly Address _address;
    private readonly StateInit? _init;

    public TonClientProvider(TonClient client, Address address, StateInit? init)
    {
        _client = client;
        _address = address;
        _init = init;
    }

    public async Task<ContractState> GetStateAsync()
    {
        return await _client.GetContractStateAsync(_address);
    }

    public async Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args)
    {
        var result = await _client.RunMethodAsync(_address, name, args);
        return new ContractGetMethodResult
        {
            Stack = result.Stack,
            GasUsed = result.GasUsed
        };
    }

    public async Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args)
    {
        // Method ID is passed directly as the name for TON HTTP API
        return await GetAsync(methodId.ToString(), args);
    }

    public async Task ExternalAsync(Cell message)
    {
        // Determine if we need to include init
        StateInit? neededInit = null;
        if (_init != null && !await _client.IsContractDeployedAsync(_address))
        {
            neededInit = _init;
        }

        // Create external message
        var externalMsg = new CommonMessageInfo.ExternalIn(
            Src: null,
            Dest: _address,
            ImportFee: 0
        );

        var msg = new Message(
            externalMsg,
            message,
            neededInit
        );

        // Send message
        await _client.SendMessageAsync(msg);
    }

    public async Task InternalAsync(ISender via, InternalMessageArgs args)
    {
        // Determine if we need to include init
        StateInit? neededInit = null;
        if (_init != null && !await _client.IsContractDeployedAsync(_address))
        {
            neededInit = _init;
        }

        // Resolve bounce
        bool bounce = args.Bounce ?? true;

        // Resolve body
        Cell? body = args.Body;

        // Send via the sender
        await via.SendAsync(new SenderArguments
        {
            To = _address,
            Value = args.Value,
            Bounce = bounce,
            SendMode = args.SendMode,
            Init = neededInit,
            Body = body,
            ExtraCurrency = args.ExtraCurrency
        });
    }

    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return _client.Open(contract);
    }

    public async Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash, int? limit = null)
    {
        var transactions = await _client.GetTransactionsAsync(
            address,
            limit ?? 100,
            lt.ToString(),
            Convert.ToBase64String(hash),
            inclusive: true
        );
        return transactions.ToArray();
    }
}


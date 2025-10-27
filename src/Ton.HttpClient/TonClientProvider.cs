using System.Numerics;
using Ton.Core.Tuple;
using Ton.Core.Types;

namespace Ton.HttpClient;

/// <summary>
///     Implementation of IContractProvider for TonClient.
/// </summary>
internal class TonClientProvider : IContractProvider
{
    readonly Address address;
    readonly TonClient client;
    readonly StateInit? init;

    public TonClientProvider(TonClient client, Address address, StateInit? init)
    {
        this.client = client;
        this.address = address;
        this.init = init;
    }

    public async Task<ContractState> GetStateAsync()
    {
        return await client.GetContractStateAsync(address);
    }

    public async Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args)
    {
        RunMethodResult result = await client.RunMethodAsync(address, name, args);
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
        if (init != null && !await client.IsContractDeployedAsync(address)) neededInit = init;

        // Create external message
        CommonMessageInfo.ExternalIn externalMsg = new(
            null,
            address,
            0
        );

        Message msg = new(
            externalMsg,
            message,
            neededInit
        );

        // Send message
        await client.SendMessageAsync(msg);
    }

    public async Task InternalAsync(ISender via, InternalMessageArgs args)
    {
        // Determine if we need to include init
        StateInit? neededInit = null;
        if (init != null && !await client.IsContractDeployedAsync(address)) neededInit = init;

        // Resolve bounce
        bool bounce = args.Bounce ?? true;

        // Resolve body
        Cell? body = args.Body;

        // Send via the sender
        await via.SendAsync(new SenderArguments
        {
            To = address,
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
        return client.Open(contract);
    }

    public async Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash,
        int? limit = null)
    {
        List<Transaction> transactions = await client.GetTransactionsAsync(
            address,
            limit ?? 100,
            lt.ToString(),
            Convert.ToBase64String(hash),
            inclusive: true
        );
        return transactions.ToArray();
    }
}
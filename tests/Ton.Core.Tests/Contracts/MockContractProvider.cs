using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Tuple;
using Ton.Core.Types;

namespace Ton.Core.Tests.Contracts;

/// <summary>
///     Mock implementation of IContractProvider for testing.
/// </summary>
public class MockContractProvider : IContractProvider
{
    readonly List<Cell> externalMessages = [];
    readonly List<(ISender Sender, InternalMessageArgs Args)> internalMessages = [];
    readonly Dictionary<string, ContractGetMethodResult> methods = new();
    readonly Dictionary<int, ContractGetMethodResult> methodsById = new();
    readonly Dictionary<Address, List<Transaction>> transactions = new();
    ContractState? state;

    public IReadOnlyList<Cell> ExternalMessages => externalMessages;
    public IReadOnlyList<(ISender Sender, InternalMessageArgs Args)> InternalMessages => internalMessages;

    public Task<ContractState> GetStateAsync()
    {
        if (state == null)
            throw new InvalidOperationException("State not set. Call SetState() first.");
        return Task.FromResult(state);
    }

    public Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args)
    {
        if (methods.TryGetValue(name, out ContractGetMethodResult? result))
            return Task.FromResult(result);
        throw new NotImplementedException($"Method '{name}' not mocked. Call SetMethodResult() first.");
    }

    public Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args)
    {
        if (methodsById.TryGetValue(methodId, out ContractGetMethodResult? result))
            return Task.FromResult(result);
        throw new NotImplementedException($"Method ID {methodId} not mocked. Call SetMethodResult() first.");
    }

    public Task ExternalAsync(Cell message)
    {
        externalMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task InternalAsync(ISender via, InternalMessageArgs args)
    {
        internalMessages.Add((via, args));
        return Task.CompletedTask;
    }

    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return new OpenedContract<T>(contract, this);
    }

    public Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash, int? limit = null)
    {
        if (transactions.TryGetValue(address, out List<Transaction>? txs))
        {
            Transaction[] result = txs.ToArray();
            if (limit.HasValue)
                result = result.Take(limit.Value).ToArray();
            return Task.FromResult(result);
        }

        return Task.FromResult(Array.Empty<Transaction>());
    }

    public void SetState(ContractState state)
    {
        this.state = state;
    }

    public void SetMethodResult(string name, ContractGetMethodResult result)
    {
        methods[name] = result;
    }

    public void SetMethodResult(int methodId, ContractGetMethodResult result)
    {
        methodsById[methodId] = result;
    }

    public void SetTransactions(Address address, Transaction[] transactions)
    {
        this.transactions[address] = new List<Transaction>(transactions);
    }
}
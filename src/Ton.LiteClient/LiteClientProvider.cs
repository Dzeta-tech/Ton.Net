using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Tuple;
using Ton.Core.Types;
using Ton.LiteClient.Models;
using Ton.LiteClient.Utils;
using AccountState = Ton.LiteClient.Models.AccountState;

namespace Ton.LiteClient;

/// <summary>
///     Implementation of IContractProvider for LiteClient.
///     Provides blockchain interaction capabilities for contracts.
/// </summary>
internal class LiteClientProvider(LiteClient client, BlockId? block, Address address, StateInit? init)
    : IContractProvider
{
    /// <inheritdoc />
    public async Task<ContractState> GetStateAsync()
    {
        BlockId blockId = await ResolveBlockAsync();
        AccountState accountState = await client.GetAccountStateAsync(address, blockId);
        return ContractStateConverter.ToContractState(accountState);
    }

    /// <inheritdoc />
    public async Task<ContractGetMethodResult> GetAsync(string name, TupleItem[] args)
    {
        BlockId blockId = await ResolveBlockAsync();
        RunMethodResult result = await client.RunMethodAsync(blockId, address, name, args);

        if (result.ExitCode != 0 && result.ExitCode != 1)
            throw new ComputeError($"Method execution failed with exit code {result.ExitCode}", result.ExitCode);

        return new ContractGetMethodResult
        {
            Stack = result.Stack,
            GasUsed = result.GasUsed
        };
    }

    /// <inheritdoc />
    public async Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args)
    {
        return await GetAsync(methodId.ToString(), args);
    }

    /// <inheritdoc />
    public async Task ExternalAsync(Cell message)
    {
        // Determine if we need to include init
        StateInit? neededInit = null;
        if (init != null)
        {
            ContractState state = await GetStateAsync();
            if (state.State is not ContractState.AccountStateInfo.Active)
                neededInit = init;
        }

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
        Builder builder = Builder.BeginCell();
        msg.Store(builder);
        Cell cell = builder.EndCell();
        await client.SendMessageAsync(cell.ToBoc());
    }

    /// <inheritdoc />
    public async Task InternalAsync(ISender via, InternalMessageArgs args)
    {
        // Determine if we need to include init
        StateInit? neededInit = null;
        if (init != null)
        {
            ContractState state = await GetStateAsync();
            if (state.State is not ContractState.AccountStateInfo.Active)
                neededInit = init;
        }

        // Resolve bounce
        bool bounce = args.Bounce ?? true;

        // Send via the sender
        await via.SendAsync(new SenderArguments
        {
            To = address,
            Value = args.Value,
            Bounce = bounce,
            SendMode = args.SendMode ?? SendMode.SendPayFwdFeesSeparately,
            Init = neededInit,
            Body = args.Body,
            ExtraCurrency = args.ExtraCurrency
        });
    }

    /// <inheritdoc />
    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return client.Open(contract);
    }

    /// <inheritdoc />
    public async Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash,
        int? limit = null)
    {
        List<Transaction> transactions = [];
        int count = Math.Min(limit ?? 100, 100); // Max 100 per request
        BigInteger currentLt = lt;
        byte[] currentHash = hash;

        while (true)
        {
            AccountTransactions txList = await client.GetAccountTransactionsAsync(
                address,
                (uint)count,
                currentLt,
                currentHash
            );

            if (txList.Transactions.Count == 0)
                break;

            foreach (Transaction tx in txList.Transactions)
            {
                transactions.Add(tx);

                if (limit.HasValue && transactions.Count >= limit.Value)
                    return [.. transactions];

                currentLt = tx.Lt;
                currentHash = tx.Hash().ToArray();
            }

            // If we got fewer transactions than requested, we've reached the end
            if (txList.Transactions.Count < count)
                break;
        }

        return [.. transactions];
    }

    async Task<BlockId> ResolveBlockAsync()
    {
        if (block != null)
            return block;

        MasterchainInfoExt info = await client.GetMasterchainInfoExtAsync();
        return info.Last;
    }
}
using System.Numerics;
using Ton.HttpClient.Api.Models;

namespace Ton.HttpClient;

/// <summary>
///     Implementation of IContractProvider for TonClient4.
/// </summary>
internal class TonClient4Provider : IContractProvider
{
    readonly Address _address;
    readonly int? _block;
    readonly TonClient4 _client;
    readonly StateInit? _init;

    public TonClient4Provider(TonClient4 client, int? block, Address address, StateInit? init)
    {
        _client = client;
        _block = block;
        _address = address;
        _init = init;
    }

    public async Task<ContractState> GetStateAsync()
    {
        // Resolve block
        int? seqno = _block;
        if (seqno == null)
        {
            LastBlock lastBlock = await _client.GetLastBlockAsync();
            seqno = lastBlock.Last.Seqno;
        }

        // Load state
        AccountInfo accountInfo = await _client.GetAccountAsync(seqno.Value, _address);

        // Convert last transaction
        ContractState.LastTransaction? last = null;
        if (accountInfo.Account.Last != null)
        {
            BigInteger ltValue = BigInteger.Parse(accountInfo.Account.Last.Lt);
            byte[] hashBytes = Convert.FromBase64String(accountInfo.Account.Last.Hash);
            last = new ContractState.LastTransaction(ltValue, hashBytes);
        }

        // Convert state
        ContractState.AccountStateInfo stateInfo;
        switch (accountInfo.Account.State)
        {
            case AccountStateActive active:
                byte[]? code = active.Code != null ? Convert.FromBase64String(active.Code) : null;
                byte[]? data = active.Data != null ? Convert.FromBase64String(active.Data) : null;
                stateInfo = new ContractState.AccountStateInfo.Active(code, data);
                break;

            case AccountStateFrozen frozen:
                byte[] stateHash = Convert.FromBase64String(frozen.StateHash);
                stateInfo = new ContractState.AccountStateInfo.Frozen(stateHash);
                break;

            case AccountStateUninit:
                stateInfo = new ContractState.AccountStateInfo.Uninit();
                break;

            default:
                throw new InvalidOperationException("Unsupported account state type");
        }

        // Convert extra currencies
        TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>? extraCurrency = null;
        if (accountInfo.Account.Balance.Currencies is { Count: > 0 })
        {
            TonDict.IDictionaryKey<TonDict.DictKeyUint> dictKey = TonDict.DictionaryKeys.Uint(32);
            extraCurrency = TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Empty(dictKey);
            foreach ((string key, string value) in accountInfo.Account.Balance.Currencies)
                extraCurrency.Set(uint.Parse(key), BigInteger.Parse(value));
        }

        BigInteger balance = BigInteger.Parse(accountInfo.Account.Balance.Coins);

        return new ContractState
        {
            Balance = balance,
            ExtraCurrency = extraCurrency,
            Last = last,
            State = stateInfo
        };
    }

    public async Task<ContractGetMethodResult> GetAsync(string methodName, TupleItem[] args)
    {
        int? seqno = _block;
        if (seqno == null)
        {
            LastBlock lastBlock = await _client.GetLastBlockAsync();
            seqno = lastBlock.Last.Seqno;
        }

        (int ExitCode, TupleReader Reader, string? ResultRaw, BlockRef Block, BlockRef ShardBlock) result =
            await _client.RunMethodAsync(seqno.Value, _address, methodName, args);

        if (result.ExitCode != 0 && result.ExitCode != 1)
            throw new ComputeError($"Exit code: {result.ExitCode}", result.ExitCode);

        return new ContractGetMethodResult
        {
            Stack = result.Reader
        };
    }

    public async Task<ContractGetMethodResult> GetAsync(int methodId, TupleItem[] args)
    {
        // TonClient4 doesn't support method ID directly, convert to string
        return await GetAsync(methodId.ToString(), args);
    }

    public async Task ExternalAsync(Cell message)
    {
        // Resolve last block
        LastBlock lastBlock = await _client.GetLastBlockAsync();

        // Check if we need to include init
        StateInit? neededInit = null;
        if (_init != null)
        {
            AccountLiteInfo account = await _client.GetAccountLiteAsync(lastBlock.Last.Seqno, _address);
            if (account.Account.State is not AccountStateActive) neededInit = _init;
        }

        // Build external message
        CommonMessageInfo.ExternalIn externalMsg = new(
            null,
            _address,
            BigInteger.Zero
        );

        Message msg = new(externalMsg, message, neededInit);

        // Serialize and send
        Builder builder = new();
        msg.Store(builder);
        Cell cell = builder.EndCell();
        byte[] boc = cell.ToBoc();

        await _client.SendMessageAsync(boc);
    }

    public async Task InternalAsync(ISender via, InternalMessageArgs args)
    {
        // Resolve last block
        LastBlock lastBlock = await _client.GetLastBlockAsync();

        // Check if we need to include init
        StateInit? neededInit = null;
        if (_init != null)
        {
            AccountLiteInfo account = await _client.GetAccountLiteAsync(lastBlock.Last.Seqno, _address);
            if (account.Account.State is not AccountStateActive) neededInit = _init;
        }

        // Resolve bounce
        bool bounce = args.Bounce ?? true;

        // Send internal message via sender
        await via.SendAsync(new SenderArguments
        {
            To = _address,
            Value = args.Value,
            Bounce = bounce,
            SendMode = args.SendMode,
            Init = neededInit,
            Body = args.Body,
            ExtraCurrency = args.ExtraCurrency
        });
    }

    public OpenedContract<T> Open<T>(T contract) where T : IContract
    {
        return ContractExtensions.Open(new TonClient4Provider(_client, _block, contract.Address, contract.Init),
            contract);
    }

    public async Task<Transaction[]> GetTransactionsAsync(Address address, BigInteger lt, byte[] hash,
        int? limit = null)
    {
        // Check limit
        if (limit is <= 0) return [];

        List<Transaction> transactions = [];
        BigInteger currentLt = lt;
        byte[] currentHash = hash;

        do
        {
            List<(BlockRef Block, Transaction Transaction)> txs =
                await _client.GetAccountTransactionsAsync(address, currentLt, currentHash);

            if (txs.Count == 0)
                break;

            Transaction firstTx = txs[0].Transaction;
            bool needSkipFirst = transactions.Count > 0 &&
                                 firstTx.Lt == (ulong)currentLt &&
                                 firstTx.Raw.Hash().SequenceEqual(currentHash);

            if (needSkipFirst) txs.RemoveAt(0);

            if (txs.Count == 0)
                break;

            Transaction lastTx = txs[^1].Transaction;
            BigInteger lastLt = lastTx.Lt;
            byte[] lastHash = lastTx.Raw.Hash();

            if (lastLt == currentLt && lastHash.SequenceEqual(currentHash))
                break;

            transactions.AddRange(txs.Select(tx => tx.Transaction));
            currentLt = lastLt;
            currentHash = lastHash;
        } while (!limit.HasValue || transactions.Count < limit.Value);

        // Apply limit
        if (limit.HasValue && transactions.Count > limit.Value) transactions = transactions.Take(limit.Value).ToList();

        return transactions.ToArray();
    }
}
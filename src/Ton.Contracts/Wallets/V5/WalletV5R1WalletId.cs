using System.Numerics;
using Ton.Core.Boc;

namespace Ton.Contracts.Wallets.V5;

/// <summary>
///     Wallet ID for V5R1
///     Schema:
///     wallet_id -- int32
///     wallet_id = global_id ^ context_id
///     context_id_client$1 = wc:int8 wallet_version:uint8 counter:uint15
///     context_id_backoffice$0 = counter:uint31
/// </summary>
public record WalletIdV5R1(int NetworkGlobalId, object Context)
{
    /// <summary>
    ///     Network global ID
    ///     -239 is mainnet, -3 is testnet
    /// </summary>
    public int NetworkGlobalId { get; init; } = NetworkGlobalId;

    /// <summary>
    ///     Context (either client context or custom context)
    /// </summary>
    public object Context { get; init; } = Context;
}

/// <summary>
///     Client context for Wallet V5R1
/// </summary>
public record WalletIdV5R1ClientContext(string WalletVersion, int Workchain, int SubwalletNumber);

/// <summary>
///     Custom context for Wallet V5R1 (31-bit unsigned integer)
/// </summary>
public record WalletIdV5R1CustomContext
{
    public WalletIdV5R1CustomContext(uint value)
    {
        if (value >= 1u << 31) throw new ArgumentException("Custom context must be a 31-bit unsigned integer");
        Value = value;
    }

    public uint Value { get; init; }
}

/// <summary>
///     Helper class for Wallet V5R1 Wallet ID
/// </summary>
public static class WalletV5R1WalletIdHelper
{
    static readonly Dictionary<string, uint> WalletV5R1VersionsSerialization = new()
    {
        { "v5r1", 0 }
    };

    public static bool IsWalletIdV5R1ClientContext(object context)
    {
        return context is WalletIdV5R1ClientContext;
    }

    /// <summary>
    ///     Load wallet ID from various formats
    /// </summary>
    public static WalletIdV5R1 LoadWalletIdV5R1(object value, int networkGlobalId)
    {
        // Convert input to int32
        int val;
        if (value is BigInteger bigInt)
        {
            val = (int)bigInt;
        }
        else if (value is byte[] bytes)
        {
            if (bytes.Length != 4) throw new ArgumentException("Buffer must be exactly 4 bytes");
            val = (int)new BitReader(new BitString(bytes, 0, 32)).LoadInt(32);
        }
        else if (value is Slice slice)
        {
            byte[] buffer = slice.LoadBuffer(4);
            val = (int)new BitReader(new BitString(buffer, 0, 32)).LoadInt(32);
        }
        else
        {
            throw new ArgumentException($"Unsupported value type: {value.GetType()}");
        }

        // XOR with network global ID to get context
        long context = val ^ (long)networkGlobalId;

        // Parse context
        Slice contextCell = new Builder()
            .StoreInt(context, 32)
            .EndCell()
            .BeginParse();

        bool isClientContext = contextCell.LoadBit();
        if (isClientContext)
        {
            long workchain = contextCell.LoadInt(8);
            long walletVersionRaw = contextCell.LoadUint(8);
            long subwalletNumber = contextCell.LoadUint(15);

            string? walletVersion = WalletV5R1VersionsSerialization
                .FirstOrDefault(x => x.Value == walletVersionRaw)
                .Key;

            if (walletVersion == null)
                throw new InvalidOperationException(
                    $"Can't deserialize walletId: unknown wallet version {walletVersionRaw}"
                );

            return new WalletIdV5R1(
                networkGlobalId,
                new WalletIdV5R1ClientContext(walletVersion, (int)workchain, (int)subwalletNumber)
            );
        }

        long customContext = contextCell.LoadUint(31);
        return new WalletIdV5R1(
            networkGlobalId,
            new WalletIdV5R1CustomContext((uint)customContext)
        );
    }

    /// <summary>
    ///     Store wallet ID
    /// </summary>
    public static Action<Builder> StoreWalletIdV5R1(WalletIdV5R1 walletId)
    {
        return builder =>
        {
            long context;
            if (IsWalletIdV5R1ClientContext(walletId.Context))
            {
                WalletIdV5R1ClientContext clientContext = (WalletIdV5R1ClientContext)walletId.Context;
                context = new Builder()
                    .StoreBit(true)
                    .StoreInt(clientContext.Workchain, 8)
                    .StoreUint(WalletV5R1VersionsSerialization[clientContext.WalletVersion], 8)
                    .StoreUint((ulong)clientContext.SubwalletNumber, 15)
                    .EndCell()
                    .BeginParse()
                    .LoadInt(32);
            }
            else if (walletId.Context is WalletIdV5R1CustomContext customContext)
            {
                context = new Builder()
                    .StoreBit(false)
                    .StoreUint(customContext.Value, 31)
                    .EndCell()
                    .BeginParse()
                    .LoadInt(32);
            }
            else
            {
                throw new ArgumentException($"Unknown context type: {walletId.Context.GetType()}");
            }

            builder.StoreInt(walletId.NetworkGlobalId ^ context, 32);
        };
    }
}
using System.Numerics;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Types;

/// <summary>
///     MasterchainStateExtra - masterchain state extra information.
///     Source:
///     https://github.com/ton-foundation/ton/blob/ae5c0720143e231c32c3d2034cfe4e533a16d969/crypto/block/block.tlb#L534
///     masterchain_state_extra#cc26
///     shard_hashes:ShardHashes
///     config:ConfigParams
///     extra_ref: flags, validator_info, prev_blocks, etc.
///     global_balance:CurrencyCollection
/// </summary>
public record MasterchainStateExtra
{
    public MasterchainStateExtra(
        BigInteger configAddress, TonDict.Dictionary<TonDict.DictKeyInt, Cell> config,
        CurrencyCollection globalBalance)
    {
        ConfigAddress = configAddress;
        Config = config;
        GlobalBalance = globalBalance;
    }

    /// <summary>
    ///     Configuration address (256 bits).
    /// </summary>
    public BigInteger ConfigAddress { get; init; }

    /// <summary>
    ///     Configuration parameters (Hashmap 32 ^Cell).
    /// </summary>
    public TonDict.Dictionary<TonDict.DictKeyInt, Cell> Config { get; init; }

    /// <summary>
    ///     Global balance (total TON in circulation).
    /// </summary>
    public CurrencyCollection GlobalBalance { get; init; }

    /// <summary>
    ///     Loads MasterchainStateExtra from slice.
    /// </summary>
    public static MasterchainStateExtra Load(Slice slice)
    {
        // Check magic
        uint magic = (uint)slice.LoadUint(16);
        if (magic != 0xcc26)
            throw new InvalidOperationException($"Invalid magic: expected 0xcc26, got 0x{magic:x4}");

        // Skip shard_hashes (Maybe ^Cell)
        if (slice.LoadBit())
            slice.LoadRef();

        // Read ConfigParams: config_addr:bits256 config:^(Hashmap 32 ^Cell)
        BigInteger configAddress = slice.LoadUintBig(256);
        TonDict.Dictionary<TonDict.DictKeyInt, Cell> config = TonDict.Dictionary<TonDict.DictKeyInt, Cell>.LoadDirect(
            TonDict.DictionaryKeys.Int(32),
            TonDict.DictionaryValues.Cell(),
            slice
        );

        // Skip the extra ref (flags, validator_info, etc.)
        // This contains advanced data that's rarely needed
        if (slice.RemainingRefs > 0)
            slice.Skip(1); // Just skip checking if we have a ref, we'll skip the ref load if not needed

        // Read global balance
        CurrencyCollection globalBalance = CurrencyCollection.Load(slice);

        return new MasterchainStateExtra(configAddress, config, globalBalance);
    }

    /// <summary>
    ///     Stores MasterchainStateExtra to builder.
    ///     Note: This is a simplified implementation that only stores the essential fields.
    /// </summary>
    public void Store(Builder builder)
    {
        // Store magic
        builder.StoreUint(0xcc26, 16);

        // Skip shard_hashes (store empty Maybe)
        builder.StoreBit(false);

        // Store ConfigParams
        builder.StoreUint(ConfigAddress, 256);
        Config.StoreDirect(builder);

        // Skip extra ref (would need to store flags, validator_info, etc.)
        // For now, store an empty cell as placeholder
        builder.StoreRef(Builder.BeginCell().EndCell());

        // Store global balance
        GlobalBalance.Store(builder);
    }
}
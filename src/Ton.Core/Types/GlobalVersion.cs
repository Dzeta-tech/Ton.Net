using Ton.Core.Boc;

namespace Ton.Core.Types;

/// <summary>
///     Global version information.
///     Source:
///     https://github.com/ton-blockchain/ton/blob/24dc184a2ea67f9c47042b4104bbb4d82289fac1/crypto/block/block.tlb#L505
///     global_version#c4 version:uint32 capabilities:uint64 = GlobalVersion;
/// </summary>
public record GlobalVersion
{
    /// <summary>
    ///     Creates a new GlobalVersion.
    /// </summary>
    public GlobalVersion(uint version, ulong capabilities)
    {
        Version = version;
        Capabilities = capabilities;
    }

    /// <summary>
    ///     Protocol version.
    /// </summary>
    public uint Version { get; init; }

    /// <summary>
    ///     Capabilities flags.
    /// </summary>
    public ulong Capabilities { get; init; }

    /// <summary>
    ///     Loads GlobalVersion from a Slice.
    /// </summary>
    public static GlobalVersion Load(Slice slice)
    {
        if (slice.LoadUint(8) != 0xc4)
            throw new InvalidOperationException("Invalid GlobalVersion prefix");

        uint version = (uint)slice.LoadUint(32);
        ulong capabilities = (ulong)slice.LoadUintBig(64);

        return new GlobalVersion(version, capabilities);
    }

    /// <summary>
    ///     Stores GlobalVersion into a Builder.
    /// </summary>
    public void Store(Builder builder)
    {
        builder.StoreUint(0xc4, 8);
        builder.StoreUint(Version, 32);
        builder.StoreUint(Capabilities, 64);
    }
}


using Ton.Adnl.Protocol;

namespace Ton.LiteClient.Models;

/// <summary>
///     Represents blockchain configuration information
/// </summary>
public sealed class ConfigInfo
{
    /// <summary>
    ///     Block identifier where the config was retrieved
    /// </summary>
    public required BlockId Block { get; init; }

    /// <summary>
    ///     State proof (BOC-encoded)
    /// </summary>
    public required byte[] StateProof { get; init; }

    /// <summary>
    ///     Config proof (BOC-encoded)
    /// </summary>
    public required byte[] ConfigProof { get; init; }

    /// <summary>
    ///     Creates ConfigInfo from ADNL protocol's LiteServerConfigInfo
    /// </summary>
    public static ConfigInfo FromAdnl(LiteServerConfigInfo adnlConfig)
    {
        return new ConfigInfo
        {
            Block = BlockId.FromAdnl(adnlConfig.Id),
            StateProof = adnlConfig.StateProof,
            ConfigProof = adnlConfig.ConfigProof
        };
    }

    public override string ToString()
    {
        return $"ConfigInfo(block:{Block.Seqno}, stateProof:{StateProof.Length}b, configProof:{ConfigProof.Length}b)";
    }
}
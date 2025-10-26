using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Addresses;

/// <summary>
///     Utility for computing contract addresses from StateInit.
/// </summary>
public static class ContractAddress
{
    /// <summary>
    ///     Computes a contract address from its StateInit (code and data).
    ///     The address is deterministic - same StateInit always produces the same address.
    /// </summary>
    /// <param name="workchain">Workchain ID (0 for basechain, -1 for masterchain)</param>
    /// <param name="init">Contract initialization state (code and data)</param>
    /// <returns>Computed contract address</returns>
    public static Address From(int workchain, StateInit init)
    {
        // Serialize StateInit to cell
        Builder builder = Builder.BeginCell();
        init.Store(builder);
        Cell cell = builder.EndCell();

        // Hash the cell
        byte[] hash = cell.Hash();

        // Create address from workchain and hash
        return new Address(workchain, hash);
    }

    /// <summary>
    ///     Computes a contract address from its StateInit on the basechain (workchain 0).
    /// </summary>
    /// <param name="init">Contract initialization state (code and data)</param>
    /// <returns>Computed contract address on workchain 0</returns>
    public static Address From(StateInit init)
    {
        return From(0, init);
    }
}
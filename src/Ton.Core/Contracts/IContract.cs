using Ton.Core.Addresses;
using Ton.Core.Types;

namespace Ton.Core.Contracts;

/// <summary>
///     Base contract interface.
///     All contracts must implement this interface.
/// </summary>
public interface IContract
{
    /// <summary>
    ///     Contract address on the blockchain.
    /// </summary>
    Address Address { get; }

    /// <summary>
    ///     Optional state initialization (code and data).
    ///     Required for deploying new contracts.
    /// </summary>
    StateInit? Init { get; }

    /// <summary>
    ///     Optional contract ABI for tooling and code generation.
    /// </summary>
    ContractABI? ABI { get; }
}
namespace Ton.Core.Contracts;

/// <summary>
///     Wraps a contract with its provider for convenient method calls.
///     This allows contract methods to automatically receive the provider without
///     needing to pass it explicitly each time.
/// </summary>
/// <typeparam name="T">Contract type</typeparam>
public class OpenedContract<T> where T : IContract
{
    /// <summary>
    ///     Creates a new opened contract.
    /// </summary>
    public OpenedContract(T contract, IContractProvider provider)
    {
        Contract = contract;
        Provider = provider;
    }

    /// <summary>
    ///     The wrapped contract.
    /// </summary>
    public T Contract { get; }

    /// <summary>
    ///     The provider used for blockchain interactions.
    /// </summary>
    public IContractProvider Provider { get; }
}

/// <summary>
///     Extension methods for convenient contract interaction.
/// </summary>
public static class ContractExtensions
{
    /// <summary>
    ///     Open a contract with a provider.
    ///     This wraps the contract and provider together for easier method calls.
    /// </summary>
    /// <example>
    ///     var wallet = client.Open(WalletV4.Create(...));
    ///     var seqno = await wallet.GetSeqnoAsync();
    /// </example>
    public static OpenedContract<T> Open<T>(this IContractProvider provider, T contract)
        where T : IContract
    {
        return new OpenedContract<T>(contract, provider);
    }
}
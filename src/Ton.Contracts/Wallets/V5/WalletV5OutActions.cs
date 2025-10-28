using Ton.Core.Addresses;
using Ton.Core.Types;

namespace Ton.Contracts.Wallets.V5;

/// <summary>
///     Base interface for all wallet V5 actions (includes OutAction and V5 extended actions)
/// </summary>
public interface IWalletV5Action
{
    string Type { get; }
}

/// <summary>
///     Add extension action for Wallet V5
/// </summary>
public record OutActionAddExtension(Address Address) : IWalletV5Action
{
    public string Type => "addExtension";
}

/// <summary>
///     Remove extension action for Wallet V5
/// </summary>
public record OutActionRemoveExtension(Address Address) : IWalletV5Action
{
    public string Type => "removeExtension";
}

/// <summary>
///     Set is public key enabled action for Wallet V5
/// </summary>
public record OutActionSetIsPublicKeyEnabled(bool IsEnabled) : IWalletV5Action
{
    public string Type => "setIsPublicKeyEnabled";
}

/// <summary>
///     Wrapper for OutAction.SendMsg to implement IWalletV5Action
/// </summary>
public record OutActionSendMsg(SendMode Mode, MessageRelaxed OutMsg) : IWalletV5Action
{
    public string Type => "sendMsg";

    /// <summary>
    ///     Convert to OutAction.SendMsg for serialization
    /// </summary>
    public OutAction.SendMsg ToOutAction()
    {
        return new OutAction.SendMsg(Mode, OutMsg);
    }

    /// <summary>
    ///     Create from OutAction.SendMsg
    /// </summary>
    public static OutActionSendMsg FromOutAction(OutAction.SendMsg action)
    {
        return new OutActionSendMsg(action.Mode, action.OutMsg);
    }
}

/// <summary>
///     Helper class for wallet V5 out actions
/// </summary>
public static class WalletV5OutActionsHelper
{
    public static bool IsOutActionExtended(IWalletV5Action action)
    {
        return action is OutActionAddExtension
            or OutActionRemoveExtension
            or OutActionSetIsPublicKeyEnabled;
    }

    public static bool IsOutActionBasic(IWalletV5Action action)
    {
        return action is OutActionSendMsg;
    }
}
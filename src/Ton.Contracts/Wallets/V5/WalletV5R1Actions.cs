using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Contracts.Wallets.V5;

/// <summary>
///     V5R1 action serialization and deserialization
/// </summary>
public static class WalletV5R1Actions
{
    const byte OutActionSetIsPublicKeyEnabledTag = 0x04;
    const byte OutActionAddExtensionTag = 0x02;
    const byte OutActionRemoveExtensionTag = 0x03;

    /// <summary>
    ///     Store extended action for V5R1
    /// </summary>
    public static Action<Builder> StoreOutActionExtendedV5R1(IWalletV5Action action)
    {
        return action switch
        {
            OutActionSetIsPublicKeyEnabled a => builder =>
            {
                builder.StoreUint(OutActionSetIsPublicKeyEnabledTag, 8);
                builder.StoreBit(a.IsEnabled);
            },
            OutActionAddExtension a => builder =>
            {
                builder.StoreUint(OutActionAddExtensionTag, 8);
                builder.StoreAddress(a.Address);
            },
            OutActionRemoveExtension a => builder =>
            {
                builder.StoreUint(OutActionRemoveExtensionTag, 8);
                builder.StoreAddress(a.Address);
            },
            _ => throw new ArgumentException($"Unknown action type: {action.Type}")
        };
    }

    /// <summary>
    ///     Load extended action for V5R1
    /// </summary>
    public static IWalletV5Action LoadOutActionExtendedV5R1(Slice slice)
    {
        long tag = slice.LoadUint(8);
        return tag switch
        {
            OutActionSetIsPublicKeyEnabledTag => new OutActionSetIsPublicKeyEnabled(
                slice.LoadBit()
            ),
            OutActionAddExtensionTag => new OutActionAddExtension(
                slice.LoadAddress() ?? throw new InvalidOperationException("Address cannot be null")
            ),
            OutActionRemoveExtensionTag => new OutActionRemoveExtension(
                slice.LoadAddress() ?? throw new InvalidOperationException("Address cannot be null")
            ),
            _ => throw new ArgumentException($"Unknown extended out action tag: 0x{tag:x}")
        };
    }

    /// <summary>
    ///     Store extended out list for V5R1
    /// </summary>
    public static Action<Builder> StoreOutListExtendedV5R1(IList<IWalletV5Action> actions)
    {
        List<IWalletV5Action> extendedActions = actions.Where(WalletV5OutActionsHelper.IsOutActionExtended).ToList();
        List<OutAction.SendMsg> basicActions = actions.Where(WalletV5OutActionsHelper.IsOutActionBasic)
            .Cast<OutActionSendMsg>()
            .Select(a => a.ToOutAction())
            .ToList();

        return builder =>
        {
            // Store basic actions (sendMsg) as a reference
            Cell? outListPacked = null;
            if (basicActions.Count > 0)
            {
                Builder listBuilder = Builder.BeginCell();
                OutList.Store(listBuilder, basicActions.Cast<OutAction>().ToList());
                outListPacked = listBuilder.EndCell();
            }

            builder.StoreMaybeRef(outListPacked);

            // Store extended actions
            if (extendedActions.Count == 0)
            {
                builder.StoreBit(false);
            }
            else
            {
                builder.StoreBit(true);
                IWalletV5Action first = extendedActions[0];
                Action<Builder> storeAction = StoreOutActionExtendedV5R1(first);
                storeAction(builder);

                if (extendedActions.Count > 1)
                {
                    List<IWalletV5Action> rest = extendedActions.Skip(1).ToList();
                    builder.StoreRef(PackExtendedActionsRec(rest));
                }
            }
        };
    }

    static Cell PackExtendedActionsRec(IList<IWalletV5Action> extendedActions)
    {
        Builder builder = Builder.BeginCell();
        IWalletV5Action first = extendedActions[0];
        Action<Builder> storeAction = StoreOutActionExtendedV5R1(first);
        storeAction(builder);

        if (extendedActions.Count > 1)
        {
            List<IWalletV5Action> rest = extendedActions.Skip(1).ToList();
            builder.StoreRef(PackExtendedActionsRec(rest));
        }

        return builder.EndCell();
    }

    /// <summary>
    ///     Load extended out list for V5R1
    /// </summary>
    public static List<IWalletV5Action> LoadOutListExtendedV5R1(Slice slice)
    {
        List<IWalletV5Action> actions = [];

        // Load basic actions (sendMsg)
        Cell? outListPacked = slice.LoadMaybeRef();
        if (outListPacked != null)
        {
            List<OutAction> loadedActions = OutList.Load(outListPacked.BeginParse());
            if (loadedActions.Any(a => a is not OutAction.SendMsg))
                throw new InvalidOperationException(
                    "Can't deserialize actions list: only sendMsg actions are allowed for wallet v5r1"
                );
            actions.AddRange(loadedActions
                .Cast<OutAction.SendMsg>()
                .Select(OutActionSendMsg.FromOutAction));
        }

        // Load extended actions
        if (slice.LoadBit())
        {
            IWalletV5Action action = LoadOutActionExtendedV5R1(slice);
            actions.Add(action);
        }

        while (slice.RemainingRefs > 0)
        {
            slice = slice.LoadRef().BeginParse();
            IWalletV5Action action = LoadOutActionExtendedV5R1(slice);
            actions.Add(action);
        }

        return actions;
    }

    /// <summary>
    ///     Convert send mode to safe V5R1 send mode
    ///     Safety rules: external messages must have IGNORE_ERRORS flag
    /// </summary>
    public static SendMode ToSafeV5R1SendMode(SendMode sendMode, string? authType)
    {
        if (authType == "internal" || authType == "extension") return sendMode;

        return sendMode | SendMode.IgnoreErrors;
    }

    /// <summary>
    ///     Patch actions send mode for V5R1 safety rules
    /// </summary>
    public static List<IWalletV5Action> PatchV5R1ActionsSendMode(IEnumerable<IWalletV5Action> actions, string? authType)
    {
        return actions.Select(action =>
        {
            if (action is OutActionSendMsg sendMsg)
                return new OutActionSendMsg(
                    ToSafeV5R1SendMode(sendMsg.Mode, authType),
                    sendMsg.OutMsg
                );
            return action;
        }).ToList();
    }
}

/// <summary>
///     Extension methods for Slice to work with V5R1 actions
/// </summary>
public static class SliceExtensionsV5R1
{
    /// <summary>
    ///     Load extended out list for V5R1
    /// </summary>
    public static List<IWalletV5Action> LoadOutListExtendedV5R1(this Slice slice)
    {
        return WalletV5R1Actions.LoadOutListExtendedV5R1(slice);
    }
}

/// <summary>
///     Extension methods for Builder to work with V5R1 actions
/// </summary>
public static class BuilderExtensionsV5R1
{
    /// <summary>
    ///     Store extended out list for V5R1
    /// </summary>
    public static Builder StoreOutListExtendedV5R1(this Builder builder, IList<IWalletV5Action> actions)
    {
        Action<Builder> storeAction = WalletV5R1Actions.StoreOutListExtendedV5R1(actions);
        storeAction(builder);
        return builder;
    }
}
using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Types;
using Ton.Crypto.Ed25519;
using Dict = Ton.Core.Dict;

namespace Ton.Contracts.Wallets.V5;

/// <summary>
///     Wallet V5R1 contract implementation
/// </summary>
public class WalletV5R1 : IContract
{
    static readonly Cell Code = Cell.FromBoc(Convert.FromHexString(
        "b5ee9c7241021401000281000114ff00f4a413f4bcf2c80b01020120020d020148030402dcd020d749c120915b8f6320d70b1f2082106578746ebd21821073696e74bdb0925f03e082106578746eba8eb48020d72101d074d721fa4030fa44f828fa443058bd915be0ed44d0810141d721f4058307f40e6fa1319130e18040d721707fdb3ce03120d749810280b99130e070e2100f020120050c020120060902016e07080019adce76a2684020eb90eb85ffc00019af1df6a2684010eb90eb858fc00201480a0b0017b325fb51341c75c875c2c7e00011b262fb513435c280200019be5f0f6a2684080a0eb90fa02c0102f20e011e20d70b1f82107369676ebaf2e08a7f0f01e68ef0eda2edfb218308d722028308d723208020d721d31fd31fd31fed44d0d200d31f20d31fd3ffd70a000af90140ccf9109a28945f0adb31e1f2c087df02b35007b0f2d0845125baf2e0855036baf2e086f823bbf2d0882292f800de01a47fc8ca00cb1f01cf16c9ed542092f80fde70db3cd81003f6eda2edfb02f404216e926c218e4c0221d73930709421c700b38e2d01d72820761e436c20d749c008f2e09320d74ac002f2e09320d71d06c712c2005230b0f2d089d74cd7393001a4e86c128407bbf2e093d74ac000f2e093ed55e2d20001c000915be0ebd72c08142091709601d72c081c12e25210b1e30f20d74a111213009601fa4001fa44f828fa443058baf2e091ed44d0810141d718f405049d7fc8ca0040048307f453f2e08b8e14038307f45bf2e08c22d70a00216e01b3b0f2d090e2c85003cf1612f400c9ed54007230d72c08248e2d21f2e092d200ed44d0d2005113baf2d08f54503091319c01810140d721d70a00f2e08ee2c8ca0058cf16c9ed5493f2c08de20010935bdb31e1d74cd0b4d6c35e"
    ))[0];

    readonly WalletIdV5R1 walletId;

    WalletV5R1(int workchain, byte[] publicKey, WalletIdV5R1? walletId = null)
    {
        this.walletId = walletId ?? new WalletIdV5R1(
            -239, // Mainnet
            new WalletIdV5R1ClientContext("v5r1", workchain, 0)
        );

        // Build data cell
        Builder dataBuilder = Builder.BeginCell()
            .StoreBit(true) // is signature auth allowed
            .StoreUint(0, 32); // Seqno

        WalletV5R1WalletIdHelper.StoreWalletIdV5R1(this.walletId)(dataBuilder);

        Cell data = dataBuilder
            .StoreBuffer(publicKey, 32)
            .StoreBit(false) // Empty plugins dict
            .EndCell();

        Init = new StateInit(Code, data);
        Address = ContractAddress.From(workchain, Init);
    }

    public Address Address { get; }
    public StateInit Init { get; }
    public ContractABI? ABI => null;

    /// <summary>
    ///     Create a new Wallet V5R1 contract
    /// </summary>
    public static WalletV5R1 Create(int workchain, byte[] publicKey, WalletIdV5R1? walletId = null)
    {
        return new WalletV5R1(workchain, publicKey, walletId);
    }

    /// <summary>
    ///     Get wallet balance
    /// </summary>
    public async Task<BigInteger> GetBalanceAsync(IContractProvider provider)
    {
        ContractState state = await provider.GetStateAsync();
        return state.Balance;
    }

    /// <summary>
    ///     Get wallet seqno
    /// </summary>
    public async Task<int> GetSeqnoAsync(IContractProvider provider)
    {
        ContractState state = await provider.GetStateAsync();
        if (state.State is ContractState.AccountStateInfo.Active)
        {
            ContractGetMethodResult result = await provider.GetAsync("seqno", []);
            return (int)result.Stack.ReadNumber();
        }

        return 0;
    }

    /// <summary>
    ///     Get wallet extensions cell
    /// </summary>
    public async Task<Cell?> GetExtensionsAsync(IContractProvider provider)
    {
        ContractState state = await provider.GetStateAsync();
        if (state.State is ContractState.AccountStateInfo.Active)
        {
            ContractGetMethodResult result = await provider.GetAsync("get_extensions", []);
            return result.Stack.ReadCellOpt();
        }

        return null;
    }

    /// <summary>
    ///     Get wallet extensions as address array
    /// </summary>
    public async Task<List<Address>> GetExtensionsArrayAsync(IContractProvider provider)
    {
        Cell? extensionsCell = await GetExtensionsAsync(provider);
        if (extensionsCell == null) return [];

        Dict.Dictionary<Dict.DictKeyBigInt, BigInteger> dict =
            Dict.Dictionary<Dict.DictKeyBigInt, BigInteger>.LoadDirect(
                Dict.DictionaryKeys.BigUint(256),
                Dict.DictionaryValues.BigInt(1),
                extensionsCell.BeginParse()
            );

        int workchain = Address.Workchain;
        return dict.Keys()
            .Select(addressHex =>
            {
                // Convert BigInteger to 32-byte array (256 bits)
                BigInteger bigInt = addressHex; // Use implicit conversion
                byte[] bytes = bigInt.ToByteArray();
                // BigInteger.ToByteArray() is little-endian, reverse to big-endian
                Array.Reverse(bytes);
                // Pad to 32 bytes if needed
                if (bytes.Length < 32)
                {
                    byte[] padded = new byte[32];
                    Array.Copy(bytes, 0, padded, 32 - bytes.Length, bytes.Length);
                    bytes = padded;
                }
                else if (bytes.Length > 32)
                {
                    // Take last 32 bytes
                    bytes = bytes[^32..];
                }

                string hexString = Convert.ToHexString(bytes).ToLower();
                return Address.ParseRaw($"{workchain}:{hexString}");
            })
            .ToList();
    }

    /// <summary>
    ///     Get is secret-key authentication enabled
    /// </summary>
    public async Task<bool> GetIsSecretKeyAuthEnabledAsync(IContractProvider provider)
    {
        ContractGetMethodResult result = await provider.GetAsync("is_signature_allowed", []);
        return result.Stack.ReadBoolean();
    }

    /// <summary>
    ///     Send signed transfer
    /// </summary>
    public async Task SendAsync(IContractProvider provider, Cell message)
    {
        await provider.ExternalAsync(message);
    }

    /// <summary>
    ///     Sign and send transfer
    /// </summary>
    public async Task SendTransferAsync(
        IContractProvider provider,
        int seqno,
        byte[] secretKey,
        List<MessageRelaxed> messages,
        SendMode sendMode,
        int? timeout = null,
        string? authType = null)
    {
        Cell transfer = CreateTransfer(seqno, secretKey, messages, sendMode, timeout, authType);
        await SendAsync(provider, transfer);
    }

    /// <summary>
    ///     Sign and send add extension request
    /// </summary>
    public async Task SendAddExtensionAsync(
        IContractProvider provider,
        int seqno,
        byte[] secretKey,
        Address extensionAddress,
        int? timeout = null)
    {
        Cell request = CreateAddExtension(seqno, secretKey, extensionAddress, timeout);
        await SendAsync(provider, request);
    }

    /// <summary>
    ///     Sign and send remove extension request
    /// </summary>
    public async Task SendRemoveExtensionAsync(
        IContractProvider provider,
        int seqno,
        byte[] secretKey,
        Address extensionAddress,
        int? timeout = null)
    {
        Cell request = CreateRemoveExtension(seqno, secretKey, extensionAddress, timeout);
        await SendAsync(provider, request);
    }

    /// <summary>
    ///     Create signed transfer
    /// </summary>
    public Cell CreateTransfer(
        int seqno,
        byte[] secretKey,
        List<MessageRelaxed> messages,
        SendMode sendMode,
        int? timeout = null,
        string? authType = null)
    {
        List<IWalletV5Action> actions = messages
            .Select(msg => (IWalletV5Action)new OutActionSendMsg(sendMode, msg))
            .ToList();

        return CreateRequest(seqno, secretKey, actions, timeout, authType);
    }

    /// <summary>
    ///     Create signed add extension request
    /// </summary>
    public Cell CreateAddExtension(
        int seqno,
        byte[] secretKey,
        Address extensionAddress,
        int? timeout = null)
    {
        List<IWalletV5Action> actions = [new OutActionAddExtension(extensionAddress)];

        return CreateRequest(seqno, secretKey, actions, timeout);
    }

    /// <summary>
    ///     Create signed remove extension request
    /// </summary>
    public Cell CreateRemoveExtension(
        int seqno,
        byte[] secretKey,
        Address extensionAddress,
        int? timeout = null)
    {
        List<IWalletV5Action> actions = [new OutActionRemoveExtension(extensionAddress)];

        return CreateRequest(seqno, secretKey, actions, timeout);
    }

    /// <summary>
    ///     Create signed request or extension auth request
    /// </summary>
    public Cell CreateRequest(
        int seqno,
        byte[] secretKey,
        List<IWalletV5Action> actions,
        int? timeout = null,
        string? authType = null,
        ulong? queryId = null)
    {
        if (actions.Count > 255) throw new ArgumentException("Maximum number of OutActions in a single request is 255");

        authType ??= "external";

        // Extension auth doesn't need signing
        if (authType == "extension")
            return Builder.BeginCell()
                .StoreUint(OpCodes.AuthExtension, 32)
                .StoreUint(queryId ?? 0, 64)
                .StoreOutListExtendedV5R1(actions)
                .EndCell();

        // Patch send modes for safety
        actions = WalletV5R1Actions.PatchV5R1ActionsSendMode(actions, authType);

        // Build signing message
        Builder signingMessageBuilder = Builder.BeginCell()
            .StoreUint(
                authType == "internal" ? OpCodes.AuthSignedInternal : OpCodes.AuthSignedExternal,
                32
            );

        WalletV5R1WalletIdHelper.StoreWalletIdV5R1(walletId)(signingMessageBuilder);

        // Handle seqno 0 special case
        if (seqno == 0)
        {
            for (int i = 0; i < 32; i++) signingMessageBuilder.StoreBit(true);
        }
        else
        {
            int actualTimeout = timeout ?? (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60);
            signingMessageBuilder.StoreUint((ulong)actualTimeout, 32);
        }

        signingMessageBuilder
            .StoreUint((ulong)seqno, 32)
            .StoreOutListExtendedV5R1(actions);

        // Sign and pack
        Cell signingMessage = signingMessageBuilder.EndCell();
        byte[] signature = Ed25519.Sign(signingMessage.Hash(), secretKey);

        // Pack signature at tail (V5 format)
        return Builder.BeginCell()
            .StoreSlice(signingMessage.BeginParse())
            .StoreBuffer(signature)
            .EndCell();
    }

    /// <summary>
    ///     Create sender for this wallet
    /// </summary>
    public ISender CreateSender(IContractProvider provider, byte[] secretKey)
    {
        return new WalletV5R1Sender(this, provider, secretKey);
    }

    public static class OpCodes
    {
        public const uint AuthExtension = 0x6578746e;
        public const uint AuthSignedExternal = 0x7369676e;
        public const uint AuthSignedInternal = 0x73696e74;
    }

    class WalletV5R1Sender(WalletV5R1 wallet, IContractProvider provider, byte[] secretKey)
        : ISender
    {
        public Address? Address => wallet.Address;

        public async Task SendAsync(SenderArguments args)
        {
            int seqno = await wallet.GetSeqnoAsync(provider);
            SendMode sendMode = args.SendMode ?? SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors;

            MessageRelaxed message = new(
                new CommonMessageInfoRelaxed.Internal(
                    true,
                    args.Bounce ?? true,
                    false,
                    null,
                    args.To,
                    new CurrencyCollection(args.Value),
                    0,
                    0,
                    0,
                    0
                ),
                args.Body ?? Builder.BeginCell().EndCell(),
                args.Init
            );

            Cell transfer = wallet.CreateTransfer(seqno, secretKey, [message], sendMode);
            await wallet.SendAsync(provider, transfer);
        }
    }
}
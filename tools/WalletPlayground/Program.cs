using System.Numerics;
using Ton.Contracts.Wallets.V5;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;
using Ton.Crypto.Mnemonic;
using Ton.HttpClient;
using WalletPlayground.Contracts;

const string MnemonicFile = "wallet.mnemonic";
const string OrbsEndpoint = "https://ton.access.orbs.network/4411c0ff5Bd3F8B62C092Ab4D238bEE463E64411/1/mainnet/toncenter-api-v2/jsonRPC";

Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine("   TON Wallet V5R1 Playground");
Console.WriteLine("═══════════════════════════════════════════════════");
Console.WriteLine();

// Load or create mnemonic
string[] mnemonic;
if (File.Exists(MnemonicFile))
{
    Console.WriteLine("📂 Loading existing wallet...");
    mnemonic = File.ReadAllText(MnemonicFile).Split(' ');
}
else
{
    Console.WriteLine("🔑 Generating new wallet...");
    mnemonic = Mnemonic.New();
    File.WriteAllText(MnemonicFile, string.Join(' ', mnemonic));
    Console.WriteLine($"✅ Mnemonic saved to: {MnemonicFile}");
}

// Create wallet
var keyPair = Mnemonic.ToWalletKey(mnemonic);
var walletId = new WalletIdV5R1(-239, new WalletIdV5R1ClientContext("v5r1", 0, 0)); // Mainnet
var wallet = WalletV5R1.Create(0, keyPair.PublicKey, walletId);

Console.WriteLine();
Console.WriteLine($"📍 Wallet Address: {wallet.Address}");
Console.WriteLine($"🔗 Explorer: https://tonviewer.com/{wallet.Address}");
Console.WriteLine();

// Connect to blockchain
var client = new TonClient(new TonClientParameters
{
    Endpoint = OrbsEndpoint,
    Timeout = 30000
});

var opened = client.Open(wallet);

// Main loop
while (true)
{
    try
    {
        // Get current state
        var state = await opened.Provider.GetStateAsync();
        var seqno = await opened.Contract.GetSeqnoAsync(opened.Provider);
        var balance = state.Balance;
        var isDeployed = state.State is Ton.Core.Contracts.ContractState.AccountStateInfo.Active;

        Console.WriteLine("───────────────────────────────────────────────────");
        Console.WriteLine($"💰 Balance: {FormatTon(balance)} TON ({balance} nanotons)");
        Console.WriteLine($"📊 Seqno: {seqno}");
        Console.WriteLine($"📦 Status: {(isDeployed ? "✅ Deployed" : "⏳ Not deployed")}");
        Console.WriteLine("───────────────────────────────────────────────────");
        Console.WriteLine();

        Console.WriteLine("What would you like to do?");
        Console.WriteLine("  1. Refresh balance");
        Console.WriteLine("  2. Send transfer");
        Console.WriteLine("  3. Deploy wallet (send 0.01 TON to self)");
        Console.WriteLine("  4. Send all remaining balance (destroy if zero)");
        Console.WriteLine("  5. Show mnemonic");
        Console.WriteLine("  6. Get proxy address by invoice ID");
        Console.WriteLine("  7. Exit");
        Console.WriteLine();
        Console.Write("Enter choice (1-7): ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        switch (choice)
        {
            case "1":
                // Just loop to refresh
                continue;

            case "2":
                await SendTransfer(opened, wallet, keyPair.SecretKey, seqno);
                break;

            case "3":
                await DeployWallet(opened, wallet, keyPair.SecretKey, seqno);
                break;

            case "4":
                await SendAllBalance(opened, wallet, keyPair.SecretKey, seqno);
                break;

            case "5":
                Console.WriteLine("🔑 Your mnemonic (keep it safe!):");
                Console.WriteLine(string.Join(' ', mnemonic));
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.WriteLine();
                break;

            case "6":
                GetProxyAddress(wallet.Address);
                break;

            case "7":
                Console.WriteLine("👋 Goodbye!");
                return;

            default:
                Console.WriteLine("❌ Invalid choice. Try again.");
                Console.WriteLine();
                break;
        }

        // Wait a bit before next refresh
        await Task.Delay(1000);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.WriteLine();
    }
}

static string FormatTon(BigInteger nanotons)
{
    var tons = (decimal)nanotons / 1_000_000_000m;
    return tons.ToString("0.########");
}

static async Task SendTransfer(
    Ton.Core.Contracts.OpenedContract<WalletV5R1> opened,
    WalletV5R1 wallet,
    byte[] secretKey,
    int seqno)
{
    Console.Write("📤 Enter destination address: ");
    var destStr = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(destStr))
    {
        Console.WriteLine("❌ Address cannot be empty");
        return;
    }

    Address destAddress;
    try
    {
        destAddress = Address.Parse(destStr);
    }
    catch
    {
        Console.WriteLine("❌ Invalid address format");
        return;
    }

    Console.Write("💰 Enter amount in TON (e.g., 0.5): ");
    var amountStr = Console.ReadLine();
    
    if (!decimal.TryParse(amountStr, out var amountTon) || amountTon <= 0)
    {
        Console.WriteLine("❌ Invalid amount");
        return;
    }

    var amountNano = new BigInteger(amountTon * 1_000_000_000m);

    Console.Write("💬 Enter comment (optional): ");
    var comment = Console.ReadLine() ?? "";

    Console.WriteLine();
    Console.WriteLine("Creating transfer...");

    // Build comment body
    var body = Builder.BeginCell()
        .StoreUint(0, 32) // Text comment opcode
        .StoreStringTail(comment)
        .EndCell();

    // Create message
    var message = new MessageRelaxed(
        new CommonMessageInfoRelaxed.Internal(
            IhrDisabled: true,
            Bounce: true,
            Bounced: false,
            Src: null,
            Dest: destAddress,
            Value: new CurrencyCollection(amountNano),
            IhrFee: 0,
            ForwardFee: 0,
            CreatedLt: 0,
            CreatedAt: 0
        ),
        body,
        null
    );

    var transfer = wallet.CreateTransfer(
        seqno: seqno,
        secretKey: secretKey,
        messages: new List<MessageRelaxed> { message },
        sendMode: SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors
    );

    Console.WriteLine("📡 Sending transaction...");
    await opened.Contract.SendAsync(opened.Provider, transfer);

    Console.WriteLine("✅ Transfer sent!");
    Console.WriteLine($"📤 Sent {FormatTon(amountNano)} TON to {destAddress}");
    Console.WriteLine();
    Console.WriteLine("⏳ Waiting for confirmation (10 seconds)...");
    await Task.Delay(10000);
    Console.WriteLine();
}

static async Task SendAllBalance(
    Ton.Core.Contracts.OpenedContract<WalletV5R1> opened,
    WalletV5R1 wallet,
    byte[] secretKey,
    int seqno)
{
    Console.Write("📤 Enter destination address: ");
    var destStr = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(destStr))
    {
        Console.WriteLine("❌ Address cannot be empty");
        return;
    }

    Address destAddress;
    try
    {
        destAddress = Address.Parse(destStr);
    }
    catch
    {
        Console.WriteLine("❌ Invalid address format");
        return;
    }

    Console.Write("💬 Enter comment (optional): ");
    var comment = Console.ReadLine() ?? "";

    Console.WriteLine();
    Console.WriteLine("💸 Sending ALL remaining balance...");
    Console.WriteLine("⚠️  This will destroy the wallet contract if balance reaches zero!");
    Console.WriteLine();

    // Build comment body
    var body = Builder.BeginCell()
        .StoreUint(0, 32) // Text comment opcode
        .StoreStringTail(comment)
        .EndCell();

    // Create message - amount set to 0 because CarryAllRemainingIncomingValue will replace it
    var message = new MessageRelaxed(
        new CommonMessageInfoRelaxed.Internal(
            IhrDisabled: true,
            Bounce: true,
            Bounced: false,
            Src: null,
            Dest: destAddress,
            Value: new CurrencyCollection(0),
            IhrFee: 0,
            ForwardFee: 0,
            CreatedLt: 0,
            CreatedAt: 0
        ),
        body,
        null
    );

    // Use mode 128 (SendRemainingBalance) to send the contract's entire balance
    // Mode 64 (SendRemainingValue) would only carry the inbound message value (which is 0 for external messages)
    // Mode 128 (SendRemainingBalance) carries the smart contract's own balance
    var transfer = wallet.CreateTransfer(
        seqno: seqno,
        secretKey: secretKey,
        messages: new List<MessageRelaxed> { message },
        sendMode: SendMode.SendRemainingBalance | SendMode.SendDestroyIfZero | SendMode.SendIgnoreErrors
    );

    Console.WriteLine("📡 Sending transaction...");
    await opened.Contract.SendAsync(opened.Provider, transfer);

    Console.WriteLine("✅ Transfer sent!");
    Console.WriteLine($"📤 Sending all remaining balance to {destAddress}");
    Console.WriteLine("💥 Wallet will be destroyed after this transaction");
    Console.WriteLine();
    Console.WriteLine("⏳ Waiting for confirmation (10 seconds)...");
    await Task.Delay(10000);
    Console.WriteLine();
}

static async Task DeployWallet(
    Ton.Core.Contracts.OpenedContract<WalletV5R1> opened,
    WalletV5R1 wallet,
    byte[] secretKey,
    int seqno)
{
    Console.WriteLine("📦 Deploying wallet by sending 0.01 TON to self...");
    Console.WriteLine();

    var amountNano = new BigInteger(0.01m * 1_000_000_000m);

    // Create message to self
    var message = new MessageRelaxed(
        new CommonMessageInfoRelaxed.Internal(
            IhrDisabled: true,
            Bounce: false,
            Bounced: false,
            Src: null,
            Dest: wallet.Address,
            Value: new CurrencyCollection(amountNano),
            IhrFee: 0,
            ForwardFee: 0,
            CreatedLt: 0,
            CreatedAt: 0
        ),
        Builder.BeginCell().StoreUint(0, 32).StoreStringTail("Deploy").EndCell(),
        wallet.Init // Include StateInit to deploy
    );

    var transfer = wallet.CreateTransfer(
        seqno: seqno,
        secretKey: secretKey,
        messages: new List<MessageRelaxed> { message },
        sendMode: SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors
    );

    Console.WriteLine("📡 Sending deployment transaction...");
    await opened.Contract.SendAsync(opened.Provider, transfer);

    Console.WriteLine("✅ Deployment transaction sent!");
    Console.WriteLine();
    Console.WriteLine("⏳ Waiting for confirmation (10 seconds)...");
    await Task.Delay(10000);
    Console.WriteLine();
}

static void GetProxyAddress(Address walletAddress)
{
    Console.Write("🔢 Enter invoice ID: ");
    var invoiceIdStr = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(invoiceIdStr))
    {
        Console.WriteLine("❌ Invoice ID cannot be empty");
        return;
    }

    if (!BigInteger.TryParse(invoiceIdStr, out var invoiceId) || invoiceId < 0)
    {
        Console.WriteLine("❌ Invalid invoice ID");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("📦 Calculating proxy address...");
    
    // Create proxy contract with wallet as owner and specified invoice ID
    var proxy = Proxy.Create(walletAddress, invoiceId);
    
    Console.WriteLine("───────────────────────────────────────────────────");
    Console.WriteLine($"📝 Invoice ID: {invoiceId}");
    Console.WriteLine($"👤 Owner: {walletAddress}");
    Console.WriteLine($"📍 Proxy Address: {proxy.Address}");
    Console.WriteLine($"🔗 Explorer: https://tonviewer.com/{proxy.Address}");
    Console.WriteLine("───────────────────────────────────────────────────");
    Console.WriteLine();
    Console.WriteLine("💡 This is the address where jettons for this invoice should be sent.");
    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
    Console.WriteLine();
}

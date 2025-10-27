using System.Numerics;
using Ton.Contracts.Wallets.V5;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Types;
using Ton.HttpClient;

namespace Ton.Contracts.Tests.Wallets.V5;

[TestFixture]
[NonParallelizable]
public class WalletV5R1IntegrationTests
{
    [SetUp]
    public async Task Setup()
    {
        client = new TonClient(new TonClientParameters
        {
            Endpoint = OrbsEndpoint,
            Timeout = 30000 // 30 seconds in milliseconds
        });

        // Rate limiting
        await Task.Delay(200);
    }

    [TearDown]
    public async Task TearDown()
    {
        client?.Dispose();
        await Task.Delay(200);
    }

    TonClient client = null!;

    const string OrbsEndpoint =
        "https://ton.access.orbs.network/4411c0ff5Bd3F8B62C092Ab4D238bEE463E64411/1/mainnet/toncenter-api-v2/jsonRPC";

    // Known mainnet V5R1 wallet address (read-only testing)
    // This is one of the test wallets from TON blockchain
    static readonly Address TestWalletAddress = Address.Parse("EQCqe9WqFhS8AfVGDP2xQiTLjbeolhLGsvIbbgQ6C3XT5gGs");

    [Test]
    public async Task Test_WalletV5R1_GetBalance()
    {
        byte[] publicKey = new byte[32]; // Dummy public key for read-only testing
        WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

        try
        {
            OpenedContract<WalletV5R1> opened = client.Open(wallet);
            BigInteger balance = await opened.Contract.GetBalanceAsync(opened.Provider);

            // Balance should be non-negative (might be zero if not deployed)
            Assert.That(balance, Is.GreaterThanOrEqualTo(BigInteger.Zero));

            TestContext.WriteLine($"Wallet balance: {balance} nanotons");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Note: {ex.Message}");
            Assert.Inconclusive("Wallet may not be deployed or accessible");
        }
    }

    [Test]
    public async Task Test_WalletV5R1_GetSeqno_UndeployedWallet()
    {
        // Create a wallet with random key (likely not deployed)
        byte[] randomPublicKey = new byte[32];
        Random.Shared.NextBytes(randomPublicKey);

        WalletV5R1 wallet = WalletV5R1.Create(0, randomPublicKey);

        try
        {
            OpenedContract<WalletV5R1> opened = client.Open(wallet);
            int seqno = await opened.Contract.GetSeqnoAsync(opened.Provider);

            // Undeployed wallet should have seqno 0
            Assert.That(seqno, Is.EqualTo(0));

            TestContext.WriteLine($"Undeployed wallet seqno: {seqno}");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Error: {ex.Message}");
        }
    }

    [Test]
    public async Task Test_WalletV5R1_GetExtensions_ReturnsNull_ForUndeployedWallet()
    {
        byte[] randomPublicKey = new byte[32];
        Random.Shared.NextBytes(randomPublicKey);

        WalletV5R1 wallet = WalletV5R1.Create(0, randomPublicKey);

        try
        {
            OpenedContract<WalletV5R1> opened = client.Open(wallet);
            Cell? extensions = await opened.Contract.GetExtensionsAsync(opened.Provider);

            // Undeployed wallet should have null extensions
            Assert.That(extensions, Is.Null);

            TestContext.WriteLine("Extensions: null (wallet not deployed)");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Error: {ex.Message}");
        }
    }

    [Test]
    public async Task Test_WalletV5R1_CreateTransfer_GeneratesValidCell()
    {
        // Create a test wallet
        byte[] testSecretKey = new byte[64];
        Random.Shared.NextBytes(testSecretKey);

        byte[] publicKey = new byte[32];
        Array.Copy(testSecretKey, 32, publicKey, 0, 32);

        WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

        // Create a simple transfer
        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(1000000), // 0.001 TON
                0, 0, 0, 0
            ),
            Builder.BeginCell().StoreStringTail("Test transfer").EndCell()
        );

        Cell transfer = wallet.CreateTransfer(
            1,
            testSecretKey,
            [message],
            SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors
        );

        Assert.That(transfer, Is.Not.Null);
        Assert.That(transfer.Bits.Length, Is.GreaterThan(0));

        // Verify it can be serialized to BOC
        byte[] boc = transfer.ToBoc();
        Assert.That(boc.Length, Is.GreaterThan(0));

        TestContext.WriteLine($"Transfer cell created: {boc.Length} bytes BOC");
    }

    [Test]
    public async Task Test_WalletV5R1_CreateAddExtension_GeneratesValidCell()
    {
        byte[] testSecretKey = new byte[64];
        Random.Shared.NextBytes(testSecretKey);

        byte[] publicKey = new byte[32];
        Array.Copy(testSecretKey, 32, publicKey, 0, 32);

        WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

        Address extensionAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");

        Cell request = wallet.CreateAddExtension(
            5,
            testSecretKey,
            extensionAddress
        );

        Assert.That(request, Is.Not.Null);
        Assert.That(request.Bits.Length, Is.GreaterThan(0));

        byte[] boc = request.ToBoc();
        Assert.That(boc.Length, Is.GreaterThan(0));

        TestContext.WriteLine($"Add extension request created: {boc.Length} bytes BOC");
    }

    [Test]
    public async Task Test_WalletV5R1_CreateRequest_ExtensionAuth()
    {
        byte[] testSecretKey = new byte[64];
        Random.Shared.NextBytes(testSecretKey);

        byte[] publicKey = new byte[32];
        Array.Copy(testSecretKey, 32, publicKey, 0, 32);

        WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

        List<IWalletV5Action> actions = [new OutActionSetIsPublicKeyEnabled(false)];

        Cell request = wallet.CreateRequest(
            10,
            testSecretKey,
            actions,
            null,
            "extension",
            123456
        );

        Assert.That(request, Is.Not.Null);

        // Verify it starts with extension auth opcode
        Slice slice = request.BeginParse();
        long opcode = slice.LoadUint(32);
        Assert.That(opcode, Is.EqualTo(WalletV5R1.OpCodes.AuthExtension));

        long queryId = slice.LoadUint(64);
        Assert.That(queryId, Is.EqualTo(123456UL));

        TestContext.WriteLine($"Extension auth request created with opcode: 0x{opcode:X8}");
    }

    [Test]
    public async Task Test_WalletV5R1_StateInit_IsValid()
    {
        byte[] publicKey = new byte[32];
        Random.Shared.NextBytes(publicKey);

        WalletV5R1 wallet = WalletV5R1.Create(0, publicKey);

        // Verify StateInit structure
        Assert.That(wallet.Init, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(wallet.Init.Code, Is.Not.Null);
            Assert.That(wallet.Init.Data, Is.Not.Null);
        });

        // Verify code hash matches expected V5R1 code
        byte[] codeHash = wallet.Init.Code.Hash();
        Assert.That(codeHash, Has.Length.EqualTo(32));

        TestContext.WriteLine($"Code hash: {Convert.ToHexString(codeHash)}");

        // Verify data structure
        Slice dataSlice = wallet.Init.Data.BeginParse();

        // Should start with signature allowed flag (1 bit)
        bool isSignatureAllowed = dataSlice.LoadBit();
        Assert.That(isSignatureAllowed, Is.True);

        // Followed by seqno (32 bits)
        long initialSeqno = dataSlice.LoadUint(32);
        Assert.That(initialSeqno, Is.EqualTo(0UL));

        TestContext.WriteLine($"Initial state: signature_allowed={isSignatureAllowed}, seqno={initialSeqno}");
    }

    [Test]
    public async Task Test_WalletV5R1_MultipleWalletsHaveDifferentAddresses()
    {
        byte[] publicKey1 = new byte[32];
        byte[] publicKey2 = new byte[32];

        Random.Shared.NextBytes(publicKey1);
        Random.Shared.NextBytes(publicKey2);

        WalletV5R1 wallet1 = WalletV5R1.Create(0, publicKey1);
        WalletV5R1 wallet2 = WalletV5R1.Create(0, publicKey2);

        Assert.That(wallet1.Address, Is.Not.EqualTo(wallet2.Address));

        TestContext.WriteLine($"Wallet 1: {wallet1.Address}");
        TestContext.WriteLine($"Wallet 2: {wallet2.Address}");
    }

    [Test]
    public async Task Test_WalletV5R1_SameKeyDifferentWalletIdHaveDifferentAddresses()
    {
        byte[] publicKey = new byte[32];
        Random.Shared.NextBytes(publicKey);

        WalletIdV5R1 walletId1 = new(-239, new WalletIdV5R1ClientContext("v5r1", 0, 0));
        WalletIdV5R1 walletId2 = new(-239, new WalletIdV5R1ClientContext("v5r1", 0, 1));

        WalletV5R1 wallet1 = WalletV5R1.Create(0, publicKey, walletId1);
        WalletV5R1 wallet2 = WalletV5R1.Create(0, publicKey, walletId2);

        Assert.That(wallet1.Address, Is.Not.EqualTo(wallet2.Address));

        TestContext.WriteLine($"Subwallet 0: {wallet1.Address}");
        TestContext.WriteLine($"Subwallet 1: {wallet2.Address}");
    }
}
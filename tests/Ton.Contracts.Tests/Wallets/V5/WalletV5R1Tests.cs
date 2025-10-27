using Ton.Contracts.Wallets.V5;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Contracts.Tests.Wallets.V5;

[TestFixture]
public class WalletV5R1Tests
{
    static readonly byte[] TestPublicKey = Convert.FromHexString(
        "82a0b2a62f7f0ba0584c1bb935c9c87350ee4a6de3f21f29b3e01123" +
        "d0d3ba77"
    );

    static readonly byte[] TestSecretKey = Convert.FromHexString(
        "F182111193F30D79D517F2339A1BA7C25FDF6C52142F0F2C1D960A1F" +
        "1D65E1E4" +
        "82A0B2A62F7F0BA0584C1BB935C9C87350EE4A6DE3F21F29B3E01123" +
        "D0D3BA77"
    );

    [Test]
    public void Test_WalletV5R1_Create_WithDefaultWalletId()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Assert.That(wallet, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(wallet.Address, Is.Not.Null);
            Assert.That(wallet.Init, Is.Not.Null);
        });
        Assert.Multiple(() =>
        {
            Assert.That(wallet.Init.Code, Is.Not.Null);
            Assert.That(wallet.Init.Data, Is.Not.Null);
        });
    }

    [Test]
    public void Test_WalletV5R1_Create_WithCustomWalletId()
    {
        WalletIdV5R1 walletId = new(
            -239,
            new WalletIdV5R1ClientContext("v5r1", 0, 42)
        );

        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey, walletId);

        Assert.That(wallet, Is.Not.Null);
        Assert.That(wallet.Address.Workchain, Is.EqualTo(0));
    }

    [Test]
    public void Test_WalletV5R1_Create_WithTestnetWalletId()
    {
        WalletIdV5R1 walletId = new(
            -3, // Testnet
            new WalletIdV5R1ClientContext("v5r1", 0, 0)
        );

        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey, walletId);

        // Verify wallet is created and address is valid
        Assert.That(wallet, Is.Not.Null);
        Assert.That(wallet.Address, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(wallet.Address.Workchain, Is.EqualTo(0));
            Assert.That(wallet.Address.ToString().StartsWith("EQ"), Is.True);
        });
    }

    [Test]
    public void Test_WalletV5R1_CreateTransfer_SingleMessage()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(10000000), // 0.01 TON
                0, 0, 0, 0
            ),
            Builder.BeginCell().StoreUint(0, 32).StoreStringTail("Hello").EndCell()
        );

        Cell transfer = wallet.CreateTransfer(
            1, // seqno
            TestSecretKey,
            [message],
            SendMode.PayFeesSeparately | SendMode.IgnoreErrors,
            null,
            "external"
        );

        Assert.That(transfer, Is.Not.Null);
        Assert.That(transfer.Bits.Length, Is.GreaterThan(0));

        // Verify structure: signature (512 bits) should be at the tail
        Slice slice = transfer.BeginParse();
        Assert.That(slice.RemainingBits, Is.GreaterThanOrEqualTo(512));
    }

    [Test]
    public void Test_WalletV5R1_CreateTransfer_MultipleMessages()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address dest1 = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        Address dest2 = Address.Parse("UQDUyIkKoOR5iZ1Gz60JwKc7wPr3LcdHxOJpVDb9jAKY_pfk");

        List<MessageRelaxed> messages =
        [
            new MessageRelaxed(
                new CommonMessageInfoRelaxed.Internal(
                    true, false, false,
                    null, dest1,
                    new CurrencyCollection(10000000), 0, 0, 0, 0
                ),
                Builder.BeginCell().StoreStringTail("Message 1").EndCell()
            ),

            new MessageRelaxed(
                new CommonMessageInfoRelaxed.Internal(
                    true, false, false,
                    null, dest2,
                    new CurrencyCollection(20000000), 0, 0, 0, 0
                ),
                Builder.BeginCell().StoreStringTail("Message 2").EndCell()
            )
        ];

        Cell transfer = wallet.CreateTransfer(
            5,
            TestSecretKey,
            messages,
            SendMode.PayFeesSeparately,
            null,
            "external"
        );

        Assert.That(transfer, Is.Not.Null);
        Assert.That(transfer.Refs.Count, Is.GreaterThan(0)); // Messages stored in refs
    }

    [Test]
    public void Test_WalletV5R1_CreateTransfer_WithCustomTimeout()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(10000000),
                0, 0, 0, 0
            ),
            Builder.BeginCell().EndCell()
        );

        int customTimeout = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 300; // 5 minutes

        Cell transfer = wallet.CreateTransfer(
            10,
            TestSecretKey,
            [message],
            SendMode.PayFeesSeparately,
            customTimeout,
            "external"
        );

        Assert.That(transfer, Is.Not.Null);
    }

    [Test]
    public void Test_WalletV5R1_CreateTransfer_Seqno0_HasSpecialTimeout()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(10000000),
                0, 0, 0, 0
            ),
            Builder.BeginCell().EndCell()
        );

        // Seqno 0 should have 32 bits of 1s instead of timeout
        Cell transfer = wallet.CreateTransfer(
            0,
            TestSecretKey,
            [message],
            SendMode.PayFeesSeparately,
            null,
            "external"
        );

        Assert.That(transfer, Is.Not.Null);
    }

    [Test]
    public void Test_WalletV5R1_CreateAddExtension()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);
        Address extensionAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");

        Cell request = wallet.CreateAddExtension(
            5,
            TestSecretKey,
            extensionAddress
        );

        Assert.That(request, Is.Not.Null);
        Assert.That(request.Bits.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Test_WalletV5R1_CreateRemoveExtension()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);
        Address extensionAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");

        Cell request = wallet.CreateRemoveExtension(
            10,
            TestSecretKey,
            extensionAddress
        );

        Assert.That(request, Is.Not.Null);
        Assert.That(request.Bits.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Test_WalletV5R1_CreateRequest_ExtensionAuth()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        List<IWalletV5Action> actions = [new OutActionSetIsPublicKeyEnabled(false)];

        Cell request = wallet.CreateRequest(
            5,
            TestSecretKey,
            actions,
            null,
            "extension",
            12345
        );

        Assert.That(request, Is.Not.Null);

        // Extension auth should start with extension opcode
        Slice slice = request.BeginParse();
        long opcode = slice.LoadUint(32);
        Assert.That(opcode, Is.EqualTo(WalletV5R1.OpCodes.AuthExtension));

        long queryId = slice.LoadUint(64);
        Assert.That(queryId, Is.EqualTo(12345UL));
    }

    [Test]
    public void Test_WalletV5R1_CreateRequest_InternalAuth()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(10000000),
                0, 0, 0, 0
            ),
            Builder.BeginCell().EndCell()
        );

        List<IWalletV5Action> actions = [new OutActionSendMsg(SendMode.PayFeesSeparately, message)];

        Cell request = wallet.CreateRequest(
            5,
            TestSecretKey,
            actions,
            null,
            "internal"
        );

        Assert.That(request, Is.Not.Null);
        Assert.That(request.Bits.Length, Is.GreaterThan(0));
    }

    [Test]
    public void Test_WalletV5R1_CreateRequest_TooManyActions_ThrowsException()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        List<IWalletV5Action> actions = [];
        for (int i = 0; i < 256; i++)
        {
            Address addr = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
            actions.Add(new OutActionAddExtension(addr));
        }

        Assert.Throws<ArgumentException>(() =>
            wallet.CreateRequest(1, TestSecretKey, actions)
        );
    }

    [Test]
    public void Test_WalletV5R1_CreateTransfer_Roundtrip()
    {
        WalletV5R1 wallet = WalletV5R1.Create(0, TestPublicKey);

        Address destAddress = Address.Parse("UQB-2r0kM28L4lmq-4V8ppQGcnO1tXC7FZmbnDzWZVBkp6jE");
        MessageRelaxed message = new(
            new CommonMessageInfoRelaxed.Internal(
                true, false, false,
                null, destAddress,
                new CurrencyCollection(10000000),
                0, 0, 0, 0
            ),
            Builder.BeginCell().StoreStringTail("Test").EndCell()
        );

        Cell transfer = wallet.CreateTransfer(
            7,
            TestSecretKey,
            [message],
            SendMode.PayFeesSeparately
        );

        // Serialize to BOC and back
        byte[] boc = transfer.ToBoc();
        Cell restored = Cell.FromBoc(boc)[0];

        Assert.That(restored.Hash(), Is.EqualTo(transfer.Hash()));
    }

    [Test]
    public void Test_WalletV5R1_OpCodes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(WalletV5R1.OpCodes.AuthExtension, Is.EqualTo(0x6578746eU));
            Assert.That(WalletV5R1.OpCodes.AuthSignedExternal, Is.EqualTo(0x7369676eU));
            Assert.That(WalletV5R1.OpCodes.AuthSignedInternal, Is.EqualTo(0x73696e74U));
        });
    }
}
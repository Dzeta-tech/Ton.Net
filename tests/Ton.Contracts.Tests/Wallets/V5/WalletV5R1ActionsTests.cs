using Ton.Contracts.Wallets.V5;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Contracts.Tests.Wallets.V5;

[TestFixture]
public class WalletV5R1ActionsTests
{
    const byte OutActionSetIsPublicKeyEnabledTag = 0x04;
    const byte OutActionAddExtensionTag = 0x02;
    const byte OutActionRemoveExtensionTag = 0x03;
    const uint OutActionSendMsgTag = 0x0ec3c86d;

    static readonly MessageRelaxed MockMessageRelaxed1 = new(
        new CommonMessageInfoRelaxed.ExternalOut(
            null,
            null,
            0,
            0
        ),
        Builder.BeginCell().StoreUint(0, 8).EndCell()
    );

    static readonly MessageRelaxed MockMessageRelaxed2 = new(
        new CommonMessageInfoRelaxed.Internal(
            true,
            false,
            false,
            null,
            Address.ParseRaw("0:" + new string('2', 64)),
            new CurrencyCollection(1),
            1,
            1,
            12345,
            123456
        ),
        Builder.BeginCell().StoreUint(0, 8).EndCell()
    );

    static readonly Address MockAddress = Address.ParseRaw("0:" + new string('1', 64));

    [Test]
    public void ShouldSerializeSetIsPublicKeyEnabledActionWithTrueFlag()
    {
        OutActionSetIsPublicKeyEnabled action = new(true);
        Builder builder = Builder.BeginCell();
        WalletV5R1Actions.StoreOutActionExtendedV5R1(action)(builder);
        Cell actual = builder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreUint(OutActionSetIsPublicKeyEnabledTag, 8)
            .StoreBit(true)
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeSetIsPublicKeyEnabledActionWithFalseFlag()
    {
        OutActionSetIsPublicKeyEnabled action = new(false);
        Builder builder = Builder.BeginCell();
        WalletV5R1Actions.StoreOutActionExtendedV5R1(action)(builder);
        Cell actual = builder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreUint(OutActionSetIsPublicKeyEnabledTag, 8)
            .StoreBit(false)
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeAddExtensionAction()
    {
        OutActionAddExtension action = new(MockAddress);
        Builder builder = Builder.BeginCell();
        WalletV5R1Actions.StoreOutActionExtendedV5R1(action)(builder);
        Cell actual = builder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreUint(OutActionAddExtensionTag, 8)
            .StoreAddress(MockAddress)
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeRemoveExtensionAction()
    {
        OutActionRemoveExtension action = new(MockAddress);
        Builder builder = Builder.BeginCell();
        WalletV5R1Actions.StoreOutActionExtendedV5R1(action)(builder);
        Cell actual = builder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreUint(OutActionRemoveExtensionTag, 8)
            .StoreAddress(MockAddress)
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeExtendedOutList()
    {
        SendMode sendMode1 = SendMode.PayFeesSeparately;
        const bool isPublicKeyEnabled = false;

        List<IWalletV5Action> actions =
        [
            new OutActionAddExtension(MockAddress),
            new OutActionSetIsPublicKeyEnabled(isPublicKeyEnabled),
            new OutActionSendMsg(sendMode1, MockMessageRelaxed1)
        ];

        Cell actual = Builder.BeginCell()
            .StoreOutListExtendedV5R1(actions)
            .EndCell();

        Builder msgBuilder1 = Builder.BeginCell();
        MockMessageRelaxed1.Store(msgBuilder1);

        Cell expected = Builder.BeginCell()
            .StoreBit(true)
            .StoreRef(
                Builder.BeginCell()
                    .StoreRef(Builder.BeginCell().EndCell())
                    .StoreUint(OutActionSendMsgTag, 32)
                    .StoreUint((byte)sendMode1, 8)
                    .StoreRef(msgBuilder1.EndCell())
                    .EndCell()
            )
            .StoreBit(true)
            .StoreUint(OutActionAddExtensionTag, 8)
            .StoreAddress(MockAddress)
            .StoreRef(
                Builder.BeginCell()
                    .StoreUint(OutActionSetIsPublicKeyEnabledTag, 8)
                    .StoreBit(isPublicKeyEnabled)
                    .EndCell()
            )
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeExtendedOutListAndProduceExpectedBoc()
    {
        const SendMode sendMode1 = SendMode.PayFeesSeparately | SendMode.IgnoreErrors;
        const bool isPublicKeyEnabled = false;

        List<IWalletV5Action> actions =
        [
            new OutActionAddExtension(MockAddress),
            new OutActionSetIsPublicKeyEnabled(isPublicKeyEnabled),
            new OutActionSendMsg(sendMode1, MockMessageRelaxed1)
        ];

        Cell actual = Builder.BeginCell()
            .StoreOutListExtendedV5R1(actions)
            .EndCell();

        Cell expected = Cell.FromBoc(Convert.FromHexString(
            "b5ee9c72410105010046000245c0a000888888888888888888888888888888888888888888888888888888888888888c0104020a0ec3c86d0302030000001cc000000000000000000000000000000304409c06218f"
        ))[0];

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldSerializeExtendedOutListAndProduceExpectedBocForComplexStructures()
    {
        SendMode sendMode1 = SendMode.PayFeesSeparately | SendMode.IgnoreErrors;
        SendMode sendMode2 = SendMode.None;
        const bool isPublicKeyEnabled = false;

        List<IWalletV5Action> actions =
        [
            new OutActionAddExtension(MockAddress),
            new OutActionSetIsPublicKeyEnabled(isPublicKeyEnabled),
            new OutActionRemoveExtension(MockAddress),
            new OutActionSendMsg(sendMode1, MockMessageRelaxed1),
            new OutActionSendMsg(sendMode2, MockMessageRelaxed2)
        ];

        Cell actual = Builder.BeginCell()
            .StoreOutListExtendedV5R1(actions)
            .EndCell();

        Cell expected = Cell.FromBoc(Convert.FromHexString(
            "b5ee9c724101080100ab000245c0a000888888888888888888888888888888888888888888888888888888888888888c0301010304400200450380022222222222222222222222222222222222222222222222222222222222222230020a0ec3c86d0005040068420011111111111111111111111111111111111111111111111111111111111111110808404404000000000000c0e40007890000020a0ec3c86d030706001cc0000000000000000000000000000000a78e5373"
        ))[0];

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void ShouldDeserializeExtendedOutList()
    {
        const SendMode sendMode1 = SendMode.PayFeesSeparately;
        const bool isPublicKeyEnabled = true;

        List<IWalletV5Action> expected =
        [
            new OutActionSendMsg(sendMode1, MockMessageRelaxed1),
            new OutActionAddExtension(MockAddress),
            new OutActionSetIsPublicKeyEnabled(isPublicKeyEnabled)
        ];

        Builder msgBuilder2 = Builder.BeginCell();
        MockMessageRelaxed1.Store(msgBuilder2);

        Cell serialized = Builder.BeginCell()
            .StoreBit(true)
            .StoreRef(
                Builder.BeginCell()
                    .StoreRef(Builder.BeginCell().EndCell())
                    .StoreUint(OutActionSendMsgTag, 32)
                    .StoreUint((byte)sendMode1, 8)
                    .StoreRef(msgBuilder2.EndCell())
                    .EndCell()
            )
            .StoreBit(true)
            .StoreUint(OutActionAddExtensionTag, 8)
            .StoreAddress(MockAddress)
            .StoreRef(
                Builder.BeginCell()
                    .StoreUint(OutActionSetIsPublicKeyEnabledTag, 8)
                    .StoreBit(isPublicKeyEnabled)
                    .EndCell()
            )
            .EndCell();

        List<IWalletV5Action> actual = serialized.BeginParse().LoadOutListExtendedV5R1();

        Assert.That(actual, Has.Count.EqualTo(expected.Count));
        for (int i = 0; i < expected.Count; i++)
        {
            IWalletV5Action item1 = expected[i];
            IWalletV5Action item2 = actual[i];
            Assert.That(item2.Type, Is.EqualTo(item1.Type));

            if (item1 is OutActionSendMsg sendMsg1 && item2 is OutActionSendMsg sendMsg2)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(sendMsg2.Mode, Is.EqualTo(sendMsg1.Mode));
                    Assert.That(sendMsg2.OutMsg.Body.Equals(sendMsg1.OutMsg.Body), Is.True);
                    Assert.That(sendMsg2.OutMsg.Info, Is.EqualTo(sendMsg1.OutMsg.Info));
                    Assert.That(sendMsg2.OutMsg.Init, Is.EqualTo(sendMsg1.OutMsg.Init));
                });
            }

            if (item1 is OutActionAddExtension addExt1 && item2 is OutActionAddExtension addExt2)
                Assert.That(addExt2.Address, Is.EqualTo(addExt1.Address));

            if (item1 is OutActionSetIsPublicKeyEnabled setPk1 && item2 is OutActionSetIsPublicKeyEnabled setPk2)
                Assert.That(setPk2.IsEnabled, Is.EqualTo(setPk1.IsEnabled));
        }
    }

    [Test]
    public void CheckToSafeSendModeAddIgnoreErrorsToExternals()
    {
        SendMode notSafeSendMode = SendMode.PayFeesSeparately;
        string authType = "external";
        SendMode safeSendMode = WalletV5R1Actions.ToSafeV5R1SendMode(notSafeSendMode, authType);

        Assert.That(safeSendMode, Is.EqualTo(notSafeSendMode | SendMode.IgnoreErrors));
    }

    [Test]
    public void CheckToSafeSendModeKeepModeForInternals()
    {
        SendMode notSafeSendMode = SendMode.PayFeesSeparately;
        string authType = "internal";
        SendMode safeSendMode = WalletV5R1Actions.ToSafeV5R1SendMode(notSafeSendMode, authType);

        Assert.That(safeSendMode, Is.EqualTo(notSafeSendMode));
    }

    [Test]
    public void CheckToSafeSendModeKeepModeForExtensions()
    {
        SendMode notSafeSendMode = SendMode.PayFeesSeparately;
        string authType = "extension";
        SendMode safeSendMode = WalletV5R1Actions.ToSafeV5R1SendMode(notSafeSendMode, authType);

        Assert.That(safeSendMode, Is.EqualTo(notSafeSendMode));
    }

    [Test]
    public void CheckToSafeSendModeDontAddIgnoreErrorsTwiceForExternals()
    {
        SendMode safeSendMode = SendMode.PayFeesSeparately | SendMode.IgnoreErrors;
        string authType = "external";
        SendMode actualSafeSendMode = WalletV5R1Actions.ToSafeV5R1SendMode(safeSendMode, authType);

        Assert.That(actualSafeSendMode, Is.EqualTo(safeSendMode));
    }

    [Test]
    public void CheckToSafeSendModeDontAddIgnoreErrorsTwiceForInternals()
    {
        SendMode safeSendMode = SendMode.PayFeesSeparately | SendMode.IgnoreErrors;
        string authType = "internal";
        SendMode actualSafeSendMode = WalletV5R1Actions.ToSafeV5R1SendMode(safeSendMode, authType);

        Assert.That(actualSafeSendMode, Is.EqualTo(safeSendMode));
    }
}
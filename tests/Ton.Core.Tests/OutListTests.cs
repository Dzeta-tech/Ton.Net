using System.Numerics;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class OutListTests
{
    const uint OutActionSendMsgTag = 0x0ec3c86d;
    const uint OutActionSetCodeTag = 0xad4de08e;
    const uint OutActionReserveTag = 0x36e6b809;
    const uint OutActionChangeLibraryTag = 0x26fa1dd4;

    static readonly Cell MockSetCodeCell = Builder.BeginCell().StoreUint(123, 8).EndCell();

    static MessageRelaxed MockMessageRelaxed1 =>
        new(
            new CommonMessageInfoRelaxed.ExternalOut(null, null, 0, 0),
            Builder.BeginCell().StoreUint(0, 8).EndCell()
        );

    static MessageRelaxed MockMessageRelaxed2 =>
        new(
            new CommonMessageInfoRelaxed.ExternalOut(null, null, 1, 1),
            Builder.BeginCell().StoreUint(1, 8).EndCell()
        );

    [Test]
    public void Test_OutAction_SendMsg_Serialize()
    {
        SendMode mode = SendMode.SendPayFwdFeesSeparately;
        OutAction.SendMsg action = new(mode, MockMessageRelaxed1);

        Builder builder = Builder.BeginCell();
        action.Store(builder);
        Cell actual = builder.EndCell();

        Builder expected = Builder.BeginCell();
        expected.StoreUint(OutActionSendMsgTag, 32);
        expected.StoreUint((byte)mode, 8);
        Builder msgBuilder = Builder.BeginCell();
        MockMessageRelaxed1.Store(msgBuilder);
        expected.StoreRef(msgBuilder.EndCell());
        Cell expectedCell = expected.EndCell();

        Assert.That(actual.Equals(expectedCell), Is.True);
    }

    [Test]
    public void Test_OutAction_SetCode_Serialize()
    {
        OutAction.SetCode action = new(MockSetCodeCell);

        Builder builder = Builder.BeginCell();
        action.Store(builder);
        Cell actual = builder.EndCell();

        Cell expected = Builder.BeginCell()
            .StoreUint(OutActionSetCodeTag, 32)
            .StoreRef(MockSetCodeCell)
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void Test_OutAction_Reserve_Serialize()
    {
        ReserveMode mode = ReserveMode.AtMostThisAmount;
        CurrencyCollection currency = new(2000000);
        OutAction.Reserve action = new(mode, currency);

        Builder builder = Builder.BeginCell();
        action.Store(builder);
        Cell actual = builder.EndCell();

        Builder expectedBuilder = Builder.BeginCell();
        expectedBuilder.StoreUint(OutActionReserveTag, 32);
        expectedBuilder.StoreUint((byte)mode, 8);
        currency.Store(expectedBuilder);
        Cell expected = expectedBuilder.EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void Test_OutAction_ChangeLibrary_Serialize()
    {
        byte mode = 0;
        Cell lib = Builder.BeginCell().StoreUint(1234, 16).EndCell();
        LibRef.Ref libRef = new(lib);
        OutAction.ChangeLibrary action = new(mode, libRef);

        Builder builder = Builder.BeginCell();
        action.Store(builder);
        Cell actual = builder.EndCell();

        Builder expectedBuilder = Builder.BeginCell();
        expectedBuilder.StoreUint(OutActionChangeLibraryTag, 32);
        expectedBuilder.StoreUint(mode, 7);
        libRef.Store(expectedBuilder);
        Cell expected = expectedBuilder.EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void Test_OutAction_SendMsg_Deserialize()
    {
        SendMode mode = SendMode.SendPayFwdFeesSeparately;

        Builder msgBuilder = Builder.BeginCell();
        MockMessageRelaxed1.Store(msgBuilder);

        Cell actionCell = Builder.BeginCell()
            .StoreUint(OutActionSendMsgTag, 32)
            .StoreUint((byte)mode, 8)
            .StoreRef(msgBuilder.EndCell())
            .EndCell();

        OutAction actual = OutAction.Load(actionCell.BeginParse());

        Assert.That(actual, Is.InstanceOf<OutAction.SendMsg>());
        OutAction.SendMsg sendMsg = (OutAction.SendMsg)actual;
        Assert.Multiple(() =>
        {
            Assert.That(sendMsg.Mode, Is.EqualTo(mode));
            Assert.That(sendMsg.OutMsg.Body.Equals(MockMessageRelaxed1.Body), Is.True);
        });
    }

    [Test]
    public void Test_OutAction_SetCode_Deserialize()
    {
        Cell actionCell = Builder.BeginCell()
            .StoreUint(OutActionSetCodeTag, 32)
            .StoreRef(MockSetCodeCell)
            .EndCell();

        OutAction actual = OutAction.Load(actionCell.BeginParse());

        Assert.That(actual, Is.InstanceOf<OutAction.SetCode>());
        OutAction.SetCode setCode = (OutAction.SetCode)actual;
        Assert.That(setCode.NewCode.Equals(MockSetCodeCell), Is.True);
    }

    [Test]
    public void Test_OutAction_Reserve_Deserialize()
    {
        ReserveMode mode = ReserveMode.ThisAmount;
        CurrencyCollection currency = new(3000000);

        Builder builder = Builder.BeginCell();
        builder.StoreUint(OutActionReserveTag, 32);
        builder.StoreUint((byte)mode, 8);
        currency.Store(builder);
        Cell actionCell = builder.EndCell();

        OutAction actual = OutAction.Load(actionCell.BeginParse());

        Assert.That(actual, Is.InstanceOf<OutAction.Reserve>());
        OutAction.Reserve reserve = (OutAction.Reserve)actual;
        Assert.Multiple(() =>
        {
            Assert.That(reserve.Mode, Is.EqualTo(mode));
            Assert.That(reserve.Currency.Coins, Is.EqualTo(currency.Coins));
        });
    }

    [Test]
    public void Test_OutAction_ChangeLibrary_Deserialize()
    {
        byte mode = 1;
        byte[] libHash = new byte[32];
        LibRef.Hash libRef = new(libHash);

        Builder builder = Builder.BeginCell();
        builder.StoreUint(OutActionChangeLibraryTag, 32);
        builder.StoreUint(mode, 7);
        libRef.Store(builder);
        Cell actionCell = builder.EndCell();

        OutAction actual = OutAction.Load(actionCell.BeginParse());

        Assert.That(actual, Is.InstanceOf<OutAction.ChangeLibrary>());
        OutAction.ChangeLibrary changeLib = (OutAction.ChangeLibrary)actual;
        Assert.Multiple(() =>
        {
            Assert.That(changeLib.Mode, Is.EqualTo(mode));
            Assert.That(changeLib.LibRef, Is.InstanceOf<LibRef.Hash>());
        });
    }

    [Test]
    public void Test_OutList_Serialize()
    {
        SendMode sendMode1 = SendMode.SendPayFwdFeesSeparately;
        SendMode sendMode2 = SendMode.SendIgnoreErrors;
        ReserveMode reserveMode = ReserveMode.ThisAmount;
        byte changeLibraryMode = 1;

        List<OutAction> actions =
        [
            new OutAction.SendMsg(sendMode1, MockMessageRelaxed1),
            new OutAction.SendMsg(sendMode2, MockMessageRelaxed2),
            new OutAction.SetCode(MockSetCodeCell),
            new OutAction.Reserve(reserveMode, new CurrencyCollection(3000000)),
            new OutAction.ChangeLibrary(changeLibraryMode,
                new LibRef.Ref(Builder.BeginCell().StoreUint(1234, 16).EndCell()))
        ];

        Builder actualBuilder = Builder.BeginCell();
        OutList.Store(actualBuilder, actions);
        Cell actual = actualBuilder.EndCell();

        // Build expected (reverse order linked list)
        Builder msg1Builder = Builder.BeginCell();
        MockMessageRelaxed1.Store(msg1Builder);

        Builder msg2Builder = Builder.BeginCell();
        MockMessageRelaxed2.Store(msg2Builder);

        Cell expected = Builder.BeginCell()
            .StoreRef(
                Builder.BeginCell()
                    .StoreRef(
                        Builder.BeginCell()
                            .StoreRef(
                                Builder.BeginCell()
                                    .StoreRef(
                                        Builder.BeginCell()
                                            .StoreRef(Builder.BeginCell().EndCell())
                                            .StoreUint(OutActionSendMsgTag, 32)
                                            .StoreUint((byte)sendMode1, 8)
                                            .StoreRef(msg1Builder.EndCell())
                                            .EndCell()
                                    )
                                    .StoreUint(OutActionSendMsgTag, 32)
                                    .StoreUint((byte)sendMode2, 8)
                                    .StoreRef(msg2Builder.EndCell())
                                    .EndCell()
                            )
                            .StoreUint(OutActionSetCodeTag, 32)
                            .StoreRef(MockSetCodeCell)
                            .EndCell()
                    )
                    .StoreUint(OutActionReserveTag, 32)
                    .StoreUint((byte)reserveMode, 8)
                    .StoreCoins(3000000)
                    .StoreBit(false) // no extra currencies
                    .EndCell()
            )
            .StoreUint(OutActionChangeLibraryTag, 32)
            .StoreUint(changeLibraryMode, 7)
            .StoreUint(1, 1)
            .StoreRef(Builder.BeginCell().StoreUint(1234, 16).EndCell())
            .EndCell();

        Assert.That(actual.Equals(expected), Is.True);
    }

    [Test]
    public void Test_OutList_Deserialize()
    {
        SendMode sendMode1 = SendMode.SendPayFwdFeesSeparately;
        SendMode sendMode2 = SendMode.SendIgnoreErrors;
        ReserveMode reserveMode = ReserveMode.ThisAmount;
        byte changeLibraryMode = 1;

        Builder msg1Builder = Builder.BeginCell();
        MockMessageRelaxed1.Store(msg1Builder);

        Builder msg2Builder = Builder.BeginCell();
        MockMessageRelaxed2.Store(msg2Builder);

        Cell rawList = Builder.BeginCell()
            .StoreRef(
                Builder.BeginCell()
                    .StoreRef(
                        Builder.BeginCell()
                            .StoreRef(
                                Builder.BeginCell()
                                    .StoreRef(
                                        Builder.BeginCell()
                                            .StoreRef(Builder.BeginCell().EndCell())
                                            .StoreUint(OutActionSendMsgTag, 32)
                                            .StoreUint((byte)sendMode1, 8)
                                            .StoreRef(msg1Builder.EndCell())
                                            .EndCell()
                                    )
                                    .StoreUint(OutActionSendMsgTag, 32)
                                    .StoreUint((byte)sendMode2, 8)
                                    .StoreRef(msg2Builder.EndCell())
                                    .EndCell()
                            )
                            .StoreUint(OutActionSetCodeTag, 32)
                            .StoreRef(MockSetCodeCell)
                            .EndCell()
                    )
                    .StoreUint(OutActionReserveTag, 32)
                    .StoreUint((byte)reserveMode, 8)
                    .StoreCoins(3000000)
                    .StoreBit(false) // no extra currencies
                    .EndCell()
            )
            .StoreUint(OutActionChangeLibraryTag, 32)
            .StoreUint(changeLibraryMode, 7)
            .StoreUint(1, 1)
            .StoreRef(Builder.BeginCell().StoreUint(1234, 16).EndCell())
            .EndCell();

        List<OutAction> actual = OutList.Load(rawList.BeginParse());

        Assert.That(actual, Has.Count.EqualTo(5));

        Assert.That(actual[0], Is.InstanceOf<OutAction.SendMsg>());
        OutAction.SendMsg sendMsg1 = (OutAction.SendMsg)actual[0];
        Assert.Multiple(() =>
        {
            Assert.That(sendMsg1.Mode, Is.EqualTo(sendMode1));

            Assert.That(actual[1], Is.InstanceOf<OutAction.SendMsg>());
        });
        OutAction.SendMsg sendMsg2 = (OutAction.SendMsg)actual[1];
        Assert.Multiple(() =>
        {
            Assert.That(sendMsg2.Mode, Is.EqualTo(sendMode2));

            Assert.That(actual[2], Is.InstanceOf<OutAction.SetCode>());
        });
        OutAction.SetCode setCode = (OutAction.SetCode)actual[2];
        Assert.Multiple(() =>
        {
            Assert.That(setCode.NewCode.Equals(MockSetCodeCell), Is.True);

            Assert.That(actual[3], Is.InstanceOf<OutAction.Reserve>());
        });
        OutAction.Reserve reserve = (OutAction.Reserve)actual[3];
        Assert.Multiple(() =>
        {
            Assert.That(reserve.Mode, Is.EqualTo(reserveMode));
            Assert.That(reserve.Currency.Coins, Is.EqualTo(BigInteger.Parse("3000000")));

            Assert.That(actual[4], Is.InstanceOf<OutAction.ChangeLibrary>());
        });
        OutAction.ChangeLibrary changeLib = (OutAction.ChangeLibrary)actual[4];
        Assert.That(changeLib.Mode, Is.EqualTo(changeLibraryMode));
    }
}
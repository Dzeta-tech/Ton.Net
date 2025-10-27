using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class RelaxedTypesTests
{
    [Test]
    public void Test_CommonMessageInfoRelaxed_Internal_WithNullSrc()
    {
        // Create internal message with null src (relaxed allows this)
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfoRelaxed info = new CommonMessageInfoRelaxed.Internal(
            true,
            true,
            false,
            null, // src can be null in relaxed
            destAddr,
            new CurrencyCollection(1000000000),
            0,
            1000,
            12345,
            1234567890
        );

        Builder builder = Builder.BeginCell();
        info.Store(builder);
        Cell cell = builder.EndCell();

        CommonMessageInfoRelaxed loaded = CommonMessageInfoRelaxed.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<CommonMessageInfoRelaxed.Internal>());
        CommonMessageInfoRelaxed.Internal @internal = (CommonMessageInfoRelaxed.Internal)loaded;
        Assert.Multiple(() =>
        {
            Assert.That(@internal.Src, Is.Null);
            Assert.That(@internal.Dest, Is.EqualTo(destAddr));
            Assert.That(@internal.Value.Coins, Is.EqualTo((BigInteger)1000000000));
        });
    }

    [Test]
    public void Test_CommonMessageInfoRelaxed_Internal_WithSrc()
    {
        // Create internal message with src
        Address srcAddr = new(0, new byte[32]);
        Address destAddr = new(-1, new byte[32]);

        CommonMessageInfoRelaxed info = new CommonMessageInfoRelaxed.Internal(
            true,
            false,
            false,
            srcAddr,
            destAddr,
            new CurrencyCollection(5000000),
            100,
            200,
            99999,
            9876543
        );

        Builder builder = Builder.BeginCell();
        info.Store(builder);
        Cell cell = builder.EndCell();

        CommonMessageInfoRelaxed loaded = CommonMessageInfoRelaxed.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<CommonMessageInfoRelaxed.Internal>());
        CommonMessageInfoRelaxed.Internal @internal = (CommonMessageInfoRelaxed.Internal)loaded;
        Assert.Multiple(() =>
        {
            Assert.That(@internal.Src, Is.Not.Null);
            Assert.That(@internal.Src!, Is.EqualTo(srcAddr));
            Assert.That(@internal.CreatedLt, Is.EqualTo((BigInteger)99999));
        });
    }

    [Test]
    public void Test_CommonMessageInfoRelaxed_ExternalOut()
    {
        // Create external out message
        Address srcAddr = new(0, new byte[32]);
        ExternalAddress destAddr = new(12345, 64);

        CommonMessageInfoRelaxed info = new CommonMessageInfoRelaxed.ExternalOut(
            srcAddr,
            destAddr,
            77777,
            8888888
        );

        Builder builder = Builder.BeginCell();
        info.Store(builder);
        Cell cell = builder.EndCell();

        CommonMessageInfoRelaxed loaded = CommonMessageInfoRelaxed.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<CommonMessageInfoRelaxed.ExternalOut>());
        CommonMessageInfoRelaxed.ExternalOut eo = (CommonMessageInfoRelaxed.ExternalOut)loaded;
        Assert.Multiple(() =>
        {
            Assert.That(eo.Src, Is.Not.Null);
            Assert.That(eo.Src!, Is.EqualTo(srcAddr));
            Assert.That(eo.Dest, Is.Not.Null);
            Assert.That(eo.Dest!.Value, Is.EqualTo((BigInteger)12345));
        });
    }

    [Test]
    public void Test_MessageRelaxed_Roundtrip()
    {
        // Create relaxed message with null src
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfoRelaxed info = new CommonMessageInfoRelaxed.Internal(
            true,
            true,
            false,
            null,
            destAddr,
            new CurrencyCollection(1000),
            0,
            0,
            0,
            0
        );

        Cell body = Builder.BeginCell().StoreUint(123, 32).EndCell();
        MessageRelaxed message = new(info, body);

        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();

        MessageRelaxed loaded = MessageRelaxed.Load(cell.BeginParse());

        Builder builder2 = Builder.BeginCell();
        loaded.Store(builder2);
        Cell cell2 = builder2.EndCell();

        Assert.Multiple(() =>
        {
            Assert.That(cell.Equals(cell2), Is.True);
            Assert.That(loaded.Info, Is.InstanceOf<CommonMessageInfoRelaxed.Internal>());
            Assert.That(loaded.Init, Is.Null);
            Assert.That(loaded.Body.Bits.Length, Is.EqualTo(32));
        });
    }

    [Test]
    public void Test_MessageRelaxed_WithStateInit()
    {
        // Create relaxed message with StateInit
        Address destAddr = new(0, new byte[32]);

        CommonMessageInfoRelaxed info = new CommonMessageInfoRelaxed.Internal(
            true,
            false,
            false,
            null,
            destAddr,
            new CurrencyCollection(0),
            0,
            0,
            0,
            0
        );

        StateInit stateInit = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            Builder.BeginCell().StoreUint(2, 8).EndCell()
        );

        Cell body = Builder.BeginCell().StoreUint(999, 32).EndCell();
        MessageRelaxed message = new(info, body, stateInit);

        Builder builder = Builder.BeginCell();
        message.Store(builder);
        Cell cell = builder.EndCell();

        MessageRelaxed loaded = MessageRelaxed.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Init, Is.Not.Null);
            Assert.That(loaded.Init!.Code, Is.Not.Null);
            Assert.That(loaded.Init!.Data, Is.Not.Null);
        });
    }

    [Test]
    public void Test_DepthBalanceInfo_Roundtrip()
    {
        DepthBalanceInfo info = new(
            15,
            new CurrencyCollection(999999999)
        );

        Builder builder = Builder.BeginCell();
        info.Store(builder);
        Cell cell = builder.EndCell();

        DepthBalanceInfo loaded = DepthBalanceInfo.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.SplitDepth, Is.EqualTo(15));
            Assert.That(loaded.Balance.Coins, Is.EqualTo((BigInteger)999999999));
        });
    }

    [Test]
    public void Test_Account_Roundtrip()
    {
        Account account = new(
            new Address(0, new byte[32]),
            new StorageInfo(
                new StorageUsed(100, 5000),
                null,
                1234567890
            ),
            new AccountStorage(
                12345,
                new CurrencyCollection(1000000000),
                new AccountState.Active(
                    new StateInit(
                        Builder.BeginCell().StoreUint(1, 8).EndCell(),
                        Builder.BeginCell().StoreUint(2, 8).EndCell()
                    )
                )
            )
        );

        Builder builder = Builder.BeginCell();
        account.Store(builder);
        Cell cell = builder.EndCell();

        Account loaded = Account.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Addr.Workchain, Is.EqualTo(0));
            Assert.That(loaded.StorageStats.LastPaid, Is.EqualTo(1234567890U));
            Assert.That(loaded.Storage.Balance.Coins, Is.EqualTo((BigInteger)1000000000));
            Assert.That(loaded.Storage.State, Is.InstanceOf<AccountState.Active>());
        });
    }
}
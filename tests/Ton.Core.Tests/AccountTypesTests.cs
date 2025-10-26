using System.Numerics;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class AccountTypesTests
{
    [Test]
    public void Test_AccountStatus_Roundtrip()
    {
        foreach (AccountStatus status in Enum.GetValues<AccountStatus>())
        {
            Builder builder = Builder.BeginCell();
            builder.StoreAccountStatus(status);
            Cell cell = builder.EndCell();

            AccountStatus loaded = cell.BeginParse().LoadAccountStatus();
            Assert.That(loaded, Is.EqualTo(status));
        }
    }

    [Test]
    public void Test_StorageUsed_Roundtrip()
    {
        StorageUsed storage = new(1000, 50000);

        Builder builder = Builder.BeginCell();
        storage.Store(builder);
        Cell cell = builder.EndCell();

        StorageUsed loaded = StorageUsed.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Cells, Is.EqualTo((BigInteger)1000));
            Assert.That(loaded.Bits, Is.EqualTo((BigInteger)50000));
        });
    }

    [Test]
    public void Test_StorageExtraInfo_None()
    {
        Builder builder = Builder.BeginCell();
        StorageExtraInfo.Store(builder, null);
        Cell cell = builder.EndCell();

        StorageExtraInfo? loaded = StorageExtraInfo.Load(cell.BeginParse());
        Assert.That(loaded, Is.Null);
    }

    [Test]
    public void Test_StorageExtraInfo_WithHash()
    {
        BigInteger dictHash = BigInteger.Parse("123456789012345678901234567890");
        StorageExtraInfo info = new(dictHash);

        Builder builder = Builder.BeginCell();
        StorageExtraInfo.Store(builder, info);
        Cell cell = builder.EndCell();

        StorageExtraInfo? loaded = StorageExtraInfo.Load(cell.BeginParse());

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.DictHash, Is.EqualTo(dictHash));
    }

    [Test]
    public void Test_StorageInfo_Roundtrip()
    {
        StorageInfo storage = new(
            new StorageUsed(100, 5000),
            new StorageExtraInfo(12345),
            1234567890,
            1000000000
        );

        Builder builder = Builder.BeginCell();
        storage.Store(builder);
        Cell cell = builder.EndCell();

        StorageInfo loaded = StorageInfo.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.Used.Cells, Is.EqualTo((BigInteger)100));
            Assert.That(loaded.Used.Bits, Is.EqualTo((BigInteger)5000));
            Assert.That(loaded.StorageExtra, Is.Not.Null);
            Assert.That(loaded.StorageExtra!.DictHash, Is.EqualTo((BigInteger)12345));
            Assert.That(loaded.LastPaid, Is.EqualTo(1234567890U));
            Assert.That(loaded.DuePayment, Is.EqualTo((BigInteger)1000000000));
        });
    }

    [Test]
    public void Test_StorageInfo_NoDuePayment()
    {
        StorageInfo storage = new(
            new StorageUsed(100, 5000),
            null,
            1234567890
        );

        Builder builder = Builder.BeginCell();
        storage.Store(builder);
        Cell cell = builder.EndCell();

        StorageInfo loaded = StorageInfo.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.StorageExtra, Is.Null);
            Assert.That(loaded.DuePayment, Is.Null);
        });
    }

    [Test]
    public void Test_AccountState_Uninit()
    {
        AccountState state = new AccountState.Uninit();

        Builder builder = Builder.BeginCell();
        state.Store(builder);
        Cell cell = builder.EndCell();

        AccountState loaded = AccountState.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<AccountState.Uninit>());
    }

    [Test]
    public void Test_AccountState_Active()
    {
        StateInit stateInit = new(
            Builder.BeginCell().StoreUint(1, 8).EndCell(),
            Builder.BeginCell().StoreUint(2, 8).EndCell()
        );
        AccountState state = new AccountState.Active(stateInit);

        Builder builder = Builder.BeginCell();
        state.Store(builder);
        Cell cell = builder.EndCell();

        AccountState loaded = AccountState.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<AccountState.Active>());
        AccountState.Active active = (AccountState.Active)loaded;
        Assert.Multiple(() =>
        {
            Assert.That(active.State.Code, Is.Not.Null);
            Assert.That(active.State.Data, Is.Not.Null);
        });
    }

    [Test]
    public void Test_AccountState_Frozen()
    {
        BigInteger stateHash = BigInteger.Parse("999999999999999999999999999999");
        AccountState state = new AccountState.Frozen(stateHash);

        Builder builder = Builder.BeginCell();
        state.Store(builder);
        Cell cell = builder.EndCell();

        AccountState loaded = AccountState.Load(cell.BeginParse());

        Assert.That(loaded, Is.InstanceOf<AccountState.Frozen>());
        AccountState.Frozen frozen = (AccountState.Frozen)loaded;
        Assert.That(frozen.StateHash, Is.EqualTo(stateHash));
    }

    [Test]
    public void Test_AccountStorage_Roundtrip()
    {
        AccountStorage storage = new(
            12345678,
            new CurrencyCollection(1000000000),
            new AccountState.Active(
                new StateInit(
                    Builder.BeginCell().StoreUint(100, 32).EndCell(),
                    Builder.BeginCell().StoreUint(200, 32).EndCell()
                )
            )
        );

        Builder builder = Builder.BeginCell();
        storage.Store(builder);
        Cell cell = builder.EndCell();

        AccountStorage loaded = AccountStorage.Load(cell.BeginParse());

        Assert.Multiple(() =>
        {
            Assert.That(loaded.LastTransLt, Is.EqualTo((BigInteger)12345678));
            Assert.That(loaded.Balance.Coins, Is.EqualTo((BigInteger)1000000000));
            Assert.That(loaded.State, Is.InstanceOf<AccountState.Active>());
        });

        AccountState.Active activeState = (AccountState.Active)loaded.State;
        Assert.Multiple(() =>
        {
            Assert.That(activeState.State.Code, Is.Not.Null);
            Assert.That(activeState.State.Code!.BeginParse().LoadUint(32), Is.EqualTo(100UL));
            Assert.That(activeState.State.Data, Is.Not.Null);
            Assert.That(activeState.State.Data!.BeginParse().LoadUint(32), Is.EqualTo(200UL));
        });
    }
}
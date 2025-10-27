using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Contracts;
using Ton.Core.Tuple;
using Ton.Core.Types;

namespace Ton.Core.Tests.Contracts;

/// <summary>
///     Test contract for testing OpenedContract pattern.
/// </summary>
public class TestContract(Address address, StateInit? init = null) : IContract
{
    public Address Address { get; } = address;
    public StateInit? Init { get; } = init;
    public ContractABI? ABI { get; }

    /// <summary>
    ///     Get method - should be callable via OpenedContract
    /// </summary>
    public async Task<BigInteger> GetBalanceAsync(OpenedContract<TestContract> contract)
    {
        ContractState state = await contract.Provider.GetStateAsync();
        return state.Balance;
    }

    /// <summary>
    ///     Get method with parameters
    /// </summary>
    public async Task<BigInteger> GetDataAsync(OpenedContract<TestContract> contract, int queryId)
    {
        ContractGetMethodResult result = await contract.Provider.GetAsync("get_data", [
            new TupleItemInt(queryId)
        ]);

        return result.Stack.ReadNumber();
    }

    /// <summary>
    ///     Send method - should be callable via OpenedContract
    /// </summary>
    public async Task SendTransferAsync(
        OpenedContract<TestContract> contract,
        ISender via,
        BigInteger amount,
        Address to)
    {
        Cell body = Builder.BeginCell()
            .StoreUint(0x12345678, 32) // op code
            .StoreAddress(to)
            .StoreCoins(amount)
            .EndCell();

        await contract.Provider.InternalAsync(via, new InternalMessageArgs
        {
            Value = amount,
            Body = body
        });
    }

    /// <summary>
    ///     Is method - should be callable via OpenedContract
    /// </summary>
    public async Task<bool> IsDeployedAsync(OpenedContract<TestContract> contract)
    {
        ContractState state = await contract.Provider.GetStateAsync();
        return state.State is ContractState.AccountStateInfo.Active;
    }

    /// <summary>
    ///     Regular method - should NOT be auto-wrapped
    /// </summary>
    public string GetName()
    {
        return "TestContract";
    }
}

public class OpenedContractTests
{
    MockContractProvider provider = null!;
    Address testAddress = null!;

    [SetUp]
    public void Setup()
    {
        provider = new MockContractProvider();
        testAddress = Address.Parse("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD");
    }

    [Test]
    public void Test_OpenedContract_Creation()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        Assert.Multiple(() =>
        {
            Assert.That(opened.Contract, Is.Not.Null);
            Assert.That(opened.Provider, Is.Not.Null);
        });
    }

    [Test]
    public void Test_OpenedContract_Extension_Method()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        Assert.That(opened, Is.Not.Null);
        Assert.That(opened.Contract.Address, Is.EqualTo(testAddress));
    }

    [Test]
    public async Task Test_OpenedContract_GetMethod()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        provider.SetState(new ContractState
        {
            Balance = 1000000000,
            State = new ContractState.AccountStateInfo.Active(null, null)
        });

        BigInteger balance = await contract.GetBalanceAsync(opened);
        Assert.That(balance, Is.EqualTo(new BigInteger(1000000000)));
    }

    [Test]
    public async Task Test_OpenedContract_GetMethod_With_Parameters()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        TupleBuilder builder = new();
        builder.WriteNumber(999);

        provider.SetMethodResult("get_data", new ContractGetMethodResult
        {
            Stack = new TupleReader(builder.Build())
        });

        BigInteger data = await contract.GetDataAsync(opened, 123);
        Assert.That(data, Is.EqualTo(new BigInteger(999)));
    }

    [Test]
    public async Task Test_OpenedContract_SendMethod()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        MockSender mockSender = new(Address.Parse("EQBvW8Z5huBkMJYdnfAEM5JqTNkuWX3diqYENkWsIL0XggGG"));
        Address toAddress = Address.Parse("EQCtW_zzk6n82ebaVQFq8P_04wOemYhtwqMd3NuArmPODRvD");

        await contract.SendTransferAsync(opened, mockSender, 500000, toAddress);

        Assert.That(provider.InternalMessages, Has.Count.EqualTo(1));
        (ISender sender, InternalMessageArgs args) = provider.InternalMessages[0];
        Assert.Multiple(() =>
        {
            Assert.That(sender, Is.Not.Null);
            Assert.That(args.Value, Is.EqualTo(new BigInteger(500000)));
            Assert.That(args.Body, Is.Not.Null);
        });
    }

    [Test]
    public async Task Test_OpenedContract_IsMethod()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        provider.SetState(new ContractState
        {
            Balance = 1000000,
            State = new ContractState.AccountStateInfo.Active(null, null)
        });

        bool isDeployed = await contract.IsDeployedAsync(opened);
        Assert.That(isDeployed, Is.True);
    }

    [Test]
    public void Test_OpenedContract_RegularMethod_NotWrapped()
    {
        TestContract contract = new(testAddress);
        OpenedContract<TestContract> opened = provider.Open(contract);

        // Regular methods (not starting with Get/Send/Is) should still work
        string name = opened.Contract.GetName();
        Assert.That(name, Is.EqualTo("TestContract"));
    }

    [Test]
    public void Test_OpenedContract_WithInit()
    {
        Cell code = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell data = Builder.BeginCell().StoreUint(2, 8).EndCell();
        StateInit init = new(code, data);

        TestContract contract = new(testAddress, init);
        OpenedContract<TestContract> opened = provider.Open(contract);

        Assert.That(opened.Contract.Init, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(opened.Contract.Init!.Code, Is.Not.Null);
            Assert.That(opened.Contract.Init.Data, Is.Not.Null);
        });
    }

    class MockSender(Address? address) : ISender
    {
        public Address? Address { get; } = address;

        public Task SendAsync(SenderArguments args)
        {
            return Task.CompletedTask;
        }
    }
}
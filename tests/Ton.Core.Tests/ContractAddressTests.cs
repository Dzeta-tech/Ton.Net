using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Types;

namespace Ton.Core.Tests;

public class ContractAddressTests
{
    [Test]
    public void Test_ContractAddress_ComputesCorrectly()
    {
        // Test case from JS SDK
        Cell code = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell data = Builder.BeginCell().StoreUint(2, 8).EndCell();
        StateInit init = new(code, data);

        Address addr = ContractAddress.From(0, init);

        // Expected address from JS SDK test
        Address expected = Address.Parse("EQCSY_vTjwGrlvTvkfwhinJ60T2oiwgGn3U7Tpw24kupIhHz");

        Assert.That(addr.ToString(), Is.EqualTo(expected.ToString()));
        Assert.That(addr.WorkChain, Is.EqualTo(0));
    }

    [Test]
    public void Test_ContractAddress_DefaultsToWorkchain0()
    {
        Cell code = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell data = Builder.BeginCell().StoreUint(2, 8).EndCell();
        StateInit init = new(code, data);

        Address addr1 = ContractAddress.From(init);
        Address addr2 = ContractAddress.From(0, init);

        Assert.That(addr1.ToString(), Is.EqualTo(addr2.ToString()));
    }

    [Test]
    public void Test_ContractAddress_DifferentWorkchains()
    {
        Cell code = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell data = Builder.BeginCell().StoreUint(2, 8).EndCell();
        StateInit init = new(code, data);

        Address addr0 = ContractAddress.From(0, init);
        Address addrMaster = ContractAddress.From(-1, init);

        // Different workchains should produce different addresses
        Assert.That(addr0.WorkChain, Is.EqualTo(0));
        Assert.That(addrMaster.WorkChain, Is.EqualTo(-1));
        Assert.That(addr0.ToString(), Is.Not.EqualTo(addrMaster.ToString()));
    }

    [Test]
    public void Test_ContractAddress_Deterministic()
    {
        // Same StateInit should always produce same address
        Cell code = Builder.BeginCell().StoreUint(123, 32).EndCell();
        Cell data = Builder.BeginCell().StoreUint(456, 32).EndCell();
        StateInit init = new(code, data);

        Address addr1 = ContractAddress.From(0, init);
        Address addr2 = ContractAddress.From(0, init);

        Assert.That(addr1.ToString(), Is.EqualTo(addr2.ToString()));
    }

    [Test]
    public void Test_ContractAddress_DifferentCodeProducesDifferentAddress()
    {
        Cell code1 = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell code2 = Builder.BeginCell().StoreUint(2, 8).EndCell();
        Cell data = Builder.BeginCell().StoreUint(100, 8).EndCell();

        StateInit init1 = new(code1, data);
        StateInit init2 = new(code2, data);

        Address addr1 = ContractAddress.From(0, init1);
        Address addr2 = ContractAddress.From(0, init2);

        Assert.That(addr1.ToString(), Is.Not.EqualTo(addr2.ToString()));
    }

    [Test]
    public void Test_ContractAddress_DifferentDataProducesDifferentAddress()
    {
        Cell code = Builder.BeginCell().StoreUint(100, 8).EndCell();
        Cell data1 = Builder.BeginCell().StoreUint(1, 8).EndCell();
        Cell data2 = Builder.BeginCell().StoreUint(2, 8).EndCell();

        StateInit init1 = new(code, data1);
        StateInit init2 = new(code, data2);

        Address addr1 = ContractAddress.From(0, init1);
        Address addr2 = ContractAddress.From(0, init2);

        Assert.That(addr1.ToString(), Is.Not.EqualTo(addr2.ToString()));
    }

    [Test]
    public void Test_ContractAddress_WithComplexStateInit()
    {
        // More realistic example with larger code/data
        Builder codeBuilder = Builder.BeginCell();
        codeBuilder.StoreUint(0x12345678, 32);
        codeBuilder.StoreUint(0xABCDEF, 24);
        Cell code = codeBuilder.EndCell();

        Builder dataBuilder = Builder.BeginCell();
        dataBuilder.StoreUint(0, 32); // seqno
        dataBuilder.StoreUint(0x9876543210, 40); // some data
        Cell data = dataBuilder.EndCell();

        StateInit init = new(code, data);

        Address addr = ContractAddress.From(0, init);

        // Should be a valid address
        Assert.That(addr, Is.Not.Null);
        Assert.That(addr.WorkChain, Is.EqualTo(0));
        Assert.That(addr.Hash.Length, Is.EqualTo(32));
    }
}
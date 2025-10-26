using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;

namespace Ton.Core.Tests;

[TestFixture]
public class BitBuilderTests
{
    [TestCase(10290, 29, "00014194_")]
    [TestCase(41732, 27, "0014609_")]
    [TestCase(62757, 22, "03D496_")]
    [TestCase(44525, 16, "ADED")]
    [TestCase(26925, 30, "0001A4B6_")]
    [TestCase(52948, 27, "0019DA9_")]
    [TestCase(12362, 20, "0304A")]
    [TestCase(31989, 16, "7CF5")]
    [TestCase(8503, 21, "0109BC_")]
    [TestCase(54308, 17, "6A124_")]
    public void Test_SerializeUint(long value, int bits, string expected)
    {
        var builder = new BitBuilder();
        builder.WriteUint(value, bits);
        var result = builder.Build();
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase(-44028, 22, "FD5012_")]
    [TestCase(-1613, 16, "F9B3")]
    [TestCase(-3640, 23, "FFE391_")]
    [TestCase(45943, 22, "02CDDE_")]
    [TestCase(-25519, 22, "FE7146_")]
    [TestCase(-31775, 31, "FFFF07C3_")]
    [TestCase(3609, 29, "000070CC_")]
    [TestCase(-38203, 20, "F6AC5")]
    [TestCase(59963, 28, "000EA3B")]
    [TestCase(-22104, 21, "FD4D44_")]
    public void Test_SerializeInt(long value, int bits, string expected)
    {
        var builder = new BitBuilder();
        builder.WriteInt(value, bits);
        var result = builder.Build();
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase("187657898555727", "6AAAC8261F94F")]
    [TestCase("220186135208421", "6C842145FA1E5")]
    [TestCase("38303065322130", "622D6209A3292")]
    [TestCase("99570315572129", "65A8F054A33A1")]
    [TestCase("14785390105803", "60D727DECD4CB")]
    [TestCase("244446854605494", "6DE52B7EF6AB6")]
    [TestCase("130189848588337", "676682FADB031")]
    [TestCase("82548661242881", "64B13DBA14C01")]
    [TestCase("248198532456807", "6E1BC395C6167")]
    [TestCase("192570661887521", "6AF2459E55E21")]
    public void Test_SerializeCoins(string value, string expected)
    {
        var builder = new BitBuilder();
        builder.WriteCoins(BigInteger.Parse(value));
        var result = builder.Build();
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [TestCase("Ef89v3kFhPfyauFSn_PWq-F6HyiBSQDZRXjoDRWq5f5IZeTm", "9FE7B7EF20B09EFE4D5C2A53FE7AD57C2F43E51029201B28AF1D01A2B55CBFC90CB_")]
    [TestCase("Ef-zUJX6ySukm-41iSbHW5Ad788NYuWPYKzuAj4vLhe8WSgF", "9FF66A12BF592574937DC6B124D8EB7203BDF9E1AC5CB1EC159DC047C5E5C2F78B3_")]
    [TestCase("Ef-x95AVmzKUKkS7isd6XF7YqZf0R0JyOzBO7jir239_feMb", "9FF63EF202B366528548977158EF4B8BDB1532FE88E84E476609DDC7157B6FEFEFB_")]
    [TestCase("EQDA1y4uDTy1pdfReyOVD6WWGaAsD7CXg4SgltHS8NzITENs", "80181AE5C5C1A796B4BAFA2F6472A1F4B2C3340581F612F0709412DA3A5E1B99099_")]
    [TestCase("Ef-BsrQDp9XMxUjQW2lnRAdZFKKzBXmATqX57NPO5fjbbEkn", "9FF036568074FAB998A91A0B6D2CE880EB22945660AF3009D4BF3D9A79DCBF1B6D9_")]
    public void Test_SerializeAddress(string address, string expected)
    {
        var builder = new BitBuilder();
        builder.WriteAddress(Address.Parse(address));
        var result = builder.Build();
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void Test_StoreBigintAndNumberForLen1()
    {
        var builder = new BitBuilder();
        builder.WriteInt(0, 1);
        builder.WriteInt(new BigInteger(0), 1);
        builder.WriteInt(-1, 1);
        builder.WriteInt(new BigInteger(-1), 1);
        Assert.That(builder.Length, Is.EqualTo(4));
    }

    [Test]
    public void Test_StoreBigintAndNumberForLen0()
    {
        var builder = new BitBuilder();
        builder.WriteInt(0, 0);
        builder.WriteInt(new BigInteger(0), 0);
        Assert.That(builder.Length, Is.EqualTo(0));
    }
}


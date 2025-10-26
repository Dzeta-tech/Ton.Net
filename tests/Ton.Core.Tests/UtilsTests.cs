using System.Numerics;
using Ton.Core.Utils;

namespace Ton.Core.Tests;

[TestFixture]
public class CoinsTests
{
    [Test]
    public void Test_ToNano_ThrowsForNaN()
    {
        Assert.Throws<ArgumentException>(() => Coins.ToNano(double.NaN));
    }

    [Test]
    public void Test_ToNano_ThrowsForInfinity()
    {
        Assert.Throws<ArgumentException>(() => Coins.ToNano(double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() => Coins.ToNano(double.NegativeInfinity));
    }

    [Test]
    public void Test_ToNano_ThrowsForInsufficientPrecision()
    {
        Assert.Throws<ArgumentException>(() => Coins.ToNano(10000000.000000001));
    }

    [TestCase("1", "1000000000")]
    [TestCase("10", "10000000000")]
    [TestCase("0.1", "100000000")]
    [TestCase("0.33", "330000000")]
    [TestCase("0.000000001", "1")]
    [TestCase("10.000000001", "10000000001")]
    [TestCase("1000000.000000001", "1000000000000001")]
    [TestCase("100000000000", "100000000000000000000")]
    public void Test_ToNano_FromString(string real, string nano)
    {
        BigInteger result = Coins.ToNano(real);
        Assert.That(result, Is.EqualTo(BigInteger.Parse(nano)));
    }

    [TestCase(0, "0")]
    [TestCase(1, "1000000000")]
    [TestCase(10, "10000000000")]
    [TestCase(0.1, "100000000")]
    [TestCase(0.33, "330000000")]
    [TestCase(0.000000001, "1")]
    [TestCase(10.000000001, "10000000001")]
    [TestCase(100000000000.0, "100000000000000000000")]
    public void Test_ToNano_FromNumber(double real, string nano)
    {
        BigInteger result = Coins.ToNano(real);
        Assert.That(result, Is.EqualTo(BigInteger.Parse(nano)));
    }

    [TestCase("1000000000", "1")]
    [TestCase("10000000000", "10")]
    [TestCase("100000000", "0.1")]
    [TestCase("330000000", "0.33")]
    [TestCase("1", "0.000000001")]
    [TestCase("10000000001", "10.000000001")]
    [TestCase("1000000000000001", "1000000.000000001")]
    [TestCase("100000000000000000000", "100000000000")]
    public void Test_FromNano(string nano, string expected)
    {
        string result = Coins.FromNano(nano);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_FromNano_RemovesTrailingZeros()
    {
        Assert.That(Coins.FromNano("1000000000"), Is.EqualTo("1"));
        Assert.That(Coins.FromNano("1500000000"), Is.EqualTo("1.5"));
        Assert.That(Coins.FromNano("1000000001"), Is.EqualTo("1.000000001"));
    }

    [Test]
    public void Test_ToNano_HandlesNegative()
    {
        Assert.That(Coins.ToNano("-1"), Is.EqualTo(BigInteger.Parse("-1000000000")));
        Assert.That(Coins.ToNano("--1"), Is.EqualTo(BigInteger.Parse("1000000000"))); // Double negative
    }

    [Test]
    public void Test_FromNano_HandlesNegative()
    {
        Assert.That(Coins.FromNano("-1000000000"), Is.EqualTo("-1"));
    }
}

[TestFixture]
public class Crc32cTests
{
    [Test]
    public void Test_Crc32c_EmptyArray()
    {
        byte[] result = Crc32c.Compute(Array.Empty<byte>());
        Assert.That(result.Length, Is.EqualTo(4));
    }

    [Test]
    public void Test_Crc32c_KnownValue()
    {
        // Test with known CRC32C value
        byte[] data = System.Text.Encoding.UTF8.GetBytes("123456789");
        byte[] result = Crc32c.Compute(data);
        
        // CRC32C of "123456789" is 0xE3069283 in little-endian
        Assert.That(result, Is.EqualTo(new byte[] { 0x83, 0x92, 0x06, 0xE3 }));
    }
}

[TestFixture]
public class Base32Tests
{
    [Test]
    public void Test_Base32_EncodeEmpty()
    {
        string result = Base32.Encode(Array.Empty<byte>());
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void Test_Base32_EncodeDecode()
    {
        byte[] original = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
        string encoded = Base32.Encode(original);
        byte[] decoded = Base32.Decode(encoded);
        
        Assert.That(decoded, Is.EqualTo(original));
    }

    [Test]
    public void Test_Base32_EncodeKnownValue()
    {
        // "Hello" in ASCII
        byte[] data = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        string result = Base32.Encode(data);
        Assert.That(result, Is.EqualTo("jbswy3dp"));
    }

    [Test]
    public void Test_Base32_DecodeKnownValue()
    {
        byte[] result = Base32.Decode("jbswy3dp");
        byte[] expected = new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Base32_DecodeThrowsOnInvalidChar()
    {
        Assert.Throws<ArgumentException>(() => Base32.Decode("invalid!"));
    }

    [Test]
    public void Test_Base32_CaseInsensitive()
    {
        byte[] lower = Base32.Decode("jbswy3dp");
        byte[] upper = Base32.Decode("JBSWY3DP");
        Assert.That(lower, Is.EqualTo(upper));
    }
}

[TestFixture]
public class MethodIdTests
{
    [Test]
    public void Test_GetMethodId_KnownValues()
    {
        // These are real TON method IDs
        int seqno = MethodId.Get("seqno");
        Assert.That(seqno & 0x10000, Is.EqualTo(0x10000)); // Should have the flag
        
        int getPublicKey = MethodId.Get("get_public_key");
        Assert.That(getPublicKey & 0x10000, Is.EqualTo(0x10000));
    }

    [Test]
    public void Test_GetMethodId_Consistency()
    {
        // Same method name should always produce the same ID
        int id1 = MethodId.Get("test_method");
        int id2 = MethodId.Get("test_method");
        Assert.That(id1, Is.EqualTo(id2));
    }

    [Test]
    public void Test_GetMethodId_DifferentNames()
    {
        // Different method names should produce different IDs
        int id1 = MethodId.Get("method1");
        int id2 = MethodId.Get("method2");
        Assert.That(id1, Is.Not.EqualTo(id2));
    }
}


using System.Text;
using Ton.Crypto.Primitives;

namespace Ton.Crypto.Tests;

[TestFixture]
public class Sha256Tests
{
    // Test Vectors from https://www.di-mgt.com.au/sha_testvectors.html
    [TestCase("abc", "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
    [TestCase("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
    [TestCase("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
        "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1")]
    public void Test_Sha256_WithString(string input, string expectedHex)
    {
        byte[] result = Sha256.Hash(input);
        string resultHex = Convert.ToHexString(result).ToLower();
        Assert.That(resultHex, Is.EqualTo(expectedHex));
    }

    [TestCase("abc", "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
    [TestCase("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
    [TestCase("abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq",
        "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1")]
    public void Test_Sha256_WithByteArray(string input, string expectedHex)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] result = Sha256.Hash(inputBytes);
        string resultHex = Convert.ToHexString(result).ToLower();
        Assert.That(resultHex, Is.EqualTo(expectedHex));
    }
}
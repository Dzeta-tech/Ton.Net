using Ton.Crypto.Primitives;

namespace Ton.Crypto.Tests;

[TestFixture]
public class HmacSha512Tests
{
    // Test Vectors from https://datatracker.ietf.org/doc/html/rfc4231
    [Test]
    public void Test_HmacSha512_Vector1()
    {
        byte[] key = "Jefe"u8.ToArray();
        byte[] data = Convert.FromHexString("7768617420646f2079612077616e7420666f72206e6f7468696e673f");
        byte[] expected =
            Convert.FromHexString(
                "164b7a7bfcf819e2e395fbe73b56e0a387bd64222e831fd610270cd7ea2505549758bf75c05a994a6d034f65f8f0e6fdcaeab1a34d4a6b4b636e070a38bce737");

        byte[] result = HmacSha512.Hash(key, data);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_HmacSha512_Vector2()
    {
        byte[] key = Convert.FromHexString("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
        byte[] data = Convert.FromHexString("4869205468657265");
        byte[] expected =
            Convert.FromHexString(
                "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854");

        byte[] result = HmacSha512.Hash(key, data);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_HmacSha512_Vector3()
    {
        byte[] key = Convert.FromHexString("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        byte[] data =
            Convert.FromHexString(
                "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd");
        byte[] expected =
            Convert.FromHexString(
                "fa73b0089d56a284efb0f0756c890be9b1b5dbdd8ee81a3655f83e33b2279d39bf3e848279a722c806b485a47e67c807b946a337bee8942674278859e13292fb");

        byte[] result = HmacSha512.Hash(key, data);
        Assert.That(result, Is.EqualTo(expected));
    }
}
using Ton.Crypto.Primitives;

namespace Ton.Crypto.Tests;

[TestFixture]
public class Pbkdf2Sha512Tests
{
    // Test Vectors from https://stackoverflow.com/questions/15593184/pbkdf2-hmac-sha-512-test-vectors
    [Test]
    public void Test_Pbkdf2_Vector1()
    {
        var password = "password";
        var salt = "salt";
        var iterations = 1;
        var keyLen = 64;
        var expected = Convert.FromHexString("867f70cf1ade02cff3752599a3a53dc4af34c7a669815ae5d513554e1c8cf252c02d470a285a0501bad999bfe943c08f050235d7d68b1da55e63f73b60a57fce");
        
        var result = Pbkdf2Sha512.DeriveKey(password, salt, iterations, keyLen);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Pbkdf2_Vector2()
    {
        var password = "password";
        var salt = "salt";
        var iterations = 2;
        var keyLen = 64;
        var expected = Convert.FromHexString("e1d9c16aa681708a45f5c7c4e215ceb66e011a2e9f0040713f18aefdb866d53cf76cab2868a39b9f7840edce4fef5a82be67335c77a6068e04112754f27ccf4e");
        
        var result = Pbkdf2Sha512.DeriveKey(password, salt, iterations, keyLen);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Pbkdf2_Vector3()
    {
        var password = "password";
        var salt = "salt";
        var iterations = 4096;
        var keyLen = 64;
        var expected = Convert.FromHexString("d197b1b33db0143e018b12f3d1d1479e6cdebdcc97c5c0f87f6902e072f457b5143f30602641b3d55cd335988cb36b84376060ecd532e039b742a239434af2d5");
        
        var result = Pbkdf2Sha512.DeriveKey(password, salt, iterations, keyLen);
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Test_Pbkdf2_Vector4()
    {
        var password = "passwordPASSWORDpassword";
        var salt = "saltSALTsaltSALTsaltSALTsaltSALTsalt";
        var iterations = 4096;
        var keyLen = 64;
        var expected = Convert.FromHexString("8c0511f4c6e597c6ac6315d8f0362e225f3c501495ba23b868c005174dc4ee71115b59f9e60cd9532fa33e0f75aefe30225c583a186cd82bd4daea9724a3d3b8");
        
        var result = Pbkdf2Sha512.DeriveKey(password, salt, iterations, keyLen);
        Assert.That(result, Is.EqualTo(expected));
    }
}


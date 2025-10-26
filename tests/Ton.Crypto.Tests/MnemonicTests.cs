using Ton.Crypto.Ed25519;
using Ton.Crypto.Primitives;

namespace Ton.Crypto.Tests;

[TestFixture]
public class MnemonicTests
{
    static readonly (string[] Mnemonics, string Key)[] TestVectors =
    [
        (
            [
                "hospital", "stove", "relief", "fringe", "tongue", "always", "charge", "angry", "urge",
                "sentence", "again", "match", "nerve", "inquiry", "senior", "coconut", "label", "tumble",
                "carry", "category", "beauty", "bean", "road", "solution"
            ],
            "9d659a6c2234db7f6e4f977e6e8653b9f5946d557163f31034011375d8f3f97df6c450a16bb1c514e22f1977e390a3025599aa1e7b00068a6aacf2119484c1bd"
        ),
        (
            [
                "dose", "ice", "enrich",
                "trigger", "test", "dove",
                "century", "still", "betray",
                "gas", "diet", "dune",
                "use", "other", "base",
                "gym", "mad", "law",
                "immense", "village", "world",
                "example", "praise", "game"
            ],
            "119dcf2840a3d56521d260b2f125eedc0d4f3795b9e627269a4b5a6dca8257bdc04ad1885c127fe863abb00752fa844e6439bb04f264d70de7cea580b32637ab"
        ),
        (
            [
                "hobby", "coil", "wisdom",
                "mechanic", "fossil", "pretty",
                "enough", "attract", "since",
                "choice", "exhaust", "hazard",
                "kit", "oven", "damp",
                "flip", "hawk", "tribe",
                "spice", "glare", "step",
                "hammer", "apple", "number"
            ],
            "764c63ecdc92b331caf3c5a81c483da8444d4ac87d87af9e3cd36ae207d94e5199ac861b19db16bc0f01adfc6897f4760dfc44f9415284c78689d4fcc28b94f7"
        ),
        (
            [
                "now", "wide", "tag",
                "purity", "diamond", "coin",
                "unit", "rack", "device",
                "replace", "cheap", "deposit",
                "mention", "fence", "elite",
                "elder", "city", "measure",
                "reward", "lion", "chef",
                "promote", "depart", "connect"
            ],
            "2a8a63e0467f1f4148e0be0cc13e922d726f0b1c29272d6743eb83cf5549128f313abf58635fd310310d1debd54f4fe1fd63631ced044ba0af96b67b85eed31b"
        ),
        (
            [
                "clinic", "toward", "wedding",
                "category", "tip", "spin",
                "purity", "absent", "army",
                "gun", "brain", "happy",
                "move", "company", "that",
                "cheap", "tank", "way",
                "shoe", "awkward", "pole",
                "protect", "wear", "crystal"
            ],
            "e5e78a8e1e509da180bc5aeb8af1a37d4311c5110402842925760a4035119362b1f8a0b9b4c2353ddfad8937ed396fb7670e88e8b72128b15006839a2a86be47"
        )
    ];

    [Test]
    public void Test_ShouldGenerateMnemonics()
    {
        string[] mnemonic = Mnemonic.Mnemonic.New();

        Assert.That(mnemonic, Has.Length.EqualTo(24));
    }

    [Test]
    public void Test_ShouldValidateMnemonics()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Mnemonic.Mnemonic.Validate(["a"]), Is.False);
            Assert.That(Mnemonic.Mnemonic.Validate([
                "hospital", "stove", "relief", "fringe", "tongue", "always", "charge", "angry", "urge",
                "sentence", "again", "match", "nerve", "inquiry", "senior", "coconut", "label", "tumble",
                "carry", "category", "beauty", "bean", "road", "solution"
            ]), Is.True);
        });
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void Test_ShouldMatchVector(int vectorIndex)
    {
        (string[] Mnemonics, string Key) vector = TestVectors[vectorIndex];
        KeyPair key = Mnemonic.Mnemonic.ToPrivateKey(vector.Mnemonics);
        KeyPair wk = Mnemonic.Mnemonic.ToWalletKey(vector.Mnemonics);

        Assert.Multiple(() =>
        {
            Assert.That(Convert.ToHexString(key.SecretKey).ToLower(), Is.EqualTo(vector.Key));
            Assert.That(Convert.ToHexString(wk.SecretKey).ToLower(), Is.EqualTo(vector.Key));
        });
    }

    [Test]
    public void Test_ShouldGenerateSameKeysForMnemonicToPrivateKeyAndMnemonicToWalletKey()
    {
        for (int i = 0; i < 10; i++)
        {
            string[] mnemonics = Mnemonic.Mnemonic.New();
            KeyPair key = Mnemonic.Mnemonic.ToPrivateKey(mnemonics);
            KeyPair wk = Mnemonic.Mnemonic.ToWalletKey(mnemonics);

            Assert.Multiple(() =>
            {
                Assert.That(Convert.ToHexString(key.SecretKey).ToLower(),
                    Is.EqualTo(Convert.ToHexString(wk.SecretKey).ToLower()));
                Assert.That(Convert.ToHexString(key.PublicKey).ToLower(),
                    Is.EqualTo(Convert.ToHexString(wk.PublicKey).ToLower()));
            });
        }
    }

    [Test]
    public void Test_ShouldGenerateMnemonicsFromRandomSeed()
    {
        byte[] seed = SecureRandom.GetBytes(32);

        // Should not throw
        string[] mnemonics = Mnemonic.Mnemonic.FromRandomSeed(seed);

        Assert.That(mnemonics, Has.Length.EqualTo(24));
    }
}
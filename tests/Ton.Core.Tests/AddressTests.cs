using Ton.Core.Addresses;

namespace Ton.Core.Tests;

[TestFixture]
public class AddressTests
{
    [Test]
    public void Test_ParseAddressesInVariousForms()
    {
        (bool IsBounceable, bool IsTestOnly, Addresses.Address Address) address1 =
            Address.ParseFriendly("0QAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4-QO");
        (bool IsBounceable, bool IsTestOnly, Address Address) address2 =
            Address.ParseFriendly("kQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi47nL");
        Address address3 = Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3");
        Address address4 = Address.Parse("-1:3333333333333333333333333333333333333333333333333333333333333333");

        Assert.Multiple(() =>
        {
            // Check address1 (non-bounceable, testOnly)
            Assert.That(address1.IsBounceable, Is.False);
            Assert.That(address1.IsTestOnly, Is.True);
            Assert.That(address1.Address.Workchain, Is.EqualTo(0));
            Assert.That(Convert.ToHexString(address1.Address.Hash).ToLower(),
                Is.EqualTo("2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));

            // Check address2 (bounceable, testOnly)
            Assert.That(address2.IsBounceable, Is.True);
            Assert.That(address2.IsTestOnly, Is.True);
            Assert.That(address2.Address.Workchain, Is.EqualTo(0));
            Assert.That(Convert.ToHexString(address2.Address.Hash).ToLower(),
                Is.EqualTo("2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));

            // Check address3 (raw)
            Assert.That(address3.Workchain, Is.EqualTo(0));
            Assert.That(Convert.ToHexString(address3.Hash).ToLower(),
                Is.EqualTo("2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));

            // Check toRawString
            Assert.That(address1.Address.ToRawString(),
                Is.EqualTo("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));
            Assert.That(address2.Address.ToRawString(),
                Is.EqualTo("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));
            Assert.That(address3.ToRawString(),
                Is.EqualTo("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3"));

            // Check address4 (workchain -1)
            Assert.That(address4.Workchain, Is.EqualTo(-1));
            Assert.That(Convert.ToHexString(address4.Hash).ToLower(),
                Is.EqualTo("3333333333333333333333333333333333333333333333333333333333333333"));
        });
    }

    [Test]
    public void Test_SerializeToFriendlyForm()
    {
        Address address = Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3");

        Assert.Multiple(() =>
        {
            // Bounceable
            Assert.That(address.ToString(),
                Is.EqualTo("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB"));
            Assert.That(address.ToString(true, true, true),
                Is.EqualTo("kQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi47nL"));
            Assert.That(address.ToString(false),
                Is.EqualTo("EQAs9VlT6S776tq3unJcP5Ogsj+ELLunLXuOb1EKcOQi4wJB"));
            Assert.That(address.ToString(false, true, true),
                Is.EqualTo("kQAs9VlT6S776tq3unJcP5Ogsj+ELLunLXuOb1EKcOQi47nL"));

            // Non-Bounceable
            Assert.That(address.ToString(true, false),
                Is.EqualTo("UQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi41-E"));
            Assert.That(address.ToString(true, false, true),
                Is.EqualTo("0QAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4-QO"));
            Assert.That(address.ToString(false, false),
                Is.EqualTo("UQAs9VlT6S776tq3unJcP5Ogsj+ELLunLXuOb1EKcOQi41+E"));
            Assert.That(address.ToString(false, false, true),
                Is.EqualTo("0QAs9VlT6S776tq3unJcP5Ogsj+ELLunLXuOb1EKcOQi4+QO"));
        });
    }

    [Test]
    public void Test_ImplementEquals()
    {
        Address address1 = Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3");
        Address address2 = Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3");
        Address address3 = Address.ParseRaw("-1:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e3");
        Address address4 = Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e5");

        Assert.Multiple(() =>
        {
            Assert.That(address1, Is.EqualTo(address2));
            Assert.That(address2, Is.EqualTo(address1));
            Assert.That(address2.Equals(address4), Is.False);
            Assert.That(address2.Equals(address3), Is.False);
            Assert.That(address4.Equals(address3), Is.False);
        });
    }

    [Test]
    public void Test_ThrowIfAddressIsInvalid()
    {
        // Invalid hash length (too short - 31 bytes)
        ArgumentException? ex1 = Assert.Throws<ArgumentException>(() =>
            Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422"));
        Assert.Multiple(() =>
        {
            Assert.That(ex1!.Message, Does.Contain("Invalid address hash length: 31"));
            Assert.That(Address.IsRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422"), Is.False);
        });

        // Invalid hash length (too short - 31 bytes, different)
        ArgumentException? ex2 = Assert.Throws<ArgumentException>(() =>
            Address.ParseRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e"));
        Assert.Multiple(() =>
        {
            Assert.That(ex2!.Message, Does.Contain("Invalid address hash length: 31"));
            Assert.That(Address.IsRaw("0:2cf55953e92efbeadab7ba725c3f93a0b23f842cbba72d7b8e6f510a70e422e"), Is.False);
        });

        // Unknown address type (ton:// prefix)
        ArgumentException? ex3 = Assert.Throws<ArgumentException>(() =>
            Address.Parse("ton://EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJB"));
        Assert.That(ex3!.Message, Does.Contain("Unknown address type"));

        // Invalid friendly address (wrong length)
        ArgumentException? ex4 = Assert.Throws<ArgumentException>(() =>
            Address.Parse("EQAs9VlT6S776tq3unJcP5Ogsj-ELLunLXuOb1EKcOQi4wJ"));
        Assert.That(ex4!.Message, Does.Contain("Unknown address type"));

        // Unknown address type (ton://transfer/ prefix)
        ArgumentException? ex5 = Assert.Throws<ArgumentException>(() =>
            Address.Parse("ton://transfer/EQDXDCFLXgiTrjGSNVBuvKPZVYlPn3J_u96xxLas3_yoRWRk"));
        Assert.That(ex5!.Message, Does.Contain("Unknown address type"));

        // ParseFriendly on ton://transfer/ format
        ArgumentException? ex6 = Assert.Throws<ArgumentException>(() =>
            Address.ParseFriendly("ton://transfer/EQDXDCFLXgiTrjGSNVBuvKPZVYlPn3J_u96xxLas3_yoRWRk"));
        Assert.Multiple(() =>
        {
            Assert.That(ex6!.Message, Does.Contain("Unknown address type"));
            Assert.That(Address.IsFriendly("ton://transfer/EQDXDCFLXgiTrjGSNVBuvKPZVYlPn3J_u96xxLas3_yoRWRk"),
                Is.False);
        });

        // ParseFriendly on raw format (mixed format)
        ArgumentException? ex7 = Assert.Throws<ArgumentException>(() =>
            Address.ParseFriendly("0:EQDXDCFLXgiTrjGSNVBuvKPZVYlPn3J_u96xxLas3_yoRWRk"));
        Assert.Multiple(() =>
        {
            Assert.That(ex7!.Message, Does.Contain("Unknown address type"));
            Assert.That(Address.IsFriendly("0:EQDXDCFLXgiTrjGSNVBuvKPZVYlPn3J_u96xxLas3_yoRWRk"), Is.False);
        });

        // Invalid characters
        ArgumentException? ex8 = Assert.Throws<ArgumentException>(() =>
            Address.ParseFriendly("!@#$%^&*AAAAAAAAAAAAAA AAAAAAAAAA AAAAAAAAAAAA A"));
        Assert.Multiple(() =>
        {
            Assert.That(ex8!.Message, Does.Contain("Unknown address type"));
            Assert.That(Address.IsFriendly("!@#$%^&*AAAAAAAAAAAAAA AAAAAAAAAA AAAAAAAAAAAA A"), Is.False);
        });

        // All spaces
        ArgumentException? ex9 = Assert.Throws<ArgumentException>(() =>
            Address.ParseFriendly("                                                "));
        Assert.Multiple(() =>
        {
            Assert.That(ex9!.Message, Does.Contain("Unknown address type"));
            Assert.That(Address.IsFriendly("                                                "), Is.False);
        });
    }
}
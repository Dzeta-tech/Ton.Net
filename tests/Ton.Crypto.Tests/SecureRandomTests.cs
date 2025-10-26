using Ton.Crypto.Primitives;

namespace Ton.Crypto.Tests;

[TestFixture]
public class SecureRandomTests
{
    [Test]
    public void Test_GetBytes()
    {
        byte[] bytes1 = SecureRandom.GetBytes(32);
        byte[] bytes2 = SecureRandom.GetBytes(32);
        
        Assert.That(bytes1.Length, Is.EqualTo(32));
        Assert.That(bytes2.Length, Is.EqualTo(32));
        
        // Two random byte arrays should be different
        Assert.That(bytes1, Is.Not.EqualTo(bytes2));
    }

    [Test]
    public void Test_GetNumber_InRange()
    {
        int min = 10;
        int max = 20;
        
        // Generate multiple random numbers and check they're all in range
        for (int i = 0; i < 100; i++)
        {
            int num = SecureRandom.GetNumber(min, max);
            Assert.That(num, Is.GreaterThanOrEqualTo(min));
            Assert.That(num, Is.LessThan(max));
        }
    }

    [Test]
    public void Test_GetNumber_Distribution()
    {
        int min = 0;
        int max = 10;
        int[] counts = new int[max - min];
        
        // Generate many random numbers
        for (int i = 0; i < 1000; i++)
        {
            int num = SecureRandom.GetNumber(min, max);
            counts[num - min]++;
        }
        
        // Check that all numbers appeared at least once (statistically very likely)
        foreach (int count in counts)
        {
            Assert.That(count, Is.GreaterThan(0));
        }
    }

    [Test]
    public void Test_GetNumber_InvalidRange()
    {
        Assert.Throws<ArgumentException>(() => SecureRandom.GetNumber(10, 10));
        Assert.Throws<ArgumentException>(() => SecureRandom.GetNumber(10, 5));
    }

    [Test]
    public void Test_GetBytes_DifferentSizes()
    {
        foreach (int size in new[] { 1, 16, 32, 64, 128, 256 })
        {
            byte[] bytes = SecureRandom.GetBytes(size);
            Assert.That(bytes.Length, Is.EqualTo(size));
        }
    }

    [Test]
    public void Test_GetNumber_LargeRange()
    {
        int min = 0;
        int max = 1000000;
        
        // Test a few times with large range
        for (int i = 0; i < 10; i++)
        {
            int num = SecureRandom.GetNumber(min, max);
            Assert.That(num, Is.GreaterThanOrEqualTo(min));
            Assert.That(num, Is.LessThan(max));
        }
    }
}


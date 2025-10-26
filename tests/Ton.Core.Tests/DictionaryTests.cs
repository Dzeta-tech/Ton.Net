using System.Numerics;
using NUnit.Framework;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.Core.Dict;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Tests;

public class DictionaryTests
{
    [Test]
    public void Test_Empty_Dictionary()
    {
        var dict = TonDict.Dictionary<DictKeyUint, ulong>.Empty();
        Assert.That(dict.Size, Is.EqualTo(0));
    }

    [Test]
    public void Test_SetGetHas_Uint_Keys()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        Assert.That(dict.Size, Is.EqualTo(3));
        Assert.That(dict.Get(13), Is.EqualTo(169UL));
        Assert.That(dict.Get(17), Is.EqualTo(289UL));
        Assert.That(dict.Get(239), Is.EqualTo(57121UL));
        Assert.That(dict.Has(13), Is.True);
        Assert.That(dict.Has(999), Is.False);
    }

    [Test]
    public void Test_Keys_Values()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        var keys = dict.Keys();
        Assert.That(keys.Length, Is.EqualTo(3));
        Assert.That(keys, Does.Contain((DictKeyUint)13));
        Assert.That(keys, Does.Contain((DictKeyUint)17));
        Assert.That(keys, Does.Contain((DictKeyUint)239));

        var values = dict.Values();
        Assert.That(values.Length, Is.EqualTo(3));
        Assert.That(values, Does.Contain(169UL));
        Assert.That(values, Does.Contain(289UL));
        Assert.That(values, Does.Contain(57121UL));
    }

    [Test]
    public void Test_Delete()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        Assert.That(dict.Size, Is.EqualTo(2));

        var deleted = dict.Delete(13);
        Assert.That(deleted, Is.True);
        Assert.That(dict.Size, Is.EqualTo(1));
        Assert.That(dict.Has(13), Is.False);

        var deleted2 = dict.Delete(999);
        Assert.That(deleted2, Is.False);
    }

    [Test]
    public void Test_Clear()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        Assert.That(dict.Size, Is.EqualTo(2));

        dict.Clear();
        Assert.That(dict.Size, Is.EqualTo(0));
    }

    [Test]
    public void Test_Serialize_Parse_RoundTrip_Uint()
    {
        // Create dict and populate
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        // Serialize to cell
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        // Parse back
        var parsed = Dict.Dictionary<DictKeyUint, ulong>.LoadDirect(
            DictionaryKeys.Uint(16),
            DictionaryValues.Uint(16),
            cell.BeginParse()
        );

        // Verify
        Assert.That(parsed.Size, Is.EqualTo(3));
        Assert.That(parsed.Get(13), Is.EqualTo(169UL));
        Assert.That(parsed.Get(17), Is.EqualTo(289UL));
        Assert.That(parsed.Get(239), Is.EqualTo(57121UL));
    }

    [Test]
    public void Test_Int_Keys()
    {
        var dict = Dict.Dictionary<DictKeyInt, long>.Empty(DictionaryKeys.Int(32), DictionaryValues.Int(32));

        dict.Set(-100, -1000);
        dict.Set(0, 0);
        dict.Set(100, 1000);

        Assert.That(dict.Get(-100), Is.EqualTo(-1000L));
        Assert.That(dict.Get(0), Is.EqualTo(0L));
        Assert.That(dict.Get(100), Is.EqualTo(1000L));

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyInt, long>.LoadDirect(
            DictionaryKeys.Int(32),
            DictionaryValues.Int(32),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(-100), Is.EqualTo(-1000L));
        Assert.That(parsed.Get(0), Is.EqualTo(0L));
        Assert.That(parsed.Get(100), Is.EqualTo(1000L));
    }

    [Test]
    public void Test_BigInt_Keys()
    {
        var dict = Dict.Dictionary<DictKeyBigInt, BigInteger>.Empty(
            DictionaryKeys.BigInt(256),
            DictionaryValues.BigInt(256)
        );

        var key1 = BigInteger.Parse("123456789012345678901234567890");
        var val1 = BigInteger.Parse("987654321098765432109876543210");

        dict.Set(key1, val1);
        Assert.That(dict.Get(key1), Is.EqualTo(val1));

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyBigInt, BigInteger>.LoadDirect(
            DictionaryKeys.BigInt(256),
            DictionaryValues.BigInt(256),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(key1), Is.EqualTo(val1));
    }

    [Test]
    public void Test_Address_Keys()
    {
        var dict = Dict.Dictionary<DictKeyAddress, ulong>.Empty(
            DictionaryKeys.Address(),
            DictionaryValues.Uint(64)
        );

        var addr1 = Address.Parse("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y");
        var addr2 = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");

        dict.Set(addr1, 100);
        dict.Set(addr2, 200);

        Assert.That(dict.Get(addr1), Is.EqualTo(100UL));
        Assert.That(dict.Get(addr2), Is.EqualTo(200UL));

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyAddress, ulong>.LoadDirect(
            DictionaryKeys.Address(),
            DictionaryValues.Uint(64),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(addr1), Is.EqualTo(100UL));
        Assert.That(parsed.Get(addr2), Is.EqualTo(200UL));
    }

    [Test]
    public void Test_Buffer_Keys()
    {
        var dict = Dict.Dictionary<DictKeyBuffer, ulong>.Empty(
            DictionaryKeys.Buffer(32),
            DictionaryValues.Uint(64)
        );

        var key1 = new byte[32];
        for (int i = 0; i < 32; i++) key1[i] = (byte)i;

        var key2 = new byte[32];
        for (int i = 0; i < 32; i++) key2[i] = (byte)(31 - i);

        dict.Set(key1, 111);
        dict.Set(key2, 222);

        Assert.That(dict.Size, Is.EqualTo(2));

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyBuffer, ulong>.LoadDirect(
            DictionaryKeys.Buffer(32),
            DictionaryValues.Uint(64),
            cell.BeginParse()
        );

        Assert.That(parsed.Size, Is.EqualTo(2));
    }

    [Test]
    public void Test_Bool_Values()
    {
        var dict = Dict.Dictionary<DictKeyUint, bool>.Empty(
            DictionaryKeys.Uint(8),
            DictionaryValues.Bool()
        );

        dict.Set(1, true);
        dict.Set(2, false);
        dict.Set(3, true);

        Assert.That(dict.Get(1), Is.True);
        Assert.That(dict.Get(2), Is.False);
        Assert.That(dict.Get(3), Is.True);

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyUint, bool>.LoadDirect(
            DictionaryKeys.Uint(8),
            DictionaryValues.Bool(),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(1), Is.True);
        Assert.That(parsed.Get(2), Is.False);
        Assert.That(parsed.Get(3), Is.True);
    }

    [Test]
    public void Test_Cell_Values()
    {
        var dict = Dict.Dictionary<DictKeyUint, Cell>.Empty(
            DictionaryKeys.Uint(16),
            DictionaryValues.Cell()
        );

        var cell1 = Builder.BeginCell().StoreUint(123, 32).EndCell();
        var cell2 = Builder.BeginCell().StoreUint(456, 32).EndCell();

        dict.Set(1, cell1);
        dict.Set(2, cell2);

        Assert.That(dict.Get(1)!.Hash().SequenceEqual(cell1.Hash()), Is.True);
        Assert.That(dict.Get(2)!.Hash().SequenceEqual(cell2.Hash()), Is.True);

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyUint, Cell>.LoadDirect(
            DictionaryKeys.Uint(16),
            DictionaryValues.Cell(),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(1)!.Hash().SequenceEqual(cell1.Hash()), Is.True);
        Assert.That(parsed.Get(2)!.Hash().SequenceEqual(cell2.Hash()), Is.True);
    }

    [Test]
    public void Test_VarUint_Values()
    {
        var dict = Dict.Dictionary<DictKeyUint, BigInteger>.Empty(
            DictionaryKeys.Uint(16),
            DictionaryValues.BigVarUint(4)
        );

        dict.Set(1, BigInteger.Parse("100"));
        dict.Set(2, BigInteger.Parse("1000000"));

        Assert.That(dict.Get(1), Is.EqualTo(BigInteger.Parse("100")));
        Assert.That(dict.Get(2), Is.EqualTo(BigInteger.Parse("1000000")));

        // Round trip
        var builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        var cell = builder.EndCell();

        var parsed = Dict.Dictionary<DictKeyUint, BigInteger>.LoadDirect(
            DictionaryKeys.Uint(16),
            DictionaryValues.BigVarUint(4),
            cell.BeginParse()
        );

        Assert.That(parsed.Get(1), Is.EqualTo(BigInteger.Parse("100")));
        Assert.That(parsed.Get(2), Is.EqualTo(BigInteger.Parse("1000000")));
    }

    [Test]
    public void Test_Store_Load_With_Ref()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);

        // Store with ref (using StoreDict)
        var builder = Builder.BeginCell();
        builder.StoreDict(dict);
        var cell = builder.EndCell();

        // Load with ref (using LoadDict)
        var parsed = cell.BeginParse().LoadDict(
            DictionaryKeys.Uint(16),
            DictionaryValues.Uint(16)
        );

        Assert.That(parsed.Get(13), Is.EqualTo(169UL));
        Assert.That(parsed.Get(17), Is.EqualTo(289UL));
    }

    [Test]
    public void Test_Empty_Dictionary_Serialization()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));

        // Store with ref
        var builder = Builder.BeginCell();
        builder.StoreDict(dict);
        var cell = builder.EndCell();

        // Should store as null ref (single 0 bit)
        var slice = cell.BeginParse();
        Assert.That(slice.RemainingBits, Is.EqualTo(1));
        Assert.That(slice.LoadBit(), Is.False);
    }

    [Test]
    public void Test_Enumeration()
    {
        var dict = Dict.Dictionary<DictKeyUint, ulong>.Empty(DictionaryKeys.Uint(16), DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        var items = new List<KeyValuePair<DictKeyUint, ulong>>();
        foreach (var item in dict)
        {
            items.Add(item);
        }

        Assert.That(items.Count, Is.EqualTo(3));
        Assert.That(items.Any(kv => kv.Key == 13 && kv.Value == 169), Is.True);
        Assert.That(items.Any(kv => kv.Key == 17 && kv.Value == 289), Is.True);
        Assert.That(items.Any(kv => kv.Key == 239 && kv.Value == 57121), Is.True);
    }
}


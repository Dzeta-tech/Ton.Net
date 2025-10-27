using System.Numerics;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using TonDict = Ton.Core.Dict;

namespace Ton.Core.Tests;

public class DictionaryTests
{
    [Test]
    public void Test_Empty_Dictionary()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty();
        Assert.That(dict.Size, Is.EqualTo(0));
    }

    [Test]
    public void Test_SetGetHas_Uint_Keys()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        Assert.Multiple(() =>
        {
            Assert.That(dict.Size, Is.EqualTo(3));
            Assert.That(dict.Get(13), Is.EqualTo(169UL));
            Assert.That(dict.Get(17), Is.EqualTo(289UL));
            Assert.That(dict.Get(239), Is.EqualTo(57121UL));
            Assert.That(dict.Has(13), Is.True);
            Assert.That(dict.Has(999), Is.False);
        });
    }

    [Test]
    public void Test_Keys_Values()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        TonDict.DictKeyUint[] keys = dict.Keys();
        Assert.That(keys, Has.Length.EqualTo(3));
        Assert.That(keys, Does.Contain((TonDict.DictKeyUint)13));
        Assert.That(keys, Does.Contain((TonDict.DictKeyUint)17));
        Assert.That(keys, Does.Contain((TonDict.DictKeyUint)239));

        ulong[] values = dict.Values();
        Assert.That(values, Has.Length.EqualTo(3));
        Assert.That(values, Does.Contain(169UL));
        Assert.That(values, Does.Contain(289UL));
        Assert.That(values, Does.Contain(57121UL));
    }

    [Test]
    public void Test_Delete()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));

        dict.Set(13, 169);
        dict.Set(17, 289);
        Assert.That(dict.Size, Is.EqualTo(2));

        bool deleted = dict.Delete(13);
        Assert.Multiple(() =>
        {
            Assert.That(deleted, Is.True);
            Assert.That(dict.Size, Is.EqualTo(1));
            Assert.That(dict.Has(13), Is.False);
        });

        bool deleted2 = dict.Delete(999);
        Assert.That(deleted2, Is.False);
    }

    [Test]
    public void Test_Clear()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));

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
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        // Serialize to cell
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        // Parse back
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> parsed =
            TonDict.Dictionary<TonDict.DictKeyUint, ulong>.LoadDirect(
                TonDict.DictionaryKeys.Uint(16),
                TonDict.DictionaryValues.Uint(16),
                cell.BeginParse()
            );

        Assert.Multiple(() =>
        {
            // Verify
            Assert.That(parsed.Size, Is.EqualTo(3));
            Assert.That(parsed.Get(13), Is.EqualTo(169UL));
            Assert.That(parsed.Get(17), Is.EqualTo(289UL));
            Assert.That(parsed.Get(239), Is.EqualTo(57121UL));
        });
    }

    [Test]
    public void Test_Int_Keys()
    {
        TonDict.Dictionary<TonDict.DictKeyInt, long> dict = TonDict.Dictionary<TonDict.DictKeyInt, long>.Empty(
            TonDict.DictionaryKeys.Int(32),
            TonDict.DictionaryValues.Int(32));

        dict.Set(-100, -1000);
        dict.Set(0, 0);
        dict.Set(100, 1000);

        Assert.Multiple(() =>
        {
            Assert.That(dict.Get(-100), Is.EqualTo(-1000L));
            Assert.That(dict.Get(0), Is.EqualTo(0L));
            Assert.That(dict.Get(100), Is.EqualTo(1000L));
        });

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyInt, long> parsed = TonDict.Dictionary<TonDict.DictKeyInt, long>.LoadDirect(
            TonDict.DictionaryKeys.Int(32),
            TonDict.DictionaryValues.Int(32),
            cell.BeginParse()
        );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(-100), Is.EqualTo(-1000L));
            Assert.That(parsed.Get(0), Is.EqualTo(0L));
            Assert.That(parsed.Get(100), Is.EqualTo(1000L));
        });
    }

    [Test]
    public void Test_BigInt_Keys()
    {
        TonDict.Dictionary<TonDict.DictKeyBigInt, BigInteger> dict =
            TonDict.Dictionary<TonDict.DictKeyBigInt, BigInteger>.Empty(
                TonDict.DictionaryKeys.BigInt(256),
                TonDict.DictionaryValues.BigInt(256)
            );

        BigInteger key1 = BigInteger.Parse("123456789012345678901234567890");
        BigInteger val1 = BigInteger.Parse("987654321098765432109876543210");

        dict.Set(key1, val1);
        Assert.That(dict.Get(key1), Is.EqualTo(val1));

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyBigInt, BigInteger> parsed =
            TonDict.Dictionary<TonDict.DictKeyBigInt, BigInteger>.LoadDirect(
                TonDict.DictionaryKeys.BigInt(256),
                TonDict.DictionaryValues.BigInt(256),
                cell.BeginParse()
            );

        Assert.That(parsed.Get(key1), Is.EqualTo(val1));
    }

    [Test]
    public void Test_Address_Keys()
    {
        TonDict.Dictionary<TonDict.DictKeyAddress, ulong> dict =
            TonDict.Dictionary<TonDict.DictKeyAddress, ulong>.Empty(
                TonDict.DictionaryKeys.Address(),
                TonDict.DictionaryValues.Uint(64)
            );

        Address addr1 = Address.Parse("EQAvDfWFG0oYX19jwNDNBBL1rKNT9XfaGP9HyTb5nb2Eml6y");
        Address addr2 = Address.Parse("EQCD39VS5jcptHL8vMjEXrzGaRcCVYto7HUn4bpAOg8xqB2N");

        dict.Set(addr1, 100);
        dict.Set(addr2, 200);

        Assert.Multiple(() =>
        {
            Assert.That(dict.Get(addr1), Is.EqualTo(100UL));
            Assert.That(dict.Get(addr2), Is.EqualTo(200UL));
        });

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyAddress, ulong> parsed =
            TonDict.Dictionary<TonDict.DictKeyAddress, ulong>.LoadDirect(
                TonDict.DictionaryKeys.Address(),
                TonDict.DictionaryValues.Uint(64),
                cell.BeginParse()
            );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(addr1), Is.EqualTo(100UL));
            Assert.That(parsed.Get(addr2), Is.EqualTo(200UL));
        });
    }

    [Test]
    public void Test_Buffer_Keys()
    {
        TonDict.Dictionary<TonDict.DictKeyBuffer, ulong> dict = TonDict.Dictionary<TonDict.DictKeyBuffer, ulong>.Empty(
            TonDict.DictionaryKeys.Buffer(32),
            TonDict.DictionaryValues.Uint(64)
        );

        byte[] key1 = new byte[32];
        for (int i = 0; i < 32; i++) key1[i] = (byte)i;

        byte[] key2 = new byte[32];
        for (int i = 0; i < 32; i++) key2[i] = (byte)(31 - i);

        dict.Set(key1, 111);
        dict.Set(key2, 222);

        Assert.That(dict.Size, Is.EqualTo(2));

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyBuffer, ulong> parsed =
            TonDict.Dictionary<TonDict.DictKeyBuffer, ulong>.LoadDirect(
                TonDict.DictionaryKeys.Buffer(32),
                TonDict.DictionaryValues.Uint(64),
                cell.BeginParse()
            );

        Assert.That(parsed.Size, Is.EqualTo(2));
    }

    [Test]
    public void Test_Bool_Values()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, bool> dict = TonDict.Dictionary<TonDict.DictKeyUint, bool>.Empty(
            TonDict.DictionaryKeys.Uint(8),
            TonDict.DictionaryValues.Bool()
        );

        dict.Set(1, true);
        dict.Set(2, false);
        dict.Set(3, true);

        Assert.Multiple(() =>
        {
            Assert.That(dict.Get(1), Is.True);
            Assert.That(dict.Get(2), Is.False);
            Assert.That(dict.Get(3), Is.True);
        });

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyUint, bool> parsed = TonDict.Dictionary<TonDict.DictKeyUint, bool>.LoadDirect(
            TonDict.DictionaryKeys.Uint(8),
            TonDict.DictionaryValues.Bool(),
            cell.BeginParse()
        );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(1), Is.True);
            Assert.That(parsed.Get(2), Is.False);
            Assert.That(parsed.Get(3), Is.True);
        });
    }

    [Test]
    public void Test_Cell_Values()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, Cell> dict = TonDict.Dictionary<TonDict.DictKeyUint, Cell>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Cell()
        );

        Cell cell1 = Builder.BeginCell().StoreUint(123, 32).EndCell();
        Cell cell2 = Builder.BeginCell().StoreUint(456, 32).EndCell();

        dict.Set(1, cell1);
        dict.Set(2, cell2);

        Assert.Multiple(() =>
        {
            Assert.That(dict.Get(1)!.Hash().SequenceEqual(cell1.Hash()), Is.True);
            Assert.That(dict.Get(2)!.Hash().SequenceEqual(cell2.Hash()), Is.True);
        });

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyUint, Cell> parsed = TonDict.Dictionary<TonDict.DictKeyUint, Cell>.LoadDirect(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Cell(),
            cell.BeginParse()
        );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(1)!.Hash().SequenceEqual(cell1.Hash()), Is.True);
            Assert.That(parsed.Get(2)!.Hash().SequenceEqual(cell2.Hash()), Is.True);
        });
    }

    [Test]
    public void Test_VarUint_Values()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> dict =
            TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.Empty(
                TonDict.DictionaryKeys.Uint(16),
                TonDict.DictionaryValues.BigVarUint(4)
            );

        dict.Set(1, BigInteger.Parse("100"));
        dict.Set(2, BigInteger.Parse("1000000"));

        Assert.Multiple(() =>
        {
            Assert.That(dict.Get(1), Is.EqualTo(BigInteger.Parse("100")));
            Assert.That(dict.Get(2), Is.EqualTo(BigInteger.Parse("1000000")));
        });

        // Round trip
        Builder builder = Builder.BeginCell();
        builder.StoreDictDirect(dict);
        Cell cell = builder.EndCell();

        TonDict.Dictionary<TonDict.DictKeyUint, BigInteger> parsed =
            TonDict.Dictionary<TonDict.DictKeyUint, BigInteger>.LoadDirect(
                TonDict.DictionaryKeys.Uint(16),
                TonDict.DictionaryValues.BigVarUint(4),
                cell.BeginParse()
            );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(1), Is.EqualTo(BigInteger.Parse("100")));
            Assert.That(parsed.Get(2), Is.EqualTo(BigInteger.Parse("1000000")));
        });
    }

    [Test]
    public void Test_Store_Load_With_Ref()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);

        // Store with ref (using StoreDict)
        Builder builder = Builder.BeginCell();
        builder.StoreDict(dict);
        Cell cell = builder.EndCell();

        // Load with ref (using LoadDict)
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> parsed = cell.BeginParse().LoadDict(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16)
        );

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Get(13), Is.EqualTo(169UL));
            Assert.That(parsed.Get(17), Is.EqualTo(289UL));
        });
    }

    [Test]
    public void Test_Empty_Dictionary_Serialization()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));

        // Store with ref
        Builder builder = Builder.BeginCell();
        builder.StoreDict(dict);
        Cell cell = builder.EndCell();

        // Should store as null ref (single 0 bit)
        Slice slice = cell.BeginParse();
        Assert.Multiple(() =>
        {
            Assert.That(slice.RemainingBits, Is.EqualTo(1));
            Assert.That(slice.LoadBit(), Is.False);
        });
    }

    [Test]
    public void Test_Enumeration()
    {
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16));
        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        List<KeyValuePair<TonDict.DictKeyUint, ulong>> items = [];
        foreach (KeyValuePair<TonDict.DictKeyUint, ulong> item in dict) items.Add(item);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(items.Any(kv => kv.Key == 13 && kv.Value == 169), Is.True);
            Assert.That(items.Any(kv => kv.Key == 17 && kv.Value == 289), Is.True);
            Assert.That(items.Any(kv => kv.Key == 239 && kv.Value == 57121), Is.True);
        });
    }

    [Test]
    public void Test_Dictionary_BOC_Serialization_Simple()
    {
        // Create dictionary with uint keys/values
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(8),
            TonDict.DictionaryValues.Uint(32)
        );

        dict.Set(1, 11);
        dict.Set(2, 22);
        dict.Set(3, 33);

        // Serialize to BOC
        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.Uint(8), TonDict.DictionaryValues.Uint(32));

        Assert.Multiple(() =>
        {
            Assert.That(restoredDict.Get(1), Is.EqualTo(11UL));
            Assert.That(restoredDict.Get(2), Is.EqualTo(22UL));
            Assert.That(restoredDict.Get(3), Is.EqualTo(33UL));
            Assert.That(restoredDict.Keys().Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public void Test_Dictionary_BOC_RoundTrip_Documentation_Example()
    {
        // This matches the JS SDK test "should parse and serialize dict from example"
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Uint(16)
        );

        dict.Set(13, 169);
        dict.Set(17, 289);
        dict.Set(239, 57121);

        // Serialize
        Cell packed = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = packed.ToBoc(false, false);

        // Deserialize
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.Uint(16), TonDict.DictionaryValues.Uint(16));

        // Verify all values
        Assert.Multiple(() =>
        {
            Assert.That(restoredDict.Get(13), Is.EqualTo(169UL));
            Assert.That(restoredDict.Get(17), Is.EqualTo(289UL));
            Assert.That(restoredDict.Get(239), Is.EqualTo(57121UL));
            Assert.That(restoredDict.Keys().Count(), Is.EqualTo(3));
        });

        // Verify structure matches original
        Assert.That(restored[0].Hash().SequenceEqual(packed.Hash()), Is.True);
    }

    [Test]
    public void Test_Dictionary_BOC_WithAddress()
    {
        Address addr1 = new(0, new byte[32]);
        Address addr2 = new(-1, new byte[32]);

        TonDict.Dictionary<TonDict.DictKeyUint, Address> dict = TonDict.Dictionary<TonDict.DictKeyUint, Address>.Empty(
            TonDict.DictionaryKeys.Uint(16),
            TonDict.DictionaryValues.Address()
        );

        dict.Set(1, addr1);
        dict.Set(2, addr2);

        // Serialize to BOC
        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyUint, Address> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.Uint(16), TonDict.DictionaryValues.Address());

        Assert.Multiple(() =>
        {
            Assert.That(restoredDict.Get(1)?.ToString(), Is.EqualTo(addr1.ToString()));
            Assert.That(restoredDict.Get(2)?.ToString(), Is.EqualTo(addr2.ToString()));
        });
    }

    [Test]
    public void Test_Dictionary_BOC_WithCell()
    {
        Cell value1 = Builder.BeginCell().StoreUint(111, 32).EndCell();
        Cell value2 = Builder.BeginCell().StoreUint(222, 32).EndCell();

        TonDict.Dictionary<TonDict.DictKeyUint, Cell> dict = TonDict.Dictionary<TonDict.DictKeyUint, Cell>.Empty(
            TonDict.DictionaryKeys.Uint(8),
            TonDict.DictionaryValues.Cell()
        );

        dict.Set(1, value1);
        dict.Set(2, value2);

        // Serialize to BOC
        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyUint, Cell> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.Uint(8), TonDict.DictionaryValues.Cell());

        Assert.Multiple(() =>
        {
            Assert.That(restoredDict.Get(1)?.Hash().SequenceEqual(value1.Hash()), Is.True);
            Assert.That(restoredDict.Get(2)?.Hash().SequenceEqual(value2.Hash()), Is.True);
        });
    }

    [Test]
    public void Test_Dictionary_BOC_EmptyValue()
    {
        // Test dictionary with BitString(0) values (empty values)
        TonDict.Dictionary<TonDict.DictKeyBigInt, BitString> dict =
            TonDict.Dictionary<TonDict.DictKeyBigInt, BitString>.Empty(
                TonDict.DictionaryKeys.BigUint(256),
                TonDict.DictionaryValues.BitString(0)
            );

        dict.Set(BigInteger.Parse("123"), BitString.Empty);

        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyBigInt, BitString> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.BigUint(256), TonDict.DictionaryValues.BitString(0));

        Assert.Multiple(() =>
        {
            Assert.That(restoredDict.Keys().First().Value, Is.EqualTo(BigInteger.Parse("123")));
            Assert.That(restoredDict.Get(BigInteger.Parse("123"))?.Length, Is.EqualTo(0));
        });
    }

    [Test]
    public void Test_Dictionary_BOC_BitStringKeys()
    {
        // Test non-byte-aligned BitString keys (9 bits)
        BitString key = new([0x54, 0x65, 0x73, 0x74], 0, 9); // "Test" but only 9 bits
        BitString value = new([0x42, 0x69, 0x74, 0x53, 0x74, 0x72, 0x69, 0x6E, 0x67], 0,
            72); // "BitString"

        TonDict.Dictionary<TonDict.DictKeyBitString, BitString> dict =
            TonDict.Dictionary<TonDict.DictKeyBitString, BitString>.Empty(
                TonDict.DictionaryKeys.BitString(9),
                TonDict.DictionaryValues.BitString(72)
            );

        dict.Set(key, value);

        // Serialize
        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyBitString, BitString> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.BitString(9), TonDict.DictionaryValues.BitString(72));

        BitString? retrievedValue = restoredDict.Get(key);
        Assert.That(retrievedValue, Is.Not.Null);
        Assert.That(retrievedValue!.ToString(), Is.EqualTo(value.ToString()));
    }

    [Test]
    public void Test_Dictionary_BOC_LargeDictionary()
    {
        // Test with more entries
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> dict = TonDict.Dictionary<TonDict.DictKeyUint, ulong>.Empty(
            TonDict.DictionaryKeys.Uint(32),
            TonDict.DictionaryValues.Uint(32)
        );

        for (uint i = 0; i < 20; i++)
            dict.Set(i, i * 100);

        // Serialize
        Cell cell = Builder.BeginCell()
            .StoreDictDirect(dict)
            .EndCell();

        byte[] boc = cell.ToBoc(false, false);

        // Deserialize and verify all values
        Cell[] restored = Cell.FromBoc(boc);
        TonDict.Dictionary<TonDict.DictKeyUint, ulong> restoredDict = restored[0].BeginParse()
            .LoadDictDirect(TonDict.DictionaryKeys.Uint(32), TonDict.DictionaryValues.Uint(32));

        Assert.That(restoredDict.Keys().Count(), Is.EqualTo(20));
        for (uint i = 0; i < 20; i++)
            Assert.That(restoredDict.Get(i), Is.EqualTo(i * 100));
    }
}
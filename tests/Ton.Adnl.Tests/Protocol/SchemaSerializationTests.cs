using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;

namespace Ton.Adnl.Tests.Protocol;

public class SchemaSerializationTests
{
    [Fact]
    public void TonNodeBlockId_ShouldSerializeAndDeserialize()
    {
        TonNodeBlockId original = new()
        {
            Workchain = -1,
            Shard = -9223372036854775808,
            Seqno = 12345
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        TonNodeBlockId deserialized = TonNodeBlockId.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Shard, deserialized.Shard);
        Assert.Equal(original.Seqno, deserialized.Seqno);
    }

    [Fact]
    public void TonNodeBlockIdExt_ShouldSerializeAndDeserialize()
    {
        byte[] rootHash = AdnlKeys.GenerateRandomBytes(32);
        byte[] fileHash = AdnlKeys.GenerateRandomBytes(32);

        TonNodeBlockIdExt original = new()
        {
            Workchain = 0,
            Shard = 1000000000,
            Seqno = 54321,
            RootHash = rootHash,
            FileHash = fileHash
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        TonNodeBlockIdExt deserialized = TonNodeBlockIdExt.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Shard, deserialized.Shard);
        Assert.Equal(original.Seqno, deserialized.Seqno);
        Assert.Equal(original.RootHash, deserialized.RootHash);
        Assert.Equal(original.FileHash, deserialized.FileHash);
    }

    [Fact]
    public void TonNodeZeroStateIdExt_ShouldSerializeAndDeserialize()
    {
        byte[] rootHash = AdnlKeys.GenerateRandomBytes(32);
        byte[] fileHash = AdnlKeys.GenerateRandomBytes(32);

        TonNodeZeroStateIdExt original = new()
        {
            Workchain = -1,
            RootHash = rootHash,
            FileHash = fileHash
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        TonNodeZeroStateIdExt deserialized = TonNodeZeroStateIdExt.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.RootHash, deserialized.RootHash);
        Assert.Equal(original.FileHash, deserialized.FileHash);
    }

    [Fact]
    public void LiteServerAccountId_ShouldSerializeAndDeserialize()
    {
        byte[] accountId = AdnlKeys.GenerateRandomBytes(32);

        LiteServerAccountId original = new()
        {
            Workchain = 0,
            Id = accountId
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerAccountId? deserialized = LiteServerAccountId.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Id, deserialized.Id);
    }

    [Fact]
    public void LiteServerMasterchainInfo_ShouldSerializeAndDeserialize()
    {
        TonNodeBlockIdExt last = new()
        {
            Workchain = -1,
            Shard = -9223372036854775808,
            Seqno = 100,
            RootHash = AdnlKeys.GenerateRandomBytes(32),
            FileHash = AdnlKeys.GenerateRandomBytes(32)
        };

        TonNodeZeroStateIdExt init = new()
        {
            Workchain = -1,
            RootHash = AdnlKeys.GenerateRandomBytes(32),
            FileHash = AdnlKeys.GenerateRandomBytes(32)
        };

        LiteServerMasterchainInfo original = new()
        {
            Last = last,
            StateRootHash = AdnlKeys.GenerateRandomBytes(32),
            Init = init
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerMasterchainInfo? deserialized = LiteServerMasterchainInfo.ReadFrom(reader);

        Assert.Equal(original.Last.Seqno, deserialized.Last.Seqno);
        Assert.Equal(original.StateRootHash, deserialized.StateRootHash);
        Assert.Equal(original.Init.Workchain, deserialized.Init.Workchain);
    }

    [Fact]
    public void LiteServerMasterchainInfoExt_ShouldSerializeAndDeserialize()
    {
        TonNodeBlockIdExt last = new()
        {
            Workchain = -1,
            Shard = -9223372036854775808,
            Seqno = 200,
            RootHash = AdnlKeys.GenerateRandomBytes(32),
            FileHash = AdnlKeys.GenerateRandomBytes(32)
        };

        TonNodeZeroStateIdExt init = new()
        {
            Workchain = -1,
            RootHash = AdnlKeys.GenerateRandomBytes(32),
            FileHash = AdnlKeys.GenerateRandomBytes(32)
        };

        LiteServerMasterchainInfoExt original = new()
        {
            Mode = 0,
            Version = 1,
            Capabilities = 123456789L,
            Last = last,
            LastUtime = 1234567890,
            Now = 1234567900,
            StateRootHash = AdnlKeys.GenerateRandomBytes(32),
            Init = init
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerMasterchainInfoExt? deserialized = LiteServerMasterchainInfoExt.ReadFrom(reader);

        Assert.Equal(original.Mode, deserialized.Mode);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Capabilities, deserialized.Capabilities);
        Assert.Equal(original.LastUtime, deserialized.LastUtime);
        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerCurrentTime_ShouldSerializeAndDeserialize()
    {
        LiteServerCurrentTime original = new()
        {
            Now = 1234567890
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerCurrentTime? deserialized = LiteServerCurrentTime.ReadFrom(reader);

        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerVersion_ShouldSerializeAndDeserialize()
    {
        LiteServerVersion original = new()
        {
            Mode = 1,
            Version = 2,
            Capabilities = 999L,
            Now = 1234567890
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerVersion? deserialized = LiteServerVersion.ReadFrom(reader);

        Assert.Equal(original.Mode, deserialized.Mode);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Capabilities, deserialized.Capabilities);
        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerBlockData_ShouldSerializeAndDeserialize()
    {
        TonNodeBlockIdExt blockId = new()
        {
            Workchain = 0,
            Shard = 1000,
            Seqno = 100,
            RootHash = AdnlKeys.GenerateRandomBytes(32),
            FileHash = AdnlKeys.GenerateRandomBytes(32)
        };

        byte[] data = AdnlKeys.GenerateRandomBytes(256);

        LiteServerBlockData original = new()
        {
            Id = blockId,
            Data = data
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerBlockData? deserialized = LiteServerBlockData.ReadFrom(reader);

        Assert.Equal(original.Id.Seqno, deserialized.Id.Seqno);
        Assert.Equal(original.Data, deserialized.Data);
    }

    [Fact]
    public void LiteServerError_ShouldSerializeAndDeserialize()
    {
        LiteServerError original = new()
        {
            Code = 404,
            Message = "Not found"
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerError? deserialized = LiteServerError.ReadFrom(reader);

        Assert.Equal(original.Code, deserialized.Code);
        Assert.Equal(original.Message, deserialized.Message);
    }

    [Fact]
    public void LiteServerTransactionId3_ShouldSerializeAndDeserialize()
    {
        LiteServerTransactionId3 original = new()
        {
            Account = AdnlKeys.GenerateRandomBytes(32),
            Lt = 123456789L
        };

        TLWriteBuffer writer = new();
        original.WriteTo(writer);
        byte[] bytes = writer.Build();

        TLReadBuffer reader = new(bytes);
        LiteServerTransactionId3? deserialized = LiteServerTransactionId3.ReadFrom(reader);

        Assert.Equal(original.Account, deserialized.Account);
        Assert.Equal(original.Lt, deserialized.Lt);
    }

    [Fact]
    public void Functions_ShouldHaveCorrectConstructorIds()
    {
        // Verify a few known function constructor IDs (computed via CRC32 of TL schema definitions)
        Assert.Equal(0x89B5E62Eu, Functions.GetMasterchainInfo);
        Assert.Equal(0x70A671DFu, Functions.GetMasterchainInfoExt);
        Assert.Equal(0x16AD5A34u, Functions.GetTime);
        Assert.Equal(0x232B940Bu, Functions.GetVersion);
    }

    [Fact]
    public void StructTypes_ShouldBeValueTypes()
    {
        // Verify that basic types are structs (value types)
        Assert.True(typeof(TonNodeBlockId).IsValueType);
        Assert.True(typeof(TonNodeBlockIdExt).IsValueType);
        Assert.True(typeof(TonNodeZeroStateIdExt).IsValueType);
    }

    [Fact]
    public void ClassTypes_ShouldBeReferenceTypes()
    {
        // Verify that liteServer types are classes (reference types)
        Assert.False(typeof(LiteServerMasterchainInfo).IsValueType);
        Assert.False(typeof(LiteServerError).IsValueType);
        Assert.False(typeof(LiteServerAccountId).IsValueType);
    }
}
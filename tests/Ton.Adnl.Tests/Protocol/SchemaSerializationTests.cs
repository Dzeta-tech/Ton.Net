using Ton.Adnl.Crypto;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;
using Xunit;

namespace Ton.Adnl.Tests.Protocol;

public class SchemaSerializationTests
{
    [Fact]
    public void TonNodeBlockId_ShouldSerializeAndDeserialize()
    {
        var original = new TonNodeBlockId(
            workchain: -1,
            shard: -9223372036854775808,
            seqno: 12345
        );

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = TonNodeBlockId.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Shard, deserialized.Shard);
        Assert.Equal(original.Seqno, deserialized.Seqno);
    }

    [Fact]
    public void TonNodeBlockIdExt_ShouldSerializeAndDeserialize()
    {
        var rootHash = AdnlKeys.GenerateRandomBytes(32);
        var fileHash = AdnlKeys.GenerateRandomBytes(32);

        var original = new TonNodeBlockIdExt(
            workchain: 0,
            shard: 1000000000,
            seqno: 54321,
            rootHash: rootHash,
            fileHash: fileHash
        );

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = TonNodeBlockIdExt.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Shard, deserialized.Shard);
        Assert.Equal(original.Seqno, deserialized.Seqno);
        Assert.Equal(original.RootHash, deserialized.RootHash);
        Assert.Equal(original.FileHash, deserialized.FileHash);
    }

    [Fact]
    public void TonNodeZeroStateIdExt_ShouldSerializeAndDeserialize()
    {
        var rootHash = AdnlKeys.GenerateRandomBytes(32);
        var fileHash = AdnlKeys.GenerateRandomBytes(32);

        var original = new TonNodeZeroStateIdExt(
            workchain: -1,
            rootHash: rootHash,
            fileHash: fileHash
        );

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = TonNodeZeroStateIdExt.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.RootHash, deserialized.RootHash);
        Assert.Equal(original.FileHash, deserialized.FileHash);
    }

    [Fact]
    public void LiteServerAccountId_ShouldSerializeAndDeserialize()
    {
        var accountId = AdnlKeys.GenerateRandomBytes(32);

        var original = new LiteServerAccountId
        {
            Workchain = 0,
            Id = accountId
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerAccountId.ReadFrom(reader);

        Assert.Equal(original.Workchain, deserialized.Workchain);
        Assert.Equal(original.Id, deserialized.Id);
    }

    [Fact]
    public void LiteServerMasterchainInfo_ShouldSerializeAndDeserialize()
    {
        var last = new TonNodeBlockIdExt(
            workchain: -1,
            shard: -9223372036854775808,
            seqno: 100,
            rootHash: AdnlKeys.GenerateRandomBytes(32),
            fileHash: AdnlKeys.GenerateRandomBytes(32)
        );

        var init = new TonNodeZeroStateIdExt(
            workchain: -1,
            rootHash: AdnlKeys.GenerateRandomBytes(32),
            fileHash: AdnlKeys.GenerateRandomBytes(32)
        );

        var original = new LiteServerMasterchainInfo
        {
            Last = last,
            StateRootHash = AdnlKeys.GenerateRandomBytes(32),
            Init = init
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerMasterchainInfo.ReadFrom(reader);

        Assert.Equal(original.Last.Seqno, deserialized.Last.Seqno);
        Assert.Equal(original.StateRootHash, deserialized.StateRootHash);
        Assert.Equal(original.Init.Workchain, deserialized.Init.Workchain);
    }

    [Fact]
    public void LiteServerMasterchainInfoExt_ShouldSerializeAndDeserialize()
    {
        var last = new TonNodeBlockIdExt(
            workchain: -1,
            shard: -9223372036854775808,
            seqno: 200,
            rootHash: AdnlKeys.GenerateRandomBytes(32),
            fileHash: AdnlKeys.GenerateRandomBytes(32)
        );

        var init = new TonNodeZeroStateIdExt(
            workchain: -1,
            rootHash: AdnlKeys.GenerateRandomBytes(32),
            fileHash: AdnlKeys.GenerateRandomBytes(32)
        );

        var original = new LiteServerMasterchainInfoExt
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

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerMasterchainInfoExt.ReadFrom(reader);

        Assert.Equal(original.Mode, deserialized.Mode);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Capabilities, deserialized.Capabilities);
        Assert.Equal(original.LastUtime, deserialized.LastUtime);
        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerCurrentTime_ShouldSerializeAndDeserialize()
    {
        var original = new LiteServerCurrentTime
        {
            Now = 1234567890
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerCurrentTime.ReadFrom(reader);

        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerVersion_ShouldSerializeAndDeserialize()
    {
        var original = new LiteServerVersion
        {
            Mode = 1,
            Version = 2,
            Capabilities = 999L,
            Now = 1234567890
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerVersion.ReadFrom(reader);

        Assert.Equal(original.Mode, deserialized.Mode);
        Assert.Equal(original.Version, deserialized.Version);
        Assert.Equal(original.Capabilities, deserialized.Capabilities);
        Assert.Equal(original.Now, deserialized.Now);
    }

    [Fact]
    public void LiteServerBlockData_ShouldSerializeAndDeserialize()
    {
        var blockId = new TonNodeBlockIdExt(
            workchain: 0,
            shard: 1000,
            seqno: 100,
            rootHash: AdnlKeys.GenerateRandomBytes(32),
            fileHash: AdnlKeys.GenerateRandomBytes(32)
        );

        var data = AdnlKeys.GenerateRandomBytes(256);

        var original = new LiteServerBlockData
        {
            Id = blockId,
            Data = data
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerBlockData.ReadFrom(reader);

        Assert.Equal(original.Id.Seqno, deserialized.Id.Seqno);
        Assert.Equal(original.Data, deserialized.Data);
    }

    [Fact]
    public void LiteServerError_ShouldSerializeAndDeserialize()
    {
        var original = new LiteServerError
        {
            Code = 404,
            Message = "Not found"
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerError.ReadFrom(reader);

        Assert.Equal(original.Code, deserialized.Code);
        Assert.Equal(original.Message, deserialized.Message);
    }

    [Fact]
    public void LiteServerTransactionId3_ShouldSerializeAndDeserialize()
    {
        var original = new LiteServerTransactionId3
        {
            Account = AdnlKeys.GenerateRandomBytes(32),
            Lt = 123456789L
        };

        var writer = new TLWriteBuffer();
        original.WriteTo(writer);
        var bytes = writer.Build();

        var reader = new TLReadBuffer(bytes);
        var deserialized = LiteServerTransactionId3.ReadFrom(reader);

        Assert.Equal(original.Account, deserialized.Account);
        Assert.Equal(original.Lt, deserialized.Lt);
    }

    [Fact]
    public void Functions_ShouldHaveCorrectConstructorIds()
    {
        // Verify a few known function constructor IDs
        Assert.Equal(0xBF56BE80u, Functions.GetMasterchainInfo);
        Assert.Equal(0x75156F9Du, Functions.GetMasterchainInfoExt);
        Assert.Equal(0x42AB5F46u, Functions.GetTime);
        Assert.Equal(0xF4F8F4B5u, Functions.GetVersion);
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


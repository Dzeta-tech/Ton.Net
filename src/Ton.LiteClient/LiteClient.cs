using System.Numerics;
using Ton.Adnl.Protocol;
using Ton.Adnl.TL;
using Ton.Core.Addresses;
using Ton.Core.Boc;
using Ton.LiteClient.Engines;
using Ton.LiteClient.Models;
using Ton.LiteClient.Parsers;

namespace Ton.LiteClient;

/// <summary>
/// High-level TON Lite Client for interacting with TON blockchain
/// </summary>
public sealed class LiteClient : IDisposable
{
    readonly ILiteEngine engine;
    readonly bool ownsEngine;

    /// <summary>
    /// Creates a new lite client with the specified engine
    /// </summary>
    /// <param name="engine">Lite engine instance</param>
    /// <param name="ownsEngine">Whether this client should dispose the engine when disposed</param>
    public LiteClient(ILiteEngine engine, bool ownsEngine = true)
    {
        ArgumentNullException.ThrowIfNull(engine);
        this.engine = engine;
        this.ownsEngine = ownsEngine;
    }

    /// <summary>
    /// Creates a new lite client with a single server connection
    /// </summary>
    /// <param name="host">Server host/IP</param>
    /// <param name="port">Server port</param>
    /// <param name="serverPublicKey">Server's Ed25519 public key (32 bytes or base64 string)</param>
    public static LiteClient Create(string host, int port, byte[] serverPublicKey)
    {
        var engine = new LiteSingleEngine(host, port, serverPublicKey);
        return new LiteClient(engine, ownsEngine: true);
    }

    /// <summary>
    /// Creates a new lite client with a single server connection (base64 public key)
    /// </summary>
    public static LiteClient Create(string host, int port, string serverPublicKeyBase64)
    {
        var engine = new LiteSingleEngine(host, port, serverPublicKeyBase64);
        return new LiteClient(engine, ownsEngine: true);
    }

    /// <summary>
    /// Gets the underlying engine
    /// </summary>
    public ILiteEngine Engine => engine;

    /// <summary>
    /// Gets current server time
    /// </summary>
    public async Task<DateTimeOffset> GetTimeAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var response = await engine.QueryAsync(
            Functions.GetTime,
            static (w, _) => { }, // Empty request
            static r => LiteServerCurrentTime.ReadFrom(r),
            Unit.Default,
            timeout,
            cancellationToken);

        return DateTimeOffset.FromUnixTimeSeconds(response.Now);
    }

    /// <summary>
    /// Gets masterchain information including latest block
    /// </summary>
    public async Task<MasterchainInfo> GetMasterchainInfoAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var response = await engine.QueryAsync(
            Functions.GetMasterchainInfo,
            static (w, _) => { }, // Empty request
            static r => LiteServerMasterchainInfo.ReadFrom(r),
            Unit.Default,
            timeout,
            cancellationToken);

        return new MasterchainInfo
        {
            Last = new BlockId(
                response.Last.Workchain,
                response.Last.Shard,
                response.Last.Seqno,
                response.Last.RootHash,
                response.Last.FileHash),
            StateRootHash = response.StateRootHash,
            Init = new ZeroStateId(
                response.Init.Workchain,
                response.Init.RootHash,
                response.Init.FileHash),
            Now = response.Last.Seqno // Using seqno as placeholder for now field
        };
    }

    /// <summary>
    /// Gets version information from the lite server
    /// </summary>
    public async Task<(int Version, long Capabilities, int Now)> GetVersionAsync(
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var response = await engine.QueryAsync(
            Functions.GetVersion,
            static (w, _) => { }, // Empty request
            static r => LiteServerVersion.ReadFrom(r),
            Unit.Default,
            timeout,
            cancellationToken);

        return (response.Version, response.Capabilities, response.Now);
    }

    /// <summary>
    /// Looks up a block by workchain, shard, and seqno
    /// </summary>
    public async Task<BlockId> LookupBlockAsync(
        int workchain,
        long shard,
        int seqno,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockId = new TonNodeBlockId(workchain, shard, seqno);
        var request = new LiteServerLookupBlockRequest
        {
            Mode = 1, // By ID
            Id = blockId,
            Lt = null,
            Utime = null
        };

        var response = await engine.QueryAsync(
            Functions.LookupBlock,
            static (w, req) =>
            {
                w.WriteInt32(req.Mode);
                req.Id.WriteTo(w); // Use built-in WriteTo method
                // Write optional Lt
                if (req.Lt.HasValue)
                {
                    w.WriteBool(true);
                    w.WriteInt64((long)req.Lt.Value);
                }
                else
                {
                    w.WriteBool(false);
                }
                // Write optional Utime
                if (req.Utime.HasValue)
                {
                    w.WriteBool(true);
                    w.WriteInt32(req.Utime.Value);
                }
                else
                {
                    w.WriteBool(false);
                }
            },
            static r => LiteServerBlockHeader.ReadFrom(r),
            request,
            timeout,
            cancellationToken);

        return new BlockId(
            response.Id.Workchain,
            response.Id.Shard,
            response.Id.Seqno,
            response.Id.RootHash,
            response.Id.FileHash);
    }

    /// <summary>
    /// Gets block header information
    /// </summary>
    public async Task<BlockHeader> GetBlockHeaderAsync(
        BlockId blockId,
        int mode = 0,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockIdExt = new TonNodeBlockIdExt(
            blockId.Workchain,
            blockId.Shard,
            blockId.Seqno,
            blockId.RootHash,
            blockId.FileHash);

        var request = new LiteServerGetBlockHeaderRequest
        {
            Mode = mode,
            Id = blockIdExt
        };

        var response = await engine.QueryAsync(
            Functions.GetBlockHeader,
            static (w, req) =>
            {
                w.WriteInt32(req.Mode);
                req.Id.WriteTo(w); // Use built-in WriteTo method
            },
            static r => LiteServerBlockHeader.ReadFrom(r),
            request,
            timeout,
            cancellationToken);

        // Parse block header from data
        var headerCell = Cell.FromBoc(response.HeaderProof)[0];
        var headerSlice = headerCell.BeginParse();

        // Read block header fields (simplified - full parsing would be more complex)
        return new BlockHeader
        {
            Id = new BlockId(
                response.Id.Workchain,
                response.Id.Shard,
                response.Id.Seqno,
                response.Id.RootHash,
                response.Id.FileHash),
            GlobalId = 0, // Would need to parse from cell
            Version = 0,
            Flags = 0,
            AfterMerge = false,
            AfterSplit = false,
            BeforeSplit = false,
            WantMerge = false,
            WantSplit = false,
            ValidatorListHashShort = 0,
            CatchainSeqno = 0,
            MinRefMcSeqno = 0,
            IsKeyBlock = false,
            PrevKeyBlockSeqno = 0,
            GenUtime = 0,
            StartLt = BigInteger.Zero,
            EndLt = BigInteger.Zero
        };
    }

    /// <summary>
    /// Gets all shard information for a given block
    /// </summary>
    public async Task<ShardInfo> GetAllShardsInfoAsync(
        BlockId blockId,
        int timeout = 5000,
        CancellationToken cancellationToken = default)
    {
        var blockIdExt = new TonNodeBlockIdExt(
            blockId.Workchain,
            blockId.Shard,
            blockId.Seqno,
            blockId.RootHash,
            blockId.FileHash);

        var request = new LiteServerGetAllShardsInfoRequest
        {
            Id = blockIdExt
        };

        var response = await engine.QueryAsync(
            Functions.GetAllShardsInfo,
            static (w, req) =>
            {
                req.Id.WriteTo(w); // Use built-in WriteTo method
            },
            static r => LiteServerAllShardsInfo.ReadFrom(r),
            request,
            timeout,
            cancellationToken);

        // Parse shard data from BOC
        var shardCollection = ShardParser.ParseShards(response.Data);

        return new ShardInfo
        {
            Block = new BlockId(
                response.Id.Workchain,
                response.Id.Shard,
                response.Id.Seqno,
                response.Id.RootHash,
                response.Id.FileHash),
            Shards = shardCollection,
            Proof = response.Proof,
            Data = response.Data
        };
    }

    public void Dispose()
    {
        if (ownsEngine)
        {
            engine.Dispose();
        }
    }

    // Helper types for requests
    struct LiteServerLookupBlockRequest
    {
        public int Mode;
        public TonNodeBlockId Id;
        public BigInteger? Lt;
        public int? Utime;
    }

    struct LiteServerGetBlockHeaderRequest
    {
        public int Mode;
        public TonNodeBlockIdExt Id;
    }

    struct LiteServerGetAllShardsInfoRequest
    {
        public TonNodeBlockIdExt Id;
    }

    struct Unit
    {
        public static readonly Unit Default = new();
    }
}


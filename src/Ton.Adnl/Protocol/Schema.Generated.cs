// Auto-generated from lite_api.tl
// DO NOT EDIT MANUALLY
// This is the protocol layer - raw TL types matching lite_api.tl exactly
// For user-facing APIs, create domain models and map in LiteClient
// Union types: Bool, adnl.Message, liteServer.BlockLink

#nullable disable
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword

using System;
using Ton.Adnl.TL;

namespace Ton.Adnl.Protocol
{
    // ============================================================================
    // Abstract base classes for union types
    // ============================================================================

    /// <summary>
    /// Base class for liteServer.BlockLink
    /// Implementations: LiteServerBlockLinkBack, LiteServerBlockLinkForward
    /// </summary>
    public abstract class LiteServerBlockLink
    {
        public abstract uint Constructor { get; }
        public abstract void WriteTo(TLWriteBuffer writer);

        public static LiteServerBlockLink ReadFrom(TLReadBuffer reader)
        {
            uint constructor = reader.ReadUInt32();
            switch (constructor)
            {
                case 0xEF7E1BEF:
                    return LiteServerBlockLinkBack.ReadFrom(reader);
                case 0x520FCE1C:
                    return LiteServerBlockLinkForward.ReadFrom(reader);
                default:
                    throw new Exception($"Unknown constructor 0x{constructor:X8} for liteServer.BlockLink");
            }
        }
    }

    // ============================================================================
    // Basic Types (tonNode.*)
    // ============================================================================

    /// <summary>
    /// tonNode.blockId = tonNode.BlockId
    /// </summary>
    public class TonNodeBlockId
    {
        public const uint Constructor = 0xB7CDB167;

        public int Workchain { get; set; }
        public long Shard { get; set; }
        public int Seqno { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteInt64(Shard);
            writer.WriteInt32(Seqno);
        }

        public static TonNodeBlockId ReadFrom(TLReadBuffer reader)
        {
            return new TonNodeBlockId
            {
                Workchain = reader.ReadInt32(),
                Shard = reader.ReadInt64(),
                Seqno = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// tonNode.blockIdExt = tonNode.BlockIdExt
    /// </summary>
    public class TonNodeBlockIdExt
    {
        public const uint Constructor = 0x6752EB78;

        public int Workchain { get; set; }
        public long Shard { get; set; }
        public int Seqno { get; set; }
        public byte[] RootHash { get; set; } = Array.Empty<byte>();
        public byte[] FileHash { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteInt64(Shard);
            writer.WriteInt32(Seqno);
            writer.WriteBytes(RootHash, 32);
            writer.WriteBytes(FileHash, 32);
        }

        public static TonNodeBlockIdExt ReadFrom(TLReadBuffer reader)
        {
            return new TonNodeBlockIdExt
            {
                Workchain = reader.ReadInt32(),
                Shard = reader.ReadInt64(),
                Seqno = reader.ReadInt32(),
                RootHash = reader.ReadInt256(),
                FileHash = reader.ReadInt256(),
            };
        }
    }

    /// <summary>
    /// tonNode.zeroStateIdExt = tonNode.ZeroStateIdExt
    /// </summary>
    public class TonNodeZeroStateIdExt
    {
        public const uint Constructor = 0x1D7235AE;

        public int Workchain { get; set; }
        public byte[] RootHash { get; set; } = Array.Empty<byte>();
        public byte[] FileHash { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteBytes(RootHash, 32);
            writer.WriteBytes(FileHash, 32);
        }

        public static TonNodeZeroStateIdExt ReadFrom(TLReadBuffer reader)
        {
            return new TonNodeZeroStateIdExt
            {
                Workchain = reader.ReadInt32(),
                RootHash = reader.ReadInt256(),
                FileHash = reader.ReadInt256(),
            };
        }
    }

    // ============================================================================
    // Lite Server Types (liteServer.*)
    // ============================================================================

    /// <summary>
    /// liteServer.error = liteServer.Error
    /// </summary>
    public class LiteServerError
    {
        public const uint Constructor = 0xBBA9E148;

        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Code);
            writer.WriteString(Message);
        }

        public static LiteServerError ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerError
            {
                Code = reader.ReadInt32(),
                Message = reader.ReadString(),
            };
        }
    }

    /// <summary>
    /// liteServer.accountId = liteServer.AccountId
    /// </summary>
    public class LiteServerAccountId
    {
        public const uint Constructor = 0x75A0E2C5;

        public int Workchain { get; set; }
        public byte[] Id { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteBytes(Id, 32);
        }

        public static LiteServerAccountId ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerAccountId
            {
                Workchain = reader.ReadInt32(),
                Id = reader.ReadInt256(),
            };
        }
    }

    /// <summary>
    /// liteServer.libraryEntry = liteServer.LibraryEntry
    /// </summary>
    public class LiteServerLibraryEntry
    {
        public const uint Constructor = 0x8AFF2446;

        public byte[] Hash { get; set; } = Array.Empty<byte>();
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBytes(Hash, 32);
            writer.WriteBuffer(Data);
        }

        public static LiteServerLibraryEntry ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerLibraryEntry
            {
                Hash = reader.ReadInt256(),
                Data = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.masterchainInfo = liteServer.MasterchainInfo
    /// </summary>
    public class LiteServerMasterchainInfo
    {
        public const uint Constructor = 0x85832881;

        public TonNodeBlockIdExt Last { get; set; }
        public byte[] StateRootHash { get; set; } = Array.Empty<byte>();
        public TonNodeZeroStateIdExt Init { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            Last.WriteTo(writer);
            writer.WriteBytes(StateRootHash, 32);
            Init.WriteTo(writer);
        }

        public static LiteServerMasterchainInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerMasterchainInfo
            {
                Last = TonNodeBlockIdExt.ReadFrom(reader),
                StateRootHash = reader.ReadInt256(),
                Init = TonNodeZeroStateIdExt.ReadFrom(reader),
            };
        }
    }

    /// <summary>
    /// liteServer.masterchainInfoExt = liteServer.MasterchainInfoExt
    /// </summary>
    public class LiteServerMasterchainInfoExt
    {
        public const uint Constructor = 0xA8CCE0F5;

        public uint Mode { get; set; }
        public int Version { get; set; }
        public long Capabilities { get; set; }
        public TonNodeBlockIdExt Last { get; set; }
        public int LastUtime { get; set; }
        public int Now { get; set; }
        public byte[] StateRootHash { get; set; } = Array.Empty<byte>();
        public TonNodeZeroStateIdExt Init { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            writer.WriteInt32(Version);
            writer.WriteInt64(Capabilities);
            Last.WriteTo(writer);
            writer.WriteInt32(LastUtime);
            writer.WriteInt32(Now);
            writer.WriteBytes(StateRootHash, 32);
            Init.WriteTo(writer);
        }

        public static LiteServerMasterchainInfoExt ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerMasterchainInfoExt
            {
                Mode = reader.ReadUInt32(),
                Version = reader.ReadInt32(),
                Capabilities = reader.ReadInt64(),
                Last = TonNodeBlockIdExt.ReadFrom(reader),
                LastUtime = reader.ReadInt32(),
                Now = reader.ReadInt32(),
                StateRootHash = reader.ReadInt256(),
                Init = TonNodeZeroStateIdExt.ReadFrom(reader),
            };
        }
    }

    /// <summary>
    /// liteServer.currentTime = liteServer.CurrentTime
    /// </summary>
    public class LiteServerCurrentTime
    {
        public const uint Constructor = 0xE953000D;

        public int Now { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Now);
        }

        public static LiteServerCurrentTime ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerCurrentTime
            {
                Now = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// liteServer.version = liteServer.Version
    /// </summary>
    public class LiteServerVersion
    {
        public const uint Constructor = 0x5A0491E5;

        public uint Mode { get; set; }
        public int Version { get; set; }
        public long Capabilities { get; set; }
        public int Now { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            writer.WriteInt32(Version);
            writer.WriteInt64(Capabilities);
            writer.WriteInt32(Now);
        }

        public static LiteServerVersion ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerVersion
            {
                Mode = reader.ReadUInt32(),
                Version = reader.ReadInt32(),
                Capabilities = reader.ReadInt64(),
                Now = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// liteServer.blockData = liteServer.BlockData
    /// </summary>
    public class LiteServerBlockData
    {
        public const uint Constructor = 0xA574ED6C;

        public TonNodeBlockIdExt Id { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBuffer(Data);
        }

        public static LiteServerBlockData ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockData
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Data = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.blockState = liteServer.BlockState
    /// </summary>
    public class LiteServerBlockState
    {
        public const uint Constructor = 0xABADDC0C;

        public TonNodeBlockIdExt Id { get; set; }
        public byte[] RootHash { get; set; } = Array.Empty<byte>();
        public byte[] FileHash { get; set; } = Array.Empty<byte>();
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBytes(RootHash, 32);
            writer.WriteBytes(FileHash, 32);
            writer.WriteBuffer(Data);
        }

        public static LiteServerBlockState ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockState
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                RootHash = reader.ReadInt256(),
                FileHash = reader.ReadInt256(),
                Data = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.blockHeader = liteServer.BlockHeader
    /// </summary>
    public class LiteServerBlockHeader
    {
        public const uint Constructor = 0x752D8219;

        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public byte[] HeaderProof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            writer.WriteBuffer(HeaderProof);
        }

        public static LiteServerBlockHeader ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockHeader
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Mode = reader.ReadUInt32(),
                HeaderProof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.sendMsgStatus = liteServer.SendMsgStatus
    /// </summary>
    public class LiteServerSendMsgStatus
    {
        public const uint Constructor = 0x3950E597;

        public int Status { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Status);
        }

        public static LiteServerSendMsgStatus ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerSendMsgStatus
            {
                Status = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// liteServer.accountState = liteServer.AccountState
    /// </summary>
    public class LiteServerAccountState
    {
        public const uint Constructor = 0x7079C751;

        public TonNodeBlockIdExt Id { get; set; }
        public TonNodeBlockIdExt Shardblk { get; set; }
        public byte[] ShardProof { get; set; } = Array.Empty<byte>();
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] State { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            Shardblk.WriteTo(writer);
            writer.WriteBuffer(ShardProof);
            writer.WriteBuffer(Proof);
            writer.WriteBuffer(State);
        }

        public static LiteServerAccountState ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerAccountState
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Shardblk = TonNodeBlockIdExt.ReadFrom(reader),
                ShardProof = reader.ReadBuffer(),
                Proof = reader.ReadBuffer(),
                State = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.runMethodResult = liteServer.RunMethodResult
    /// </summary>
    public class LiteServerRunMethodResult
    {
        public const uint Constructor = 0xA39A616B;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public TonNodeBlockIdExt Shardblk { get; set; }
        public byte[] ShardProof { get; set; } = Array.Empty<byte>();
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] StateProof { get; set; } = Array.Empty<byte>();
        public byte[] InitC7 { get; set; } = Array.Empty<byte>();
        public byte[] LibExtras { get; set; } = Array.Empty<byte>();
        public int ExitCode { get; set; }
        public byte[] Result { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            Shardblk.WriteTo(writer);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(ShardProof);
            }
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(Proof);
            }
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteBuffer(StateProof);
            }
            if ((Mode & (1u << 3)) != 0)
            {
                writer.WriteBuffer(InitC7);
            }
            if ((Mode & (1u << 4)) != 0)
            {
                writer.WriteBuffer(LibExtras);
            }
            writer.WriteInt32(ExitCode);
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteBuffer(Result);
            }
        }

        public static LiteServerRunMethodResult ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerRunMethodResult();
            result.Mode = reader.ReadUInt32();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.Shardblk = TonNodeBlockIdExt.ReadFrom(reader);
            if ((result.Mode & (1u << 0)) != 0)
                result.ShardProof = reader.ReadBuffer();
            if ((result.Mode & (1u << 0)) != 0)
                result.Proof = reader.ReadBuffer();
            if ((result.Mode & (1u << 1)) != 0)
                result.StateProof = reader.ReadBuffer();
            if ((result.Mode & (1u << 3)) != 0)
                result.InitC7 = reader.ReadBuffer();
            if ((result.Mode & (1u << 4)) != 0)
                result.LibExtras = reader.ReadBuffer();
            result.ExitCode = reader.ReadInt32();
            if ((result.Mode & (1u << 2)) != 0)
                result.Result = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.shardInfo = liteServer.ShardInfo
    /// </summary>
    public class LiteServerShardInfo
    {
        public const uint Constructor = 0x9FE6CD84;

        public TonNodeBlockIdExt Id { get; set; }
        public TonNodeBlockIdExt Shardblk { get; set; }
        public byte[] ShardProof { get; set; } = Array.Empty<byte>();
        public byte[] ShardDescr { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            Shardblk.WriteTo(writer);
            writer.WriteBuffer(ShardProof);
            writer.WriteBuffer(ShardDescr);
        }

        public static LiteServerShardInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerShardInfo
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Shardblk = TonNodeBlockIdExt.ReadFrom(reader),
                ShardProof = reader.ReadBuffer(),
                ShardDescr = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.allShardsInfo = liteServer.AllShardsInfo
    /// </summary>
    public class LiteServerAllShardsInfo
    {
        public const uint Constructor = 0x098FE72D;

        public TonNodeBlockIdExt Id { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBuffer(Proof);
            writer.WriteBuffer(Data);
        }

        public static LiteServerAllShardsInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerAllShardsInfo
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Proof = reader.ReadBuffer(),
                Data = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.transactionInfo = liteServer.TransactionInfo
    /// </summary>
    public class LiteServerTransactionInfo
    {
        public const uint Constructor = 0x0EDEED47;

        public TonNodeBlockIdExt Id { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] Transaction { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBuffer(Proof);
            writer.WriteBuffer(Transaction);
        }

        public static LiteServerTransactionInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerTransactionInfo
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Proof = reader.ReadBuffer(),
                Transaction = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.transactionList = liteServer.TransactionList
    /// </summary>
    public class LiteServerTransactionList
    {
        public const uint Constructor = 0xB92ED79D;

        public TonNodeBlockIdExt[] Ids { get; set; } = Array.Empty<TonNodeBlockIdExt>();
        public byte[] Transactions { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32((uint)Ids.Length);
                foreach (var item in Ids)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBuffer(Transactions);
        }

        public static LiteServerTransactionList ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerTransactionList();
            uint idsCount = reader.ReadUInt32();
            result.Ids = new TonNodeBlockIdExt[idsCount];
            for (int i = 0; i < idsCount; i++)
            {
                result.Ids[i] = TonNodeBlockIdExt.ReadFrom(reader);
            }
            result.Transactions = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.transactionMetadata = liteServer.TransactionMetadata
    /// </summary>
    public class LiteServerTransactionMetadata
    {
        public const uint Constructor = 0xFF706385;

        public uint Mode { get; set; }
        public int Depth { get; set; }
        public LiteServerAccountId Initiator { get; set; }
        public long InitiatorLt { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            writer.WriteInt32(Depth);
            Initiator.WriteTo(writer);
            writer.WriteInt64(InitiatorLt);
        }

        public static LiteServerTransactionMetadata ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerTransactionMetadata
            {
                Mode = reader.ReadUInt32(),
                Depth = reader.ReadInt32(),
                Initiator = LiteServerAccountId.ReadFrom(reader),
                InitiatorLt = reader.ReadInt64(),
            };
        }
    }

    /// <summary>
    /// liteServer.transactionId = liteServer.TransactionId
    /// </summary>
    public class LiteServerTransactionId
    {
        public const uint Constructor = 0x2824971B;

        public uint Mode { get; set; }
        public byte[] Account { get; set; } = Array.Empty<byte>();
        public long Lt { get; set; }
        public byte[] Hash { get; set; } = Array.Empty<byte>();
        public LiteServerTransactionMetadata Metadata { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBytes(Account, 32);
            }
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteInt64(Lt);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteBytes(Hash, 32);
            }
            if ((Mode & (1u << 8)) != 0)
            {
                Metadata.WriteTo(writer);
            }
        }

        public static LiteServerTransactionId ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerTransactionId();
            result.Mode = reader.ReadUInt32();
            if ((result.Mode & (1u << 0)) != 0)
                result.Account = reader.ReadInt256();
            if ((result.Mode & (1u << 1)) != 0)
                result.Lt = reader.ReadInt64();
            if ((result.Mode & (1u << 2)) != 0)
                result.Hash = reader.ReadInt256();
            if ((result.Mode & (1u << 8)) != 0)
                result.Metadata = LiteServerTransactionMetadata.ReadFrom(reader);
            return result;
        }
    }

    /// <summary>
    /// liteServer.transactionId3 = liteServer.TransactionId3
    /// </summary>
    public class LiteServerTransactionId3
    {
        public const uint Constructor = 0x2C81DA77;

        public byte[] Account { get; set; } = Array.Empty<byte>();
        public long Lt { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBytes(Account, 32);
            writer.WriteInt64(Lt);
        }

        public static LiteServerTransactionId3 ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerTransactionId3
            {
                Account = reader.ReadInt256(),
                Lt = reader.ReadInt64(),
            };
        }
    }

    /// <summary>
    /// liteServer.blockTransactions = liteServer.BlockTransactions
    /// </summary>
    public class LiteServerBlockTransactions
    {
        public const uint Constructor = 0x2F546C5C;

        public TonNodeBlockIdExt Id { get; set; }
        public uint ReqCount { get; set; }
        public bool Incomplete { get; set; }
        public LiteServerTransactionId[] Ids { get; set; } = Array.Empty<LiteServerTransactionId>();
        public byte[] Proof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteUInt32(ReqCount);
            writer.WriteBool(Incomplete);
            writer.WriteUInt32((uint)Ids.Length);
                foreach (var item in Ids)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBuffer(Proof);
        }

        public static LiteServerBlockTransactions ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerBlockTransactions();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.ReqCount = reader.ReadUInt32();
            result.Incomplete = reader.ReadBool();
            uint idsCount = reader.ReadUInt32();
            result.Ids = new LiteServerTransactionId[idsCount];
            for (int i = 0; i < idsCount; i++)
            {
                result.Ids[i] = LiteServerTransactionId.ReadFrom(reader);
            }
            result.Proof = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.blockTransactionsExt = liteServer.BlockTransactionsExt
    /// </summary>
    public class LiteServerBlockTransactionsExt
    {
        public const uint Constructor = 0xFB8FFCE4;

        public TonNodeBlockIdExt Id { get; set; }
        public uint ReqCount { get; set; }
        public bool Incomplete { get; set; }
        public byte[] Transactions { get; set; } = Array.Empty<byte>();
        public byte[] Proof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteUInt32(ReqCount);
            writer.WriteBool(Incomplete);
            writer.WriteBuffer(Transactions);
            writer.WriteBuffer(Proof);
        }

        public static LiteServerBlockTransactionsExt ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockTransactionsExt
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                ReqCount = reader.ReadUInt32(),
                Incomplete = reader.ReadBool(),
                Transactions = reader.ReadBuffer(),
                Proof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.signature = liteServer.Signature
    /// </summary>
    public class LiteServerSignature
    {
        public const uint Constructor = 0xA3DEF855;

        public byte[] NodeIdShort { get; set; } = Array.Empty<byte>();
        public byte[] Signature { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBuffer(NodeIdShort);
            writer.WriteBuffer(Signature);
        }

        public static LiteServerSignature ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerSignature
            {
                NodeIdShort = reader.ReadBuffer(),
                Signature = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.signatureSet = liteServer.SignatureSet
    /// </summary>
    public class LiteServerSignatureSet
    {
        public const uint Constructor = 0x92E15597;

        public int ValidatorSetHash { get; set; }
        public int CatchainSeqno { get; set; }
        public LiteServerSignature[] Signatures { get; set; } = Array.Empty<LiteServerSignature>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(ValidatorSetHash);
            writer.WriteInt32(CatchainSeqno);
            writer.WriteUInt32((uint)Signatures.Length);
                foreach (var item in Signatures)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerSignatureSet ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerSignatureSet();
            result.ValidatorSetHash = reader.ReadInt32();
            result.CatchainSeqno = reader.ReadInt32();
            uint signaturesCount = reader.ReadUInt32();
            result.Signatures = new LiteServerSignature[signaturesCount];
            for (int i = 0; i < signaturesCount; i++)
            {
                result.Signatures[i] = LiteServerSignature.ReadFrom(reader);
            }
            return result;
        }
    }

    /// <summary>
    /// liteServer.blockLinkBack = liteServer.BlockLink
    /// Inherits from: LiteServerBlockLink
    /// </summary>
    public class LiteServerBlockLinkBack : LiteServerBlockLink
    {
        public override uint Constructor => 0xEF7E1BEF;

        public bool ToKeyBlock { get; set; }
        public TonNodeBlockIdExt From { get; set; }
        public TonNodeBlockIdExt To { get; set; }
        public byte[] DestProof { get; set; } = Array.Empty<byte>();
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] StateProof { get; set; } = Array.Empty<byte>();

        public override void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBool(ToKeyBlock);
            From.WriteTo(writer);
            To.WriteTo(writer);
            writer.WriteBuffer(DestProof);
            writer.WriteBuffer(Proof);
            writer.WriteBuffer(StateProof);
        }

        public static LiteServerBlockLinkBack ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockLinkBack
            {
                ToKeyBlock = reader.ReadBool(),
                From = TonNodeBlockIdExt.ReadFrom(reader),
                To = TonNodeBlockIdExt.ReadFrom(reader),
                DestProof = reader.ReadBuffer(),
                Proof = reader.ReadBuffer(),
                StateProof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.blockLinkForward = liteServer.BlockLink
    /// Inherits from: LiteServerBlockLink
    /// </summary>
    public class LiteServerBlockLinkForward : LiteServerBlockLink
    {
        public override uint Constructor => 0x520FCE1C;

        public bool ToKeyBlock { get; set; }
        public TonNodeBlockIdExt From { get; set; }
        public TonNodeBlockIdExt To { get; set; }
        public byte[] DestProof { get; set; } = Array.Empty<byte>();
        public byte[] ConfigProof { get; set; } = Array.Empty<byte>();
        public LiteServerSignatureSet Signatures { get; set; }

        public override void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBool(ToKeyBlock);
            From.WriteTo(writer);
            To.WriteTo(writer);
            writer.WriteBuffer(DestProof);
            writer.WriteBuffer(ConfigProof);
            Signatures.WriteTo(writer);
        }

        public static LiteServerBlockLinkForward ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerBlockLinkForward
            {
                ToKeyBlock = reader.ReadBool(),
                From = TonNodeBlockIdExt.ReadFrom(reader),
                To = TonNodeBlockIdExt.ReadFrom(reader),
                DestProof = reader.ReadBuffer(),
                ConfigProof = reader.ReadBuffer(),
                Signatures = LiteServerSignatureSet.ReadFrom(reader),
            };
        }
    }

    /// <summary>
    /// liteServer.partialBlockProof = liteServer.PartialBlockProof
    /// </summary>
    public class LiteServerPartialBlockProof
    {
        public const uint Constructor = 0x0D2E280F;

        public bool Complete { get; set; }
        public TonNodeBlockIdExt From { get; set; }
        public TonNodeBlockIdExt To { get; set; }
        public LiteServerBlockLink[] Steps { get; set; } = Array.Empty<LiteServerBlockLink>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBool(Complete);
            From.WriteTo(writer);
            To.WriteTo(writer);
            writer.WriteUInt32((uint)Steps.Length);
                foreach (var item in Steps)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerPartialBlockProof ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerPartialBlockProof();
            result.Complete = reader.ReadBool();
            result.From = TonNodeBlockIdExt.ReadFrom(reader);
            result.To = TonNodeBlockIdExt.ReadFrom(reader);
            uint stepsCount = reader.ReadUInt32();
            result.Steps = new LiteServerBlockLink[stepsCount];
            for (int i = 0; i < stepsCount; i++)
            {
                result.Steps[i] = LiteServerBlockLink.ReadFrom(reader);
            }
            return result;
        }
    }

    /// <summary>
    /// liteServer.configInfo = liteServer.ConfigInfo
    /// </summary>
    public class LiteServerConfigInfo
    {
        public const uint Constructor = 0xAE7B272F;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public byte[] StateProof { get; set; } = Array.Empty<byte>();
        public byte[] ConfigProof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteBuffer(StateProof);
            writer.WriteBuffer(ConfigProof);
        }

        public static LiteServerConfigInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerConfigInfo
            {
                Mode = reader.ReadUInt32(),
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                StateProof = reader.ReadBuffer(),
                ConfigProof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.validatorStats = liteServer.ValidatorStats
    /// </summary>
    public class LiteServerValidatorStats
    {
        public const uint Constructor = 0xB9F796D8;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public int Count { get; set; }
        public bool Complete { get; set; }
        public byte[] StateProof { get; set; } = Array.Empty<byte>();
        public byte[] DataProof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteInt32(Count);
            writer.WriteBool(Complete);
            writer.WriteBuffer(StateProof);
            writer.WriteBuffer(DataProof);
        }

        public static LiteServerValidatorStats ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerValidatorStats
            {
                Mode = reader.ReadUInt32(),
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Count = reader.ReadInt32(),
                Complete = reader.ReadBool(),
                StateProof = reader.ReadBuffer(),
                DataProof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.libraryResult = liteServer.LibraryResult
    /// </summary>
    public class LiteServerLibraryResult
    {
        public const uint Constructor = 0x8B84430C;

        public LiteServerLibraryEntry[] Result { get; set; } = Array.Empty<LiteServerLibraryEntry>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32((uint)Result.Length);
                foreach (var item in Result)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerLibraryResult ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerLibraryResult();
            uint resultCount = reader.ReadUInt32();
            result.Result = new LiteServerLibraryEntry[resultCount];
            for (int i = 0; i < resultCount; i++)
            {
                result.Result[i] = LiteServerLibraryEntry.ReadFrom(reader);
            }
            return result;
        }
    }

    /// <summary>
    /// liteServer.libraryResultWithProof = liteServer.LibraryResultWithProof
    /// </summary>
    public class LiteServerLibraryResultWithProof
    {
        public const uint Constructor = 0x99370A1F;

        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public LiteServerLibraryEntry[] Result { get; set; } = Array.Empty<LiteServerLibraryEntry>();
        public byte[] StateProof { get; set; } = Array.Empty<byte>();
        public byte[] DataProof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            writer.WriteUInt32((uint)Result.Length);
                foreach (var item in Result)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBuffer(StateProof);
            writer.WriteBuffer(DataProof);
        }

        public static LiteServerLibraryResultWithProof ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerLibraryResultWithProof();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.Mode = reader.ReadUInt32();
            uint resultCount = reader.ReadUInt32();
            result.Result = new LiteServerLibraryEntry[resultCount];
            for (int i = 0; i < resultCount; i++)
            {
                result.Result[i] = LiteServerLibraryEntry.ReadFrom(reader);
            }
            result.StateProof = reader.ReadBuffer();
            result.DataProof = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.shardBlockLink = liteServer.ShardBlockLink
    /// </summary>
    public class LiteServerShardBlockLink
    {
        public const uint Constructor = 0xD30DCF72;

        public TonNodeBlockIdExt Id { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBuffer(Proof);
        }

        public static LiteServerShardBlockLink ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerShardBlockLink
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Proof = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.shardBlockProof = liteServer.ShardBlockProof
    /// </summary>
    public class LiteServerShardBlockProof
    {
        public const uint Constructor = 0x08763470;

        public TonNodeBlockIdExt MasterchainId { get; set; }
        public LiteServerShardBlockLink[] Links { get; set; } = Array.Empty<LiteServerShardBlockLink>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            MasterchainId.WriteTo(writer);
            writer.WriteUInt32((uint)Links.Length);
                foreach (var item in Links)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerShardBlockProof ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerShardBlockProof();
            result.MasterchainId = TonNodeBlockIdExt.ReadFrom(reader);
            uint linksCount = reader.ReadUInt32();
            result.Links = new LiteServerShardBlockLink[linksCount];
            for (int i = 0; i < linksCount; i++)
            {
                result.Links[i] = LiteServerShardBlockLink.ReadFrom(reader);
            }
            return result;
        }
    }

    /// <summary>
    /// liteServer.lookupBlockResult = liteServer.LookupBlockResult
    /// </summary>
    public class LiteServerLookupBlockResult
    {
        public const uint Constructor = 0x57C7CCC5;

        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public TonNodeBlockIdExt McBlockId { get; set; }
        public byte[] ClientMcStateProof { get; set; } = Array.Empty<byte>();
        public byte[] McBlockProof { get; set; } = Array.Empty<byte>();
        public LiteServerShardBlockLink[] ShardLinks { get; set; } = Array.Empty<LiteServerShardBlockLink>();
        public byte[] Header { get; set; } = Array.Empty<byte>();
        public byte[] PrevHeader { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            McBlockId.WriteTo(writer);
            writer.WriteBuffer(ClientMcStateProof);
            writer.WriteBuffer(McBlockProof);
            writer.WriteUInt32((uint)ShardLinks.Length);
                foreach (var item in ShardLinks)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBuffer(Header);
            writer.WriteBuffer(PrevHeader);
        }

        public static LiteServerLookupBlockResult ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerLookupBlockResult();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.Mode = reader.ReadUInt32();
            result.McBlockId = TonNodeBlockIdExt.ReadFrom(reader);
            result.ClientMcStateProof = reader.ReadBuffer();
            result.McBlockProof = reader.ReadBuffer();
            uint shardlinksCount = reader.ReadUInt32();
            result.ShardLinks = new LiteServerShardBlockLink[shardlinksCount];
            for (int i = 0; i < shardlinksCount; i++)
            {
                result.ShardLinks[i] = LiteServerShardBlockLink.ReadFrom(reader);
            }
            result.Header = reader.ReadBuffer();
            result.PrevHeader = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.outMsgQueueSize = liteServer.OutMsgQueueSize
    /// </summary>
    public class LiteServerOutMsgQueueSize
    {
        public const uint Constructor = 0xA7C64C85;

        public TonNodeBlockIdExt Id { get; set; }
        public int Size { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteInt32(Size);
        }

        public static LiteServerOutMsgQueueSize ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerOutMsgQueueSize
            {
                Id = TonNodeBlockIdExt.ReadFrom(reader),
                Size = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// liteServer.outMsgQueueSizes = liteServer.OutMsgQueueSizes
    /// </summary>
    public class LiteServerOutMsgQueueSizes
    {
        public const uint Constructor = 0xE9DD53E2;

        public LiteServerOutMsgQueueSize[] Shards { get; set; } = Array.Empty<LiteServerOutMsgQueueSize>();
        public int ExtMsgQueueSizeLimit { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32((uint)Shards.Length);
                foreach (var item in Shards)
                {
                    item.WriteTo(writer);
                }
            writer.WriteInt32(ExtMsgQueueSizeLimit);
        }

        public static LiteServerOutMsgQueueSizes ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerOutMsgQueueSizes();
            uint shardsCount = reader.ReadUInt32();
            result.Shards = new LiteServerOutMsgQueueSize[shardsCount];
            for (int i = 0; i < shardsCount; i++)
            {
                result.Shards[i] = LiteServerOutMsgQueueSize.ReadFrom(reader);
            }
            result.ExtMsgQueueSizeLimit = reader.ReadInt32();
            return result;
        }
    }

    /// <summary>
    /// liteServer.blockOutMsgQueueSize = liteServer.BlockOutMsgQueueSize
    /// </summary>
    public class LiteServerBlockOutMsgQueueSize
    {
        public const uint Constructor = 0x8ACDBE1B;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public long Size { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteInt64(Size);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(Proof);
            }
        }

        public static LiteServerBlockOutMsgQueueSize ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerBlockOutMsgQueueSize();
            result.Mode = reader.ReadUInt32();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.Size = reader.ReadInt64();
            if ((result.Mode & (1u << 0)) != 0)
                result.Proof = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.accountDispatchQueueInfo = liteServer.AccountDispatchQueueInfo
    /// </summary>
    public class LiteServerAccountDispatchQueueInfo
    {
        public const uint Constructor = 0x9B52AABB;

        public byte[] Addr { get; set; } = Array.Empty<byte>();
        public long Size { get; set; }
        public long MinLt { get; set; }
        public long MaxLt { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBuffer(Addr);
            writer.WriteInt64(Size);
            writer.WriteInt64(MinLt);
            writer.WriteInt64(MaxLt);
        }

        public static LiteServerAccountDispatchQueueInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerAccountDispatchQueueInfo
            {
                Addr = reader.ReadBuffer(),
                Size = reader.ReadInt64(),
                MinLt = reader.ReadInt64(),
                MaxLt = reader.ReadInt64(),
            };
        }
    }

    /// <summary>
    /// liteServer.dispatchQueueInfo = liteServer.DispatchQueueInfo
    /// </summary>
    public class LiteServerDispatchQueueInfo
    {
        public const uint Constructor = 0x28AA9828;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerAccountDispatchQueueInfo[] AccountDispatchQueues { get; set; } = Array.Empty<LiteServerAccountDispatchQueueInfo>();
        public bool Complete { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteUInt32((uint)AccountDispatchQueues.Length);
                foreach (var item in AccountDispatchQueues)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBool(Complete);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(Proof);
            }
        }

        public static LiteServerDispatchQueueInfo ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerDispatchQueueInfo();
            result.Mode = reader.ReadUInt32();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.AccountDispatchQueues = Array.Empty<LiteServerAccountDispatchQueueInfo>();
            result.Complete = reader.ReadBool();
            if ((result.Mode & (1u << 0)) != 0)
                result.Proof = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.dispatchQueueMessage = liteServer.DispatchQueueMessage
    /// </summary>
    public class LiteServerDispatchQueueMessage
    {
        public const uint Constructor = 0x84C423EA;

        public byte[] Addr { get; set; } = Array.Empty<byte>();
        public long Lt { get; set; }
        public byte[] Hash { get; set; } = Array.Empty<byte>();
        public LiteServerTransactionMetadata Metadata { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteBuffer(Addr);
            writer.WriteInt64(Lt);
            writer.WriteBytes(Hash, 32);
            Metadata.WriteTo(writer);
        }

        public static LiteServerDispatchQueueMessage ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerDispatchQueueMessage
            {
                Addr = reader.ReadBuffer(),
                Lt = reader.ReadInt64(),
                Hash = reader.ReadInt256(),
                Metadata = LiteServerTransactionMetadata.ReadFrom(reader),
            };
        }
    }

    /// <summary>
    /// liteServer.dispatchQueueMessages = liteServer.DispatchQueueMessages
    /// </summary>
    public class LiteServerDispatchQueueMessages
    {
        public const uint Constructor = 0x93B42D0B;

        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerDispatchQueueMessage[] Messages { get; set; } = Array.Empty<LiteServerDispatchQueueMessage>();
        public bool Complete { get; set; }
        public byte[] Proof { get; set; } = Array.Empty<byte>();
        public byte[] MessagesBoc { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteUInt32((uint)Messages.Length);
                foreach (var item in Messages)
                {
                    item.WriteTo(writer);
                }
            writer.WriteBool(Complete);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(Proof);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteBuffer(MessagesBoc);
            }
        }

        public static LiteServerDispatchQueueMessages ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerDispatchQueueMessages();
            result.Mode = reader.ReadUInt32();
            result.Id = TonNodeBlockIdExt.ReadFrom(reader);
            result.Messages = Array.Empty<LiteServerDispatchQueueMessage>();
            result.Complete = reader.ReadBool();
            if ((result.Mode & (1u << 0)) != 0)
                result.Proof = reader.ReadBuffer();
            if ((result.Mode & (1u << 2)) != 0)
                result.MessagesBoc = reader.ReadBuffer();
            return result;
        }
    }

    /// <summary>
    /// liteServer.debug.verbosity = liteServer.debug.Verbosity
    /// </summary>
    public class LiteServerDebugVerbosity
    {
        public const uint Constructor = 0x5D404733;

        public int Value { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Value);
        }

        public static LiteServerDebugVerbosity ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerDebugVerbosity
            {
                Value = reader.ReadInt32(),
            };
        }
    }

    /// <summary>
    /// liteServer.nonfinal.candidateId = liteServer.nonfinal.CandidateId
    /// </summary>
    public class LiteServerNonfinalCandidateId
    {
        public const uint Constructor = 0x55047FEE;

        public TonNodeBlockIdExt BlockId { get; set; }
        public byte[] Creator { get; set; } = Array.Empty<byte>();
        public byte[] CollatedDataHash { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            BlockId.WriteTo(writer);
            writer.WriteBuffer(Creator);
            writer.WriteBytes(CollatedDataHash, 32);
        }

        public static LiteServerNonfinalCandidateId ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerNonfinalCandidateId
            {
                BlockId = TonNodeBlockIdExt.ReadFrom(reader),
                Creator = reader.ReadBuffer(),
                CollatedDataHash = reader.ReadInt256(),
            };
        }
    }

    /// <summary>
    /// liteServer.nonfinal.candidate = liteServer.nonfinal.Candidate
    /// </summary>
    public class LiteServerNonfinalCandidate
    {
        public const uint Constructor = 0x80C3468C;

        public LiteServerNonfinalCandidateId Id { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public byte[] CollatedData { get; set; } = Array.Empty<byte>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBuffer(Data);
            writer.WriteBuffer(CollatedData);
        }

        public static LiteServerNonfinalCandidate ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerNonfinalCandidate
            {
                Id = LiteServerNonfinalCandidateId.ReadFrom(reader),
                Data = reader.ReadBuffer(),
                CollatedData = reader.ReadBuffer(),
            };
        }
    }

    /// <summary>
    /// liteServer.nonfinal.candidateInfo = liteServer.nonfinal.CandidateInfo
    /// </summary>
    public class LiteServerNonfinalCandidateInfo
    {
        public const uint Constructor = 0x4DEC01D5;

        public LiteServerNonfinalCandidateId Id { get; set; }
        public bool Available { get; set; }
        public long ApprovedWeight { get; set; }
        public long SignedWeight { get; set; }
        public long TotalWeight { get; set; }

        public  void WriteTo(TLWriteBuffer writer)
        {
            Id.WriteTo(writer);
            writer.WriteBool(Available);
            writer.WriteInt64(ApprovedWeight);
            writer.WriteInt64(SignedWeight);
            writer.WriteInt64(TotalWeight);
        }

        public static LiteServerNonfinalCandidateInfo ReadFrom(TLReadBuffer reader)
        {
            return new LiteServerNonfinalCandidateInfo
            {
                Id = LiteServerNonfinalCandidateId.ReadFrom(reader),
                Available = reader.ReadBool(),
                ApprovedWeight = reader.ReadInt64(),
                SignedWeight = reader.ReadInt64(),
                TotalWeight = reader.ReadInt64(),
            };
        }
    }

    /// <summary>
    /// liteServer.nonfinal.validatorGroupInfo = liteServer.nonfinal.ValidatorGroupInfo
    /// </summary>
    public class LiteServerNonfinalValidatorGroupInfo
    {
        public const uint Constructor = 0x562ED2D1;

        public TonNodeBlockId NextBlockId { get; set; }
        public int CcSeqno { get; set; }
        public TonNodeBlockIdExt[] Prev { get; set; } = Array.Empty<TonNodeBlockIdExt>();
        public LiteServerNonfinalCandidateInfo[] Candidates { get; set; } = Array.Empty<LiteServerNonfinalCandidateInfo>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            NextBlockId.WriteTo(writer);
            writer.WriteInt32(CcSeqno);
            writer.WriteUInt32((uint)Prev.Length);
                foreach (var item in Prev)
                {
                    item.WriteTo(writer);
                }
            writer.WriteUInt32((uint)Candidates.Length);
                foreach (var item in Candidates)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerNonfinalValidatorGroupInfo ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerNonfinalValidatorGroupInfo();
            result.NextBlockId = TonNodeBlockId.ReadFrom(reader);
            result.CcSeqno = reader.ReadInt32();
            uint prevCount = reader.ReadUInt32();
            result.Prev = new TonNodeBlockIdExt[prevCount];
            for (int i = 0; i < prevCount; i++)
            {
                result.Prev[i] = TonNodeBlockIdExt.ReadFrom(reader);
            }
            uint candidatesCount = reader.ReadUInt32();
            result.Candidates = new LiteServerNonfinalCandidateInfo[candidatesCount];
            for (int i = 0; i < candidatesCount; i++)
            {
                result.Candidates[i] = LiteServerNonfinalCandidateInfo.ReadFrom(reader);
            }
            return result;
        }
    }

    /// <summary>
    /// liteServer.nonfinal.validatorGroups = liteServer.nonfinal.ValidatorGroups
    /// </summary>
    public class LiteServerNonfinalValidatorGroups
    {
        public const uint Constructor = 0x34F73081;

        public LiteServerNonfinalValidatorGroupInfo[] Groups { get; set; } = Array.Empty<LiteServerNonfinalValidatorGroupInfo>();

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32((uint)Groups.Length);
                foreach (var item in Groups)
                {
                    item.WriteTo(writer);
                }
        }

        public static LiteServerNonfinalValidatorGroups ReadFrom(TLReadBuffer reader)
        {
            var result = new LiteServerNonfinalValidatorGroups();
            uint groupsCount = reader.ReadUInt32();
            result.Groups = new LiteServerNonfinalValidatorGroupInfo[groupsCount];
            for (int i = 0; i < groupsCount; i++)
            {
                result.Groups[i] = LiteServerNonfinalValidatorGroupInfo.ReadFrom(reader);
            }
            return result;
        }
    }

    // ============================================================================
    // Request Classes (auto mode flag handling)
    // ============================================================================

    /// <summary>
    /// Request: liteServer.getMasterchainInfo = liteServer.MasterchainInfo
    /// Constructor: 0x89B5E62E
    /// </summary>
    public sealed class GetMasterchainInfoRequest : ILiteRequest
    {

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x89B5E62E); // liteServer.getMasterchainInfo
        }
    }

    /// <summary>
    /// Request: liteServer.getMasterchainInfoExt = liteServer.MasterchainInfoExt
    /// Constructor: 0x70A671DF
    /// </summary>
    public sealed class GetMasterchainInfoExtRequest : ILiteRequest
    {
        public uint Mode { get; set; }

        public GetMasterchainInfoExtRequest(uint mode)
        {
            Mode = mode;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x70A671DF); // liteServer.getMasterchainInfoExt
            writer.WriteUInt32(Mode);
        }
    }

    /// <summary>
    /// Request: liteServer.getTime = liteServer.CurrentTime
    /// Constructor: 0x16AD5A34
    /// </summary>
    public sealed class GetTimeRequest : ILiteRequest
    {

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x16AD5A34); // liteServer.getTime
        }
    }

    /// <summary>
    /// Request: liteServer.getVersion = liteServer.Version
    /// Constructor: 0x232B940B
    /// </summary>
    public sealed class GetVersionRequest : ILiteRequest
    {

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x232B940B); // liteServer.getVersion
        }
    }

    /// <summary>
    /// Request: liteServer.getBlock = liteServer.BlockData
    /// Constructor: 0x6377CF0D
    /// </summary>
    public sealed class GetBlockRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }

        public GetBlockRequest(TonNodeBlockIdExt id)
        {
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x6377CF0D); // liteServer.getBlock
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getState = liteServer.BlockState
    /// Constructor: 0xBA6E2EB6
    /// </summary>
    public sealed class GetStateRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }

        public GetStateRequest(TonNodeBlockIdExt id)
        {
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xBA6E2EB6); // liteServer.getState
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getBlockHeader = liteServer.BlockHeader
    /// Constructor: 0x21EC069E
    /// </summary>
    public sealed class GetBlockHeaderRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }

        public GetBlockHeaderRequest(TonNodeBlockIdExt id, uint mode)
        {
            Id = id;
            Mode = mode;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x21EC069E); // liteServer.getBlockHeader
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
        }
    }

    /// <summary>
    /// Request: liteServer.sendMessage = liteServer.SendMsgStatus
    /// Constructor: 0x690AD482
    /// </summary>
    public sealed class SendMessageRequest : ILiteRequest
    {
        public byte[] Body { get; set; } = Array.Empty<byte>();

        public SendMessageRequest(byte[] body)
        {
            Body = body;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x690AD482); // liteServer.sendMessage
            writer.WriteBuffer(Body);
        }
    }

    /// <summary>
    /// Request: liteServer.getAccountState = liteServer.AccountState
    /// Constructor: 0x6B890E25
    /// </summary>
    public sealed class GetAccountStateRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerAccountId Account { get; set; }

        public GetAccountStateRequest(TonNodeBlockIdExt id, LiteServerAccountId account)
        {
            Id = id;
            Account = account;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x6B890E25); // liteServer.getAccountState
            Id.WriteTo(writer);
            Account.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getAccountStatePrunned = liteServer.AccountState
    /// Constructor: 0x5A698507
    /// </summary>
    public sealed class GetAccountStatePrunnedRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerAccountId Account { get; set; }

        public GetAccountStatePrunnedRequest(TonNodeBlockIdExt id, LiteServerAccountId account)
        {
            Id = id;
            Account = account;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x5A698507); // liteServer.getAccountStatePrunned
            Id.WriteTo(writer);
            Account.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.runSmcMethod = liteServer.RunMethodResult
    /// Constructor: 0x5CC65DD2
    /// </summary>
    public sealed class RunSmcMethodRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerAccountId Account { get; set; }
        public long MethodId { get; set; }
        public byte[] @Params { get; set; } = Array.Empty<byte>();

        public RunSmcMethodRequest(uint mode, TonNodeBlockIdExt id, LiteServerAccountId account, long methodId, byte[] @params)
        {
            Mode = mode;
            Id = id;
            Account = account;
            MethodId = methodId;
            @Params = @params;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x5CC65DD2); // liteServer.runSmcMethod
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            Account.WriteTo(writer);
            writer.WriteInt64(MethodId);
            writer.WriteBuffer(@Params);
        }
    }

    /// <summary>
    /// Request: liteServer.getShardInfo = liteServer.ShardInfo
    /// Constructor: 0x46A2F425
    /// </summary>
    public sealed class GetShardInfoRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public int Workchain { get; set; }
        public long Shard { get; set; }
        public bool Exact { get; set; }

        public GetShardInfoRequest(TonNodeBlockIdExt id, int workchain, long shard, bool exact)
        {
            Id = id;
            Workchain = workchain;
            Shard = shard;
            Exact = exact;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x46A2F425); // liteServer.getShardInfo
            Id.WriteTo(writer);
            writer.WriteInt32(Workchain);
            writer.WriteInt64(Shard);
            writer.WriteBool(Exact);
        }
    }

    /// <summary>
    /// Request: liteServer.getAllShardsInfo = liteServer.AllShardsInfo
    /// Constructor: 0x74D3FD6B
    /// </summary>
    public sealed class GetAllShardsInfoRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }

        public GetAllShardsInfoRequest(TonNodeBlockIdExt id)
        {
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x74D3FD6B); // liteServer.getAllShardsInfo
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getOneTransaction = liteServer.TransactionInfo
    /// Constructor: 0xD40F24EA
    /// </summary>
    public sealed class GetOneTransactionRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public LiteServerAccountId Account { get; set; }
        public long Lt { get; set; }

        public GetOneTransactionRequest(TonNodeBlockIdExt id, LiteServerAccountId account, long lt)
        {
            Id = id;
            Account = account;
            Lt = lt;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xD40F24EA); // liteServer.getOneTransaction
            Id.WriteTo(writer);
            Account.WriteTo(writer);
            writer.WriteInt64(Lt);
        }
    }

    /// <summary>
    /// Request: liteServer.getTransactions = liteServer.TransactionList
    /// Constructor: 0x1C40E7A1
    /// </summary>
    public sealed class GetTransactionsRequest : ILiteRequest
    {
        public uint Count { get; set; }
        public LiteServerAccountId Account { get; set; }
        public long Lt { get; set; }
        public byte[] Hash { get; set; } = Array.Empty<byte>();

        public GetTransactionsRequest(uint count, LiteServerAccountId account, long lt, byte[] hash)
        {
            Count = count;
            Account = account;
            Lt = lt;
            Hash = hash;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x1C40E7A1); // liteServer.getTransactions
            writer.WriteUInt32(Count);
            Account.WriteTo(writer);
            writer.WriteInt64(Lt);
            writer.WriteBytes(Hash, 32);
        }
    }

    /// <summary>
    /// Request: liteServer.lookupBlock = liteServer.BlockHeader
    /// Constructor: 0xFAC8F71E
    /// </summary>
    public sealed class LookupBlockRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockId Id { get; set; }
        public long? Lt { get; set; }
        public int? Utime { get; set; }

        public LookupBlockRequest(uint mode, TonNodeBlockId id, long? lt = null, int? utime = null)
        {
            Mode = mode;
            Id = id;
            Lt = lt;
            Utime = utime;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xFAC8F71E); // liteServer.lookupBlock
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteInt64(Lt.Value);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteInt32(Utime.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.lookupBlockWithProof = liteServer.LookupBlockResult
    /// Constructor: 0x9C045FF8
    /// </summary>
    public sealed class LookupBlockWithProofRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockId Id { get; set; }
        public TonNodeBlockIdExt McBlockId { get; set; }
        public long? Lt { get; set; }
        public int? Utime { get; set; }

        public LookupBlockWithProofRequest(uint mode, TonNodeBlockId id, TonNodeBlockIdExt mcBlockId, long? lt = null, int? utime = null)
        {
            Mode = mode;
            Id = id;
            McBlockId = mcBlockId;
            Lt = lt;
            Utime = utime;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x9C045FF8); // liteServer.lookupBlockWithProof
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            McBlockId.WriteTo(writer);
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteInt64(Lt.Value);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteInt32(Utime.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.listBlockTransactions = liteServer.BlockTransactions
    /// Constructor: 0xADFCC7DA
    /// </summary>
    public sealed class ListBlockTransactionsRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public uint Count { get; set; }
        public LiteServerTransactionId3 After { get; set; }
        public bool? ReverseOrder { get; set; }
        public bool? WantProof { get; set; }

        public ListBlockTransactionsRequest(TonNodeBlockIdExt id, uint mode, uint count, LiteServerTransactionId3 after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            Id = id;
            Mode = mode;
            Count = count;
            After = after;
            ReverseOrder = reverseOrder;
            WantProof = wantProof;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xADFCC7DA); // liteServer.listBlockTransactions
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            writer.WriteUInt32(Count);
            if ((Mode & (1u << 7)) != 0)
            {
                After.WriteTo(writer);
            }
            if ((Mode & (1u << 6)) != 0)
            {
                writer.WriteBool(ReverseOrder.Value);
            }
            if ((Mode & (1u << 5)) != 0)
            {
                writer.WriteBool(WantProof.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.listBlockTransactionsExt = liteServer.BlockTransactionsExt
    /// Constructor: 0x0079DD5C
    /// </summary>
    public sealed class ListBlockTransactionsExtRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public uint Count { get; set; }
        public LiteServerTransactionId3 After { get; set; }
        public bool? ReverseOrder { get; set; }
        public bool? WantProof { get; set; }

        public ListBlockTransactionsExtRequest(TonNodeBlockIdExt id, uint mode, uint count, LiteServerTransactionId3 after = null, bool? reverseOrder = null, bool? wantProof = null)
        {
            Id = id;
            Mode = mode;
            Count = count;
            After = after;
            ReverseOrder = reverseOrder;
            WantProof = wantProof;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x0079DD5C); // liteServer.listBlockTransactionsExt
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            writer.WriteUInt32(Count);
            if ((Mode & (1u << 7)) != 0)
            {
                After.WriteTo(writer);
            }
            if ((Mode & (1u << 6)) != 0)
            {
                writer.WriteBool(ReverseOrder.Value);
            }
            if ((Mode & (1u << 5)) != 0)
            {
                writer.WriteBool(WantProof.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getBlockProof = liteServer.PartialBlockProof
    /// Constructor: 0x8AEA9C44
    /// </summary>
    public sealed class GetBlockProofRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt KnownBlock { get; set; }
        public TonNodeBlockIdExt TargetBlock { get; set; }

        public GetBlockProofRequest(uint mode, TonNodeBlockIdExt knownBlock, TonNodeBlockIdExt targetBlock = null)
        {
            Mode = mode;
            KnownBlock = knownBlock;
            TargetBlock = targetBlock;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x8AEA9C44); // liteServer.getBlockProof
            writer.WriteUInt32(Mode);
            KnownBlock.WriteTo(writer);
            if ((Mode & (1u << 0)) != 0)
            {
                TargetBlock.WriteTo(writer);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getConfigAll = liteServer.ConfigInfo
    /// Constructor: 0x911B26B7
    /// </summary>
    public sealed class GetConfigAllRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }

        public GetConfigAllRequest(uint mode, TonNodeBlockIdExt id)
        {
            Mode = mode;
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x911B26B7); // liteServer.getConfigAll
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getConfigParams = liteServer.ConfigInfo
    /// Constructor: 0x9EF88D63
    /// </summary>
    public sealed class GetConfigParamsRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public int[] ParamList { get; set; } = Array.Empty<int>();

        public GetConfigParamsRequest(uint mode, TonNodeBlockIdExt id, int[] paramList)
        {
            Mode = mode;
            Id = id;
            ParamList = paramList;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x9EF88D63); // liteServer.getConfigParams
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteUInt32((uint)ParamList.Length);
                foreach (var item in ParamList)
                {
                    writer.WriteInt32(item);
                }
        }
    }

    /// <summary>
    /// Request: liteServer.getValidatorStats = liteServer.ValidatorStats
    /// Constructor: 0xE7253699
    /// </summary>
    public sealed class GetValidatorStatsRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public int Limit { get; set; }
        public byte[] StartAfter { get; set; }
        public int? ModifiedAfter { get; set; }

        public GetValidatorStatsRequest(uint mode, TonNodeBlockIdExt id, int limit, byte[] startAfter = null, int? modifiedAfter = null)
        {
            Mode = mode;
            Id = id;
            Limit = limit;
            StartAfter = startAfter;
            ModifiedAfter = modifiedAfter;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xE7253699); // liteServer.getValidatorStats
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteInt32(Limit);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBuffer(StartAfter);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteInt32(ModifiedAfter.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getLibraries = liteServer.LibraryResult
    /// Constructor: 0x7E1E1899
    /// </summary>
    public sealed class GetLibrariesRequest : ILiteRequest
    {
        public byte[][] LibraryList { get; set; } = Array.Empty<byte[]>();

        public GetLibrariesRequest(byte[][] libraryList)
        {
            LibraryList = libraryList;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x7E1E1899); // liteServer.getLibraries
            writer.WriteUInt32((uint)LibraryList.Length);
                foreach (var item in LibraryList)
                {
                    writer.WriteBytes(item, 32);
                }
        }
    }

    /// <summary>
    /// Request: liteServer.getLibrariesWithProof = liteServer.LibraryResultWithProof
    /// Constructor: 0x8C026C31
    /// </summary>
    public sealed class GetLibrariesWithProofRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }
        public uint Mode { get; set; }
        public byte[][] LibraryList { get; set; } = Array.Empty<byte[]>();

        public GetLibrariesWithProofRequest(TonNodeBlockIdExt id, uint mode, byte[][] libraryList)
        {
            Id = id;
            Mode = mode;
            LibraryList = libraryList;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x8C026C31); // liteServer.getLibrariesWithProof
            Id.WriteTo(writer);
            writer.WriteUInt32(Mode);
            writer.WriteUInt32((uint)LibraryList.Length);
                foreach (var item in LibraryList)
                {
                    writer.WriteBytes(item, 32);
                }
        }
    }

    /// <summary>
    /// Request: liteServer.getShardBlockProof = liteServer.ShardBlockProof
    /// Constructor: 0x4CA60350
    /// </summary>
    public sealed class GetShardBlockProofRequest : ILiteRequest
    {
        public TonNodeBlockIdExt Id { get; set; }

        public GetShardBlockProofRequest(TonNodeBlockIdExt id)
        {
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x4CA60350); // liteServer.getShardBlockProof
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.getOutMsgQueueSizes = liteServer.OutMsgQueueSizes
    /// Constructor: 0x7BC19C36
    /// </summary>
    public sealed class GetOutMsgQueueSizesRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public int? Wc { get; set; }
        public long? Shard { get; set; }

        public GetOutMsgQueueSizesRequest(uint mode, int? wc = null, long? shard = null)
        {
            Mode = mode;
            Wc = wc;
            Shard = shard;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x7BC19C36); // liteServer.getOutMsgQueueSizes
            writer.WriteUInt32(Mode);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteInt32(Wc.Value);
            }
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteInt64(Shard.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getBlockOutMsgQueueSize = liteServer.BlockOutMsgQueueSize
    /// Constructor: 0x8F6C7779
    /// </summary>
    public sealed class GetBlockOutMsgQueueSizeRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public bool? WantProof { get; set; }

        public GetBlockOutMsgQueueSizeRequest(uint mode, TonNodeBlockIdExt id, bool? wantProof = null)
        {
            Mode = mode;
            Id = id;
            WantProof = wantProof;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x8F6C7779); // liteServer.getBlockOutMsgQueueSize
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBool(WantProof.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getDispatchQueueInfo = liteServer.DispatchQueueInfo
    /// Constructor: 0x01E66BF3
    /// </summary>
    public sealed class GetDispatchQueueInfoRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public byte[] AfterAddr { get; set; }
        public int MaxAccounts { get; set; }
        public bool? WantProof { get; set; }

        public GetDispatchQueueInfoRequest(uint mode, TonNodeBlockIdExt id, int maxAccounts, byte[] afterAddr = null, bool? wantProof = null)
        {
            Mode = mode;
            Id = id;
            MaxAccounts = maxAccounts;
            AfterAddr = afterAddr;
            WantProof = wantProof;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x01E66BF3); // liteServer.getDispatchQueueInfo
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteBuffer(AfterAddr);
            }
            writer.WriteInt32(MaxAccounts);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBool(WantProof.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.getDispatchQueueMessages = liteServer.DispatchQueueMessages
    /// Constructor: 0xBBFD6439
    /// </summary>
    public sealed class GetDispatchQueueMessagesRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public TonNodeBlockIdExt Id { get; set; }
        public byte[] Addr { get; set; } = Array.Empty<byte>();
        public long AfterLt { get; set; }
        public int MaxMessages { get; set; }
        public bool? WantProof { get; set; }
        public bool? OneAccount { get; set; }
        public bool? MessagesBoc { get; set; }

        public GetDispatchQueueMessagesRequest(uint mode, TonNodeBlockIdExt id, byte[] addr, long afterLt, int maxMessages, bool? wantProof = null, bool? oneAccount = null, bool? messagesBoc = null)
        {
            Mode = mode;
            Id = id;
            Addr = addr;
            AfterLt = afterLt;
            MaxMessages = maxMessages;
            WantProof = wantProof;
            OneAccount = oneAccount;
            MessagesBoc = messagesBoc;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xBBFD6439); // liteServer.getDispatchQueueMessages
            writer.WriteUInt32(Mode);
            Id.WriteTo(writer);
            writer.WriteBuffer(Addr);
            writer.WriteInt64(AfterLt);
            writer.WriteInt32(MaxMessages);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteBool(WantProof.Value);
            }
            if ((Mode & (1u << 1)) != 0)
            {
                writer.WriteBool(OneAccount.Value);
            }
            if ((Mode & (1u << 2)) != 0)
            {
                writer.WriteBool(MessagesBoc.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.nonfinal.getValidatorGroups = liteServer.nonfinal.ValidatorGroups
    /// Constructor: 0xA59915E3
    /// </summary>
    public sealed class NonfinalGetValidatorGroupsRequest : ILiteRequest
    {
        public uint Mode { get; set; }
        public int? Wc { get; set; }
        public long? Shard { get; set; }

        public NonfinalGetValidatorGroupsRequest(uint mode, int? wc = null, long? shard = null)
        {
            Mode = mode;
            Wc = wc;
            Shard = shard;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xA59915E3); // liteServer.nonfinal.getValidatorGroups
            writer.WriteUInt32(Mode);
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteInt32(Wc.Value);
            }
            if ((Mode & (1u << 0)) != 0)
            {
                writer.WriteInt64(Shard.Value);
            }
        }
    }

    /// <summary>
    /// Request: liteServer.nonfinal.getCandidate = liteServer.nonfinal.Candidate
    /// Constructor: 0x300794DE
    /// </summary>
    public sealed class NonfinalGetCandidateRequest : ILiteRequest
    {
        public LiteServerNonfinalCandidateId Id { get; set; }

        public NonfinalGetCandidateRequest(LiteServerNonfinalCandidateId id)
        {
            Id = id;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x300794DE); // liteServer.nonfinal.getCandidate
            Id.WriteTo(writer);
        }
    }

    /// <summary>
    /// Request: liteServer.queryPrefix = Object
    /// Constructor: 0x72D3E686
    /// </summary>
    public sealed class QueryPrefixRequest : ILiteRequest
    {

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x72D3E686); // liteServer.queryPrefix
        }
    }

    /// <summary>
    /// Request: liteServer.query = Object
    /// Constructor: 0x798C06DF
    /// </summary>
    public sealed class QueryRequest : ILiteRequest
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public QueryRequest(byte[] data)
        {
            Data = data;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0x798C06DF); // liteServer.query
            writer.WriteBuffer(Data);
        }
    }

    /// <summary>
    /// Request: liteServer.waitMasterchainSeqno = Object
    /// Constructor: 0xBAEAB892
    /// </summary>
    public sealed class WaitMasterchainSeqnoRequest : ILiteRequest
    {
        public int Seqno { get; set; }
        public int TimeoutMs { get; set; }

        public WaitMasterchainSeqnoRequest(int seqno, int timeoutMs)
        {
            Seqno = seqno;
            TimeoutMs = timeoutMs;
        }

        public void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteUInt32(0xBAEAB892); // liteServer.waitMasterchainSeqno
            writer.WriteInt32(Seqno);
            writer.WriteInt32(TimeoutMs);
        }
    }

    // ============================================================================
    // Function Constructors
    // ============================================================================

    public static class Functions
    {
        public const uint GetMasterchainInfo = 0x89B5E62E;
        public const uint GetMasterchainInfoExt = 0x70A671DF;
        public const uint GetTime = 0x16AD5A34;
        public const uint GetVersion = 0x232B940B;
        public const uint GetBlock = 0x6377CF0D;
        public const uint GetState = 0xBA6E2EB6;
        public const uint GetBlockHeader = 0x21EC069E;
        public const uint SendMessage = 0x690AD482;
        public const uint GetAccountState = 0x6B890E25;
        public const uint GetAccountStatePrunned = 0x5A698507;
        public const uint RunSmcMethod = 0x5CC65DD2;
        public const uint GetShardInfo = 0x46A2F425;
        public const uint GetAllShardsInfo = 0x74D3FD6B;
        public const uint GetOneTransaction = 0xD40F24EA;
        public const uint GetTransactions = 0x1C40E7A1;
        public const uint LookupBlock = 0xFAC8F71E;
        public const uint LookupBlockWithProof = 0x9C045FF8;
        public const uint ListBlockTransactions = 0xADFCC7DA;
        public const uint ListBlockTransactionsExt = 0x0079DD5C;
        public const uint GetBlockProof = 0x8AEA9C44;
        public const uint GetConfigAll = 0x911B26B7;
        public const uint GetConfigParams = 0x9EF88D63;
        public const uint GetValidatorStats = 0xE7253699;
        public const uint GetLibraries = 0x7E1E1899;
        public const uint GetLibrariesWithProof = 0x8C026C31;
        public const uint GetShardBlockProof = 0x4CA60350;
        public const uint GetOutMsgQueueSizes = 0x7BC19C36;
        public const uint GetBlockOutMsgQueueSize = 0x8F6C7779;
        public const uint GetDispatchQueueInfo = 0x01E66BF3;
        public const uint GetDispatchQueueMessages = 0xBBFD6439;
        public const uint NonfinalGetValidatorGroups = 0xA59915E3;
        public const uint NonfinalGetCandidate = 0x300794DE;
        public const uint QueryPrefix = 0x72D3E686;
        public const uint Query = 0x798C06DF;
        public const uint WaitMasterchainSeqno = 0xBAEAB892;
    }
}
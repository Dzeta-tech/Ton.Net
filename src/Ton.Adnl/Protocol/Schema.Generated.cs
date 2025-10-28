// Auto-generated from lite_api.tl
// DO NOT EDIT MANUALLY
// This is the protocol layer - raw TL types matching lite_api.tl exactly
// For user-facing APIs, create domain models and map in LiteClient
// Union types: Bool, adnl.Message, liteServer.BlockLink

#nullable disable

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
                case 0x5353875B:
                    return LiteServerBlockLinkBack.ReadFrom(reader);
                case 0x775A5528:
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
    public readonly struct TonNodeBlockId
    {
        public readonly int Workchain;
        public readonly long Shard;
        public readonly int Seqno;

        public TonNodeBlockId(int workchain, long shard, int seqno)
        {
            Workchain = workchain;
            Shard = shard;
            Seqno = seqno;
        }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteInt64(Shard);
            writer.WriteInt32(Seqno);
        }

        public static TonNodeBlockId ReadFrom(TLReadBuffer reader)
        {
            return new TonNodeBlockId(
                reader.ReadInt32(),
                reader.ReadInt64(),
                reader.ReadInt32()
            );
        }
    }

    /// <summary>
    /// tonNode.blockIdExt = tonNode.BlockIdExt
    /// </summary>
    public readonly struct TonNodeBlockIdExt
    {
        public readonly int Workchain;
        public readonly long Shard;
        public readonly int Seqno;
        public readonly byte[] RootHash;
        public readonly byte[] FileHash;

        public TonNodeBlockIdExt(int workchain, long shard, int seqno, byte[] rootHash, byte[] fileHash)
        {
            Workchain = workchain;
            Shard = shard;
            Seqno = seqno;
            RootHash = rootHash;
            FileHash = fileHash;
        }

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
            return new TonNodeBlockIdExt(
                reader.ReadInt32(),
                reader.ReadInt64(),
                reader.ReadInt32(),
                reader.ReadInt256(),
                reader.ReadInt256()
            );
        }
    }

    /// <summary>
    /// tonNode.zeroStateIdExt = tonNode.ZeroStateIdExt
    /// </summary>
    public readonly struct TonNodeZeroStateIdExt
    {
        public readonly int Workchain;
        public readonly byte[] RootHash;
        public readonly byte[] FileHash;

        public TonNodeZeroStateIdExt(int workchain, byte[] rootHash, byte[] fileHash)
        {
            Workchain = workchain;
            RootHash = rootHash;
            FileHash = fileHash;
        }

        public  void WriteTo(TLWriteBuffer writer)
        {
            writer.WriteInt32(Workchain);
            writer.WriteBytes(RootHash, 32);
            writer.WriteBytes(FileHash, 32);
        }

        public static TonNodeZeroStateIdExt ReadFrom(TLReadBuffer reader)
        {
            return new TonNodeZeroStateIdExt(
                reader.ReadInt32(),
                reader.ReadInt256(),
                reader.ReadInt256()
            );
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
        public const uint Constructor = 0x1BB566EA;

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
        public const uint Constructor = 0x88729074;

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
        public const uint Constructor = 0xFC3C1D28;

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
        public const uint Constructor = 0xF9333637;

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
        public const uint Constructor = 0xAE76CCDA;

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
        public const uint Constructor = 0x1D512914;

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
        public const uint Constructor = 0xB33314CF;

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
        public const uint Constructor = 0x27A85F37;

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
        public const uint Constructor = 0x6A14E75E;

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
        public const uint Constructor = 0x071783EB;

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
        public const uint Constructor = 0x0D5B50AB;

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
        public const uint Constructor = 0x7F151E0C;

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
        public const uint Constructor = 0xB9CA2418;

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
        public const uint Constructor = 0x8943A75D;

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
        public const uint Constructor = 0x26DFD53B;

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
        public const uint Constructor = 0x8BBF0C77;

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
        public const uint Constructor = 0xED0EC787;

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
        public const uint Constructor = 0xFE240165;

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
        public const uint Constructor = 0xE944EBD2;

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
        public const uint Constructor = 0xAD4463EC;

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
        public const uint Constructor = 0x01FB4F1A;

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
        public const uint Constructor = 0xC495AF34;

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
        public const uint Constructor = 0x78AB7D2A;

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
        public const uint Constructor = 0x0DF0E11B;

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
        public override uint Constructor => 0x5353875B;

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
        public override uint Constructor => 0x775A5528;

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
        public const uint Constructor = 0xF3BB3510;

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
        public const uint Constructor = 0xC87640D7;

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
        public const uint Constructor = 0xEBB8ABD9;

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
        public const uint Constructor = 0x6A34CEC1;

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
        public const uint Constructor = 0xEE983C56;

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
        public const uint Constructor = 0xDDD11B76;

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
        public const uint Constructor = 0x330401A1;

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
        public const uint Constructor = 0x8850F75A;

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
        public const uint Constructor = 0xFE7CB74A;

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
        public const uint Constructor = 0x2DE458AE;

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
        public const uint Constructor = 0xE9E602FB;

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
        public const uint Constructor = 0x3F213E07;

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
        public const uint Constructor = 0x569404CB;

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
        public const uint Constructor = 0x2352C9EC;

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
    /// liteServer.debug.verbosity = liteServer.debug.Verbosity
    /// </summary>
    public class LiteServerDebugVerbosity
    {
        public const uint Constructor = 0xDC8427F8;

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
        public const uint Constructor = 0x24EECDA9;

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
        public const uint Constructor = 0x87870AE4;

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
        public const uint Constructor = 0x95FDCCF3;

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
        public const uint Constructor = 0x928BCA39;

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
        public const uint Constructor = 0xF982422F;

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
    // Function Constructors
    // ============================================================================

    public static class Functions
    {
        public const uint GetMasterchainInfo = 0xBF56BE80;
        public const uint GetMasterchainInfoExt = 0x75156F9D;
        public const uint GetTime = 0x42AB5F46;
        public const uint GetVersion = 0xF4F8F4B5;
        public const uint GetBlock = 0x1DDB0DDB;
        public const uint GetState = 0x41B17E3E;
        public const uint GetBlockHeader = 0x749F54EC;
        public const uint SendMessage = 0x60D6EE71;
        public const uint GetAccountState = 0x28665BE0;
        public const uint GetAccountStatePrunned = 0xFD37FA8F;
        public const uint RunSmcMethod = 0x0B88730C;
        public const uint GetShardInfo = 0x284B701A;
        public const uint GetAllShardsInfo = 0xB91D6D84;
        public const uint GetOneTransaction = 0x230202EB;
        public const uint GetTransactions = 0xC2C4D530;
        public const uint LookupBlock = 0x99FCF33D;
        public const uint LookupBlockWithProof = 0xD0F378D8;
        public const uint ListBlockTransactions = 0x05A2C1A4;
        public const uint ListBlockTransactionsExt = 0x01D462AB;
        public const uint GetBlockProof = 0x123269BC;
        public const uint GetConfigAll = 0x369D3BA0;
        public const uint GetConfigParams = 0xB72CCEC6;
        public const uint GetValidatorStats = 0xEA3D087F;
        public const uint GetLibraries = 0xEAA43351;
        public const uint GetLibrariesWithProof = 0x325B04FF;
        public const uint GetShardBlockProof = 0x082EF15E;
        public const uint GetOutMsgQueueSizes = 0xACC852AC;
        public const uint GetBlockOutMsgQueueSize = 0x4A5FA346;
        public const uint GetDispatchQueueInfo = 0x47BC4364;
        public const uint WantProofMode0True = 0x77A2BE1D;
        public const uint NonfinalGetValidatorGroups = 0x5AAF2C7E;
        public const uint NonfinalGetCandidate = 0x0252FEEE;
        public const uint QueryPrefix = 0x67A0F35A;
        public const uint Query = 0x751C45EA;
        public const uint WaitMasterchainSeqno = 0x7DB21E79;
    }
}
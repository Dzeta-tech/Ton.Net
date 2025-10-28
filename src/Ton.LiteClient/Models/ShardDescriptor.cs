namespace Ton.LiteClient.Models;

/// <summary>
/// Represents a single shard descriptor with its location and sequence number
/// </summary>
public readonly record struct ShardDescriptor
{
    /// <summary>
    /// Workchain ID
    /// </summary>
    public int Workchain { get; init; }

    /// <summary>
    /// Shard identifier (64-bit)
    /// </summary>
    public long ShardId { get; init; }

    /// <summary>
    /// Shard sequence number
    /// </summary>
    public int Seqno { get; init; }

    public ShardDescriptor(int workchain, long shardId, int seqno)
    {
        Workchain = workchain;
        ShardId = shardId;
        Seqno = seqno;
    }

    /// <summary>
    /// Returns the shard ID as a hex string
    /// </summary>
    public string ShardIdHex => $"0x{ShardId:X16}";

    public override string ToString() =>
        $"Shard(wc:{Workchain}, shard:{ShardIdHex}, seqno:{Seqno})";
}

/// <summary>
/// Collection of shard descriptors with helper methods
/// </summary>
public sealed class ShardCollection
{
    readonly List<ShardDescriptor> shards;

    public ShardCollection(IEnumerable<ShardDescriptor> shards)
    {
        this.shards = shards.ToList();
    }

    public ShardCollection()
    {
        this.shards = new List<ShardDescriptor>();
    }

    /// <summary>
    /// Gets all shards in the collection
    /// </summary>
    public IReadOnlyList<ShardDescriptor> All => shards;

    /// <summary>
    /// Gets shards for a specific workchain
    /// </summary>
    public IEnumerable<ShardDescriptor> GetByWorkchain(int workchain) =>
        shards.Where(s => s.Workchain == workchain);

    /// <summary>
    /// Gets a shard by workchain and shard ID
    /// </summary>
    public ShardDescriptor? GetShard(int workchain, long shardId) =>
        shards.FirstOrDefault(s => s.Workchain == workchain && s.ShardId == shardId);

    /// <summary>
    /// Gets the seqno for a specific shard
    /// </summary>
    public int? GetSeqno(int workchain, long shardId) =>
        GetShard(workchain, shardId)?.Seqno;

    /// <summary>
    /// Returns all unique workchains present in the collection
    /// </summary>
    public IEnumerable<int> GetWorkchains() =>
        shards.Select(s => s.Workchain).Distinct();

    /// <summary>
    /// Gets the total number of shards
    /// </summary>
    public int Count => shards.Count;

    /// <summary>
    /// Adds a shard to the collection
    /// </summary>
    public void Add(ShardDescriptor shard)
    {
        shards.Add(shard);
    }

    /// <summary>
    /// Adds multiple shards to the collection
    /// </summary>
    public void AddRange(IEnumerable<ShardDescriptor> shardsToAdd)
    {
        shards.AddRange(shardsToAdd);
    }

    public override string ToString() =>
        $"ShardCollection(count:{Count}, workchains:[{string.Join(", ", GetWorkchains())}])";
}


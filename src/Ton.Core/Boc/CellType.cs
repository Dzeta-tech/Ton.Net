namespace Ton.Core.Boc;

/// <summary>
///     Type of cell as defined in TVM specification.
/// </summary>
public enum CellType
{
    /// <summary>
    ///     Ordinary cell containing data and references.
    /// </summary>
    Ordinary = -1,

    /// <summary>
    ///     Pruned branch cell - represents a pruned subtree in Merkle proofs.
    /// </summary>
    PrunedBranch = 1,

    /// <summary>
    ///     Library cell - contains a library reference.
    /// </summary>
    Library = 2,

    /// <summary>
    ///     Merkle proof cell - root of a Merkle proof.
    /// </summary>
    MerkleProof = 3,

    /// <summary>
    ///     Merkle update cell - represents a Merkle update operation.
    /// </summary>
    MerkleUpdate = 4
}
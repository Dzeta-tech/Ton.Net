using System.Text.Json.Serialization;

namespace Ton.HttpClient.Api.Models;

/// <summary>
///     Storage statistics for account.
/// </summary>
public record StorageStat(
    [property: JsonPropertyName("lastPaid")]
    int LastPaid,
    [property: JsonPropertyName("duePayment")]
    string? DuePayment,
    [property: JsonPropertyName("used")] StorageUsedStat Used
);

/// <summary>
///     Storage used statistics.
/// </summary>
public record StorageUsedStat(
    [property: JsonPropertyName("bits")] int Bits,
    [property: JsonPropertyName("cells")] int Cells,
    [property: JsonPropertyName("publicCells")]
    int? PublicCells
);

/// <summary>
///     Account balance information.
/// </summary>
public record AccountBalance(
    [property: JsonPropertyName("coins")] string Coins,
    [property: JsonPropertyName("currencies")]
    Dictionary<string, string>? Currencies
);

/// <summary>
///     Last transaction info.
/// </summary>
public record LastTransactionInfo(
    [property: JsonPropertyName("lt")] string Lt,
    [property: JsonPropertyName("hash")] string Hash
);

/// <summary>
///     Account state - union type for uninit/active/frozen.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AccountStateUninit), "uninit")]
[JsonDerivedType(typeof(AccountStateActive), "active")]
[JsonDerivedType(typeof(AccountStateFrozen), "frozen")]
public abstract record AccountState;

public record AccountStateUninit : AccountState;

public record AccountStateActive(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("data")] string? Data,
    [property: JsonPropertyName("codeHash")]
    string? CodeHash,
    [property: JsonPropertyName("dataHash")]
    string? DataHash
) : AccountState;

public record AccountStateFrozen(
    [property: JsonPropertyName("stateHash")]
    string StateHash
) : AccountState;

/// <summary>
///     Account details.
/// </summary>
public record AccountDetails(
    [property: JsonPropertyName("state")] AccountState State,
    [property: JsonPropertyName("balance")]
    AccountBalance Balance,
    [property: JsonPropertyName("last")] LastTransactionInfo? Last,
    [property: JsonPropertyName("storageStat")]
    StorageStat? StorageStat
);

/// <summary>
///     Block reference info.
/// </summary>
public record BlockRef(
    [property: JsonPropertyName("workchain")]
    int Workchain,
    [property: JsonPropertyName("seqno")] int Seqno,
    [property: JsonPropertyName("shard")] string Shard,
    [property: JsonPropertyName("rootHash")]
    string RootHash,
    [property: JsonPropertyName("fileHash")]
    string FileHash
);

/// <summary>
///     Account info response from v4 API.
/// </summary>
public record AccountInfo(
    [property: JsonPropertyName("account")]
    AccountDetails Account,
    [property: JsonPropertyName("block")] BlockRef Block
);

/// <summary>
///     Account lite info response (without code/data).
/// </summary>
public record AccountLiteInfo(
    [property: JsonPropertyName("account")]
    AccountDetails Account
);

/// <summary>
///     Account changed response.
/// </summary>
public record AccountChanged(
    [property: JsonPropertyName("changed")]
    bool Changed,
    [property: JsonPropertyName("block")] BlockRef Block
);
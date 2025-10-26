namespace Ton.Core.Contracts;

/// <summary>
///     ABI error definition.
/// </summary>
public record ABIError(string Message);

/// <summary>
///     Type reference in ABI.
/// </summary>
public abstract record ABITypeRef
{
    /// <summary>
    ///     Simple type reference (e.g., "int", "address", "cell").
    /// </summary>
    public record Simple(
        string Type,
        bool? Optional = null,
        object? Format = null
    ) : ABITypeRef;

    /// <summary>
    ///     Dictionary type reference.
    /// </summary>
    public record Dict(
        string Key,
        string Value,
        object? Format = null,
        object? KeyFormat = null,
        object? ValueFormat = null
    ) : ABITypeRef;
}

/// <summary>
///     Field definition in an ABI type.
/// </summary>
public record ABIField(string Name, ABITypeRef Type);

/// <summary>
///     Type definition in ABI.
/// </summary>
public record ABIType(string Name, int? Header, ABIField[] Fields);

/// <summary>
///     Argument definition for getters.
/// </summary>
public record ABIArgument(string Name, ABITypeRef Type);

/// <summary>
///     Getter method definition.
/// </summary>
public record ABIGetter(
    string Name,
    int? MethodId = null,
    ABIArgument[]? Arguments = null,
    ABITypeRef? ReturnType = null
);

/// <summary>
///     Receiver message type.
/// </summary>
public abstract record ABIReceiverMessage
{
    /// <summary>
    ///     Typed message (specific message type).
    /// </summary>
    public record Typed(string Type) : ABIReceiverMessage;

    /// <summary>
    ///     Any message.
    /// </summary>
    public record Any : ABIReceiverMessage;

    /// <summary>
    ///     Empty message.
    /// </summary>
    public record Empty : ABIReceiverMessage;

    /// <summary>
    ///     Text message (optional specific text).
    /// </summary>
    public record Text(string? TextValue = null) : ABIReceiverMessage;
}

/// <summary>
///     Receiver definition.
/// </summary>
public record ABIReceiver(
    string Receiver, // "internal" or "external"
    ABIReceiverMessage Message
);

/// <summary>
///     Contract ABI definition.
///     Used for code generation, tooling, and documentation.
/// </summary>
public record ContractABI
{
    /// <summary>
    ///     Contract name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Custom type definitions.
    /// </summary>
    public ABIType[]? Types { get; init; }

    /// <summary>
    ///     Error code to error message mapping.
    /// </summary>
    public Dictionary<int, ABIError>? Errors { get; init; }

    /// <summary>
    ///     Get method definitions.
    /// </summary>
    public ABIGetter[]? Getters { get; init; }

    /// <summary>
    ///     Receiver (message handler) definitions.
    /// </summary>
    public ABIReceiver[]? Receivers { get; init; }
}
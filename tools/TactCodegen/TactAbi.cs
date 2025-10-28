using System.Text.Json;
using System.Text.Json.Serialization;

namespace TactCodegen;

public record TactAbi(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("types")] List<TactType> Types,
    [property: JsonPropertyName("receivers")] List<TactReceiver> Receivers,
    [property: JsonPropertyName("getters")] List<TactGetter> Getters,
    [property: JsonPropertyName("errors")] Dictionary<string, TactError> Errors
);

public record TactType(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("header")] long? Header,
    [property: JsonPropertyName("fields")] List<TactField> Fields
);

public record TactField(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] TactFieldType Type
);

public record TactFieldType(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("optional")] bool Optional,
    [property: JsonPropertyName("format")] JsonElement? Format
);

public record TactReceiver(
    [property: JsonPropertyName("receiver")] string Receiver,
    [property: JsonPropertyName("message")] TactMessage Message
);

public record TactMessage(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("type")] string? Type
);

public record TactGetter(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("arguments")] List<TactGetterArg> Arguments,
    [property: JsonPropertyName("returnType")] TactFieldType? ReturnType
);

public record TactGetterArg(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] TactFieldType Type
);

public record TactError(
    [property: JsonPropertyName("message")] string Message
);

public record TactPackage(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("abi")] string? Abi
);


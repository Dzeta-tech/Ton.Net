using System.Text;

namespace TactCodegen;

public class CSharpContractGenerator
{
    // Standard types from Ton.Core that should not be generated
    private static readonly HashSet<string> StandardTypes = new()
    {
        "StateInit", "Context", "SendParameters", "MessageParameters", 
        "DeployParameters", "StdAddress", "VarAddress", "BasechainAddress",
        "DataSize", "SignedBundle"
    };

    public string Generate(TactAbi abi, string? codeBoc = null)
    {
        var sb = new StringBuilder();
        
        // File header
        sb.AppendLine("// Auto-generated from Tact ABI");
        sb.AppendLine("// DO NOT EDIT MANUALLY");
        sb.AppendLine();
        sb.AppendLine("using Ton.Core.Boc;");
        sb.AppendLine("using Ton.Core.Addresses;");
        sb.AppendLine("using Ton.Core.Types;");
        sb.AppendLine("using Ton.Core.Contracts;");
        sb.AppendLine("using System.Numerics;");
        sb.AppendLine();
        sb.AppendLine($"namespace Generated.Contracts;");
        sb.AppendLine();
        
        // Generate message/struct types (skip standard types and internal data types)
        foreach (var type in abi.Types)
        {
            if (type.Name.EndsWith("$Data")) continue; // Skip internal data types
            if (StandardTypes.Contains(type.Name)) continue; // Skip standard SDK types
            GenerateType(sb, type);
        }
        
        // Generate contract class
        GenerateContract(sb, abi, codeBoc);
        
        return sb.ToString();
    }

    private void GenerateType(StringBuilder sb, TactType type)
    {
        sb.AppendLine($"public record {type.Name}(");
        
        for (int i = 0; i < type.Fields.Count; i++)
        {
            var field = type.Fields[i];
            var csType = MapToCSharpType(field.Type);
            var comma = i < type.Fields.Count - 1 ? "," : "";
            sb.AppendLine($"    {csType} {ToPascalCase(field.Name)}{comma}");
        }
        
        sb.AppendLine(")");
        sb.AppendLine("{");
        
        // Add opcode if it's a message
        if (type.Header.HasValue)
        {
            sb.AppendLine($"    public const uint OpCode = 0x{type.Header.Value:X8};");
            sb.AppendLine();
        }
        
        // Generate Store method
        GenerateStoreMethod(sb, type);
        sb.AppendLine();
        
        // Generate Load method
        GenerateLoadMethod(sb, type);
        
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private void GenerateStoreMethod(StringBuilder sb, TactType type)
    {
        sb.AppendLine("    public void Store(Builder builder)");
        sb.AppendLine("    {");
        
        if (type.Header.HasValue)
        {
            sb.AppendLine($"        builder.StoreUint(OpCode, 32);");
        }
        
        foreach (var field in type.Fields)
        {
            var fieldName = ToPascalCase(field.Name);
            GenerateStoreField(sb, field, fieldName);
        }
        
        sb.AppendLine("    }");
    }

    private void GenerateStoreField(StringBuilder sb, TactField field, string fieldName)
    {
        var fieldType = field.Type;
        
        if (fieldType.Optional)
        {
            sb.AppendLine($"        if ({fieldName} != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            builder.StoreBit(true);");
        }
        
        string indent = fieldType.Optional ? "            " : "        ";
        
        switch (fieldType.Type)
        {
            case "bool":
                sb.AppendLine($"{indent}builder.StoreBit({fieldName});");
                break;
            case "int":
                var intFormat = GetIntFormat(fieldType);
                sb.AppendLine($"{indent}builder.StoreInt({fieldName}, {intFormat.bits});");
                break;
            case "uint":
                var uintFormat = GetIntFormat(fieldType);
                sb.AppendLine($"{indent}builder.StoreUint({fieldName}, {uintFormat.bits});");
                break;
            case "address":
                sb.AppendLine($"{indent}builder.StoreAddress({fieldName});");
                break;
            case "cell":
                sb.AppendLine($"{indent}builder.StoreRef({fieldName});");
                break;
            case "slice":
                sb.AppendLine($"{indent}builder.StoreSlice({fieldName});");
                break;
            case "fixed-bytes":
                var bytesLen = GetIntFormat(fieldType).bits / 8;
                sb.AppendLine($"{indent}builder.StoreBytes({fieldName});");
                break;
            default:
                if (IsCustomType(fieldType.Type))
                {
                    sb.AppendLine($"{indent}{fieldName}.Store(builder);");
                }
                break;
        }
        
        if (fieldType.Optional)
        {
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            sb.AppendLine("            builder.StoreBit(false);");
            sb.AppendLine("        }");
        }
    }

    private void GenerateLoadMethod(StringBuilder sb, TactType type)
    {
        sb.AppendLine($"    public static {type.Name} Load(Slice slice)");
        sb.AppendLine("    {");
        
        if (type.Header.HasValue)
        {
            sb.AppendLine("        var opcode = slice.LoadUint(32);");
            sb.AppendLine($"        if (opcode != OpCode) throw new InvalidOperationException($\"Invalid opcode: {{opcode}}\");");
            sb.AppendLine();
        }
        
        foreach (var field in type.Fields)
        {
            GenerateLoadField(sb, field);
        }
        
        sb.AppendLine();
        sb.Append("        return new ");
        sb.Append(type.Name);
        sb.Append("(");
        
        for (int i = 0; i < type.Fields.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(ToCamelCase(type.Fields[i].Name));
        }
        
        sb.AppendLine(");");
        sb.AppendLine("    }");
    }

    private void GenerateLoadField(StringBuilder sb, TactField field)
    {
        var varName = ToCamelCase(field.Name);
        var fieldType = field.Type;
        
        if (fieldType.Optional)
        {
            sb.AppendLine($"        var has{ToPascalCase(field.Name)} = slice.LoadBit();");
            sb.AppendLine($"        {MapToCSharpType(fieldType)} {varName} = null;");
            sb.AppendLine($"        if (has{ToPascalCase(field.Name)})");
            sb.AppendLine("        {");
        }
        else
        {
            sb.Append($"        var {varName} = ");
        }
        
        string indent = fieldType.Optional ? "            " : "";
        string assign = fieldType.Optional ? $"{varName} = " : "";
        
        switch (fieldType.Type)
        {
            case "bool":
                sb.AppendLine($"{indent}{assign}slice.LoadBit();");
                break;
            case "int":
                var intFormat = GetIntFormat(fieldType);
                if (intFormat.bits <= 64)
                    sb.AppendLine($"{indent}{assign}slice.LoadInt({intFormat.bits});");
                else
                    sb.AppendLine($"{indent}{assign}slice.LoadIntBig({intFormat.bits});");
                break;
            case "uint":
                var uintFormat = GetIntFormat(fieldType);
                if (uintFormat.bits <= 64)
                    sb.AppendLine($"{indent}{assign}(ulong)slice.LoadUint({uintFormat.bits});");
                else
                    sb.AppendLine($"{indent}{assign}slice.LoadUintBig({uintFormat.bits});");
                break;
            case "address":
                sb.AppendLine($"{indent}{assign}slice.LoadAddress()!;");
                break;
            case "cell":
                sb.AppendLine($"{indent}{assign}slice.LoadRef();");
                break;
            case "slice":
                sb.AppendLine($"{indent}{assign}slice.LoadRemainder();");
                break;
            case "fixed-bytes":
                var bytesLen = GetIntFormat(fieldType).bits / 8;
                sb.AppendLine($"{indent}{assign}slice.LoadBytes({bytesLen});");
                break;
            default:
                if (IsCustomType(fieldType.Type))
                {
                    sb.AppendLine($"{indent}{assign}{fieldType.Type}.Load(slice);");
                }
                break;
        }
        
        if (fieldType.Optional)
        {
            sb.AppendLine("        }");
        }
    }

    private void GenerateContract(StringBuilder sb, TactAbi abi, string? codeBoc)
    {
        var contractName = abi.Name;
        var dataType = abi.Types.FirstOrDefault(t => t.Name.EndsWith("$Data"));
        
        sb.AppendLine($"public class {contractName} : IContract");
        sb.AppendLine("{");
        
        // Static Code cell
        if (!string.IsNullOrEmpty(codeBoc))
        {
            sb.AppendLine($"    public static readonly Cell Code = Cell.FromBoc(Convert.FromBase64String(\"{codeBoc}\"))[0];");
        }
        else
        {
            sb.AppendLine("    // TODO: Add contract code BOC here");
            sb.AppendLine("    public static readonly Cell Code = Cell.FromBoc(Convert.FromBase64String(\"\"))[0];");
        }
        sb.AppendLine();
        
        // Constructor
        sb.AppendLine("    public Address Address { get; }");
        sb.AppendLine("    public StateInit? Init { get; }");
        sb.AppendLine("    public ContractABI? ABI => null; // TODO: Implement ABI");
        sb.AppendLine();
        sb.AppendLine($"    public {contractName}(Address address, StateInit? init = null)");
        sb.AppendLine("    {");
        sb.AppendLine("        Address = address;");
        sb.AppendLine("        Init = init;");
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // Static Create method
        if (dataType != null)
        {
            sb.Append($"    public static {contractName} Create(");
            for (int i = 0; i < dataType.Fields.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var field = dataType.Fields[i];
                sb.Append($"{MapToCSharpType(field.Type)} {ToCamelCase(field.Name)}");
            }
            sb.AppendLine(")");
            sb.AppendLine("    {");
            sb.AppendLine("        var builder = Builder.BeginCell();");
            
            foreach (var field in dataType.Fields)
            {
                var fieldName = ToCamelCase(field.Name);
                GenerateStoreField(sb, field, fieldName);
            }
            
            sb.AppendLine("        var dataCell = builder.EndCell();");
            sb.AppendLine("        var init = new StateInit(");
            sb.AppendLine("            code: Code,");
            sb.AppendLine("            data: dataCell");
            sb.AppendLine("        );");
            sb.AppendLine("        var address = ContractAddress.From(0, init);");
            sb.AppendLine($"        return new {contractName}(address, init);");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        
        // Send methods for each receiver
        foreach (var receiver in abi.Receivers)
        {
            if (receiver.Message.Kind == "typed" && receiver.Message.Type != null)
            {
                var msgType = receiver.Message.Type;
                sb.AppendLine($"    public async Task Send{msgType}Async(IContractProvider provider, ISender sender, {msgType} message, BigInteger value, bool bounce = true)");
                sb.AppendLine("    {");
                sb.AppendLine("        var body = Builder.BeginCell();");
                sb.AppendLine("        message.Store(body);");
                sb.AppendLine("        var bodyCell = body.EndCell();");
                sb.AppendLine();
                sb.AppendLine("        await provider.InternalAsync(sender, new InternalMessageArgs");
                sb.AppendLine("        {");
                sb.AppendLine("            To = Address,");
                sb.AppendLine("            Value = value,");
                sb.AppendLine("            Bounce = bounce,");
                sb.AppendLine("            Body = bodyCell,");
                sb.AppendLine("            SendMode = SendMode.SendPayFwdFeesSeparately | SendMode.SendIgnoreErrors");
                sb.AppendLine("        });");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }
        
        // Get methods
        foreach (var getter in abi.Getters)
        {
            sb.Append($"    public async Task<{MapToCSharpType(getter.ReturnType)}> Get{ToPascalCase(getter.Name)}Async(IContractProvider provider");
            
            foreach (var arg in getter.Arguments)
            {
                sb.Append($", {MapToCSharpType(arg.Type)} {ToCamelCase(arg.Name)}");
            }
            
            sb.AppendLine(")");
            sb.AppendLine("    {");
            sb.AppendLine($"        var result = await provider.GetAsync(\"{getter.Name}\", [/* TODO: build arguments */]);");
            sb.AppendLine("        // TODO: Parse result");
            sb.AppendLine("        throw new NotImplementedException();");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
        
        sb.AppendLine("}");
    }

    private string MapToCSharpType(TactFieldType? fieldType)
    {
        if (fieldType == null) return "object";
        
        var nullable = fieldType.Optional ? "?" : "";
        
        return fieldType.Type switch
        {
            "bool" => $"bool{nullable}",
            "int" => GetIntFormat(fieldType).bits <= 64 ? $"long{nullable}" : $"BigInteger{nullable}",
            "uint" => GetIntFormat(fieldType).bits <= 64 ? $"ulong{nullable}" : $"BigInteger{nullable}",
            "address" => $"Address{nullable}",
            "cell" => $"Cell{nullable}",
            "slice" => $"Slice{nullable}",
            "fixed-bytes" => $"byte[]{nullable}",
            _ => IsCustomType(fieldType.Type) ? $"{fieldType.Type}{nullable}" : $"object{nullable}"
        };
    }

    private (int bits, bool signed) GetIntFormat(TactFieldType fieldType)
    {
        if (fieldType.Format == null) return (257, true);
        
        if (fieldType.Format.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            return (fieldType.Format.Value.GetInt32(), fieldType.Type == "int");
        }
        
        return (257, true);
    }

    private bool IsCustomType(string? typeName)
    {
        if (typeName == null) return false;
        
        var primitives = new[] { "bool", "int", "uint", "address", "cell", "slice", "fixed-bytes" };
        return !primitives.Contains(typeName);
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLower(input[0]) + input.Substring(1);
    }
}


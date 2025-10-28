using System.Text;
using System.Text.Json;

namespace TactCodegen;

/// <summary>
/// Generates C# contract wrappers from Tact compiler ABI output
/// Usage: dotnet run -- path/to/Contract.abi
/// Or: dotnet run -- path/to/Contract.pkg (preferred - includes code BOC)
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: dotnet run -- path/to/Contract.abi");
            Console.WriteLine("   or: dotnet run -- path/to/Contract.pkg (preferred)");
            return;
        }

        var inputPath = args[0];
        if (!File.Exists(inputPath))
        {
            Console.WriteLine($"File not found: {inputPath}");
            return;
        }

        TactPackage? pkg = null;
        TactAbi? abi = null;
        string? codeBoc = null;
        
        var json = await File.ReadAllTextAsync(inputPath);
        
        // Try to parse as package first (contains code + ABI)
        try
        {
            pkg = JsonSerializer.Deserialize<TactPackage>(json);
            if (pkg?.Abi != null)
            {
                abi = JsonSerializer.Deserialize<TactAbi>(pkg.Abi);
                codeBoc = pkg.Code;
            }
        }
        catch
        {
            // Not a package, try as plain ABI
        }
        
        // If not a package, try as plain ABI
        if (abi == null)
        {
            abi = JsonSerializer.Deserialize<TactAbi>(json);
        }
        
        if (abi == null)
        {
            Console.WriteLine("Failed to parse ABI or package");
            return;
        }

        var generator = new CSharpContractGenerator();
        var code = generator.Generate(abi, codeBoc);

        var outputPath = Path.ChangeExtension(inputPath, ".cs");
        await File.WriteAllTextAsync(outputPath, code);
        
        Console.WriteLine($"Generated: {outputPath}");
        if (codeBoc != null)
        {
            Console.WriteLine("✓ Contract code included");
        }
        else
        {
            Console.WriteLine("⚠ Contract code not found - you'll need to add it manually");
        }
    }
}


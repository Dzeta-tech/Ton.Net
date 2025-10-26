namespace Ton.Core.Contracts;

/// <summary>
///     Exception thrown when a contract compute phase fails.
///     Contains the exit code and optional logs for debugging.
/// </summary>
public class ComputeError : Exception
{
    /// <summary>
    ///     Creates a new ComputeError.
    /// </summary>
    public ComputeError(string message, int exitCode, string? debugLogs = null, string? logs = null)
        : base(message)
    {
        ExitCode = exitCode;
        DebugLogs = debugLogs;
        Logs = logs;
    }

    /// <summary>
    ///     TVM exit code from the compute phase.
    ///     Common codes:
    ///     - 0-1: Success
    ///     - 2-31: Stack errors
    ///     - 32-63: Exception errors
    ///     - 64-127: Custom contract errors
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    ///     Debug logs from the execution (if available).
    /// </summary>
    public string? DebugLogs { get; }

    /// <summary>
    ///     Execution logs (if available).
    /// </summary>
    public string? Logs { get; }

    /// <summary>
    ///     Creates a ComputeError with a default message based on exit code.
    /// </summary>
    public static ComputeError FromExitCode(int exitCode, string? debugLogs = null, string? logs = null)
    {
        string message = exitCode switch
        {
            0 or 1 => "Success",
            2 => "Stack underflow",
            3 => "Stack overflow",
            4 => "Integer overflow",
            5 => "Integer out of range",
            6 => "Invalid opcode",
            7 => "Type check error",
            8 => "Cell overflow",
            9 => "Cell underflow",
            10 => "Dictionary error",
            13 => "Out of gas",
            32 => "Action list invalid",
            33 => "Action invalid",
            34 => "Invalid source address",
            35 => "Invalid destination address",
            36 => "Not enough TON",
            37 => "Not enough extra currencies",
            38 => "Not enough funds to process message",
            _ => $"Exit code {exitCode}"
        };

        return new ComputeError(message, exitCode, debugLogs, logs);
    }
}
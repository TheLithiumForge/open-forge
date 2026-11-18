using System.Collections.Immutable;

namespace OpenForge.Cli.Composition.Shared.Git;

/// <summary>Reads Git process results with one rule for warnings and one reader for NUL-separated lists.</summary>
internal static class GitOutput
{
    /// <summary>
    /// A call that exits 0 but writes to stderr is untrusted: Git warned, so its stdout may not describe the selected work tree.
    /// </summary>
    internal static bool IsUntrusted((int ExitCode, string Output, string Error)? result)
        => result is { ExitCode: 0 } read && !string.IsNullOrWhiteSpace(read.Error);

    /// <summary>Returns the entries of a complete NUL-terminated list, or null when the output is not one.</summary>
    internal static ImmutableArray<string>? ReadNulEntries(string output)
    {
        if (output.Length > 0 && (output[^1] != '\0' || output[0] == '\0' || output.Contains("\0\0", StringComparison.Ordinal))) return null;
        return [.. output.Split('\0', StringSplitOptions.RemoveEmptyEntries)];
    }
}

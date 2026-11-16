namespace OpenForge.Cli.Core.Framework.Tools.Shared.Paths;

/// <summary>
/// The pure edit of the Windows user <c>Path</c> value. Open Forge only appends its own bin entry, and Remove
/// never edits PATH. Every other byte, including unexpanded <c>%VAR%</c> entries, order, empty entries,
/// duplicates and a trailing separator, stays as written. A value it can't edit safely returns null.
/// </summary>
internal static class ToolWindowsUserPath
{
    internal const char Separator = ';';
    // The documented limit for one environment variable value, in UTF-16 characters.
    internal const int MaximumLength = 32767;

    internal static string? Append(string? value, string bin)
    {
        if (!IsSafe(bin) || value is not null && !IsSafe(value)) return null;
        if (string.IsNullOrEmpty(value)) return bin;
        if (ToolPathPolicy.Contains(value, bin, windows: true)) return value;
        var applied = value.EndsWith(Separator) ? value + bin : $"{value}{Separator}{bin}";
        return applied.Length > MaximumLength ? null : applied;
    }

    private static bool IsSafe(string value) => value.Length <= MaximumLength && !value.Contains('\0');
}

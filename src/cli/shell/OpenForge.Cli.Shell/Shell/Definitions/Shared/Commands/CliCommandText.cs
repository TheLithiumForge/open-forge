namespace OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

internal static class CliCommandText
{
    internal static string Quote(string value)
        => Quote(value, OperatingSystem.IsWindows());

    internal static string Quote(string value, bool windowsShell)
    {
        if (value.Contains('\n') || value.Contains('\r') || value.Contains('\t'))
        {
            var escapedControls = windowsShell
                ? value.Replace("`", "``", StringComparison.Ordinal).Replace("\"", "`\"", StringComparison.Ordinal)
                    .Replace("$", "`$", StringComparison.Ordinal).Replace("\r", "`r", StringComparison.Ordinal)
                    .Replace("\n", "`n", StringComparison.Ordinal).Replace("\t", "`t", StringComparison.Ordinal)
                : value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)
                    .Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)
                    .Replace("\t", "\\t", StringComparison.Ordinal);
            return windowsShell ? $"\"{escapedControls}\"" : $"$'{escapedControls}'";
        }
        if (value.Length > 0 && value.All(character => char.IsLetterOrDigit(character) || "-_/.:=".Contains(character)))
        {
            return value;
        }
        var escaped = windowsShell
            ? value.Replace("'", "''", StringComparison.Ordinal)
            : value.Replace("'", "'\\''", StringComparison.Ordinal);
        return $"'{escaped}'";
    }
}

using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Paths;

internal static class ToolPathPolicy
{
    internal const string PathVariable = "PATH";
    internal const string StartMarker = "# >>> Open Forge PATH >>>";
    internal const string EndMarker = "# <<< Open Forge PATH <<<";
    internal const string RestartInstruction = "Restart terminals and agent apps to use the changed PATH.";
    internal static readonly IReadOnlyList<ToolShellFile> StartupFiles =
    [
        new(".zshrc", ToolShellKind.Posix),
        new(".bashrc", ToolShellKind.Posix),
        new(".profile", ToolShellKind.Posix),
        new(".config/fish/config.fish", ToolShellKind.Fish),
    ];

    internal static bool Contains(string? path, string bin, bool windows)
        => path is not null && path.Split(windows ? ';' : ':').Any(value => SameEntry(value, bin, windows));

    internal static bool SameEntry(string left, string right, bool windows)
        => string.Equals(left.Trim().Trim('"').TrimEnd('/', '\\'), right.Trim().Trim('"').TrimEnd('/', '\\'),
            windows ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    internal static string ManualLine(string bin, ToolShellKind shell)
        => shell switch
        {
            ToolShellKind.Posix => $"export PATH={ToolLauncherWriter.ShellArgument(bin)}:\"$PATH\"",
            ToolShellKind.Fish => $"set -gx PATH {ToolLauncherWriter.ShellArgument(bin)} $PATH",
            _ => throw new ArgumentOutOfRangeException(nameof(shell)),
        };

    internal static string Block(string bin, ToolShellKind shell)
        => $"{StartMarker}\n{ManualLine(bin, shell)}\n{EndMarker}\n";
}

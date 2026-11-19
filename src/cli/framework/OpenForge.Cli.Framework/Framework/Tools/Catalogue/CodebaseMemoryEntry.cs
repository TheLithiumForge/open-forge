using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue;

/// <summary>
/// Codebase Memory 0.12.1. Its installers always fetch the latest release, so the tested version is shown, not pinned.
/// The Tool setup design records the upstream evidence for every command here.
/// </summary>
internal static class CodebaseMemoryEntry
{
    internal const string Id = "codebase-memory";
    internal const string Version = "0.12.1";
    internal const string Command = "codebase-memory-mcp";
    internal const string IgnorePath = ".cbmignore";
    private const string Raw = "https://raw.githubusercontent.com/DeusData/codebase-memory-mcp/main";

    internal static ToolEntry Create(ToolPlatform platform) => new()
    {
        Id = Id,
        Name = "Codebase Memory",
        Purpose = "Indexes the codebase into a knowledge graph that coding agents can query.",
        Category = ToolDefinitions.CodeSearchCategory,
        Upstream = new("https://github.com/DeusData/codebase-memory-mcp"),
        Licence = "MIT",
        TestedVersion = Version,
        Method = ToolMethod.Script,
        Detection = new($"{Command} --version"),
        Install = new(
            [platform == ToolPlatform.Windows
                ? new($"irm {Raw}/install.ps1 | iex", ToolShell.PowerShell)
                : new($"curl -fsSL {Raw}/install.sh | bash")],
            "Codebase Memory's installer configures every coding agent it finds for your account and adds its folder to your PATH"),
        Uninstall = new(
            [platform == ToolPlatform.Windows
                // The binary's own default folder is ~/.local/bin, so Windows names the folder install.ps1 uses.
                ? new($"{Command} uninstall --yes --dir \"$env:LOCALAPPDATA\\Programs\\{Command}\"", ToolShell.PowerShell)
                : new($"{Command} uninstall --yes")],
            "Removes Codebase Memory, its PATH entry and its entries from every coding agent on your account. Your indexes stay."),
        Integrations =
        [
            new ToolMarkedBlock(IgnorePath, "Keep archived Open Forge memory out of the index",
                "# Open Forge codebase-memory archives start", "# Open Forge codebase-memory archives end", "/", "/"),
        ],
    };
}

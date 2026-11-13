using System.Collections.Frozen;

namespace OpenForge.Cli.Core.Framework.Tools;

internal static class ToolVocabulary
{
    internal const int ManifestSchema = 1;
    internal const string CodeNavigationRole = "code-navigation";
    internal const string CodeGraphCapability = "code-graph";
    internal const string JsonMembers = "json-members";
    internal const string TomlTable = "toml-table";
    internal static readonly FrozenSet<string> Targets = Set("win-x64", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64");
    internal static readonly FrozenSet<string> Adapters = Set("codegraph-1.6", "cbm-0.12", "qmd-2.8");
    internal static readonly FrozenSet<string> Templates = Set("codegraph-manual-v1", "cbm-manual-v1", "qmd-inventory-v1");
    internal static readonly FrozenSet<string> Skills = Set("code-graph-usage-v1", "docs-search-usage-v1");
    internal static readonly FrozenSet<string> Capabilities = Set(CodeGraphCapability, "docs-search", "code-to-docs-links");
    internal static readonly FrozenSet<string> Roles = Set(CodeNavigationRole, "docs-search", "code-to-docs-links");
    internal static readonly FrozenSet<string> Actions = Set("render-members", "native-settings", "render-helper");
    internal static readonly FrozenSet<string> Checks = Set("artifact-hash", "installed-files", "native-settings", "host-members", "index-state", "hook-policy", "process-policy", "cache-permissions", "dependency-lock", "source-membership", "usage-skill");
    internal static readonly FrozenSet<string> Variables = Set("install", "entry", "worktree", "codeRoot", "state", "cache", "hookSink", "projectId", "indexDirName");
    internal static readonly FrozenSet<string> Hosts = Set("claude-code", "codex");

    private static FrozenSet<string> Set(params string[] values) => values.ToFrozenSet(StringComparer.Ordinal);
}

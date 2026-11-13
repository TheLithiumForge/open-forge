using System.Collections.Frozen;

namespace OpenForge.Cli.Core.Framework.Tools;

internal static class ToolVocabulary
{
    internal const int ManifestSchema = 1;
    internal const string FirstPartyOrigin = "first-party";
    internal const string ThirdPartyOrigin = "third-party";
    internal static readonly FrozenSet<string> Origins = Set(FirstPartyOrigin, ThirdPartyOrigin);
    internal const string CodeNavigationRole = "code-navigation";
    internal const string CodeGraphCapability = "code-graph";
    internal const string JsonMembers = "json-members";
    internal const string ClaudeCodeHost = "claude-code";
    internal const string CodexHost = "codex";
    internal const string InstallAction = "install";
    internal const string ConfigureAction = "configure";
    internal const string ChooseAction = "choose";
    internal const string UpdateAction = "update";
    internal const string RemoveAction = "remove";
    internal const string UninstallAction = "uninstall";
    internal const string IndexCleanupAction = "index-cleanup";
    internal const string ActivateAction = "activate";
    internal const string DeactivateAction = "deactivate";
    internal const string TelemetryOnAction = "telemetry-on";
    internal const string TelemetryOffAction = "telemetry-off";
    internal const string WorkspaceScope = "workspace";
    internal const string HostScope = "host";
    internal const string InstallVariable = "install";
    internal const string EntryVariable = "entry";
    internal const string WorktreeVariable = "worktree";
    internal const string CodeRootVariable = "codeRoot";
    internal const string StateVariable = "state";
    internal const string CacheVariable = "cache";
    internal const string HookSinkVariable = "hookSink";
    internal const string ProjectIdVariable = "projectId";
    internal const string IndexDirNameVariable = "indexDirName";
    internal const string HostVariable = "host";
    internal const string TargetVariable = "target";
    internal const string OpenForgeVariable = "openForge";
    internal const string CodeRootPrefix = "${" + CodeRootVariable + "}/";
    internal const int MaximumCommandTimeoutSeconds = 3600;
    internal static readonly FrozenSet<string> LifecycleActions = Set(InstallAction, UninstallAction, IndexCleanupAction, ActivateAction, DeactivateAction, TelemetryOnAction, TelemetryOffAction);
    internal static readonly FrozenSet<string> RunActions = Set(InstallAction, ConfigureAction, UpdateAction, RemoveAction, ChooseAction);
    internal static readonly FrozenSet<string> Targets = Set("win-x64", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64");
    internal static readonly FrozenSet<string> Adapters = Set("codegraph-1.6", "cbm-0.12", "qmd-2.8");
    internal static readonly FrozenSet<string> Templates = Set("codegraph-manual-v1", "cbm-manual-v1", "qmd-inventory-v1");
    internal static readonly FrozenSet<string> Skills = Set("code-graph-usage-v1", "docs-search-usage-v1");
    internal static readonly FrozenSet<string> Capabilities = Set(CodeGraphCapability, "docs-search", "code-to-docs-links");
    internal static readonly FrozenSet<string> Roles = Set(CodeNavigationRole, "docs-search", "code-to-docs-links");
    internal static readonly FrozenSet<string> Actions = Set("render-members", "native-settings", "render-helper");
    internal static readonly FrozenSet<string> Checks = Set("artifact-hash", "installed-files", "native-settings", "host-members", "index-state", "hook-policy", "process-policy", "cache-permissions", "dependency-lock", "source-membership", "usage-skill");
    internal static readonly FrozenSet<string> Variables = Set(InstallVariable, EntryVariable, WorktreeVariable, CodeRootVariable, StateVariable, CacheVariable, HookSinkVariable, ProjectIdVariable, IndexDirNameVariable, HostVariable, TargetVariable, OpenForgeVariable);
    internal static readonly FrozenSet<string> Hosts = Set(ClaudeCodeHost, CodexHost);

    internal static int OriginOrder(string? origin) => origin switch
    {
        FirstPartyOrigin => 0,
        ThirdPartyOrigin => 1,
        _ => 2,
    };

    private static FrozenSet<string> Set(params string[] values) => values.ToFrozenSet(StringComparer.Ordinal);
}

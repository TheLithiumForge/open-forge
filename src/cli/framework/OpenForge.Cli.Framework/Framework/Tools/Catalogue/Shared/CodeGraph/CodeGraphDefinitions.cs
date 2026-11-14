using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;

namespace OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodeGraph;

internal static class CodeGraphDefinitions
{
    internal const string NpmEntryPath = "npm-shim.js";
    internal const string Id = "codegraph";
    internal const string Version = "1.6.2";
    internal const string ProfileId = "codegraph-1.6";
    internal const string Repository = "colbymchenry/codegraph";
    internal const string Package = "@colbymchenry/codegraph";
    internal const string BundledNodeVersion = "24.16.0";
    internal const string WrapperNodeRange = ">=20";
    internal const string WindowsLauncher = "bin/codegraph.cmd";
    internal const string UnixLauncher = "bin/codegraph";
    internal const string WindowsRuntime = "node.exe";
    internal const string UnixRuntime = "node";
    internal const string ApplicationEntry = "lib/dist/bin/codegraph.js";
    internal const string LiftoffArgument = "--liftoff-only";
    internal const string WarningArgument = "--disable-warning=ExperimentalWarning";
    internal const string LocalArgument = "--location";
    internal const string LocalLocation = "local";
    internal const string TargetArgument = "--target";
    internal const string Targets = "claude,codex";
    internal const string KeepCliArgument = "--keep-cli";
    internal const string TelemetryCommand = "telemetry";
    internal const string TelemetryStatusCommand = "status";
    internal const string NoColorArgument = "--no-color";
    internal const string NoDownloadValue = "1";
    internal const string TelemetryChangeId = "telemetry-off";
    internal const string TelemetryPath = ToolRecipeDefinitions.NativeHomeToken + "/.codegraph/telemetry.json";
    internal const string TelemetryQueuePattern = ToolRecipeDefinitions.NativeHomeToken + "/.codegraph/telemetry-queue*.jsonl";
    internal const string ConfigurationPath = "codegraph.json";
    internal const string ExcludePointer = "/exclude";
    internal const string ArchivePattern = ToolRecipeDefinitions.MainRootToken + "/memory/archived/**";
    internal const string IndexDirectory = ".codegraph/";
    internal const string IgnorePath = ".gitignore";
    internal const string IgnoreChangeId = "index-ignore";
    internal const string IgnoreStartMarker = "# Open Forge codegraph index start";
    internal const string IgnoreEndMarker = "# Open Forge codegraph index end";
    internal const string ClaudeMcpPath = ".mcp.json";
    internal const string ClaudeSettingsPath = ".claude/settings.json";
    internal const string ClaudeInstructionsPath = ".claude/CLAUDE.md";
    internal const string CodexConfigurationPath = ".codex/config.toml";
    internal const string CodexInstructionsPath = "AGENTS.md";
    internal const string LegacyClaudePath = ".claude.json";
    internal const string NativeBackupSuffix = ".backup";
}

using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Tool.Models;

internal enum ToolFindingCode
{
    InvalidInput,
    UnknownTool,
    SettingsUnavailable,
    OpenForgeMissing,
    IntegrationConflict,
    RequirementMissing,
    ConfirmationRequired,
    CommandFailed,
    TargetChangedDuringApply,
    WriteFailed,
    Interrupted,
}

/// <summary>One finding. <see cref="Detail"/> carries the fact its wording needs, such as a requirement name or a cause.</summary>
internal sealed record ToolFinding(ToolFindingCode Code, CliSemanticStatus Status, string? ToolId = null, string? Detail = null)
{
    public string? Path { get; init; }
}

internal sealed record ToolListResult : ICliCommandResult
{
    public string Command => ToolCommandDefinitions.Identity(ToolCommandKind.List);
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    internal string? WorkspacePath => Workspace?.LexicalRoot;
    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public required ToolListData Data { get; init; }
    public IReadOnlyList<ToolFinding> Findings { get; init; } = [];
    public CliNextAction? Next { get; init; }
}

internal sealed record ToolListData(IReadOnlyList<ToolListEntry> Tools);

internal sealed record ToolListEntry
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Purpose { get; init; }
    public required string Method { get; init; }
    public required string Upstream { get; init; }
    public required string Licence { get; init; }
    public required bool Configured { get; init; }
    public required bool Integrated { get; init; }
    public required bool Installed { get; init; }
    public string? Version { get; init; }
}

internal sealed record ToolWriteResult : ICliCommandResult
{
    public string Command => ToolCommandDefinitions.Identity(Kind);
    [JsonIgnore]
    public required ToolCommandKind Kind { get; init; }
    public required CliSemanticStatus Status { get; init; }
    public required CliWorkspace? Workspace { get; init; }
    internal string? WorkspacePath => Workspace?.LexicalRoot;
    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    public required ToolWriteData Data { get; init; }
    public IReadOnlyList<ToolFinding> Findings { get; init; } = [];
    public CliNextAction? Next { get; init; }
}

internal sealed record ToolWriteData(bool Preview, IReadOnlyList<ToolWriteTool> Tools);

internal sealed record ToolWriteTool
{
    public required string Id { get; init; }
    public required string Name { get; init; }

    /// <summary>The version detected before the run, or null when the tool wasn't installed.</summary>
    public string? InstalledVersion { get; init; }

    /// <summary>The tool's own uninstall description, shown only by Remove.</summary>
    public string? UninstallDescription { get; init; }
    public IReadOnlyList<ToolWriteStep> Steps { get; init; } = [];
    public IReadOnlyList<ToolWriteChange> Changes { get; init; } = [];
}

/// <summary>One command with its plain effect and outcome. The output tail is kept only for a failed command.</summary>
internal sealed record ToolWriteStep
{
    public required string Kind { get; init; }
    public required string Command { get; init; }
    public string? Effect { get; init; }
    public IReadOnlyList<string> Notes { get; init; } = [];
    public required string Outcome { get; init; }
    public int? ExitCode { get; init; }
    public string? Output { get; init; }
}

/// <summary>One of Open Forge's own file changes. A settings change has no purpose of its own and Rendering words it.</summary>
internal sealed record ToolWriteChange
{
    public required string Path { get; init; }
    public required string Kind { get; init; }
    public string? Purpose { get; init; }
    public required string Outcome { get; init; }
}

/// <summary>The wire names for step kinds, change kinds and outcomes. Text and JSON share them.</summary>
internal static class ToolWire
{
    internal const string Install = "install";
    internal const string Setup = "setup";
    internal const string Uninstall = "uninstall";
    internal const string Integration = "integration";
    internal const string AddTool = "add-tool";
    internal const string RemoveTool = "remove-tool";
    internal const string Planned = "planned";
    internal const string Done = "done";
    internal const string Skipped = "skipped";
    internal const string Failed = "failed";
    internal const string NotStarted = "not-started";
    internal const string NotRun = "not-run";

    internal static readonly string NpmMethod = ToolDefinitions.MethodName(ToolMethod.Npm);
    internal static readonly string NpmRequirement = ToolDefinitions.RequirementName(ToolRequirement.Npm);
    internal static readonly string PowerShellRequirement = ToolDefinitions.RequirementName(ToolRequirement.PowerShell);
}

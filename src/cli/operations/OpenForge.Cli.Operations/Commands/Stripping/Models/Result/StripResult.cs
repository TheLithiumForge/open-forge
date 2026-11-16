using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Result;

/// <summary>The one result strip and uninstall both return, so their reports share one shape.</summary>
internal sealed record StripResult : ICliCommandResult
{
    public required StripCommandKind Kind { get; init; }

    public string Command => StrippingDefinitions.CommandIdentity(Kind);

    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    public CliNextAction? Next { get; init; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal bool DryRun { get; init; }

    internal ImmutableArray<StripPart> Parts { get; init; } = [];

    internal IEnumerable<string> PartNames => Parts.Select(StrippingDefinitions.PartName);

    internal string? Flatten { get; init; }

    internal ImmutableArray<StripPlanEntry> Plan { get; init; } = [];

    internal ImmutableArray<StripKeptFile> Kept { get; init; } = [];

    internal ImmutableArray<StripCollision> Collisions { get; init; } = [];

    internal ImmutableArray<StripFinding> Findings { get; init; } = [];

    internal bool Verified { get; init; }

    internal StripRecoveryState Recovery { get; init; } = StripRecoveryState.NotRequired;

    internal string? RecoveryPath { get; init; }

    internal bool NoOp => Status == CliSemanticStatus.Complete && Plan.IsEmpty;
}

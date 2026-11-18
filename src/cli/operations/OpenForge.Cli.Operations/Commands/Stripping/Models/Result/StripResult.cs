using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Result;

/// <summary>
/// What happened to the plan as a whole. A run that stops part way reports how many changes it made in its
/// finding and keeps its recovery bundle, so no entry carries its own outcome.
/// </summary>
internal enum StripRunOutcome
{
    Planned,
    NotStarted,
    Unknown,
    Done,
}

/// <summary>The one result uninstall and export both return, so their reports share one shape.</summary>
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

    // Export only: the destination's full path and how many files it copies byte for byte.
    internal string? Destination { get; init; }

    internal int Copied { get; init; }

    internal ImmutableArray<StripPlanEntry> Plan { get; init; } = [];

    internal ImmutableArray<StripKeptFile> Kept { get; init; } = [];

    internal ImmutableArray<StripCollision> Collisions { get; init; } = [];

    internal ImmutableArray<StripFinding> Findings { get; init; } = [];

    internal StripRunOutcome Outcome { get; init; } = StripRunOutcome.Planned;

    internal bool Verified { get; init; }

    internal StripRecoveryState Recovery { get; init; } = StripRecoveryState.NotRequired;

    internal string? RecoveryPath { get; init; }

    // An export always writes its copy, so only uninstall can be a no-op.
    internal bool NoOp => Kind == StripCommandKind.Uninstall && Status == CliSemanticStatus.Complete && Plan.IsEmpty;
}

using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Request;

internal enum RouteMoveMode
{
    Apply,
    DryRun,
}

internal sealed record RouteMoveRequest
{
    internal RouteMoveRequest(
        CliWorkspace workspace,
        string sourceReference,
        string destinationTarget,
        RouteMoveMode mode,
        bool allowInteractiveSourceSelection = false,
        string? frozenSourceReference = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationTarget);
        if (frozenSourceReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(frozenSourceReference);
        }
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Move mode is not defined.");
        }

        Workspace = workspace;
        SourceReference = sourceReference;
        DestinationTarget = destinationTarget;
        Mode = mode;
        AllowInteractiveSourceSelection = allowInteractiveSourceSelection;
        FrozenSourceReference = frozenSourceReference;
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal string DestinationTarget { get; }

    internal RouteMoveMode Mode { get; }

    internal bool AllowInteractiveSourceSelection { get; }

    internal string? FrozenSourceReference { get; }

    internal string ResolutionReference => FrozenSourceReference ?? SourceReference;

    internal bool IsDryRun => Mode == RouteMoveMode.DryRun;

    internal RouteMoveRequest FreezeSource(string exactSourceReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exactSourceReference);
        return new(
            workspace: Workspace,
            sourceReference: SourceReference,
            destinationTarget: DestinationTarget,
            mode: Mode,
            allowInteractiveSourceSelection: AllowInteractiveSourceSelection,
            frozenSourceReference: exactSourceReference);
    }
}

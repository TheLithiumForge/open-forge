using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;

internal enum RouteRemoveMode
{
    Apply,
    DryRun,
}

internal sealed record RouteRemoveRequest
{
    internal RouteRemoveRequest(
        CliWorkspace workspace,
        string sourceReference,
        RouteRemoveMode mode,
        bool automatic = false,
        bool allowInteractiveSourceSelection = false,
        bool allowInteractiveConfirmation = false,
        string? frozenSourceReference = null)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        if (frozenSourceReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(frozenSourceReference);
        }
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Route Remove mode is not defined.");
        }

        Workspace = workspace;
        SourceReference = sourceReference;
        Mode = mode;
        Automatic = automatic;
        AllowInteractiveSourceSelection = allowInteractiveSourceSelection;
        AllowInteractiveConfirmation = allowInteractiveConfirmation;
        FrozenSourceReference = frozenSourceReference;
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal RouteRemoveMode Mode { get; }

    internal bool Automatic { get; }

    internal bool AllowInteractiveSourceSelection { get; }

    internal bool AllowInteractiveConfirmation { get; }

    internal string? FrozenSourceReference { get; }

    internal string ResolutionReference => FrozenSourceReference ?? SourceReference;

    internal bool IsDryRun => Mode == RouteRemoveMode.DryRun;

    internal RouteRemoveRequest FreezeSource(string exactSourceReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exactSourceReference);
        return new(
            workspace: Workspace,
            sourceReference: SourceReference,
            mode: Mode,
            automatic: Automatic,
            allowInteractiveSourceSelection: AllowInteractiveSourceSelection,
            allowInteractiveConfirmation: AllowInteractiveConfirmation,
            frozenSourceReference: exactSourceReference);
    }
}

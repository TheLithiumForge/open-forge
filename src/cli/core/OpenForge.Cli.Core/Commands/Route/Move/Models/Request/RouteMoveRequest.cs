using OpenForge.Cli.Core.Framework.Workspace;

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
        RouteMoveMode mode)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationTarget);
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
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal string DestinationTarget { get; }

    internal RouteMoveMode Mode { get; }

    internal bool IsDryRun => Mode == RouteMoveMode.DryRun;
}

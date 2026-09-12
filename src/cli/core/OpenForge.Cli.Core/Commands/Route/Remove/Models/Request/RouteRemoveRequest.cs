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
        RouteRemoveMode mode)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
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
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }

    internal RouteRemoveMode Mode { get; }

    internal bool IsDryRun => Mode == RouteRemoveMode.DryRun;
}

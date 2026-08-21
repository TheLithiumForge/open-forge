using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal sealed record RouteListRequest
{
    internal RouteListRequest(
        CliWorkspace workspace,
        string? sourceReference,
        RouteListDepth requestedDepth)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (sourceReference is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        }

        ArgumentNullException.ThrowIfNull(requestedDepth);
        Workspace = workspace;
        SourceReference = sourceReference;
        RequestedDepth = requestedDepth;
    }

    internal CliWorkspace Workspace { get; }

    internal string? SourceReference { get; }

    internal RouteListDepth RequestedDepth { get; }
}

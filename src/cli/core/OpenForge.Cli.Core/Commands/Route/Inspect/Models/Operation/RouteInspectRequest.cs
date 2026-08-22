using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;

internal sealed record RouteInspectRequest
{
    internal RouteInspectRequest(CliWorkspace workspace, string sourceReference)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        Workspace = workspace;
        SourceReference = sourceReference;
    }

    internal CliWorkspace Workspace { get; }

    internal string SourceReference { get; }
}

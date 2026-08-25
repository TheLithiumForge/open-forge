using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectResolver
{
    private PhysicalPathResolution ResolveExactPath(
        CliWorkspace workspace,
        string logicalPath)
    {
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, logicalPath));
    }

    private static RouteInspectResolution? ReadEarlyPathResult(
        string requestedPath,
        RouteInspectSelection unresolvedSelection,
        PhysicalPathResolution exactPathPhysical,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(unresolvedSelection, requestedPath);
        }

        if (exactPathPhysical.State == PhysicalPathState.Missing)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.MissingSource,
                    requestedPath,
                    "The exact source path does not exist.")]);
        }

        if (exactPathPhysical.State != PhysicalPathState.Contained)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Blocked,
                unresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    requestedPath,
                    "The exact source path crosses an unproved physical boundary.")]);
        }

        return null;
    }
}

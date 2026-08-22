using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectPhysicalVerifier
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteInspectPhysicalVerifier(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal PhysicalPathResolution ResolveCandidate(
        CliWorkspace workspace,
        string logicalPath)
    {
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            RouteLogicalPath.ToLexicalPath(workspace.LexicalRoot, logicalPath));
    }

    internal RouteInspectResolutionIssue? VerifyPhysicalLayers(
        CliWorkspace workspace,
        RouteSource source)
    {
        var basePhysical = ResolveCandidate(workspace, source.CanonicalPath);
        if (basePhysical.State != PhysicalPathState.Contained)
        {
            return RouteInspectResolutionSupport.CreateIssue(
                basePhysical.State == PhysicalPathState.Missing
                    ? RouteInspectResolutionIssueCode.MissingSource
                    : RouteInspectResolutionIssueCode.UnsafeSource,
                source.CanonicalPath,
                "The source physical boundary could not be proved.");
        }

        if (!PhysicalIdentityTracker.PathComparer.Equals(
                basePhysical.ResolvedPhysicalPath!,
                source.PhysicalPath))
        {
            return RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.UnsafeSource,
                source.CanonicalPath,
                "The source does not match its catalogue physical identity.");
        }

        if (source.Overwrite is null)
        {
            return null;
        }

        var overwritePath = source.Overwrite.CanonicalLogicalPath;
        var overwritePhysical = ResolveCandidate(workspace, overwritePath);
        if (overwritePhysical.State != PhysicalPathState.Contained
            || !PhysicalIdentityTracker.PathComparer.Equals(
                overwritePhysical.ResolvedPhysicalPath!,
                source.Overwrite.PhysicalPath))
        {
            return RouteInspectResolutionSupport.CreateIssue(
                overwritePhysical.State == PhysicalPathState.Missing
                    ? RouteInspectResolutionIssueCode.ReadUnavailable
                    : RouteInspectResolutionIssueCode.UnsafeSource,
                overwritePath,
                "The overwrite physical boundary could not be proved.");
        }

        return null;
    }

    internal bool MatchesCataloguePhysicalIdentity(
        RouteSource source,
        string requestedPath,
        PhysicalPathResolution physical)
    {
        var expected = string.Equals(requestedPath, source.OverwritePath, StringComparison.Ordinal)
            ? source.Overwrite!.PhysicalPath
            : source.PhysicalPath;
        return physical.State == PhysicalPathState.Contained
            && PhysicalIdentityTracker.PathComparer.Equals(physical.ResolvedPhysicalPath!, expected);
    }
}

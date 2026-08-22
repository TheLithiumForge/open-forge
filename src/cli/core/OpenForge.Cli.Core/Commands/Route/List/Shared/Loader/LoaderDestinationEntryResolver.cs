using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal sealed class LoaderDestinationEntryResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal LoaderDestinationEntryResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal LoaderDestinationEntryResolution Resolve(
        CliWorkspace workspace,
        RouteSourceCatalogue catalogue,
        RouteLoaderDestinationParseResult destination,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.State != RouteLoaderDestinationParseState.Valid)
        {
            throw new ArgumentException("A Loader destination entry resolver requires a valid parsed destination.", nameof(destination));
        }

        var physical = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            RouteLogicalPath.ToLexicalPath(workspace.LexicalRoot, destination.CanonicalPath!));
        switch (physical.State)
        {
            case PhysicalPathState.Missing:
                return Incomplete(
                    destination.AttemptedDestination,
                    RouteListFindingCode.LoaderUnavailable,
                    "The declared Loader destination is missing.");
            case PhysicalPathState.Contained:
                break;
            default:
                return Blocked(
                    destination.AttemptedDestination,
                    RouteListFindingCode.PhysicalBoundary,
                    "The declared Loader destination crosses an unproved physical boundary.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(destination.AttemptedDestination);
        }

        if (destination.CanonicalPath!.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration cannot target an overwrite companion.");
        }

        var source = catalogue.FindByPath(destination.CanonicalPath);
        if (source is null)
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderUnavailable,
                "The declared Loader destination is not a recognized source.");
        }

        if (string.Equals(destination.CanonicalPath, source.OverwritePath, StringComparison.Ordinal))
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration cannot target an overwrite companion.");
        }

        if (source.Kind != RouteSourceKind.Entrypoint)
        {
            return Incomplete(
                destination.AttemptedDestination,
                RouteListFindingCode.LoaderMalformed,
                "A Loader root declaration must identify an entrypoint.");
        }

        if (source.IsRouteAmbiguous)
        {
            return Blocked(
                destination.AttemptedDestination,
                RouteListFindingCode.RouteAmbiguous,
                "The declared Loader route has ambiguous entrypoint meaning.");
        }

        if (!PhysicalIdentityTracker.PathComparer.Equals(
                physical.ResolvedPhysicalPath!,
                source.PhysicalPath))
        {
            return Blocked(
                destination.AttemptedDestination,
                RouteListFindingCode.PhysicalBoundary,
                "The declared Loader destination does not match its catalogue physical identity.");
        }

        return LoaderDestinationEntryResolution.Resolved(source);
    }

    private static LoaderDestinationEntryResolution Incomplete(
        string subject,
        RouteListFindingCode code,
        string cause)
    {
        return LoaderDestinationEntryResolution.Incomplete(
            new RouteListSelectionIssue(code, subject, cause));
    }

    private static LoaderDestinationEntryResolution Blocked(
        string subject,
        RouteListFindingCode code,
        string cause)
    {
        return LoaderDestinationEntryResolution.Blocked(
            new RouteListSelectionIssue(code, subject, cause));
    }

    private static LoaderDestinationEntryResolution Interrupted(string subject)
    {
        return LoaderDestinationEntryResolution.Interrupted(
            new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Loader root resolution was interrupted."));
    }

}

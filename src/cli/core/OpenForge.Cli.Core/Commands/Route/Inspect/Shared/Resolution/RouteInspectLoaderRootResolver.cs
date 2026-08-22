using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectLoaderRootResolver
{
    private const string LoaderPath = ".agents/loader.md";

    private readonly RouteInspectPhysicalVerifier _physicalVerifier;

    internal RouteInspectLoaderRootResolver(RouteInspectPhysicalVerifier physicalVerifier)
    {
        ArgumentNullException.ThrowIfNull(physicalVerifier);
        _physicalVerifier = physicalVerifier;
    }

    internal RouteInspectLoaderRootResolution Resolve(
        CliWorkspace workspace,
        RouteSourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        var roots = new List<string>();
        var issues = new List<RouteInspectResolutionIssue>();
        if (cancellationToken.IsCancellationRequested)
        {
            return new RouteInspectLoaderRootResolution(roots, issues, true, false);
        }

        var loader = catalogue.FindByPath(LoaderPath);
        if (loader is null)
        {
            return new RouteInspectLoaderRootResolution(roots, issues, false, true);
        }

        if (loader.Base.ReadState != FileReadState.Complete)
        {
            issues.Add(new RouteInspectResolutionIssue(
                RouteInspectResolutionIssueCode.ReadUnavailable,
                LoaderPath,
                "The Loader file could not be read completely."));
            return new RouteInspectLoaderRootResolution(roots, issues, false, false);
        }

        var parsed = RouteLoaderEntriesParser.Parse(loader.Base.Body!);
        if (parsed.State == RouteLoaderEntriesParseState.Malformed)
        {
            issues.Add(new RouteInspectResolutionIssue(
                RouteInspectResolutionIssueCode.IncompleteRoute,
                parsed.AttemptedDestination ?? LoaderPath,
                parsed.Cause!));
        }

        foreach (var destination in parsed.Destinations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new RouteInspectLoaderRootResolution(roots, issues, true, false);
            }

            if (destination.State == RouteLoaderDestinationParseState.Unsafe)
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    destination.AttemptedDestination,
                    destination.Cause!));
                continue;
            }

            var path = destination.CanonicalPath!;
            var physical = _physicalVerifier.ResolveCandidate(workspace, path);
            if (physical.State == PhysicalPathState.Missing)
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.IncompleteRoute,
                    destination.AttemptedDestination,
                    "The Loader destination is missing."));
                continue;
            }

            if (physical.State != PhysicalPathState.Contained)
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    destination.AttemptedDestination,
                    "The Loader destination crosses an unproved physical boundary."));
                continue;
            }

            var source = catalogue.FindByPath(path);
            if (source is null
                || string.Equals(path, source.OverwritePath, StringComparison.Ordinal)
                || source.Kind != RouteSourceKind.Entrypoint)
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.IncompleteRoute,
                    destination.AttemptedDestination,
                    "The Loader destination is not a recognized entrypoint source."));
                continue;
            }

            if (source.IsRouteAmbiguous)
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.AmbiguousRoute,
                    destination.AttemptedDestination,
                    "The Loader destination has ambiguous authored route meaning."));
                continue;
            }

            if (!_physicalVerifier.MatchesCataloguePhysicalIdentity(source, path, physical))
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.UnsafeSource,
                    destination.AttemptedDestination,
                    "The Loader destination does not match its catalogue physical identity."));
                continue;
            }

            if (roots.Contains(path, StringComparer.Ordinal))
            {
                issues.Add(new RouteInspectResolutionIssue(
                    RouteInspectResolutionIssueCode.IncompleteRoute,
                    destination.AttemptedDestination,
                    "The Loader declares the same root source more than once."));
                continue;
            }

            roots.Add(path);
        }

        var areRootFactsComplete = parsed.State == RouteLoaderEntriesParseState.Valid
            && issues.Count == 0;
        return new RouteInspectLoaderRootResolution(roots, issues, false, areRootFactsComplete);
    }
}

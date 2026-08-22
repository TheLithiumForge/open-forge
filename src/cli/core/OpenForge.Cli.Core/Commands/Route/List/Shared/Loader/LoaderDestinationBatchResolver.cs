using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal sealed class LoaderDestinationBatchResolver
{
    private const string LoaderPath = ".agents/loader.md";

    private readonly LoaderDestinationEntryResolver _entryResolver;

    internal LoaderDestinationBatchResolver(LoaderDestinationEntryResolver entryResolver)
    {
        ArgumentNullException.ThrowIfNull(entryResolver);
        _entryResolver = entryResolver;
    }

    internal LoaderDestinationResolution Resolve(
        CliWorkspace workspace,
        RouteSourceCatalogue catalogue,
        IReadOnlyList<RouteLoaderDestinationParseResult> destinations,
        CancellationToken cancellationToken)
    {
        var selectedSources = new Dictionary<string, RouteSource>(StringComparer.Ordinal);
        var issues = new List<RouteListSelectionIssue>();
        var hasBlocked = false;
        var hasIncomplete = false;
        var hasInterrupted = false;

        foreach (var destination in destinations)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                issues.Add(new RouteListSelectionIssue(
                    RouteListFindingCode.Interrupted,
                    LoaderPath,
                    "Loader root resolution was interrupted."));
                return new LoaderDestinationResolution(
                    LoaderDestinationResolutionState.Interrupted,
                    selectedSources.Values,
                    issues);
            }

            if (destination.State == RouteLoaderDestinationParseState.Unsafe)
            {
                hasBlocked = true;
                issues.Add(new RouteListSelectionIssue(
                    RouteListFindingCode.PhysicalBoundary,
                    destination.AttemptedDestination,
                    destination.Cause!));
                continue;
            }

            var resolved = _entryResolver.Resolve(
                workspace,
                catalogue,
                destination,
                cancellationToken);
            if (resolved.State != LoaderDestinationEntryResolutionState.Resolved)
            {
                issues.Add(resolved.Issue!);
                hasInterrupted |= resolved.State == LoaderDestinationEntryResolutionState.Interrupted;
                hasBlocked |= resolved.State == LoaderDestinationEntryResolutionState.Blocked;
                hasIncomplete |= resolved.State == LoaderDestinationEntryResolutionState.Incomplete;
                continue;
            }

            var source = resolved.Source!;
            if (!selectedSources.TryAdd(source.CanonicalPath, source))
            {
                hasIncomplete = true;
                issues.Add(new RouteListSelectionIssue(
                    RouteListFindingCode.LoaderMalformed,
                    destination.AttemptedDestination,
                    "The Loader declares the same root source more than once."));
            }
        }

        LoaderDestinationResolutionState state;
        if (hasInterrupted)
        {
            state = LoaderDestinationResolutionState.Interrupted;
        }
        else if (hasBlocked)
        {
            state = LoaderDestinationResolutionState.Blocked;
        }
        else if (hasIncomplete)
        {
            state = LoaderDestinationResolutionState.Incomplete;
        }
        else
        {
            state = LoaderDestinationResolutionState.Resolved;
        }
        return new LoaderDestinationResolution(state, selectedSources.Values, issues);
    }
}

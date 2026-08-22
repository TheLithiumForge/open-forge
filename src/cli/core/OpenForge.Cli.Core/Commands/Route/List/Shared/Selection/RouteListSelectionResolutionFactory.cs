using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal static class RouteListSelectionResolutionFactory
{
    internal static RouteListSelectionResolution Resolved(
        RouteListSelection selection,
        IEnumerable<RouteSource> sources)
    {
        ArgumentNullException.ThrowIfNull(selection);
        var materializedSources = RouteListSelectionResolutionValidation.MaterializeSources(sources);
        if (!selection.IsResolved)
        {
            throw new ArgumentException("A resolved selection outcome requires resolved selection identity.", nameof(selection));
        }

        if (selection.Kind != RouteListSelectionKind.LoaderRoots)
        {
            if (materializedSources.Count != 1)
            {
                throw new ArgumentException("An explicit resolved selection requires one source.", nameof(sources));
            }

            var source = materializedSources[0];
            if (!string.Equals(source.Id, selection.ResolvedId, StringComparison.Ordinal)
                || !string.Equals(source.CanonicalPath, selection.ResolvedPath, StringComparison.Ordinal))
            {
                throw new ArgumentException("The resolved source must match the selection identity.", nameof(sources));
            }
        }

        return RouteListSelectionResolutionValidation.Create(
            RouteListSelectionResolutionState.Resolved,
            selection,
            materializedSources,
            []);
    }

    internal static RouteListSelectionResolution Invalid(
        RouteListSelection selection,
        RouteListSelectionIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return RouteListSelectionResolutionValidation.Create(
            RouteListSelectionResolutionState.Invalid,
            selection,
            [],
            [issue]);
    }

    internal static RouteListSelectionResolution Blocked(
        RouteListSelection selection,
        IEnumerable<RouteSource> sources,
        IEnumerable<RouteListSelectionIssue> issues)
    {
        return Create(
            RouteListSelectionResolutionState.Blocked,
            selection,
            sources,
            issues);
    }

    internal static RouteListSelectionResolution Incomplete(
        RouteListSelection selection,
        IEnumerable<RouteSource> sources,
        IEnumerable<RouteListSelectionIssue> issues)
    {
        return Create(
            RouteListSelectionResolutionState.Incomplete,
            selection,
            sources,
            issues);
    }

    internal static RouteListSelectionResolution Interrupted(
        RouteListSelection selection,
        IEnumerable<RouteSource> sources,
        IEnumerable<RouteListSelectionIssue> issues)
    {
        return Create(
            RouteListSelectionResolutionState.Interrupted,
            selection,
            sources,
            issues);
    }

    private static RouteListSelectionResolution Create(
        RouteListSelectionResolutionState state,
        RouteListSelection selection,
        IEnumerable<RouteSource> sources,
        IEnumerable<RouteListSelectionIssue> issues)
    {
        return RouteListSelectionResolutionValidation.Create(
            state,
            selection,
            RouteListSelectionResolutionValidation.MaterializeSources(sources),
            RouteListSelectionResolutionValidation.MaterializeIssues(issues));
    }
}

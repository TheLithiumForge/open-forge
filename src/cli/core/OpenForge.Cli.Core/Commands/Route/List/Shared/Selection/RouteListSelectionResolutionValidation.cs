using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal static class RouteListSelectionResolutionValidation
{
    internal static RouteListSelectionResolution Create(
        RouteListSelectionResolutionState state,
        RouteListSelection selection,
        IReadOnlyList<RouteSource> sources,
        IReadOnlyList<RouteListSelectionIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ValidateIssues(state, issues);

        if (state == RouteListSelectionResolutionState.Invalid && sources.Count != 0)
        {
            throw new ArgumentException("Invalid selection cannot retain selected sources.", nameof(sources));
        }

        if (state == RouteListSelectionResolutionState.Blocked
            && selection.Kind != RouteListSelectionKind.LoaderRoots
            && sources.Count != 0)
        {
            throw new ArgumentException("A blocked explicit selection cannot retain selected sources.", nameof(sources));
        }

        if (selection.Kind == RouteListSelectionKind.LoaderRoots
            && sources.Any(source => source.Kind != RouteSourceKind.Entrypoint))
        {
            throw new ArgumentException("Loader-root selection can retain only entrypoint sources.", nameof(sources));
        }

        if ((state is RouteListSelectionResolutionState.Invalid or RouteListSelectionResolutionState.Blocked)
            && selection.Kind != RouteListSelectionKind.LoaderRoots
            && selection.IsResolved)
        {
            throw new ArgumentException("Invalid and blocked explicit outcomes retain attempted-only selection identity.", nameof(selection));
        }

        return new RouteListSelectionResolution(
            state,
            selection,
            sources,
            issues);
    }

    internal static IReadOnlyList<RouteSource> MaterializeSources(IEnumerable<RouteSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        var materialized = sources.ToArray();
        if (materialized.Any(source => source is null))
        {
            throw new ArgumentException("Selected sources cannot contain null.", nameof(sources));
        }

        if (materialized.Select(source => source.CanonicalPath).Distinct(StringComparer.Ordinal).Count()
            != materialized.Length)
        {
            throw new ArgumentException("Selected sources cannot contain duplicate canonical paths.", nameof(sources));
        }

        return new ReadOnlyCollection<RouteSource>(
            materialized.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray());
    }

    internal static IReadOnlyList<RouteListSelectionIssue> MaterializeIssues(
        IEnumerable<RouteListSelectionIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        var materialized = issues.ToArray();
        if (materialized.Any(issue => issue is null))
        {
            throw new ArgumentException("Selection issues cannot contain null.", nameof(issues));
        }

        return new ReadOnlyCollection<RouteListSelectionIssue>(materialized);
    }

    private static void ValidateIssues(
        RouteListSelectionResolutionState state,
        IReadOnlyList<RouteListSelectionIssue> issues)
    {
        var statusRule = ReadStatusRule(state);
        if (statusRule.RequiredFindingStatus is null)
        {
            if (issues.Count != 0)
            {
                throw new ArgumentException("A resolved selection cannot contain issues.", nameof(issues));
            }

            return;
        }

        if (issues.Count == 0)
        {
            throw new ArgumentException("A non-resolved selection requires an issue.", nameof(issues));
        }

        if (issues.Any(issue => !statusRule.AllowedFindingStatuses.Contains(issue.Status)))
        {
            throw new ArgumentException("A selection issue is incompatible with the resolution state.", nameof(issues));
        }

        if (!issues.Any(issue => issue.Status == statusRule.RequiredFindingStatus.Value))
        {
            throw new ArgumentException("A selection outcome requires an issue matching its state.", nameof(issues));
        }
    }

    private static RouteListFindingResultStatusRule ReadStatusRule(
        RouteListSelectionResolutionState state)
    {
        return state switch
        {
            RouteListSelectionResolutionState.Resolved =>
                RouteListDefinitions.ReadFindingResultStatusRule(CliSemanticStatus.Complete),
            RouteListSelectionResolutionState.Invalid =>
                RouteListDefinitions.ReadFindingResultStatusRule(CliSemanticStatus.Invalid),
            RouteListSelectionResolutionState.Blocked =>
                RouteListDefinitions.ReadFindingResultStatusRule(CliSemanticStatus.Blocked),
            RouteListSelectionResolutionState.Incomplete =>
                RouteListDefinitions.ReadFindingResultStatusRule(CliSemanticStatus.Incomplete),
            RouteListSelectionResolutionState.Interrupted =>
                RouteListDefinitions.ReadFindingResultStatusRule(CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The selection resolution state is not defined."),
        };
    }
}

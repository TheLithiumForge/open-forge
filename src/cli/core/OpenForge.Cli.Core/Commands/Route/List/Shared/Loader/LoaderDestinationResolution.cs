using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal enum LoaderDestinationResolutionState
{
    Resolved,
    Incomplete,
    Blocked,
    Interrupted,
}

internal sealed class LoaderDestinationResolution
{
    internal LoaderDestinationResolution(
        LoaderDestinationResolutionState state,
        IEnumerable<RouteSource> selectedSources,
        IEnumerable<RouteListSelectionIssue> issues)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Loader resolution state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(selectedSources);
        ArgumentNullException.ThrowIfNull(issues);
        var sources = selectedSources.ToArray();
        var findings = issues.ToArray();
        if (sources.Any(source => source is null))
        {
            throw new ArgumentException("Loader sources cannot contain null.", nameof(selectedSources));
        }

        if (findings.Any(issue => issue is null))
        {
            throw new ArgumentException("Loader issues cannot contain null.", nameof(issues));
        }

        if (sources.Any(source => source.Kind != RouteSourceKind.Entrypoint)
            || sources.Select(source => source.CanonicalPath).Distinct(StringComparer.Ordinal).Count() != sources.Length)
        {
            throw new ArgumentException("Loader selection can retain only unique entrypoint sources.", nameof(selectedSources));
        }

        var resultStatus = state switch
        {
            LoaderDestinationResolutionState.Resolved => CliSemanticStatus.Complete,
            LoaderDestinationResolutionState.Incomplete => CliSemanticStatus.Incomplete,
            LoaderDestinationResolutionState.Blocked => CliSemanticStatus.Blocked,
            LoaderDestinationResolutionState.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Loader resolution state is not defined."),
        };
        var statusRule = RouteListDefinitions.ReadFindingResultStatusRule(resultStatus);
        if (statusRule.RequiredFindingStatus is null && findings.Length != 0
            || statusRule.RequiredFindingStatus is { } required
                && !findings.Any(issue => issue.Status == required)
            || findings.Any(issue => !statusRule.AllowedFindingStatuses.Contains(issue.Status)))
        {
            throw new ArgumentException("Loader issues do not match the aggregate resolution state.", nameof(issues));
        }

        State = state;
        SelectedSources = new ReadOnlyCollection<RouteSource>(
            sources.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray());
        Issues = new ReadOnlyCollection<RouteListSelectionIssue>(findings);
    }

    internal LoaderDestinationResolutionState State { get; }

    internal IReadOnlyList<RouteSource> SelectedSources { get; }

    internal IReadOnlyList<RouteListSelectionIssue> Issues { get; }
}

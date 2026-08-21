using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;
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
        IEnumerable<RouteListSource> selectedSources,
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

        if (sources.Any(source => source.Kind != RouteListSourceKind.Entrypoint)
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
        SelectedSources = new ReadOnlyCollection<RouteListSource>(
            sources.OrderBy(source => source.CanonicalPath, StringComparer.Ordinal).ToArray());
        Issues = new ReadOnlyCollection<RouteListSelectionIssue>(findings);
    }

    internal LoaderDestinationResolutionState State { get; }

    internal IReadOnlyList<RouteListSource> SelectedSources { get; }

    internal IReadOnlyList<RouteListSelectionIssue> Issues { get; }
}

internal sealed class LoaderDestinationResolver
{
    private const string LoaderPath = ".agents/loader.md";

    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly LoaderDestinationEntryResolver _entryResolver;

    internal LoaderDestinationResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
        _entryResolver = new LoaderDestinationEntryResolver(physicalPathResolver);
    }

    internal async ValueTask<LoaderDestinationResolution> ResolveAsync(
        CliWorkspace workspace,
        RouteListSourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(LoaderPath);
        }

        var physical = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            CombineWorkspacePath(workspace.LexicalRoot, LoaderPath));
        switch (physical.State)
        {
            case PhysicalPathState.Missing:
                return Incomplete(
                    new RouteListSelectionIssue(
                        RouteListFindingCode.LoaderUnavailable,
                        LoaderPath,
                        "The Loader file is missing."));
            case PhysicalPathState.Contained:
                break;
            default:
                return Blocked(
                    new RouteListSelectionIssue(
                        RouteListFindingCode.PhysicalBoundary,
                        LoaderPath,
                        "The Loader physical boundary could not be proved."));
        }

        var read = await StrictUtf8FileReader.ReadAsync(
                physical.ResolvedPhysicalPath!,
                LoaderPath,
                cancellationToken)
            .ConfigureAwait(false);
        switch (read.State)
        {
            case FileReadState.Cancelled:
                return Interrupted(LoaderPath);
            case FileReadState.Missing:
                return Incomplete(
                    new RouteListSelectionIssue(
                        RouteListFindingCode.LoaderUnavailable,
                        LoaderPath,
                        "The Loader file became unavailable before it could be read."));
            case FileReadState.InvalidEncoding:
                return Incomplete(
                    new RouteListSelectionIssue(
                        RouteListFindingCode.LoaderMalformed,
                        LoaderPath,
                        "The Loader file is not valid strict UTF-8."));
            case FileReadState.AccessDenied:
            case FileReadState.InputOutputFailure:
                return Incomplete(
                    new RouteListSelectionIssue(
                        RouteListFindingCode.LoaderUnavailable,
                        LoaderPath,
                        "The Loader file could not be read."));
            case FileReadState.Complete:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(read.State), read.State, "The Loader read state is not defined.");
        }

        var entries = LoaderEntriesParser.Parse(read.Value!);
        if (entries.State == LoaderEntriesParseState.Malformed)
        {
            var issue = new RouteListSelectionIssue(
                RouteListFindingCode.LoaderMalformed,
                entries.AttemptedDestination ?? LoaderPath,
                entries.Cause!);
            if (entries.Destinations.Count == 0)
            {
                return Incomplete(issue);
            }

            return AddMalformedIssue(
                ResolveDestinations(
                    workspace,
                    catalogue,
                    entries.Destinations,
                    cancellationToken),
                issue);
        }

        return ResolveDestinations(
            workspace,
            catalogue,
            entries.Destinations,
            cancellationToken);
    }

    private LoaderDestinationResolution ResolveDestinations(
        CliWorkspace workspace,
        RouteListSourceCatalogue catalogue,
        IReadOnlyList<LoaderDestinationParseResult> destinations,
        CancellationToken cancellationToken)
    {
        var selectedSources = new Dictionary<string, RouteListSource>(StringComparer.Ordinal);
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

            if (destination.State == LoaderDestinationParseState.Unsafe)
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

    private static string CombineWorkspacePath(string workspaceRoot, string logicalPath)
    {
        var relative = logicalPath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(workspaceRoot, relative);
    }

    private static LoaderDestinationResolution Incomplete(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Incomplete,
            [],
            [issue]);
    }

    private static LoaderDestinationResolution AddMalformedIssue(
        LoaderDestinationResolution resolution,
        RouteListSelectionIssue issue)
    {
        var state = resolution.State == LoaderDestinationResolutionState.Resolved
            ? LoaderDestinationResolutionState.Incomplete
            : resolution.State;
        return new LoaderDestinationResolution(
            state,
            resolution.SelectedSources,
            resolution.Issues.Append(issue));
    }

    private static LoaderDestinationResolution Blocked(RouteListSelectionIssue issue)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Blocked,
            [],
            [issue]);
    }

    private static LoaderDestinationResolution Interrupted(string subject)
    {
        return new LoaderDestinationResolution(
            LoaderDestinationResolutionState.Interrupted,
            [],
            [new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Loader root resolution was interrupted.")]);
    }

}

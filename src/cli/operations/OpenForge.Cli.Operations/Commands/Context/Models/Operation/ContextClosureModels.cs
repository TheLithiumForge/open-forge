using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Context.Models.Result;

namespace OpenForge.Cli.Core.Commands.Context.Models.Operation;

internal sealed record ContextSelectedGraphSource
{
    public required ContextGraphSource Source { get; init; }

    public required IReadOnlyList<ContextInclusionReason> InclusionReasons { get; init; }

    public ContextApplicability? Applicability { get; init; }
}

internal sealed record ContextClosureResolution
{
    internal ContextClosureResolution(
        IEnumerable<ContextRequestedSource> requestedSources,
        IEnumerable<ContextSelectedGraphSource> startupSources,
        IEnumerable<ContextSelectedGraphSource> combinedSources,
        IEnumerable<ContextSelectedGraphSource> resultSources,
        IEnumerable<ContextFinding> findings,
        bool startupIncluded,
        bool selectionComplete,
        bool selectionBlocked,
        IEnumerable<ContextPendingCondition>? pendingConditions = null)
    {
        RequestedSources = Snapshot(requestedSources);
        StartupSources = Snapshot(startupSources);
        CombinedSources = Snapshot(combinedSources);
        ResultSources = Snapshot(resultSources);
        PendingConditions = Snapshot(pendingConditions ?? []);
        Findings = Snapshot(findings);
        StartupIncluded = startupIncluded;
        SelectionComplete = selectionComplete;
        SelectionBlocked = selectionBlocked;
    }

    internal IReadOnlyList<ContextRequestedSource> RequestedSources { get; }

    internal IReadOnlyList<ContextSelectedGraphSource> StartupSources { get; }

    internal IReadOnlyList<ContextSelectedGraphSource> CombinedSources { get; }

    internal IReadOnlyList<ContextSelectedGraphSource> ResultSources { get; }

    internal IReadOnlyList<ContextPendingCondition> PendingConditions { get; }

    internal IReadOnlyList<ContextFinding> Findings { get; }

    internal bool StartupIncluded { get; }

    internal bool SelectionComplete { get; }

    internal bool SelectionBlocked { get; }

    private static IReadOnlyList<T> Snapshot<T>(IEnumerable<T> values)
        where T : class
        => new ReadOnlyCollection<T>(values.ToArray());
}

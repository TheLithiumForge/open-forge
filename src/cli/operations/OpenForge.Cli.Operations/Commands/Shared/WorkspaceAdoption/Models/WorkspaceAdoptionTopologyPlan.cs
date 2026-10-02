using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed class WorkspaceAdoptionTopologyPlan
{
    private WorkspaceAdoptionTopologyPlan(
        IReadOnlyDictionary<string, string> reusedEntrypoints,
        IReadOnlyList<string> eligibleSourcePaths,
        IReadOnlyList<string> createdEntrypointPaths,
        string? cause)
    {
        if (cause is not null
            && (reusedEntrypoints.Count > 0
                || eligibleSourcePaths.Count > 0
                || createdEntrypointPaths.Count > 0))
        {
            throw new ArgumentException(
                "A blocked workspace-adoption topology plan cannot carry consumable planning facts.",
                nameof(cause));
        }

        ReusedEntrypoints = ImmutableSortedDictionary.CreateRange(
            StringComparer.Ordinal,
            reusedEntrypoints);
        EligibleSourcePaths = eligibleSourcePaths
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        CreatedEntrypointPaths = createdEntrypointPaths
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        Cause = cause;
    }

    internal IReadOnlyDictionary<string, string> ReusedEntrypoints { get; }

    internal IReadOnlyList<string> EligibleSourcePaths { get; }

    internal IReadOnlyList<string> CreatedEntrypointPaths { get; }

    internal string? Cause { get; }

    internal static WorkspaceAdoptionTopologyPlan Complete(
        IReadOnlyDictionary<string, string> reusedEntrypoints,
        IReadOnlyList<string> eligibleSourcePaths,
        IReadOnlyList<string> createdEntrypointPaths)
        => new(reusedEntrypoints, eligibleSourcePaths, createdEntrypointPaths, cause: null);

    internal static WorkspaceAdoptionTopologyPlan Blocked(string cause)
        => new(
            ImmutableSortedDictionary.Create<string, string>(StringComparer.Ordinal),
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            cause);
}

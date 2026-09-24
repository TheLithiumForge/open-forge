using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Planning;

internal sealed record RemoveNavigationPlan(
    ImmutableArray<RemoveNavigationChange> Changes,
    ImmutableArray<FileStateSnapshot> Inputs)
{
    internal static RemoveNavigationPlan Empty { get; } = new([], []);

    internal bool Matches(RemoveNavigationPlan actual)
    {
        ArgumentNullException.ThrowIfNull(actual);
        return Changes.Length == actual.Changes.Length
            && Changes.Zip(actual.Changes).All(pair =>
                pair.First.Change.Kind == pair.Second.Change.Kind
                && pair.First.Change.Expectation == pair.Second.Change.Expectation
                && pair.First.Change.IntendedBytes.AsSpan().SequenceEqual(pair.Second.Change.IntendedBytes.AsSpan())
                && MatchesSnapshot(pair.First.Snapshot, pair.Second.Snapshot))
            && Inputs.Length == actual.Inputs.Length
            && Inputs.Zip(actual.Inputs).All(pair => MatchesSnapshot(pair.First, pair.Second));
    }

    private static bool MatchesSnapshot(FileStateSnapshot left, FileStateSnapshot right)
        => left.Expectation == right.Expectation
            && left.Bytes.AsSpan().SequenceEqual(right.Bytes.AsSpan());
}

internal sealed record RemoveNavigationChange(
    PlannedFileChange Change,
    FileStateSnapshot Snapshot,
    string LogicalPath);

internal sealed record RemoveNavigationSourceText(
    string Text,
    FileStateSnapshot Snapshot);

internal abstract record RemoveNavigationPlanningOutcome
{
    private RemoveNavigationPlanningOutcome()
    {
    }

    internal sealed record Available(RemoveNavigationPlan Plan) : RemoveNavigationPlanningOutcome;

    internal sealed record Unavailable(string Cause) : RemoveNavigationPlanningOutcome;
}

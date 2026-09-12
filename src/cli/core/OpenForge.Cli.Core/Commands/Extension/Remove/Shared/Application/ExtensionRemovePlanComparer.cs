using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal static class ExtensionRemovePlanComparer
{
    internal static bool Matches(ExtensionRemovePlan expected, ExtensionRemovePlan actual)
        => expected.Selection.SelectedBy == actual.Selection.SelectedBy
            && expected.Selection.Ids.SequenceEqual(actual.Selection.Ids, StringComparer.Ordinal)
            && DependencyEquals(expected.Dependencies, actual.Dependencies)
            && expected.Planning.Authority == actual.Planning.Authority
            && expected.Planning.Decisions.Select(DecisionKey)
                .SequenceEqual(actual.Planning.Decisions.Select(DecisionKey), StringComparer.Ordinal)
            && LibraryBoundariesEqual(expected, actual)
            && DictionaryEquals(expected.Topology.IntendedTargetBytes, actual.Topology.IntendedTargetBytes)
            && ChangesEqual(
                ExtensionRemoveApplicationOperation.ReadChanges(expected),
                ExtensionRemoveApplicationOperation.ReadChanges(actual));

    private static bool LibraryBoundariesEqual(ExtensionRemovePlan expected, ExtensionRemovePlan actual)
    {
        var expectedPaths = expected.Planning.Decisions.Select(decision => decision.Path).ToArray();
        var actualPaths = actual.Planning.Decisions.Select(decision => decision.Path).ToArray();
        return expectedPaths.Length == actualPaths.Length && expectedPaths.Zip(actualPaths).All(pair =>
            ExtensionRemoveLibraryBoundaryPolicy.Matches(
                pair.First.LibraryBoundary ?? throw new ArgumentException("The planned Library boundary was not observed.", nameof(expected)),
                pair.Second.LibraryBoundary ?? throw new ArgumentException("The current Library boundary was not observed.", nameof(actual))));
    }

    private static bool DependencyEquals(
        ExtensionRemoveDependencyPlan expected,
        ExtensionRemoveDependencyPlan actual)
        => expected.Packages.Select(PackageKey)
                .SequenceEqual(actual.Packages.Select(PackageKey), StringComparer.Ordinal)
            && expected.RemovalOrder.SequenceEqual(actual.RemovalOrder, StringComparer.Ordinal)
            && expected.RetainedDependentBlockers.Select(BlockerKey)
                .SequenceEqual(actual.RetainedDependentBlockers.Select(BlockerKey), StringComparer.Ordinal)
            && expected.RetainedOrphanDependencyIds.SequenceEqual(
                actual.RetainedOrphanDependencyIds,
                StringComparer.Ordinal);

    private static string PackageKey(ExtensionRemovePackageFact package)
        => $"{package.Id}:{package.SelectedForRemoval}:{string.Join(',', package.Dependencies)}";

    private static string BlockerKey(ExtensionRemoveRetainedDependentBlocker blocker)
        => $"{blocker.DependencyId}:{string.Join(',', blocker.RetainedDependentIds)}";

    private static string DecisionKey(ExtensionRemovePlanningDecision decision)
        => $"{decision.Path.Path}:{decision.Path.Classification}:{decision.Path.Action}:"
            + $"{string.Join(',', decision.Path.SelectedOwnerIds)}:"
            + $"{string.Join(',', decision.Path.RemainingOwnerIds)}:{decision.Disposition}";

    private static bool DictionaryEquals(
        IReadOnlyDictionary<string, ImmutableArray<byte>> expected,
        IReadOnlyDictionary<string, ImmutableArray<byte>> actual)
        => expected.Count == actual.Count
            && expected.All(pair => actual.TryGetValue(pair.Key, out var bytes)
                && pair.Value.AsSpan().SequenceEqual(bytes.AsSpan()));

    private static bool ChangesEqual(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair =>
                pair.First.Kind == pair.Second.Kind
                && pair.First.Expectation == pair.Second.Expectation
                && pair.First.IntendedBytes.AsSpan().SequenceEqual(pair.Second.IntendedBytes.AsSpan()));
}

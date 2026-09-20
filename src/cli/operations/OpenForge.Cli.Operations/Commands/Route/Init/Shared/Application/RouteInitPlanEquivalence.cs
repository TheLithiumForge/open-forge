using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;

internal static class RouteInitPlanEquivalence
{
    internal static bool Matches(RouteInitPlan expected, RouteInitPlan actual)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);
        return ReferenceEquals(expected.Request, actual.Request)
            && Matches(expected.Preview, actual.Preview)
            && SequenceMatches(expected.DirectoryCreations, actual.DirectoryCreations, Matches)
            && SequenceMatches(expected.FileChanges, actual.FileChanges, Matches)
            && SequenceMatches(expected.RecoveryTargets, actual.RecoveryTargets, Matches)
            && expected.Ownership?.State == actual.Ownership?.State
            && expected.Ownership?.Snapshot?.Expectation == actual.Ownership?.Snapshot?.Expectation;
    }

    private static bool Matches(RouteInitResultFormation expected, RouteInitResultFormation actual)
        => ReferenceEquals(expected.Workspace, actual.Workspace)
            && expected.Mode == actual.Mode
            && expected.Scaffold == actual.Scaffold
            && expected.Target == actual.Target
            && expected.Plan == actual.Plan
            && Matches(expected.Framework, actual.Framework)
            && SequenceMatches(expected.Entrypoints, actual.Entrypoints, Matches)
            && expected.Effects.SequenceEqual(actual.Effects)
            && expected.UnchangedPaths.SequenceEqual(actual.UnchangedPaths, StringComparer.Ordinal)
            && expected.Lifecycle == actual.Lifecycle
            && expected.Recovery == actual.Recovery
            && expected.Verification == actual.Verification
            && expected.Findings.SequenceEqual(actual.Findings);

    private static bool Matches(RouteInitFramework? expected, RouteInitFramework? actual)
        => expected is null || actual is null
            ? expected is null && actual is null
            : string.Equals(
                    expected.InventoryFingerprint,
                    actual.InventoryFingerprint,
                    StringComparison.Ordinal)
                && expected.Segments.SequenceEqual(actual.Segments);

    private static bool Matches(RouteInitEntrypoint expected, RouteInitEntrypoint actual)
        => string.Equals(expected.Id, actual.Id, StringComparison.Ordinal)
            && string.Equals(expected.Path, actual.Path, StringComparison.Ordinal)
            && expected.Form == actual.Form
            && expected.Current == actual.Current
            && expected.Ownership == actual.Ownership
            && Matches(expected.Metadata, actual.Metadata)
            && string.Equals(
                expected.SourceAssetPath,
                actual.SourceAssetPath,
                StringComparison.Ordinal)
            && expected.Outcome == actual.Outcome;

    private static bool Matches(RouteInitMetadata? expected, RouteInitMetadata? actual)
        => expected is null || actual is null
            ? expected is null && actual is null
            : string.Equals(expected.Description, actual.Description, StringComparison.Ordinal)
                && expected.DescriptionSource == actual.DescriptionSource
                && string.Equals(
                    expected.Responsibility,
                    actual.Responsibility,
                    StringComparison.Ordinal)
                && expected.ResponsibilitySource == actual.ResponsibilitySource
                && expected.Tags.SequenceEqual(actual.Tags, StringComparer.Ordinal)
                && expected.TagsSource == actual.TagsSource;

    private static bool Matches(
        PlannedDirectoryCreation expected,
        PlannedDirectoryCreation actual)
        => expected.Expectation == actual.Expectation;

    private static bool Matches(PlannedFileChange expected, PlannedFileChange actual)
        => expected.Kind == actual.Kind
            && expected.Expectation == actual.Expectation
            && expected.IntendedBytes.AsSpan().SequenceEqual(actual.IntendedBytes.AsSpan());

    private static bool Matches(RecoveryBundleTarget expected, RecoveryBundleTarget actual)
        => Matches(expected.Change, actual.Change)
            && Matches(expected.Before, actual.Before);

    private static bool Matches(FileStateSnapshot expected, FileStateSnapshot actual)
        => expected.Expectation == actual.Expectation
            && expected.HasBytes == actual.HasBytes
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());

    private static bool SequenceMatches<T>(
        IReadOnlyList<T> expected,
        IReadOnlyList<T> actual,
        Func<T, T, bool> matches)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            if (!matches(expected[index], actual[index]))
            {
                return false;
            }
        }

        return true;
    }
}

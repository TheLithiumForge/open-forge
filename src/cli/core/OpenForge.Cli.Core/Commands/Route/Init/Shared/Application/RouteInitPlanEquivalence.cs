using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

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
            && Matches(expected.IntendedLifecycle, actual.IntendedLifecycle);
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

    private static bool Matches(
        FrameworkLifecycleState? expected,
        FrameworkLifecycleState? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return string.Equals(expected.Coverage, actual.Coverage, StringComparison.Ordinal)
            && Matches(expected.Source, actual.Source)
            && SequenceMatches(expected.Targets, actual.Targets, Matches)
            && SequenceMatches(expected.GeneratedRegions, actual.GeneratedRegions, Matches);
    }

    private static bool Matches(FrameworkLifecycleSource expected, FrameworkLifecycleSource actual)
        => string.Equals(expected.Id, actual.Id, StringComparison.Ordinal)
            && string.Equals(expected.Version, actual.Version, StringComparison.Ordinal)
            && string.Equals(
                expected.InventoryFingerprint,
                actual.InventoryFingerprint,
                StringComparison.Ordinal);

    private static bool Matches(FrameworkLifecycleTarget expected, FrameworkLifecycleTarget actual)
        => string.Equals(expected.Path, actual.Path, StringComparison.Ordinal)
            && string.Equals(expected.SourceAssetPath, actual.SourceAssetPath, StringComparison.Ordinal)
            && string.Equals(expected.Region, actual.Region, StringComparison.Ordinal)
            && string.Equals(
                expected.BaselineFingerprint,
                actual.BaselineFingerprint,
                StringComparison.Ordinal)
            && string.Equals(expected.FingerprintKind, actual.FingerprintKind, StringComparison.Ordinal);

    private static bool Matches(FrameworkGeneratedRegion expected, FrameworkGeneratedRegion actual)
        => string.Equals(expected.Path, actual.Path, StringComparison.Ordinal)
            && string.Equals(expected.Region, actual.Region, StringComparison.Ordinal);

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

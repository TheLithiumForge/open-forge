using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;

internal static class RouteCreatePlanEquivalence
{
    internal static bool Matches(
        RouteCreatePlan expected,
        RouteCreatePlan actual)
        => MatchesRequest(expected, actual)
            && MatchesPreview(expected.Preview, actual.Preview)
            && RouteCreateFormationEquivalence.Matches(
                expected.NavigationFormation,
                actual.NavigationFormation)
            && MatchesSource(expected.TargetSource, actual.TargetSource)
            && MatchesSource(expected.ParentSource, actual.ParentSource)
            && MatchesOptionalSource(expected.TemplateSource, actual.TemplateSource)
            && expected.TargetSnapshot.Expectation == actual.TargetSnapshot.Expectation
            && expected.IntendedTargetBytes.AsSpan().SequenceEqual(actual.IntendedTargetBytes.AsSpan())
            && MatchesNavigationChanges(expected.NavigationChanges, actual.NavigationChanges)
            && MatchesDirectories(expected.DirectoryCreations, actual.DirectoryCreations)
            && MatchesChanges(expected.FileChanges, actual.FileChanges)
            && MatchesRecovery(expected, actual);

    private static bool MatchesRequest(
        RouteCreatePlan expected,
        RouteCreatePlan actual)
        => ReferenceEquals(expected.Request.Workspace, actual.Request.Workspace)
            && string.Equals(expected.Request.FileTarget, actual.Request.FileTarget, StringComparison.Ordinal)
            && string.Equals(
                expected.Request.Metadata.Description,
                actual.Request.Metadata.Description,
                StringComparison.Ordinal)
            && expected.Request.Metadata.Tags.SequenceEqual(actual.Request.Metadata.Tags)
            && string.Equals(
                expected.Request.Metadata.Responsibility,
                actual.Request.Metadata.Responsibility,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Request.TemplateReference,
                actual.Request.TemplateReference,
                StringComparison.Ordinal)
            && expected.Request.Mode == actual.Request.Mode;

    private static bool MatchesOptionalSource(
        SourceLogicalSource? expected,
        SourceLogicalSource? actual)
        => expected is null || actual is null
            ? expected is null && actual is null
            : MatchesSource(expected, actual);

    internal static bool MatchesSource(
        SourceLogicalSource expected,
        SourceLogicalSource actual)
        => string.Equals(
                expected.Identity.AutomaticId,
                actual.Identity.AutomaticId,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Identity.CanonicalBasePath,
                actual.Identity.CanonicalBasePath,
                StringComparison.Ordinal)
            && PhysicalIdentityTracker.PathComparer.Equals(
                expected.Base.PhysicalPath,
                actual.Base.PhysicalPath)
            && expected.Base.Form == actual.Base.Form
            && expected.Base.Kind == actual.Base.Kind
            && MatchesOptionalLayer(expected.Overwrite, actual.Overwrite);

    private static bool MatchesOptionalLayer(
        SourceLayer? expected,
        SourceLayer? actual)
        => expected is null || actual is null
            ? expected is null && actual is null
            : string.Equals(expected.CanonicalPath, actual.CanonicalPath, StringComparison.Ordinal)
                && PhysicalIdentityTracker.PathComparer.Equals(
                    expected.PhysicalPath,
                    actual.PhysicalPath)
                && expected.Form == actual.Form
                && expected.Kind == actual.Kind;

    private static bool MatchesPreview(
        RouteCreateResultFormation expected,
        RouteCreateResultFormation actual)
        => expected.Mode == actual.Mode
            && Equals(expected.Target, actual.Target)
            && Equals(expected.Parent, actual.Parent)
            && MatchesMetadata(expected.Metadata, actual.Metadata)
            && Equals(expected.Template, actual.Template)
            && Equals(expected.Plan, actual.Plan)
            && MatchesEffects(expected.Effects, actual.Effects)
            && expected.UnchangedPaths.SequenceEqual(actual.UnchangedPaths)
            && Equals(expected.Recovery, actual.Recovery)
            && expected.Verification == actual.Verification
            && MatchesFindings(expected.Findings, actual.Findings);

    private static bool MatchesMetadata(
        RouteCreateMetadata expected,
        RouteCreateMetadata actual)
        => string.Equals(expected.Description, actual.Description, StringComparison.Ordinal)
            && string.Equals(
                expected.Responsibility,
                actual.Responsibility,
                StringComparison.Ordinal)
            && expected.Tags.SequenceEqual(actual.Tags);

    private static bool MatchesEffects(
        IReadOnlyList<RouteCreateEffect> expected,
        IReadOnlyList<RouteCreateEffect> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!string.Equals(before.Path, after.Path, StringComparison.Ordinal)
                || before.Kind != after.Kind
                || before.Action != after.Action
                || !MatchesChange(before.Change, after.Change)
                || before.Outcome != after.Outcome
                || before.Residual != after.Residual)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesChange(
        RouteCreateEffectChange? expected,
        RouteCreateEffectChange? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return string.Equals(expected.Before, actual.Before, StringComparison.Ordinal)
            && string.Equals(expected.Expected, actual.Expected, StringComparison.Ordinal);
    }

    private static bool MatchesNavigationChanges(
        IReadOnlyList<RouteCreateNavigationChange> expected,
        IReadOnlyList<RouteCreateNavigationChange> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var before = expected[index];
            var after = actual[index];
            if (!MatchesSource(before.Source, after.Source)
                || before.Before.Expectation != after.Before.Expectation
                || !before.IntendedBytes.AsSpan().SequenceEqual(after.IntendedBytes.AsSpan())
                || !MatchesBoundedChange(before.RegionChange, after.RegionChange))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesBoundedChange(
        GeneratedNavigationBoundedChange? expected,
        GeneratedNavigationBoundedChange? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return expected.Kind == actual.Kind
            && expected.ContentLocation == actual.ContentLocation
            && string.Equals(expected.BeforeBody, actual.BeforeBody, StringComparison.Ordinal)
            && string.Equals(expected.ExpectedBody, actual.ExpectedBody, StringComparison.Ordinal)
            && string.Equals(expected.Prefix, actual.Prefix, StringComparison.Ordinal)
            && string.Equals(expected.Suffix, actual.Suffix, StringComparison.Ordinal);
    }

    private static bool MatchesDirectories(
        IReadOnlyList<PlannedDirectoryCreation> expected,
        IReadOnlyList<PlannedDirectoryCreation> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        return expected.Zip(actual).All(pair =>
            pair.First.Expectation == pair.Second.Expectation);
    }

    private static bool MatchesFindings(
        IReadOnlyList<RouteCreateFinding> expected,
        IReadOnlyList<RouteCreateFinding> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            if (expected[index].Code != actual[index].Code
                || !string.Equals(
                    expected[index].Target,
                    actual[index].Target,
                    StringComparison.Ordinal)
                || !string.Equals(
                    expected[index].Cause,
                    actual[index].Cause,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesChanges(
        IReadOnlyList<PlannedFileChange> expected,
        IReadOnlyList<PlannedFileChange> actual)
    {
        if (expected.Count != actual.Count)
        {
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            if (expected[index].Kind != actual[index].Kind
                || expected[index].Expectation != actual[index].Expectation
                || !expected[index].IntendedBytes.AsSpan().SequenceEqual(actual[index].IntendedBytes.AsSpan()))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesRecovery(
        RouteCreatePlan expected,
        RouteCreatePlan actual)
    {
        if (expected.RecoveryTargets.Length != actual.RecoveryTargets.Length)
        {
            return false;
        }

        for (var index = 0; index < expected.RecoveryTargets.Length; index++)
        {
            if (!MatchesRecoveryTarget(
                    expected.RecoveryTargets[index],
                    actual.RecoveryTargets[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesRecoveryTarget(
        RecoveryBundleTarget expected,
        RecoveryBundleTarget actual)
        => expected.Change.Kind == actual.Change.Kind
            && expected.Change.Expectation == actual.Change.Expectation
            && expected.Change.IntendedBytes.AsSpan().SequenceEqual(actual.Change.IntendedBytes.AsSpan())
            && expected.Before.Expectation == actual.Before.Expectation
            && expected.Before.HasBytes == actual.Before.HasBytes
            && expected.Before.Bytes.AsSpan().SequenceEqual(actual.Before.Bytes.AsSpan());
}

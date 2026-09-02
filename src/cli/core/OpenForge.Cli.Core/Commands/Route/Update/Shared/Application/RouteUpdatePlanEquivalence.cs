using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdatePlanEquivalence
{
    internal bool Matches(RouteUpdatePlan expected, RouteUpdatePlan actual)
        => MatchesTarget(expected.Preview.Target, actual.Preview.Target)
            && expected.Observation.TargetSnapshot.Expectation
                == actual.Observation.TargetSnapshot.Expectation
            && MatchesOptionalExpectation(
                expected.Observation.OverwriteSnapshot,
                actual.Observation.OverwriteSnapshot)
            && MatchesTemplate(expected.Template, actual.Template)
            && MatchesChanges(expected.FileChanges, actual.FileChanges)
            && MatchesNavigation(expected, actual);

    private static bool MatchesTarget(RouteUpdateTarget expected, RouteUpdateTarget actual)
        => string.Equals(expected.Requested, actual.Requested, StringComparison.Ordinal)
            && expected.SelectedBy == actual.SelectedBy
            && string.Equals(expected.Id, actual.Id, StringComparison.Ordinal)
            && string.Equals(expected.Path, actual.Path, StringComparison.Ordinal)
            && expected.Form == actual.Form
            && expected.OverwritePaths.SequenceEqual(actual.OverwritePaths, StringComparer.Ordinal);

    private static bool MatchesOptionalExpectation(
        FileStateSnapshot? expected,
        FileStateSnapshot? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return expected.Expectation == actual.Expectation;
    }

    private static bool MatchesTemplate(
        RouteTemplateResolution? expected,
        RouteTemplateResolution? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return expected.State == actual.State
            && Equals(expected.Template, actual.Template)
            && Equals(expected.Issue, actual.Issue)
            && string.Equals(expected.Target, actual.Target, StringComparison.Ordinal)
            && expected.BodyBytes.AsSpan().SequenceEqual(actual.BodyBytes.AsSpan())
            && MatchesSource(expected.Source, actual.Source);
    }

    private static bool MatchesSource(SourceLogicalSource? expected, SourceLogicalSource? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return MatchesLayer(expected.Base, actual.Base)
            && MatchesOptionalLayer(expected.Overwrite, actual.Overwrite)
            && string.Equals(
                expected.Identity.AutomaticId,
                actual.Identity.AutomaticId,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Identity.CanonicalBasePath,
                actual.Identity.CanonicalBasePath,
                StringComparison.Ordinal);
    }

    private static bool MatchesOptionalLayer(SourceLayer? expected, SourceLayer? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return MatchesLayer(expected, actual);
    }

    private static bool MatchesLayer(SourceLayer expected, SourceLayer actual)
        => expected.Kind == actual.Kind
            && expected.Form == actual.Form
            && string.Equals(expected.CanonicalPath, actual.CanonicalPath, StringComparison.Ordinal)
            && string.Equals(expected.PhysicalPath, actual.PhysicalPath, PathComparison());

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
                || !expected[index].IntendedBytes.AsSpan()
                    .SequenceEqual(actual[index].IntendedBytes.AsSpan()))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesNavigation(RouteUpdatePlan expected, RouteUpdatePlan actual)
    {
        if (expected.Navigation.Regions.Length != actual.Navigation.Regions.Length)
        {
            return false;
        }

        for (var index = 0; index < expected.Navigation.Regions.Length; index++)
        {
            var expectedRegion = expected.Navigation.Regions[index];
            var actualRegion = actual.Navigation.Regions[index];
            if (expectedRegion.IsTarget != actualRegion.IsTarget
                || !MatchesSource(expectedRegion.Source, actualRegion.Source)
                || expectedRegion.Snapshot.Expectation != actualRegion.Snapshot.Expectation)
            {
                return false;
            }
        }

        return MatchesRouteFacts(expected, actual);
    }

    private static bool MatchesRouteFacts(RouteUpdatePlan expected, RouteUpdatePlan actual)
    {
        var expectedTopology = expected.Navigation.Formation.Topology;
        var actualTopology = actual.Navigation.Formation.Topology;
        var paths = expected.Navigation.Regions
            .Select(region => region.Source.Identity.CanonicalBasePath)
            .Append(expected.Observation.TargetSource.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            if (!MatchesNode(expectedTopology.FindByPath(path), actualTopology.FindByPath(path)))
            {
                return false;
            }
        }

        return expectedTopology.LoaderRootPaths.SequenceEqual(
            actualTopology.LoaderRootPaths,
            StringComparer.Ordinal);
    }

    private static bool MatchesNode(SourceRouteNode? expected, SourceRouteNode? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return expected.ParentState == actual.ParentState
            && string.Equals(
                expected.Identity.AutomaticId,
                actual.Identity.AutomaticId,
                StringComparison.Ordinal)
            && string.Equals(
                expected.Identity.CanonicalBasePath,
                actual.Identity.CanonicalBasePath,
                StringComparison.Ordinal)
            && expected.ParentPaths.SequenceEqual(actual.ParentPaths, StringComparer.Ordinal)
            && expected.ChildPaths.SequenceEqual(actual.ChildPaths, StringComparer.Ordinal);
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}

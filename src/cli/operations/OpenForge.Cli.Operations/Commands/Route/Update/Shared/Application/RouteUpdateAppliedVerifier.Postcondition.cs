using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed partial class RouteUpdateAppliedVerifier
{
    private static bool MatchesPostcondition(
        RouteUpdatePlan expected,
        RouteUpdatePlan actual)
        => MatchesTarget(expected.Preview.Target, actual.Preview.Target)
            && MatchesOptionalExpectation(
                expected.Observation.OverwriteSnapshot,
                actual.Observation.OverwriteSnapshot)
            && MatchesTemplate(expected.Template, actual.Template)
            && expected.Preview.Patch.Description.Expected
                == actual.Preview.Patch.Description.Expected
            && expected.Preview.Patch.Responsibility.Expected
                == actual.Preview.Patch.Responsibility.Expected
            && MatchesTags(
                expected.Preview.Patch.Tags.Expected,
                actual.Preview.Patch.Tags.Expected)
            && MatchesBodyTransition(
                expected.Preview.Plan.Body,
                actual.Preview.Plan.Body);

    private static bool MatchesTarget(
        RouteUpdateTarget expected,
        RouteUpdateTarget actual)
        => string.Equals(expected.Requested, actual.Requested, StringComparison.Ordinal)
            && expected.SelectedBy == actual.SelectedBy
            && string.Equals(expected.Id, actual.Id, StringComparison.Ordinal)
            && string.Equals(expected.Path, actual.Path, StringComparison.Ordinal)
            && expected.Form == actual.Form
            && expected.OverwritePaths.SequenceEqual(
                actual.OverwritePaths,
                StringComparer.Ordinal);

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
            && expected.BodyBytes.AsSpan().SequenceEqual(actual.BodyBytes.AsSpan());
    }

    private static bool MatchesTags(
        System.Collections.Immutable.ImmutableArray<string>? expected,
        System.Collections.Immutable.ImmutableArray<string>? actual)
    {
        if (expected is null || actual is null)
        {
            return expected is null && actual is null;
        }

        return expected.Value.SequenceEqual(actual.Value, StringComparer.Ordinal);
    }

    private static bool MatchesBodyTransition(
        RouteUpdateBodyState expected,
        RouteUpdateBodyState actual)
        => expected == actual
            || expected == RouteUpdateBodyState.TemplateCopied
                && actual == RouteUpdateBodyState.AuthoredBodyProtected;
}

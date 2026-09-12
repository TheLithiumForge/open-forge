using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal sealed partial class RouteInspectResult
{
    private static void ValidateStatus(
        CliSemanticStatus status,
        RouteInspectSelection selection,
        CliWorkspace? workspace,
        RouteInspectIdentity? identity,
        RouteInspectProfile? profile,
        IReadOnlyList<RouteInspectObservation> observations,
        IReadOnlyList<RouteInspectCondition> conditions,
        CliNextAction? next)
    {
        switch (status)
        {
            case CliSemanticStatus.Complete:
                RequireProfile(selection, workspace, identity, profile, RouteInspectCompleteness.Complete, RouteInspectSafety.Safe);
                if (conditions.Count != 0 || observations.Any(HasAutomaticIdObservation))
                {
                    throw new ArgumentException("Complete route-inspect results cannot contain conditions or automatic-ID observations.");
                }

                RequireNext(next, required: false);
                return;
            case CliSemanticStatus.Attention:
                if (selection.SelectionMethod is not (RouteInspectSelectionMethod.ExactPath or RouteInspectSelectionMethod.Interactive))
                {
                    throw new ArgumentException("Attention requires exact-path or interactive selection.", nameof(selection));
                }

                RequireProfile(selection, workspace, identity, profile, RouteInspectCompleteness.Complete, RouteInspectSafety.Safe);
                RequireObservation(observations);
                if (conditions.Count != 0)
                {
                    throw new ArgumentException("Attention route-inspect results cannot contain conditions.", nameof(conditions));
                }

                RequireNext(next, selection.SelectionMethod == RouteInspectSelectionMethod.Interactive);
                return;
            case CliSemanticStatus.Incomplete:
                RequireProfile(selection, workspace, identity, profile, RouteInspectCompleteness.Incomplete, RouteInspectSafety.Safe);
                RequireCondition(conditions, CliSemanticStatus.Incomplete);
                RequireAllowedConditions(conditions, CliSemanticStatus.Incomplete);
                RequireNext(next, required: true);
                return;
            case CliSemanticStatus.Invalid:
                if (selection.IsResolved || identity is not null || profile is not null || observations.Count != 0)
                {
                    throw new ArgumentException(
                        "Invalid route-inspect results require unresolved selection and no identity, profile, or observations.",
                        nameof(selection));
                }

                RequireCondition(conditions, CliSemanticStatus.Invalid);
                RequireAllowedConditions(conditions, CliSemanticStatus.Invalid);
                RequireNext(next, required: true);
                return;
            case CliSemanticStatus.Blocked:
                RequireCondition(conditions, CliSemanticStatus.Blocked);
                RequireAllowedConditions(conditions, CliSemanticStatus.Incomplete, CliSemanticStatus.Blocked);
                RequireNext(next, required: true);
                if (profile is not null && profile.Safety != RouteInspectSafety.Blocked)
                {
                    throw new ArgumentException("A blocked route-inspect profile must have blocked safety.", nameof(profile));
                }

                return;
            case CliSemanticStatus.Failed:
                RequireCondition(conditions, status);
                RequireAllowedConditions(
                    conditions,
                    CliSemanticStatus.Incomplete,
                    CliSemanticStatus.Blocked,
                    CliSemanticStatus.Failed);
                RequireNext(next, required: true);
                return;
            case CliSemanticStatus.Interrupted:
                RequireCondition(conditions, status);
                RequireAllowedConditions(
                    conditions,
                    CliSemanticStatus.Incomplete,
                    CliSemanticStatus.Blocked,
                    CliSemanticStatus.Interrupted);
                RequireNext(next, required: true);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "The route-inspect result status is not defined.");
        }
    }

    private static bool HasAutomaticIdObservation(RouteInspectObservation observation)
    {
        return observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique;
    }

    private static void RequireObservation(IReadOnlyList<RouteInspectObservation> observations)
    {
        if (!observations.Any(HasAutomaticIdObservation))
        {
            throw new ArgumentException("Attention requires an automatic-ID observation.", nameof(observations));
        }
    }

    private static void RequireProfile(
        RouteInspectSelection selection,
        CliWorkspace? workspace,
        RouteInspectIdentity? identity,
        RouteInspectProfile? profile,
        RouteInspectCompleteness completeness,
        RouteInspectSafety safety)
    {
        if (!selection.IsResolved || workspace is null || identity is null || profile is null
            || profile.Completeness != completeness || profile.Safety != safety)
        {
            throw new ArgumentException("Route-inspect selection, facts, completeness, or safety are contradictory.", nameof(profile));
        }
    }

    private static void RequireCondition(IReadOnlyList<RouteInspectCondition> conditions, CliSemanticStatus status)
    {
        if (!conditions.Any(condition => condition.Status == status))
        {
            throw new ArgumentException($"The route-inspect result requires a {status} condition.", nameof(conditions));
        }
    }

    private static void RequireAllowedConditions(
        IReadOnlyList<RouteInspectCondition> conditions,
        params CliSemanticStatus[] allowedStatuses)
    {
        if (conditions.Any(condition => !allowedStatuses.Contains(condition.Status)))
        {
            throw new ArgumentException(
                "A route-inspect condition has higher precedence than the result status.",
                nameof(conditions));
        }
    }

    private static void RequireNext(CliNextAction? next, bool required)
    {
        if (required && next is null)
        {
            throw new ArgumentException("This route-inspect result status requires a next action.", nameof(next));
        }

        if (!required && next is not null)
        {
            throw new ArgumentException("This route-inspect result status cannot contain a next action.", nameof(next));
        }
    }
}

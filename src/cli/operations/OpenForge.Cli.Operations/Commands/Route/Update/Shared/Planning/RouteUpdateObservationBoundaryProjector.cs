using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal static class RouteUpdateObservationBoundaryProjector
{
    internal static RouteUpdatePlanningBoundary Stop(RouteUpdateObservationBoundaryInput input)
        => new()
        {
            Formation = new RouteUpdateResultFormation
            {
                Workspace = input.Request.Workspace,
                Mode = input.Request.Mode,
                Target = input.Target,
                Patch = RouteUpdatePatch.Unresolved(input.Request.Patch),
                Template = input.Request.TemplateReference is { } template
                    ? RouteUpdateTemplate.Unresolved(template)
                    : null,
                Plan = new RouteUpdatePlanFacts
                {
                    Completeness = input.IsIncomplete
                        ? RouteUpdatePlanCompleteness.Incomplete
                        : RouteUpdatePlanCompleteness.NotEstablished,
                    Safety = input.IsIncomplete
                        ? RouteUpdatePlanSafety.NotEstablished
                        : RouteUpdatePlanSafety.Blocked,
                    Body = RouteUpdateBodyState.NotEstablished,
                },
                Effects = [],
                UnchangedPaths = [],
                Recovery = RouteUpdateRecovery.NotRequired(),
                Verification = RouteUpdateVerificationState.NotRequested,
                Findings =
                [
                    new RouteUpdateFinding(
                        input.Code,
                        input.Cause,
                        input.Target.Path ?? input.Target.Requested),
                ],
            },
        };
}

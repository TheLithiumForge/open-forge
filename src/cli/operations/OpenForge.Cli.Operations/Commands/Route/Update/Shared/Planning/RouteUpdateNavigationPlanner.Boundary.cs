using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdateNavigationPlanner
{
    private static RouteUpdateNavigationBuild Stop(
        RouteUpdateDestinationPlan destination,
        RouteUpdateFindingCode code,
        string cause,
        string? target = null)
    {
        var observation = destination.Body.Metadata.Observation;
        var incomplete = code is RouteUpdateFindingCode.ProjectionIncomplete
            or RouteUpdateFindingCode.InspectionIncomplete
            or RouteUpdateFindingCode.Interrupted;
        return RouteUpdateNavigationBuild.Stop(
            new RouteUpdatePlanningBoundary
            {
                Formation = new RouteUpdateResultFormation
                {
                    Workspace = observation.Request.Workspace,
                    Mode = observation.Request.Mode,
                    Target = observation.Target,
                    Patch = destination.Body.Metadata.Patch,
                    Template = destination.Body.Template,
                    Plan = new RouteUpdatePlanFacts
                    {
                        Completeness = incomplete
                            ? RouteUpdatePlanCompleteness.Incomplete
                            : RouteUpdatePlanCompleteness.NotEstablished,
                        Safety = incomplete
                            ? RouteUpdatePlanSafety.NotEstablished
                            : RouteUpdatePlanSafety.Blocked,
                        Body = destination.Body.State,
                    },
                    Effects = [],
                    UnchangedPaths = [],
                    Recovery = RouteUpdateRecovery.NotRequired(),
                    Verification = RouteUpdateVerificationState.NotRequested,
                    Findings =
                    [
                        new RouteUpdateFinding(
                            code,
                            cause,
                            target ?? observation.Target.Path),
                    ],
                },
            });
    }
}

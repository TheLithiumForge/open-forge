using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal static class RouteUpdateMetadataBoundaryProjector
{
    internal static RouteUpdateMetadataPatchBuild Stop(
        RouteUpdateObservation observation,
        string cause)
        => RouteUpdateMetadataPatchBuild.Stop(
            new RouteUpdatePlanningBoundary
            {
                Formation = new RouteUpdateResultFormation
                {
                    Workspace = observation.Request.Workspace,
                    Mode = observation.Request.Mode,
                    Target = observation.Target,
                    Patch = Patch(observation.Request.Patch, layout: null),
                    Template = observation.Request.TemplateReference is { } template
                        ? RouteUpdateTemplate.Unresolved(template)
                        : null,
                    Plan = new RouteUpdatePlanFacts
                    {
                        Completeness = RouteUpdatePlanCompleteness.NotEstablished,
                        Safety = RouteUpdatePlanSafety.Blocked,
                        Body = RouteUpdateBodyState.NotEstablished,
                    },
                    Effects = [],
                    UnchangedPaths = [],
                    Recovery = RouteUpdateRecovery.NotRequired(),
                    Verification = RouteUpdateVerificationState.NotRequested,
                    Findings =
                    [
                        new RouteUpdateFinding(
                            RouteUpdateFindingCode.MetadataPreservationUnsafe,
                            cause,
                            observation.Target.Path),
                    ],
                },
            });

    internal static RouteUpdatePatch Patch(
        RouteUpdatePatchRequest request,
        RouteUpdateMetadataLayout? layout)
    {
        if (layout is null)
        {
            return RouteUpdatePatch.Unresolved(request);
        }

        return new RouteUpdatePatch
        {
            Description = new RouteUpdateDescriptionPatch
            {
                Requested = request.Description.Requested,
                Before = layout.Description,
                Expected = request.Description.Requested ? request.Description.Value : null,
                State = ReadState(
                    request.Description.Requested,
                    string.Equals(
                        layout.Description,
                        request.Description.Value,
                        StringComparison.Ordinal)),
            },
            Responsibility = new RouteUpdateResponsibilityPatch
            {
                Requested = request.Responsibility.Operation
                    != RouteUpdateResponsibilityOperation.NotRequested,
                Operation = request.Responsibility.Operation,
                Before = layout.Responsibility,
                Expected = request.Responsibility.Value,
                State = ReadState(
                    request.Responsibility.Operation
                        != RouteUpdateResponsibilityOperation.NotRequested,
                    string.Equals(
                        layout.Responsibility,
                        request.Responsibility.Value,
                        StringComparison.Ordinal)),
            },
            Tags = new RouteUpdateTagsPatch
            {
                Requested = request.Tags.Requested,
                Before = layout.TagsMember is null ? null : layout.Tags,
                Expected = request.Tags.Requested ? request.Tags.Values : null,
                State = ReadState(
                    request.Tags.Requested,
                    layout.Tags.SequenceEqual(request.Tags.Values)),
            },
        };
    }

    private static RouteUpdatePatchState ReadState(bool requested, bool unchanged)
    {
        if (!requested)
        {
            return RouteUpdatePatchState.NotRequested;
        }

        return unchanged ? RouteUpdatePatchState.Unchanged : RouteUpdatePatchState.Changed;
    }
}

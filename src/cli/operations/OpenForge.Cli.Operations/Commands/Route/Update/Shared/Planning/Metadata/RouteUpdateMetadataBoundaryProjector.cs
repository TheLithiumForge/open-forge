using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal static class RouteUpdateMetadataBoundaryProjector
{
    internal static RouteUpdateMetadataPatchBuild Stop(
        RouteUpdateObservation observation,
        string cause,
        RouteUpdateFindingCode code = RouteUpdateFindingCode.MetadataPreservationUnsafe)
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
                            code,
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
            ApplyTo = new RouteUpdateApplyToPatch
            {
                Requested = request.ApplyTo.Operation != RouteUpdateApplyToOperation.NotRequested,
                Operation = request.ApplyTo.Operation,
                Before = layout.ApplyToPatterns.IsEmpty
                    ? null
                    : layout.ApplyToPatterns.Select(pattern => pattern.Text).ToImmutableArray(),
                Expected = request.ApplyTo.Operation == RouteUpdateApplyToOperation.Set
                    ? request.ApplyTo.Values
                    : null,
                State = ReadApplyToState(request.ApplyTo, layout),
            },
        };
    }

    private static RouteUpdatePatchState ReadApplyToState(
        RouteUpdateApplyToRequest request,
        RouteUpdateMetadataLayout layout)
    {
        if (request.Operation == RouteUpdateApplyToOperation.NotRequested)
        {
            return RouteUpdatePatchState.NotRequested;
        }

        if (request.Operation == RouteUpdateApplyToOperation.Clear)
        {
            return layout.ApplyToMembers.IsEmpty
                ? RouteUpdatePatchState.Unchanged
                : RouteUpdatePatchState.Changed;
        }

        if (request.Operation == RouteUpdateApplyToOperation.Set)
        {
            var before = layout.ApplyToPatterns.Select(pattern => pattern.Text).ToHashSet(StringComparer.Ordinal);
            var after = request.Values.ToHashSet(StringComparer.Ordinal);
            return before.SetEquals(after)
                ? RouteUpdatePatchState.Unchanged
                : RouteUpdatePatchState.Changed;
        }

        throw new ArgumentOutOfRangeException(
            nameof(request),
            request.Operation,
            "The Route Update applyTo operation is not defined.");
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

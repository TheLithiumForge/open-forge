using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateMetadataPatch
{
    public required RouteUpdateObservation Observation { get; init; }

    public required RouteUpdatePatch Patch { get; init; }

    public required ImmutableArray<byte> IntendedTargetBytes { get; init; }

    public required ImmutableArray<RouteUpdatePreviewHunk> Preview { get; init; }
}

internal sealed class RouteUpdateMetadataPatchBuild
{
    private RouteUpdateMetadataPatchBuild(
        RouteUpdateMetadataPatch? patch,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((patch is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Update metadata patching must contain exactly one patch or boundary.");
        }

        Patch = patch;
        Boundary = boundary;
    }

    internal RouteUpdateMetadataPatch? Patch { get; }

    internal RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateMetadataPatchBuild Complete(
        RouteUpdateMetadataPatch patch)
        => new(patch: patch, boundary: null);

    internal static RouteUpdateMetadataPatchBuild Stop(
        RouteUpdatePlanningBoundary boundary)
        => new(patch: null, boundary: boundary);
}

internal sealed record RouteUpdateBodyPlanInput
{
    public required RouteUpdateMetadataPatch Metadata { get; init; }

    public RouteTemplateResolution? Template { get; init; }
}

internal sealed record RouteUpdateBodyPlan
{
    public required RouteUpdateMetadataPatch Metadata { get; init; }

    public RouteUpdateTemplate? Template { get; init; }

    public required RouteUpdateBodyState State { get; init; }

    public required ImmutableArray<byte> IntendedTargetBytes { get; init; }

    public required ImmutableArray<RouteUpdatePreviewHunk> Preview { get; init; }
}

internal sealed class RouteUpdateBodyPlanBuild
{
    private RouteUpdateBodyPlanBuild(
        RouteUpdateBodyPlan? body,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((body is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Update body planning must contain exactly one plan or boundary.");
        }

        Body = body;
        Boundary = boundary;
    }

    internal RouteUpdateBodyPlan? Body { get; }

    internal RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateBodyPlanBuild Complete(RouteUpdateBodyPlan body)
        => new(body: body, boundary: null);

    internal static RouteUpdateBodyPlanBuild Stop(
        RouteUpdatePlanningBoundary boundary)
        => new(body: null, boundary: boundary);
}

internal sealed record RouteUpdateDestinationPlan
{
    public required RouteUpdateBodyPlan Body { get; init; }

    public RouteTemplateResolution? Template { get; init; }

    public required ImmutableArray<byte> IntendedTargetBytes { get; init; }

    public required SourceAuthoredMetadataFacts IntendedMetadata { get; init; }
}

internal sealed class RouteUpdateDestinationBuild
{
    private RouteUpdateDestinationBuild(
        RouteUpdateDestinationPlan? destination,
        RouteUpdatePlanningBoundary? boundary)
    {
        if ((destination is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Update destination planning must contain exactly one plan or boundary.");
        }

        Destination = destination;
        Boundary = boundary;
    }

    internal RouteUpdateDestinationPlan? Destination { get; }

    internal RouteUpdatePlanningBoundary? Boundary { get; }

    internal static RouteUpdateDestinationBuild Complete(
        RouteUpdateDestinationPlan destination)
        => new(destination: destination, boundary: null);

    internal static RouteUpdateDestinationBuild Stop(
        RouteUpdatePlanningBoundary boundary)
        => new(destination: null, boundary: boundary);
}

using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateMetadataPatcher(
    RouteUpdateMetadataLayoutReader layoutReader,
    RouteUpdateMetadataEditPlanner editPlanner,
    RouteUpdateMetadataByteEditor byteEditor)
{
    private readonly RouteUpdateMetadataLayoutReader _layoutReader = layoutReader;
    private readonly RouteUpdateMetadataEditPlanner _editPlanner = editPlanner;
    private readonly RouteUpdateMetadataByteEditor _byteEditor = byteEditor;

    internal RouteUpdateMetadataPatchBuild Build(
        RouteUpdateObservation observation)
    {
        var layoutRead = _layoutReader.Read(observation);
        if (layoutRead.Layout is not { } layout)
        {
            return RouteUpdateMetadataBoundaryProjector.Stop(
                observation,
                layoutRead.Cause ?? "The target metadata layout is unsafe.");
        }

        var editBuild = _editPlanner.Build(
            new RouteUpdateMetadataEditPlanningInput
            {
                Layout = layout,
                Request = observation.Request.Patch,
            });
        if (editBuild.Plan is not { } editPlan)
        {
            return RouteUpdateMetadataBoundaryProjector.Stop(
                observation,
                editBuild.Cause ?? "The target metadata edit is unsafe.");
        }

        var intendedBytes = _byteEditor.Apply(
            new RouteUpdateMetadataByteEditInput
            {
                Observation = observation,
                Edits = editPlan.Edits,
            });
        return RouteUpdateMetadataPatchBuild.Complete(
            new RouteUpdateMetadataPatch
            {
                Observation = observation,
                Patch = RouteUpdateMetadataBoundaryProjector.Patch(
                    observation.Request.Patch,
                    layout),
                IntendedTargetBytes = intendedBytes,
                Preview = editPlan.Preview,
            });
    }
}

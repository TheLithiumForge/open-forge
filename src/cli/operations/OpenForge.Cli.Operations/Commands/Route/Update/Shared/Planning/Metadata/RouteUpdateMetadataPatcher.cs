using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

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
        if (observation.Metadata.Syntax.AuthoredForm is null
            && (!observation.Request.Patch.Description.Requested || !observation.Request.Patch.Tags.Requested
                || string.IsNullOrWhiteSpace(observation.Request.Patch.Description.Value)
                || observation.Request.Patch.Tags.Values.IsEmpty))
        {
            return RouteUpdateMetadataBoundaryProjector.Stop(observation,
                "The intended document requires complete description and tag metadata.", RouteUpdateFindingCode.InvalidPatch);
        }

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
                DefaultForm = observation.MetadataSettings?.Document.Frontmatter ?? FrontmatterForm.Scoped,
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

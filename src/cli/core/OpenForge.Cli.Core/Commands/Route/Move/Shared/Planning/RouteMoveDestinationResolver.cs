using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveDestinationResolver(FileExpectationValidator expectationValidator)
{
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal RouteMoveDestinationResolution Resolve(
        RouteMoveCategoryInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        var projected = RouteMoveDestinationProjector.Project(inventory);
        if (projected.Boundary is { } projectionBoundary)
        {
            return new RouteMoveDestinationResolution(destination: null, projectionBoundary);
        }

        var projection = projected.Projection
            ?? throw new InvalidOperationException(
                "A successful Route Move destination projection requires its projection.");
        var availabilityBoundary = ReadAvailabilityBoundary(projection);
        if (availabilityBoundary is not null)
        {
            return new RouteMoveDestinationResolution(destination: null, availabilityBoundary);
        }

        return new RouteMoveDestinationResolution(
            FormDestination(projection),
            boundary: null);
    }

    private RouteMoveResultFormation? ReadAvailabilityBoundary(
        RouteMoveDestinationProjection projection)
    {
        var operation = projection.Inventory.Subject.Request;
        foreach (var destination in projection.Items)
        {
            var logicalPath = SourceLogicalPath.ToLexicalPath(
                operation.Workspace.LexicalRoot,
                destination.DestinationPath);
            var resolution = _expectationValidator.ResolvePath(operation.Workspace, logicalPath);
            if (resolution.State == PhysicalPathState.Missing)
            {
                continue;
            }

            if (resolution.State != PhysicalPathState.Contained
                || LinkTargetReader.Read(logicalPath).State != PathComponentState.Ordinary)
            {
                return RouteMoveDestinationProjector.Stop(
                    projection.Inventory,
                    RouteMoveFindingCode.DestinationUnsafe,
                    CliSemanticStatus.Blocked,
                    destination.DestinationPath,
                    "A destination target has an unsafe physical boundary.").Boundary;
            }

            return RouteMoveDestinationProjector.Stop(
                projection.Inventory,
                RouteMoveFindingCode.DestinationOccupied,
                CliSemanticStatus.Blocked,
                destination.DestinationPath,
                "A destination target is already occupied.").Boundary;
        }

        return null;
    }

    private static RouteMoveResolvedDestination FormDestination(
        RouteMoveDestinationProjection projection)
    {
        var inventory = projection.Inventory;
        var subject = inventory.Subject;
        var operation = subject.Request;
        return new RouteMoveResolvedDestination
        {
            Inventory = inventory,
            Destination = new RouteMoveDestination
            {
                Requested = operation.DestinationTarget,
                Id = projection.DestinationId,
                Path = projection.DestinationPath,
                ParentId = projection.Parent.Identity.AutomaticId,
                ParentPath = projection.Parent.Identity.CanonicalBasePath,
            },
            Subject = new RouteMoveSubject
            {
                Kind = subject.Kind,
                Layers = [.. subject.Layers.Select(layer => new RouteMoveSubjectLayer
                {
                    Layer = ReadLayer(layer.Layer.Kind),
                    SourcePath = layer.Layer.CanonicalPath,
                    DestinationPath = RouteMoveDestinationProjector.DestinationLayerPath(
                        projection.DestinationPath,
                        layer.Layer.Kind),
                }).OrderBy(layer => layer.SourcePath, StringComparer.Ordinal)],
                Items = ProjectSubjectItems(projection),
            },
        };
    }

    private static ImmutableArray<RouteMoveSubjectItem> ProjectSubjectItems(
        RouteMoveDestinationProjection projection)
    {
        var subject = projection.Inventory.Subject;
        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return [];
        }

        return [.. projection.Items.Select(item => new RouteMoveSubjectItem
        {
            Kind = item.Item.Kind,
            Layer = item.Item.Layer,
            SourceId = item.Item.SourceId,
            SourcePath = ToCanonical(
                subject.Request.Workspace.LexicalRoot,
                item.Item.SourcePath),
            DestinationPath = item.DestinationPath,
        }).OrderBy(item => item.SourcePath, StringComparer.Ordinal)];
    }

    private static RouteMoveLayerKind ReadLayer(SourceLayerKind kind)
        => kind switch
        {
            SourceLayerKind.Base => RouteMoveLayerKind.Base,
            SourceLayerKind.Overwrite => RouteMoveLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The source layer kind is not defined."),
        };

    private static string ToCanonical(string workspaceRoot, string logicalPath)
        => Path.GetRelativePath(workspaceRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/');
}

using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveCategoryInventoryReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteCategoryFilesystemReader _filesystemReader = new(
        physicalPathResolver,
        expectationValidator);
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RouteMoveCategoryInventoryResult> ReadAsync(
        RouteMoveResolvedSubject subject,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(subject);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver, subject.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (!RouteOwnershipEvidence.IsEstablished(ownership))
        {
            return OwnershipBoundary(subject, ownership);
        }

        return await ReadTrustedInventoryAsync(subject, ownership, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RouteMoveCategoryInventoryResult> ReadTrustedInventoryAsync(
        RouteMoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership,
        CancellationToken cancellationToken)
    {
        if (RouteMoveOwnershipProjector.HasSubjectClaims(ownership, subject))
        {
            return Stop(
                subject,
                ownership,
                RouteMoveFindingCode.OwnershipClaimed,
                CliSemanticStatus.Blocked,
                "The selected Route Move subject contains lifecycle-managed paths.");
        }

        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return ReadLeaf(subject, ownership);
        }

        var read = await _filesystemReader.ReadAsync(
            new RouteCategoryFilesystemRequest
            {
                Workspace = subject.Request.Workspace,
                EntrypointLogicalPath = subject.Layers[0].Snapshot.LogicalPath,
                Catalogue = subject.Catalogue,
                EntrypointPaths = [.. subject.Layers.Select(layer => layer.Layer.CanonicalPath)],
                ExposedPaths = subject.NavigationExposure.ExposedPaths,
            },
            cancellationToken).ConfigureAwait(false);
        return ReadFilesystemResult(subject, ownership, read);
    }

    private static RouteMoveCategoryInventoryResult ReadFilesystemResult(
        RouteMoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership,
        RouteCategoryFilesystemRead read)
    {
        if (read.State == RouteCategoryFilesystemReadState.Complete)
        {
            return new RouteMoveCategoryInventoryResult(
                new RouteMoveCategoryInventory
                {
                    Subject = subject,
                    Ownership = ownership,
                    Items = [.. read.Items.Select(ProjectItem)],
                },
                boundary: null);
        }

        if (read.State == RouteCategoryFilesystemReadState.Interrupted)
        {
            return Stop(
                subject,
                ownership,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                read.Cause ?? "Route Move category inventory was interrupted.");
        }

        if (read.State == RouteCategoryFilesystemReadState.Unsafe)
        {
            return Stop(
                subject,
                ownership,
                RouteMoveFindingCode.CategoryUnsafe,
                CliSemanticStatus.Blocked,
                read.Cause ?? "The selected category contains an unsafe filesystem item.");
        }

        return Stop(
            subject,
            ownership,
            RouteMoveFindingCode.CategoryInventoryIncomplete,
            CliSemanticStatus.Incomplete,
            read.Cause ?? "The complete category filesystem inventory could not be established.");
    }

    private static RouteMoveInventoryItem ProjectItem(RouteCategoryFilesystemItem item)
        => new()
        {
            Kind = item.Kind switch
            {
                RouteCategoryFilesystemItemKind.Directory => RouteMoveItemKind.Directory,
                RouteCategoryFilesystemItemKind.Entrypoint => RouteMoveItemKind.Entrypoint,
                RouteCategoryFilesystemItemKind.NativeSource => RouteMoveItemKind.NativeSource,
                RouteCategoryFilesystemItemKind.RoutedMarkdown => RouteMoveItemKind.RoutedMarkdown,
                RouteCategoryFilesystemItemKind.UnroutedMarkdown => RouteMoveItemKind.UnroutedMarkdown,
                RouteCategoryFilesystemItemKind.Resource => RouteMoveItemKind.Resource,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item.Kind, "The category item kind is not defined."),
            },
            Layer = item.Layer is { } layer ? ReadLayer(layer) : null,
            SourceId = item.SourceId,
            SourcePath = item.SourcePath,
            RelativePath = item.RelativePath,
            Snapshot = item.Snapshot,
        };

    private static RouteMoveCategoryInventoryResult ReadLeaf(
        RouteMoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership)
        => new(
            new RouteMoveCategoryInventory
            {
                Subject = subject,
                Ownership = ownership,
                Items = [.. subject.Layers.Select(layer => new RouteMoveInventoryItem
                {
                    Kind = RouteMoveItemKind.RoutedMarkdown,
                    Layer = ReadLayer(layer.Layer.Kind),
                    SourceId = subject.SelectedSource.Identity.AutomaticId,
                    SourcePath = layer.Snapshot.LogicalPath,
                    RelativePath = Path.GetFileName(layer.Snapshot.LogicalPath),
                    Snapshot = layer.Snapshot,
                })],
            },
            boundary: null);

    private static RouteMoveCategoryInventoryResult OwnershipBoundary(
        RouteMoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership)
    {
        return Stop(subject, ownership, RouteMoveFindingCode.OwnershipUnavailable,
            CliSemanticStatus.Complete, RouteOwnershipEvidence.Cause(ownership));
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

    private static RouteMoveCategoryInventoryResult Stop(
        RouteMoveResolvedSubject subject,
        WorkspaceOwnershipRead ownership,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
    {
        var formation = RouteMoveBoundary.Start(subject.Request) with
        {
            Source = subject.Source,
            Subject = new RouteMoveSubject { Kind = subject.Kind },
            Ownership = RouteMoveOwnershipProjector.Project(ownership, subject),
        };
        return new RouteMoveCategoryInventoryResult(
            inventory: null,
            RouteMoveBoundary.Stop(
                formation,
                code,
                status,
                subject.SelectedSource.Identity.CanonicalBasePath,
                cause));
    }

}

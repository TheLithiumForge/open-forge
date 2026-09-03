using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveCategoryInventoryReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator,
    LifecycleOwnershipReader ownershipReader)
{
    private readonly RouteMoveCategoryFilesystemReader _filesystemReader = new(
        physicalPathResolver,
        expectationValidator);
    private readonly LifecycleOwnershipReader _ownershipReader = ownershipReader;

    internal async ValueTask<RouteMoveCategoryInventoryResult> ReadAsync(
        RouteMoveCategoryInventoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = request.Subject;
        var ownership = await _ownershipReader.ReadAsync(
            subject.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (ownership.Framework.State != LifecycleOwnershipReadState.Trusted
            || ownership.Extensions.State != LifecycleOwnershipReadState.Trusted)
        {
            return OwnershipBoundary(subject, ownership);
        }

        return await ReadTrustedInventoryAsync(subject, ownership, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RouteMoveCategoryInventoryResult> ReadTrustedInventoryAsync(
        RouteMoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
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

        var read = await _filesystemReader.ReadAsync(subject, ownership, cancellationToken)
            .ConfigureAwait(false);
        return ReadFilesystemResult(subject, ownership, read);
    }

    private static RouteMoveCategoryInventoryResult ReadFilesystemResult(
        RouteMoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        RouteMoveCategoryFilesystemReadResult read)
    {
        if (read.Inventory is { } inventory)
        {
            return new RouteMoveCategoryInventoryResult(inventory, boundary: null);
        }

        if (read.State == RouteMoveCategoryFilesystemReadState.Interrupted)
        {
            return Stop(
                subject,
                ownership,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                read.Cause ?? "Route Move category inventory was interrupted.");
        }

        if (read.State == RouteMoveCategoryFilesystemReadState.Unsafe)
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

    private static RouteMoveCategoryInventoryResult ReadLeaf(
        RouteMoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership)
        => new(
            new RouteMoveCategoryInventory
            {
                Subject = subject,
                Ownership = ownership,
                Items = subject.Layers.Select(layer => new RouteMoveInventoryItem
                {
                    Kind = RouteMoveItemKind.RoutedMarkdown,
                    Layer = ReadLayer(layer.Layer.Kind),
                    SourceId = subject.SelectedSource.Identity.AutomaticId,
                    SourcePath = layer.Snapshot.LogicalPath,
                    RelativePath = Path.GetFileName(layer.Snapshot.LogicalPath),
                    Snapshot = layer.Snapshot,
                }).ToImmutableArray(),
            },
            boundary: null);

    private static RouteMoveCategoryInventoryResult OwnershipBoundary(
        RouteMoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership)
    {
        var interrupted = ownership.Framework.State == LifecycleOwnershipReadState.Interrupted
            || ownership.Extensions.State == LifecycleOwnershipReadState.Interrupted;
        if (interrupted)
        {
            return Stop(
                subject,
                ownership,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                ownership.Findings.FirstOrDefault()?.Cause
                    ?? "Lifecycle ownership inspection was interrupted.");
        }

        return Stop(
            subject,
            ownership,
            RouteMoveFindingCode.OwnershipUnavailable,
            CliSemanticStatus.Blocked,
            ownership.Findings.FirstOrDefault()?.Cause
                ?? "Lifecycle ownership could not be established from one trusted snapshot.");
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
        LifecycleOwnershipReadResult ownership,
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

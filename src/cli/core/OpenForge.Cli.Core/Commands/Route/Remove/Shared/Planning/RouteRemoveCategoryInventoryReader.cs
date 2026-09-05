using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveCategoryInventoryReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator,
    LifecycleOwnershipReader ownershipReader)
{
    private readonly RouteRemoveCategoryFilesystemReader _filesystemReader = new(
        physicalPathResolver,
        expectationValidator);
    private readonly LifecycleOwnershipReader _ownershipReader = ownershipReader;

    internal async ValueTask<RouteRemoveCategoryInventoryResult> ReadAsync(
        RouteRemoveCategoryInventoryRequest request,
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

    private async ValueTask<RouteRemoveCategoryInventoryResult> ReadTrustedInventoryAsync(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        CancellationToken cancellationToken)
    {
        if (RouteRemoveOwnershipProjector.HasSubjectClaims(ownership, subject))
        {
            return Stop(
                subject,
                ownership,
                RouteRemoveFindingCode.OwnershipClaimed,
                CliSemanticStatus.Blocked,
                "The selected Route Remove subject contains lifecycle-managed paths.");
        }

        if (subject.Kind == RouteRemoveSubjectKind.Leaf)
        {
            return ReadLeaf(subject, ownership);
        }

        var read = await _filesystemReader.ReadAsync(subject, ownership, cancellationToken)
            .ConfigureAwait(false);
        return ReadFilesystemResult(subject, ownership, read);
    }

    private static RouteRemoveCategoryInventoryResult ReadFilesystemResult(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        RouteRemoveCategoryFilesystemReadResult read)
    {
        if (read.Inventory is { } inventory)
        {
            return new RouteRemoveCategoryInventoryResult(inventory, boundary: null);
        }

        if (read.State == RouteRemoveCategoryFilesystemReadState.Interrupted)
        {
            return Stop(
                subject,
                ownership,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                read.Cause ?? "Route Remove category inventory was interrupted.");
        }

        if (read.State == RouteRemoveCategoryFilesystemReadState.Unsafe)
        {
            return Stop(
                subject,
                ownership,
                RouteRemoveFindingCode.CategoryUnsafe,
                CliSemanticStatus.Blocked,
                read.Cause ?? "The selected category contains an unsafe filesystem item.");
        }

        return Stop(
            subject,
            ownership,
            RouteRemoveFindingCode.CategoryInventoryIncomplete,
            CliSemanticStatus.Incomplete,
            read.Cause ?? "The complete category filesystem inventory could not be established.");
    }

    private static RouteRemoveCategoryInventoryResult ReadLeaf(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership)
        => new(
            new RouteRemoveCategoryInventory
            {
                Subject = subject,
                Ownership = ownership,
                Items = subject.Layers.Select(layer => new RouteRemoveInventoryItem
                {
                    Kind = RouteRemoveItemKind.RoutedMarkdown,
                    Layer = ReadLayer(layer.Layer.Kind),
                    SourceId = subject.SelectedSource.Identity.AutomaticId,
                    SourcePath = layer.Snapshot.LogicalPath,
                    RelativePath = Path.GetFileName(layer.Snapshot.LogicalPath),
                    Snapshot = layer.Snapshot,
                }).ToImmutableArray(),
            },
            boundary: null);

    private static RouteRemoveCategoryInventoryResult OwnershipBoundary(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership)
    {
        var interrupted = ownership.Framework.State == LifecycleOwnershipReadState.Interrupted
            || ownership.Extensions.State == LifecycleOwnershipReadState.Interrupted;
        if (interrupted)
        {
            return Stop(
                subject,
                ownership,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                ownership.Findings.FirstOrDefault()?.Cause
                    ?? "Lifecycle ownership inspection was interrupted.");
        }

        return Stop(
            subject,
            ownership,
            RouteRemoveFindingCode.OwnershipUnavailable,
            CliSemanticStatus.Blocked,
            ownership.Findings.FirstOrDefault()?.Cause
                ?? "Lifecycle ownership could not be established from one trusted snapshot.");
    }

    private static RouteRemoveLayerKind ReadLayer(SourceLayerKind kind)
        => kind switch
        {
            SourceLayerKind.Base => RouteRemoveLayerKind.Base,
            SourceLayerKind.Overwrite => RouteRemoveLayerKind.Overwrite,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The source layer kind is not defined."),
        };

    private static RouteRemoveCategoryInventoryResult Stop(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause)
    {
        var formation = RouteRemoveBoundary.Start(subject.Request) with
        {
            Source = subject.Source,
            Subject = new RouteRemoveSubject { Kind = subject.Kind },
            Ownership = RouteRemoveOwnershipProjector.Project(ownership, subject),
        };
        return new RouteRemoveCategoryInventoryResult(
            inventory: null,
            RouteRemoveBoundary.Stop(
                formation,
                code,
                status,
                subject.SelectedSource.Identity.CanonicalBasePath,
                cause));
    }

}

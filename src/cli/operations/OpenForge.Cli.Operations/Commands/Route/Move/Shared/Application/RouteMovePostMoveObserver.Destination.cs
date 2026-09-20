using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMovePostMoveObserver
{
    private async ValueTask<DestinationObservation> ResolveDestinationAsync(
        RouteMovePlan plan,
        CancellationToken cancellationToken)
    {
        var path = plan.Projection.Destination.Destination.Path
            ?? throw new InvalidOperationException("A complete Route Move plan requires its destination path.");
        var request = new RouteMoveRequest(plan.Request.Workspace, path, path, RouteMoveMode.Apply);
        var resolved = await _subjectResolver.ResolveAsync(
            request,
            cancellationToken).ConfigureAwait(false);
        if (resolved.Subject is not { } subject)
        {
            return DestinationObservation.Stop(ReadBoundary(cancellationToken));
        }

        var read = await _inventoryReader.ReadAsync(
            subject,
            cancellationToken).ConfigureAwait(false);
        if (read.Inventory is not { } inventory)
        {
            return DestinationObservation.Stop(ReadBoundary(cancellationToken));
        }

        var mismatch = ReadDestinationMismatch(plan, inventory);
        return mismatch is null
            ? DestinationObservation.Complete(inventory)
            : DestinationObservation.Stop(Failed(mismatch));
    }

    private static string? ReadDestinationMismatch(
        RouteMovePlan plan,
        RouteMoveCategoryInventory actual)
    {
        var expected = plan.Projection.Destination;
        var subject = actual.Subject;
        if (subject.Kind != expected.Subject.Kind
            || subject.SelectedSource.Identity.AutomaticId != expected.Destination.Id
            || subject.SelectedSource.Identity.CanonicalBasePath != expected.Destination.Path)
        {
            return "The final Route Move destination subject identity or form changed.";
        }

        if (ReadLifecycleMismatch(plan.Projection.Ownership, actual.Ownership) is { } lifecycleCause)
        {
            return lifecycleCause;
        }

        if (!LayersMatch(expected.Subject.Layers, subject.Layers))
        {
            return "The final Route Move destination layers changed.";
        }

        var itemsMatch = subject.Kind == RouteMoveSubjectKind.Leaf
            ? actual.Items.All(item => item.Kind != RouteMoveItemKind.Directory)
            : ItemsMatch(
                plan.Request.Workspace.LexicalRoot,
                expected.Subject.Items,
                actual.Items);
        return itemsMatch ? null : "The final Route Move destination inventory changed.";
    }

    private static bool LayersMatch(
        IReadOnlyList<RouteMoveSubjectLayer> expected,
        IReadOnlyList<RouteMoveResolvedLayer> actual)
        => expected.Count == actual.Count
            && expected.All(layer => actual.Any(candidate => candidate.Layer.Kind switch
            {
                SourceLayerKind.Base => layer.Layer == RouteMoveLayerKind.Base
                    && layer.DestinationPath == candidate.Layer.CanonicalPath,
                SourceLayerKind.Overwrite => layer.Layer == RouteMoveLayerKind.Overwrite
                    && layer.DestinationPath == candidate.Layer.CanonicalPath,
                _ => false,
            }));

    private static bool ItemsMatch(
        string workspaceRoot,
        IReadOnlyList<RouteMoveSubjectItem> expected,
        IReadOnlyList<RouteMoveInventoryItem> actual)
        => expected.Count == actual.Count
            && expected.All(item => actual.Any(candidate =>
                item.DestinationPath == Canonical(workspaceRoot, candidate.SourcePath)
                && item.Kind == candidate.Kind
                && item.Layer == candidate.Layer
                && ExpectedDestinationId(item) == candidate.SourceId));

    private static string? ExpectedDestinationId(RouteMoveSubjectItem item)
        => item.SourceId is null ? null : SourceIdentity.DeriveId(item.DestinationPath);

    private static string? ReadLifecycleMismatch(
        WorkspaceOwnershipRead expected,
        WorkspaceOwnershipRead actual)
    {
        if (expected.State != actual.State || expected.Cause != actual.Cause)
        {
            return "The final Route Move ownership observation changed.";
        }

        if (expected.Snapshot?.Expectation != actual.Snapshot?.Expectation)
        {
            return "The final Route Move ownership lock expectation changed.";
        }

        return RouteOwnershipEvidence.Claims(expected).SequenceEqual(RouteOwnershipEvidence.Claims(actual))
            ? null
            : "The final Route Move ownership claims changed.";
    }

    private static string Canonical(string workspaceRoot, string path)
        => Path.GetRelativePath(workspaceRoot, path)
            .Replace(Path.DirectorySeparatorChar, '/');

    private sealed record DestinationObservation(
        RouteMoveCategoryInventory? Inventory,
        RouteMovePostMoveVerification? Verification)
    {
        internal static DestinationObservation Complete(RouteMoveCategoryInventory inventory)
            => new(inventory, Verification: null);

        internal static DestinationObservation Stop(RouteMovePostMoveVerification verification)
            => new(Inventory: null, verification);
    }
}

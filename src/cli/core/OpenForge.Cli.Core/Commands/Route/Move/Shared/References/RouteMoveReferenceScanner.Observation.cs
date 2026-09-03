using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal sealed partial class RouteMoveReferenceScanner
{
    internal async ValueTask<RouteMoveReferencePostMoveResult> ObserveAsync(
        RouteMovePlan plan,
        RouteMarkdownCatalogue catalogue,
        SourceCatalogue sourceCatalogue,
        CancellationToken cancellationToken)
    {
        var expectedPaths = plan.Projection.References.Documents
            .Select(document => document.DestinationSourcePath)
            .Order(StringComparer.Ordinal);
        if (!expectedPaths.SequenceEqual(catalogue.SelectedPaths.Order(StringComparer.Ordinal)))
        {
            return Failed("The final Route Move Markdown catalogue changed.");
        }

        try
        {
            foreach (var document in plan.Projection.References.Documents)
            {
                var observed = await ObserveDocumentAsync(
                    plan,
                    sourceCatalogue,
                    document,
                    cancellationToken).ConfigureAwait(false);
                if (observed.State != RouteMoveReferencePostMoveState.Verified)
                {
                    return observed;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }

        return Verified();
    }

    private async ValueTask<RouteMoveReferencePostMoveResult> ObserveDocumentAsync(
        RouteMovePlan plan,
        SourceCatalogue sourceCatalogue,
        RouteMoveReferenceDocumentPlan expected,
        CancellationToken cancellationToken)
    {
        var document = await ReadDocumentAsync(
            plan.Request.Workspace,
            expected.DestinationSourcePath,
            cancellationToken).ConfigureAwait(false);
        if (document is null || !document.Snapshot.Bytes.AsSpan().SequenceEqual(
                ExpectedBytes(plan, expected).AsSpan()))
        {
            return Failed("A final Route Move reference document changed from its accepted bytes.");
        }

        var meanings = await ReadMeaningsAsync(
            plan,
            sourceCatalogue,
            expected.DestinationSourcePath,
            document,
            cancellationToken).ConfigureAwait(false);
        return MeaningsMatch(expected.Meanings, meanings)
            ? Verified()
            : Failed("A final Route Move authored reference changed meaning.");
    }

    private async ValueTask<ImmutableArray<RouteMoveReferenceMeaning>> ReadMeaningsAsync(
        RouteMovePlan plan,
        SourceCatalogue sourceCatalogue,
        string sourcePath,
        RouteMoveReferenceDocument document,
        CancellationToken cancellationToken)
    {
        var values = new List<RouteMoveReferenceMeaning>();
        foreach (var link in document.Facts.Links.Where(link => IsAuthoredLink(document, link)))
        {
            var facts = await _destinationResolver.ResolveAsync(
                new SourceLinkDestinationInput
                {
                    Workspace = plan.Request.Workspace,
                    Catalogue = sourceCatalogue,
                    SourceCanonicalPath = sourcePath,
                    RawDestination = link.RawDestination,
                },
                cancellationToken).ConfigureAwait(false);
            if (facts.Target.Kind == SourceLinkTargetKind.Local
                && facts.Target.Path is { } targetPath)
            {
                values.Add(new RouteMoveReferenceMeaning
                {
                    TargetId = facts.Target.Id,
                    TargetPath = targetPath,
                    Layer = facts.Target.Layer,
                    Resolution = facts.Target.Resolution,
                    Fragment = facts.Fragment,
                });
            }
        }

        return values.ToImmutableArray();
    }

    private static ImmutableArray<byte> ExpectedBytes(
        RouteMovePlan plan,
        RouteMoveReferenceDocumentPlan document)
    {
        var logicalPath = Path.Combine(
            plan.Request.Workspace.LexicalRoot,
            document.DestinationSourcePath.Replace('/', Path.DirectorySeparatorChar));
        var effect = plan.Projection.FileChanges.FirstOrDefault(change => string.Equals(
            change.LogicalPath,
            logicalPath,
            StringComparison.Ordinal));
        if (effect is not null)
        {
            return effect.IntendedBytes;
        }

        if (string.Equals(document.SourcePath, document.DestinationSourcePath, StringComparison.Ordinal))
        {
            return document.Snapshot.Bytes;
        }

        throw new InvalidOperationException(
            "A moved Route Move reference document requires one accepted destination effect.");
    }

    private static bool MeaningsMatch(
        IReadOnlyList<RouteMoveReferenceMeaning> expected,
        IReadOnlyList<RouteMoveReferenceMeaning> actual)
        => expected.Count == actual.Count
            && expected.Zip(actual).All(pair => pair.First.TargetId == pair.Second.TargetId
                && pair.First.TargetPath == pair.Second.TargetPath
                && pair.First.Layer == pair.Second.Layer
                && pair.First.Resolution == pair.Second.Resolution
                && pair.First.Fragment == pair.Second.Fragment);

    private static RouteMoveReferencePostMoveResult Verified()
        => new(RouteMoveReferencePostMoveState.Verified, Cause: null);

    private static RouteMoveReferencePostMoveResult Failed(string cause)
        => new(RouteMoveReferencePostMoveState.Failed, cause);

    private static RouteMoveReferencePostMoveResult Interrupted()
        => new(
            RouteMoveReferencePostMoveState.Interrupted,
            "Final Route Move reference observation was interrupted.");
}

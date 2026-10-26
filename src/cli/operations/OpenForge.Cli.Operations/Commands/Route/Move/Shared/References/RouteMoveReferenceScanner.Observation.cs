using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

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

        if (expected.IncludedRows is { } rows)
        {
            var parsed = SourceGeneratedEntriesParser.Parse(document.Facts);
            if (parsed.State != SourceGeneratedEntriesState.Complete) return Failed("Final outside authored rows are unavailable.");
            var intended = new Framework.Documents.Markdown.MarkdownDocumentParser().Parse(rows.IntendedText);
            var remaining = parsed.Entries.ToList();
            var actualMeanings = new List<RouteMoveReferenceMeaning>();
            foreach (var row in rows.ExpectedEntries)
            {
                var literal = intended.Source[row.Span.Start..row.Span.End];
                var index = remaining.FindIndex(candidate => document.Text[candidate.Span.Start..candidate.Span.End] == literal);
                if (index < 0) return Failed("A final outside authored row changed its untouched bytes or order.");
                var actual = remaining[index];
                remaining.RemoveRange(0, index + 1);
                var facts = await _destinationResolver.ResolveAsync(new SourceLinkDestinationInput
                {
                    Workspace = plan.Request.Workspace,
                    Catalogue = sourceCatalogue,
                    SourceCanonicalPath = expected.DestinationSourcePath,
                    RawDestination = actual.Destination,
                }, cancellationToken).ConfigureAwait(false);
                if (facts.Target.Kind == SourceLinkTargetKind.Local && facts.Target.Path is { } path)
                    actualMeanings.Add(new RouteMoveReferenceMeaning
                    {
                        TargetId = facts.Target.Id,
                        TargetPath = path,
                        Layer = facts.Target.Layer,
                        Resolution = facts.Target.Resolution,
                        Fragment = facts.Fragment,
                    });
            }
            if (!MeaningsMatch(rows.Meanings, actualMeanings)) return Failed("A final outside authored row changed meaning.");
        }

        var meanings = await ReadMeaningsAsync(
            plan,
            sourceCatalogue,
            expected,
            document,
            cancellationToken).ConfigureAwait(false);
        return MeaningsMatch(expected.Meanings, meanings)
            ? Verified()
            : Failed("A final Route Move authored reference changed meaning.");
    }

    private async ValueTask<ImmutableArray<RouteMoveReferenceMeaning>> ReadMeaningsAsync(
        RouteMovePlan plan,
        SourceCatalogue sourceCatalogue,
        RouteMoveReferenceDocumentPlan expected,
        RouteMoveReferenceDocument document,
        CancellationToken cancellationToken)
    {
        var values = new List<RouteMoveReferenceMeaning>();
        var sourcePath = expected.DestinationSourcePath;
        foreach (var link in document.Facts.Links.Where(link => IsAuthoredLink(document, link)))
        {
            if (expected.IgnoredDestinations.Contains(link.RawDestination, StringComparer.Ordinal)) continue;
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

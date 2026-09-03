using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.References;

internal sealed class RouteMoveReferencePlanner
{
    private readonly RouteMarkdownCatalogueReader _catalogueReader;
    private readonly RouteMoveReferenceScanner _scanner;

    internal RouteMoveReferencePlanner(
        RouteMarkdownCatalogueReader catalogueReader,
        MarkdownDocumentParser markdownParser,
        SourceLinkDestinationResolver destinationResolver,
        FileExpectationValidator expectationValidator)
    {
        _catalogueReader = catalogueReader;
        _scanner = new RouteMoveReferenceScanner(
            markdownParser,
            destinationResolver,
            expectationValidator);
    }

    internal async ValueTask<RouteMoveReferencePlanningResult> BuildAsync(
        RouteMoveReferencePlanningRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var catalogue = await _catalogueReader.ReadAsync(
            request.CatalogueRequest,
            cancellationToken).ConfigureAwait(false);
        var input = new RouteMoveReferenceScanInput
        {
            Request = request,
            Catalogue = catalogue,
            MovedPaths = RouteMoveReferenceChangeProjector.BuildMovedPathMap(
                request.Destination),
        };
        var catalogueBoundary = ReadCatalogueBoundary(input);
        if (catalogueBoundary is not null)
        {
            return new RouteMoveReferencePlanningResult(plan: null, catalogueBoundary);
        }

        var scanned = await _scanner.ScanAsync(input, cancellationToken).ConfigureAwait(false);
        if (scanned.Boundary is { } scanBoundary)
        {
            return new RouteMoveReferencePlanningResult(plan: null, scanBoundary);
        }

        return new RouteMoveReferencePlanningResult(
            FormPlan(
                input,
                scanned.Scan
                    ?? throw new InvalidOperationException(
                        "A successful Route Move reference scan requires its scan.")),
            boundary: null);
    }

    internal async ValueTask<RouteMoveReferencePostMoveResult> ObserveAsync(
        RouteMovePlan plan,
        Framework.Sources.Models.Inventory.SourceCatalogue sourceCatalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var catalogue = await _catalogueReader.ReadAsync(
            plan.Projection.References.Request.CatalogueRequest,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.Coverage == RouteMarkdownCatalogueCoverage.Interrupted)
        {
            return new RouteMoveReferencePostMoveResult(
                RouteMoveReferencePostMoveState.Interrupted,
                "Final Route Move reference observation was interrupted.");
        }

        if (catalogue.Coverage != RouteMarkdownCatalogueCoverage.Complete)
        {
            return new RouteMoveReferencePostMoveResult(
                RouteMoveReferencePostMoveState.Failed,
                catalogue.Findings.FirstOrDefault()?.Cause
                    ?? "Final Route Move reference coverage is incomplete.");
        }

        return await _scanner.ObserveAsync(plan, catalogue, sourceCatalogue, cancellationToken)
            .ConfigureAwait(false);
    }

    private static RouteMoveReferencePlan FormPlan(
        RouteMoveReferenceScanInput input,
        RouteMoveReferenceScan scan)
    {
        var changes = RouteMoveReferenceChangeProjector.BuildFileChanges(
            input.Request.Destination,
            scan.Documents);
        return new RouteMoveReferencePlan
        {
            Request = input.Request,
            Catalogue = input.Catalogue,
            References = new RouteMoveReferences
            {
                Coverage = RouteMoveCoverage.Complete,
                ScannedSourceCount = input.Catalogue.SelectedPaths.Length,
                InspectedSourceCount = scan.Documents.Length,
                OccurrenceCount = scan.OccurrenceCount,
                Rewrites = scan.Rewrites
                    .OrderBy(rewrite => rewrite.SourcePath, StringComparer.Ordinal)
                    .ThenBy(rewrite => rewrite.Location.ByteOffset)
                    .ToImmutableArray(),
            },
            Documents = scan.Documents,
            FileChanges = changes,
        };
    }

    private static RouteMoveResultFormation? ReadCatalogueBoundary(
        RouteMoveReferenceScanInput input)
    {
        var catalogue = input.Catalogue;
        if (catalogue.Coverage == RouteMarkdownCatalogueCoverage.Complete)
        {
            return null;
        }

        var (code, status) = catalogue.Coverage switch
        {
            RouteMarkdownCatalogueCoverage.Blocked =>
                (RouteMoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked),
            RouteMarkdownCatalogueCoverage.Incomplete =>
                (RouteMoveFindingCode.ReferenceCoverageIncomplete, CliSemanticStatus.Incomplete),
            RouteMarkdownCatalogueCoverage.Interrupted =>
                (RouteMoveFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            RouteMarkdownCatalogueCoverage.Complete => throw new ArgumentOutOfRangeException(
                nameof(input),
                catalogue.Coverage,
                "Complete reference coverage cannot form a catalogue boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                catalogue.Coverage,
                "The Markdown catalogue coverage is not defined."),
        };
        return RouteMoveReferenceScanner.Stop(
            input,
            code,
            status,
            catalogue.Findings.FirstOrDefault()?.Path,
            catalogue.Findings.FirstOrDefault()?.Cause
                ?? "Complete Markdown reference coverage could not be established.");
    }
}

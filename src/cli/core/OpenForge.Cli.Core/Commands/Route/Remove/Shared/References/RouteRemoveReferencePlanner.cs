using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;

internal sealed class RouteRemoveReferencePlanner
{
    private readonly RouteMarkdownCatalogueReader _catalogueReader;
    private readonly RouteRemoveReferenceScanner _scanner;

    internal RouteRemoveReferencePlanner(
        RouteMarkdownCatalogueReader catalogueReader,
        MarkdownDocumentParser markdownParser,
        SourceLinkDestinationResolver destinationResolver,
        FileExpectationValidator expectationValidator)
    {
        _catalogueReader = catalogueReader;
        _scanner = new RouteRemoveReferenceScanner(markdownParser, destinationResolver, expectationValidator);
    }

    internal async ValueTask<RouteRemoveReferencePlanningResult> BuildAsync(
        RouteRemoveReferencePlanningRequest request,
        RouteRemoveCategoryInventory inventory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var catalogue = await _catalogueReader.ReadAsync(
            request.CatalogueRequest,
            cancellationToken).ConfigureAwait(false);
        var input = new RouteRemoveReferenceScanInput { Request = request, Catalogue = catalogue };
        if (catalogue.Coverage != RouteMarkdownCatalogueCoverage.Complete)
        {
            var (code, status) = catalogue.Coverage switch
            {
                RouteMarkdownCatalogueCoverage.Blocked =>
                    (RouteRemoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked),
                RouteMarkdownCatalogueCoverage.Incomplete =>
                    (RouteRemoveFindingCode.ReferenceCoverageIncomplete, CliSemanticStatus.Incomplete),
                RouteMarkdownCatalogueCoverage.Interrupted =>
                    (RouteRemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted),
                _ => throw new ArgumentOutOfRangeException(nameof(request), catalogue.Coverage, null),
            };
            return new RouteRemoveReferencePlanningResult(
                plan: null,
                RouteRemoveBoundary.Stop(
                    RouteRemoveBoundary.Start(request.Subject.Request) with
                    {
                        Source = request.Subject.Source,
                        Subject = new RouteRemoveSubject { Kind = request.Subject.Kind },
                    },
                    code,
                    status,
                    catalogue.Findings.FirstOrDefault()?.Path,
                    catalogue.Findings.FirstOrDefault()?.Cause
                        ?? "Complete Markdown reference coverage could not be established."));
        }

        var scanned = await _scanner.ScanAsync(input, inventory, cancellationToken).ConfigureAwait(false);
        if (scanned.Boundary is { } boundary)
        {
            return new RouteRemoveReferencePlanningResult(plan: null, boundary);
        }

        var scan = scanned.Scan
            ?? throw new InvalidOperationException("A successful Route Remove scan requires its facts.");
        return new RouteRemoveReferencePlanningResult(
            new RouteRemoveReferencePlan
            {
                Request = request,
                Catalogue = catalogue,
                References = new RouteRemoveReferences
                {
                    Coverage = RouteRemoveCoverage.Complete,
                    ScannedSourceCount = catalogue.SelectedPaths.Length,
                    InspectedSourceCount = catalogue.SelectedPaths.Length,
                    OccurrenceCount = scan.OccurrenceCount,
                    Detachments = scan.Detachments,
                },
                Documents = scan.Documents,
                FileChanges = RouteRemoveReferenceChangeProjector.BuildFileChanges(scan.Documents),
            },
            boundary: null);
    }

    internal async ValueTask<RouteRemoveReferenceAbsenceResult> ProveAbsenceAsync(
        RouteRemoveAbsenceScope scope,
        SourceCatalogue sourceCatalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(sourceCatalogue);
        var catalogue = await _catalogueReader.ReadAsync(
            new RouteMarkdownCatalogueRequest(
                scope.Request.Workspace,
                ["."],
                [],
                new RouteMarkdownCatalogueFilters([".md"])),
            cancellationToken).ConfigureAwait(false);
        if (catalogue.Coverage == RouteMarkdownCatalogueCoverage.Complete)
        {
            return await _scanner.ProveAbsenceAsync(
                scope,
                sourceCatalogue,
                catalogue,
                cancellationToken).ConfigureAwait(false);
        }

        var (code, status) = catalogue.Coverage switch
        {
            RouteMarkdownCatalogueCoverage.Blocked =>
                (RouteRemoveFindingCode.ReferenceUnsafe, CliSemanticStatus.Blocked),
            RouteMarkdownCatalogueCoverage.Incomplete =>
                (RouteRemoveFindingCode.ReferenceCoverageIncomplete, CliSemanticStatus.Incomplete),
            RouteMarkdownCatalogueCoverage.Interrupted =>
                (RouteRemoveFindingCode.Interrupted, CliSemanticStatus.Interrupted),
            _ => throw new ArgumentOutOfRangeException(
                nameof(scope),
                catalogue.Coverage,
                "The Markdown catalogue coverage is not defined."),
        };
        return new RouteRemoveReferenceAbsenceResult
        {
            References = new RouteRemoveReferences
            {
                Coverage = status switch
                {
                    CliSemanticStatus.Incomplete => RouteRemoveCoverage.Incomplete,
                    CliSemanticStatus.Blocked => RouteRemoveCoverage.Blocked,
                    CliSemanticStatus.Interrupted => RouteRemoveCoverage.Interrupted,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(scope),
                        status,
                        "The Route Remove absence-reference status is not defined."),
                },
                ScannedSourceCount = 0,
                InspectedSourceCount = 0,
                OccurrenceCount = 0,
            },
            Finding = new RouteRemoveFinding(
                code,
                status,
                catalogue.Findings.FirstOrDefault()?.Path,
                catalogue.Findings.FirstOrDefault()?.Cause
                    ?? "Complete Markdown reference coverage could not be established."),
        };
    }

    internal ValueTask<RouteRemoveReferencePostRemoveResult> ObserveAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
        => _scanner.ObserveAsync(plan, cancellationToken);
}

internal sealed record RouteRemoveReferenceAbsenceResult
{
    public required RouteRemoveReferences References { get; init; }

    public RouteRemoveFinding? Finding { get; init; }
}

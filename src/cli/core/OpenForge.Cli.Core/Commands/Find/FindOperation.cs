using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Application;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Commands.Find.Shared.Projection;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Find;

internal sealed class FindOperation(
    FindSourceResolver sourceResolver,
    FindLayerInspector layerInspector,
    FindMatcher matcher,
    FindProjectionBuilder projectionBuilder,
    FindResultBuilder resultBuilder)
{
    private readonly FindSourceResolver _sourceResolver = sourceResolver;
    private readonly FindLayerInspector _layerInspector = layerInspector;
    private readonly FindMatcher _matcher = matcher;
    private readonly FindProjectionBuilder _projectionBuilder = projectionBuilder;
    private readonly FindResultBuilder _resultBuilder = resultBuilder;

    internal ValueTask<FindResult> ExecuteAsync(
        FindRequest request,
        CancellationToken cancellationToken)
        => ExecuteCoreAsync(request, cancellationToken);

    private async ValueTask<FindResult> ExecuteCoreAsync(
        FindRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestEcho = new FindRequestEcho(
            request.Workspace,
            request.UniverseFilter,
            request.Query,
            request.Presentation);
        var inspections = new List<FindLayerInspectionFacts>();
        var matches = new List<FindMatch>();
        var projections = new List<FindProjection>();
        var findings = new List<FindFinding>();
        var projectionRequested = request.Presentation.Content.Effective.Count != 0;
        var matchingCoverage = FindCoverageState.NotStarted;
        var projectionCoverage = projectionRequested
            ? FindProjectionCoverageState.NotStarted
            : FindProjectionCoverageState.NotRequested;
        SourceReadSession? sourceSession = null;
        FindUniverseResolution? universeResolution = null;
        FindUniverse? universe = null;
        FindMatchingFacts? matchingFacts = null;
        SourceRouteFacts? routeFacts = null;
        FindTerminalEvent? terminalEvent = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            sourceSession = await _sourceResolver
                .ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            terminalEvent = CreateInterruptedEvent();
            matchingCoverage = FindCoverageState.Interrupted;
        }
        catch (Exception)
        {
            terminalEvent = CreateFailedEvent();
            matchingCoverage = FindCoverageState.Failed;
        }

        if (terminalEvent is null)
        {
            if (sourceSession is null)
            {
                terminalEvent = CreateFailedEvent();
                matchingCoverage = FindCoverageState.Failed;
            }
            else
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _sourceResolver.ProbePairedOverwritePaths(request, sourceSession, cancellationToken);
                    universeResolution = _sourceResolver.ResolveUniverse(
                        new FindUniverseInput(request, sourceSession));
                    universe = universeResolution.Universe;
                    findings.AddRange(universeResolution.Findings);

                    if (sourceSession.Catalogue.IsCancelled
                        || universeResolution.Findings.Any(
                            finding => finding.Code == FindFindingCode.Interrupted))
                    {
                        terminalEvent = CreateInterruptedEvent();
                        matchingCoverage = FindCoverageState.Interrupted;
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                catch (OperationCanceledException)
                {
                    terminalEvent = CreateInterruptedEvent();
                    matchingCoverage = FindCoverageState.Interrupted;
                }
                catch (Exception)
                {
                    terminalEvent = CreateFailedEvent();
                    matchingCoverage = FindCoverageState.Failed;
                }
            }
        }

        if (universe is not null && terminalEvent is null)
        {
            terminalEvent = await InspectSelectedLayers(
                request,
                universeResolution
                    ?? throw new InvalidOperationException("A Find universe resolution is required for selected-layer inspection."),
                sourceSession
                    ?? throw new InvalidOperationException("A Find source context is required for selected-layer inspection."),
                inspections,
                cancellationToken);
        }

        var terminalBeforeMatching = terminalEvent;
        if (universe is not null
            && (terminalEvent is null || inspections.Count != 0))
        {
            try
            {
                matchingFacts = _matcher.Match(new FindMatchingInput(request, universe, inspections));
                matches.AddRange(matchingFacts.Matches);
                findings.AddRange(matchingFacts.Findings);
                matchingCoverage = terminalBeforeMatching is null
                    ? matchingFacts.Coverage
                    : ReadTerminalCoverage(terminalBeforeMatching);
            }
            catch (OperationCanceledException)
            {
                terminalEvent ??= CreateInterruptedEvent();
                matchingCoverage = ReadTerminalCoverage(terminalEvent);
            }
            catch (Exception)
            {
                terminalEvent ??= CreateFailedEvent();
                matchingCoverage = ReadTerminalCoverage(terminalEvent);
            }
        }
        else if (terminalEvent is not null && matchingCoverage == FindCoverageState.NotStarted)
        {
            matchingCoverage = ReadTerminalCoverage(terminalEvent);
        }

        if (terminalEvent is null && cancellationToken.IsCancellationRequested)
        {
            terminalEvent = CreateInterruptedEvent();
            if (matchingCoverage == FindCoverageState.NotStarted)
            {
                matchingCoverage = FindCoverageState.Interrupted;
            }
        }

        if (universe is not null && matchingFacts is not null && projectionRequested)
        {
            if (terminalEvent is null
                && !cancellationToken.IsCancellationRequested
                && matches.Count != 0
                && request.Presentation.Content.Effective.Any(
                    part => part.Kind == FindContentPartKind.Metadata))
            {
                try
                {
                    var establishedSourceSession = sourceSession
                        ?? throw new InvalidOperationException("A Find source context is required for route facts.");
                    var establishedSelection = universeResolution
                        ?.Selection
                        ?? throw new InvalidOperationException("A Find universe selection is required for route facts.");
                    routeFacts = await _sourceResolver
                        .ReadRouteFactsAsync(
                            new FindRouteFactsInput(
                                establishedSourceSession,
                                establishedSelection),
                            cancellationToken)
                        .ConfigureAwait(false);
                    if (routeFacts?.IsCancelled == true)
                    {
                        terminalEvent = CreateInterruptedEvent();
                    }
                }
                catch (OperationCanceledException)
                {
                    terminalEvent ??= CreateInterruptedEvent();
                }
                catch (Exception)
                {
                    terminalEvent ??= CreateFailedEvent();
                }
            }

            if (terminalEvent is null && cancellationToken.IsCancellationRequested)
            {
                terminalEvent = CreateInterruptedEvent();
            }

            try
            {
                var projectionFacts = _projectionBuilder.Build(
                    new FindProjectionInput(
                        request,
                        universe,
                        inspections,
                        matches,
                        routeFacts));
                projections.AddRange(projectionFacts.Projections);
                findings.AddRange(projectionFacts.Findings);
                projectionCoverage = projectionFacts.Coverage;
            }
            catch (OperationCanceledException)
            {
                terminalEvent ??= CreateInterruptedEvent();
                projectionCoverage = ReadTerminalProjectionCoverage(terminalEvent);
            }
            catch (Exception)
            {
                terminalEvent ??= CreateFailedEvent();
                projectionCoverage = ReadTerminalProjectionCoverage(terminalEvent);
            }
        }

        var resultInput = new FindResultInput(
            requestEcho,
            universe,
            inspections,
            matches,
            projections,
            findings,
            new FindStageCompletion(matchingCoverage, projectionCoverage),
            terminalEvent);
        return _resultBuilder.Build(resultInput);
    }

    private async ValueTask<FindTerminalEvent?> InspectSelectedLayers(
        FindRequest request,
        FindUniverseResolution universeResolution,
        SourceReadSession sourceSession,
        ICollection<FindLayerInspectionFacts> inspections,
        CancellationToken cancellationToken)
    {
        foreach (var source in universeResolution.Selection.Sources)
        {
            foreach (var layer in ReadLayers(source))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return CreateInterruptedEvent();
                }

                try
                {
                    var inspection = await _layerInspector
                        .InspectAsync(
                            new FindLayerInspectionInput(
                                source,
                                sourceSession.DocumentReader,
                                layer),
                            cancellationToken)
                        .ConfigureAwait(false);
                    inspections.Add(inspection);
                }
                catch (OperationCanceledException)
                {
                    return CreateInterruptedEvent();
                }
                catch (Exception)
                {
                    return CreateFailedEvent();
                }

                if (cancellationToken.IsCancellationRequested
                    || inspections.Last().Findings.Any(
                        finding => finding.Code == FindFindingCode.Interrupted))
                {
                    return CreateInterruptedEvent();
                }
            }
        }

        return null;
    }

    private static IEnumerable<SourceLayer> ReadLayers(SourceLogicalSource source)
    {
        yield return source.Base;
        if (source.Overwrite is { } overwrite)
        {
            yield return overwrite;
        }
    }

    private static FindTerminalEvent CreateInterruptedEvent()
        => new(
            FindTerminalEventKind.Interrupted,
            "The Find invocation was cancelled.");

    private static FindTerminalEvent CreateFailedEvent()
        => new(
            FindTerminalEventKind.Failed,
            "An unexpected failure prevented Find from completing.");

    private static FindCoverageState ReadTerminalCoverage(FindTerminalEvent terminalEvent)
        => terminalEvent.Kind switch
        {
            FindTerminalEventKind.Failed => FindCoverageState.Failed,
            FindTerminalEventKind.Interrupted => FindCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(terminalEvent),
                terminalEvent.Kind,
                "The Find terminal event kind is not defined."),
        };

    private static FindProjectionCoverageState ReadTerminalProjectionCoverage(
        FindTerminalEvent terminalEvent)
        => terminalEvent.Kind switch
        {
            FindTerminalEventKind.Failed => FindProjectionCoverageState.Failed,
            FindTerminalEventKind.Interrupted => FindProjectionCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(terminalEvent),
                terminalEvent.Kind,
                "The Find terminal event kind is not defined."),
        };
}

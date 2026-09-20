using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Resolution;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Shared.Inspection;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using static OpenForge.Cli.Core.Commands.References.Shared.Result.ReferencesFindingFactory;

namespace OpenForge.Cli.Core.Commands.References;

internal sealed class ReferencesOperation
{
    private readonly ReferencesSourceResolver _sourceResolver;
    private readonly ReferencesLayerInspector _layerInspector;
    private readonly ReferencesDestinationResolver _destinationResolver;
    private readonly ReferencesResultBuilder _resultBuilder;

    internal ReferencesOperation(
        ReferencesSourceResolver sourceResolver,
        ReferencesLayerInspector layerInspector,
        ReferencesDestinationResolver destinationResolver,
        ReferencesResultBuilder resultBuilder)
    {
        ArgumentNullException.ThrowIfNull(sourceResolver);
        ArgumentNullException.ThrowIfNull(layerInspector);
        ArgumentNullException.ThrowIfNull(destinationResolver);
        ArgumentNullException.ThrowIfNull(resultBuilder);
        _sourceResolver = sourceResolver;
        _layerInspector = layerInspector;
        _destinationResolver = destinationResolver;
        _resultBuilder = resultBuilder;
    }

    internal ValueTask<ReferencesResult> ExecuteAsync(
        ReferencesRequest request,
        CancellationToken cancellationToken)
        => ExecuteCoreAsync(request, cancellationToken);

    private async ValueTask<ReferencesResult> ExecuteCoreAsync(
        ReferencesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var echo = new ReferencesRequestEcho(request);
        var findings = new List<ReferencesFinding>();
        var incomingOccurrences = new List<ReferencesOccurrence>();
        var outgoingOccurrences = new List<ReferencesOccurrence>();
        var incomingCoverage = request.RequestsIncoming ? ReferencesCoverage.Incomplete : ReferencesCoverage.Complete;
        var outgoingCoverage = request.RequestsOutgoing ? ReferencesCoverage.Incomplete : ReferencesCoverage.Complete;
        ReferencesSource? selectedSource = null;
        ReferencesIncomingSelection? incomingSelection = null;
        SourceReadSession? sourceSession = null;
        SourceLogicalSource? logicalSource = null;
        SourceUniverseFilterResolution? filterResolution = null;
        var interrupted = false;
        var failed = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            sourceSession = await _sourceResolver
                .ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            if (sourceSession.Catalogue.IsCancelled)
            {
                AddEvent(findings, ReferencesFindingCode.Interrupted, null);
                interrupted = true;
            }
            ReferencesSourceFindingMapper.AddRootIssues(findings, sourceSession.Catalogue.SelectAll().RootIssues);
        }
        catch (OperationCanceledException)
        {
            AddEvent(findings, ReferencesFindingCode.Interrupted, null);
            interrupted = true;
        }
        catch (Exception)
        {
            AddEvent(findings, ReferencesFindingCode.OperationFailed, null);
            failed = true;
        }

        if (sourceSession is not null && !interrupted && !failed)
        {
            try
            {
                var resolution = _sourceResolver.ResolveReference(
                    request.SourceReference,
                    sourceSession);
                if (resolution.State == SourceReferenceResolutionState.Resolved
                    && resolution.Source is { } source)
                {
                    logicalSource = source;
                    selectedSource = ReferencesSourceProjection.Create(source);
                    ReferencesSourceFindingMapper.AddSourceCatalogueIssues(
                        findings,
                        sourceSession.Catalogue,
                        ToSourceIdentity(source),
                        ReadSourcePaths(source),
                        null);
                }
                else
                {
                    ReferencesSourceFindingMapper.AddSourceResolutionFinding(findings, resolution);
                }
            }
            catch (Exception)
            {
                AddEvent(findings, ReferencesFindingCode.OperationFailed, null);
                failed = true;
            }
        }

        if (sourceSession is not null && request.RequestsIncoming && !interrupted && !failed)
        {
            try
            {
                filterResolution = _sourceResolver.ResolveUniverse(
                    sourceSession,
                    request.SelectorOccurrences);
                incomingSelection = CreateIncomingSelection(
                    filterResolution,
                    request.SelectorOccurrences.Count == 0
                        ? ReferencesSelectionMode.Default
                        : ReferencesSelectionMode.Filtered);
                ReferencesSourceFindingMapper.AddSelectorFindings(findings, filterResolution);
                ReferencesSourceFindingMapper.AddSourceCatalogueIssues(
                    findings,
                    sourceSession.Catalogue,
                    null,
                    null,
                    filterResolution.IsResolved ? filterResolution.Selection : null,
                    ReferencesDirection.In);
                if (!filterResolution.IsResolved)
                {
                    incomingCoverage = ReferencesCoverage.Blocked;
                }
            }
            catch (OperationCanceledException)
            {
                AddEvent(findings, ReferencesFindingCode.Interrupted, ReferencesDirection.In);
                interrupted = true;
            }
            catch (Exception)
            {
                AddEvent(findings, ReferencesFindingCode.OperationFailed, ReferencesDirection.In);
                failed = true;
            }
        }

        if (sourceSession is not null && request.RequestsOutgoing && logicalSource is not null && !interrupted && !failed)
        {
            var inspection = await InspectLayersAsync(
                new ReferencesLayerScanInput(
                    sourceSession,
                    logicalSource,
                    ReferencesDirection.Out,
                    ReferencesProvenance.SelectedSource,
                    sourceSession.Catalogue),
                outgoingOccurrences,
                findings,
                cancellationToken).ConfigureAwait(false);
            outgoingCoverage = inspection.Coverage;
            interrupted |= inspection.Interrupted;
            failed |= inspection.Failed;
        }
        else if (request.RequestsOutgoing && logicalSource is null && !interrupted && !failed)
        {
            outgoingCoverage = ReferencesCoverage.Blocked;
        }

        if (sourceSession is not null
            && request.RequestsIncoming
            && logicalSource is not null
            && filterResolution is { IsResolved: true }
            && !interrupted
            && !failed)
        {
            var effectiveSelection = filterResolution.Selection;
            var attemptedPhysicalPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
            var inspectedSources = new List<ReferencesSourceLayerEvidence>();
            var coverage = ReferencesCoverage.Complete;
            var mode = request.SelectorOccurrences.Count == 0
                ? ReferencesSelectionMode.Default
                : ReferencesSelectionMode.Filtered;
            foreach (var source in effectiveSelection.Sources)
            {
                foreach (var layer in ReadLayers(source))
                {
                    if (!attemptedPhysicalPaths.Add(layer.PhysicalPath))
                    {
                        continue;
                    }

                    var inspection = await _layerInspector.InspectAsync(
                        new ReferencesLayerInspectionInput(
                            sourceSession,
                            source,
                            layer,
                            ReferencesDirection.In,
                            mode == ReferencesSelectionMode.Default
                                ? ReferencesProvenance.DefaultIncomingScan
                                : ReferencesProvenance.FilteredIncomingScan),
                        findings,
                        cancellationToken).ConfigureAwait(false);
                    if (!inspection.Established)
                    {
                        coverage = MergeCoverage(
                            coverage,
                            inspection.Blocked ? ReferencesCoverage.Blocked : ReferencesCoverage.Incomplete);
                    }

                    if (inspection.Established)
                    {
                        inspectedSources.Add(new ReferencesSourceLayerEvidence(
                            ToSourceIdentity(source),
                            layer.Kind,
                            layer.CanonicalPath));
                    }

                    if (inspection.Inspection is not null)
                    {
                        // The incoming scan resolves every source's links so it can find the ones
                        // that point at the selected source. Those resolutions describe where
                        // other files' links lead, which belongs to the outgoing view of the file
                        // that owns them, so they are not recorded as findings here. Only an
                        // occurrence that matches the selected source is kept.
                        var unownedTargetFindings = new List<ReferencesFinding>();
                        foreach (var authored in inspection.Inspection.Links)
                        {
                            ReferencesDestinationFacts targetFacts;
                            try
                            {
                                targetFacts = await ResolveAsync(
                                    sourceSession.Catalogue,
                                    source,
                                    layer,
                                    authored,
                                    unownedTargetFindings,
                                    cancellationToken).ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                                AddEvent(findings, ReferencesFindingCode.Interrupted, ReferencesDirection.In);
                                interrupted = true;
                                break;
                            }
                            catch (Exception)
                            {
                                AddEvent(findings, ReferencesFindingCode.OperationFailed, ReferencesDirection.In);
                                failed = true;
                                break;
                            }

                            if (targetFacts.Target.Kind == ReferencesTargetKind.Local
                                && TargetMatchesSource(targetFacts.Target, logicalSource)
                                && targetFacts.Target.Resolution is ReferencesTargetResolution.Complete
                                    or ReferencesTargetResolution.FragmentMissing
                                    or ReferencesTargetResolution.Unreadable
                                    or ReferencesTargetResolution.EncodingUnsupported)
                            {
                                incomingOccurrences.Add(ToOccurrence(authored, layer, targetFacts.Target));
                            }
                        }
                    }

                    if (interrupted || failed)
                    {
                        break;
                    }

                    if (inspection.Interrupted)
                    {
                        interrupted = true;
                        break;
                    }

                    if (inspection.Failed)
                    {
                        failed = true;
                        break;
                    }
                }

                if (interrupted || failed)
                {
                    break;
                }
            }

            if (interrupted)
            {
                coverage = ReferencesCoverage.Incomplete;
            }
            else if (failed)
            {
                coverage = ReferencesCoverage.Incomplete;
            }

            incomingCoverage = coverage;
            if (incomingSelection is not null)
            {
                incomingSelection = RebindInspectedSources(incomingSelection, inspectedSources);
            }
        }
        else if (request.RequestsIncoming && logicalSource is null && !interrupted && !failed)
        {
            incomingCoverage = ReferencesCoverage.Blocked;
        }

        if (interrupted && !findings.Any(finding => finding.Code == ReferencesFindingCode.Interrupted))
        {
            AddEvent(findings, ReferencesFindingCode.Interrupted, null);
        }

        if (failed && !findings.Any(finding => finding.Code == ReferencesFindingCode.OperationFailed))
        {
            AddEvent(findings, ReferencesFindingCode.OperationFailed, null);
        }

        if (incomingSelection is null && request.RequestsIncoming)
        {
            var roleOccurrences = new Dictionary<SourceUniverseSelectorRole, int>();
            var resolved = new List<ReferencesSelectorResolution>();
            foreach (var selector in request.SelectorOccurrences)
            {
                roleOccurrences.TryGetValue(selector.Role, out var priorOccurrence);
                var occurrence = priorOccurrence + 1;
                roleOccurrences[selector.Role] = occurrence;
                resolved.Add(new ReferencesSelectorResolution(
                    role: selector.Role,
                    occurrence: occurrence,
                    supplied: selector.Value,
                    form: SourceReferenceParser.Parse(selector.Value).Kind,
                    resolution: SourceReferenceResolutionState.Unknown,
                    source: null,
                    expansion: null,
                    candidates: []));
            }

            incomingSelection = new ReferencesIncomingSelection(
                mode: request.SelectorOccurrences.Count == 0 ? ReferencesSelectionMode.Default : ReferencesSelectionMode.Filtered,
                supplied: request.SelectorOccurrences.Select(ToSelectorOccurrence),
                resolved: resolved,
                effectiveSources: [],
                inspectedSources: []);
        }

        return _resultBuilder.Build(new ReferencesResultInput
        {
            Request = echo,
            Source = selectedSource,
            IncomingSelection = incomingSelection,
            IncomingOccurrences = incomingOccurrences,
            OutgoingOccurrences = outgoingOccurrences,
            IncomingCoverage = incomingCoverage,
            OutgoingCoverage = outgoingCoverage,
            Findings = findings,
        });
    }

    private async ValueTask<LayerInspectionResult> InspectLayersAsync(
        ReferencesLayerScanInput input,
        ICollection<ReferencesOccurrence> occurrences,
        ICollection<ReferencesFinding> findings,
        CancellationToken cancellationToken)
    {
        var session = input.Session;
        var source = input.Source;
        var direction = input.Direction;
        var provenance = input.Provenance;
        var catalogue = input.Catalogue;
        var coverage = ReferencesCoverage.Complete;
        var interrupted = false;
        var failed = false;
        foreach (var layer in ReadLayers(source))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
                return new LayerInspectionResult(ReferencesCoverage.Incomplete, true, false);
            }

            var inspection = await _layerInspector.InspectAsync(
                new ReferencesLayerInspectionInput(
                    session,
                    source,
                    layer,
                    direction,
                    provenance),
                findings,
                cancellationToken).ConfigureAwait(false);
            if (!inspection.Established)
            {
                coverage = MergeCoverage(
                    coverage,
                    inspection.Blocked ? ReferencesCoverage.Blocked : ReferencesCoverage.Incomplete);
            }

            if (inspection.Inspection is not null)
            {
                foreach (var authored in inspection.Inspection.Links)
                {
                    ReferencesDestinationFacts targetFacts;
                    try
                    {
                        targetFacts = await ResolveAsync(
                            catalogue,
                            source,
                            layer,
                            authored,
                            findings,
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        AddEvent(findings, ReferencesFindingCode.Interrupted, direction);
                        interrupted = true;
                        break;
                    }
                    catch (Exception)
                    {
                        AddEvent(findings, ReferencesFindingCode.OperationFailed, direction);
                        failed = true;
                        break;
                    }

                    occurrences.Add(ToOccurrence(authored, layer, targetFacts.Target));
                }
            }

            interrupted |= inspection.Interrupted;
            failed |= inspection.Failed;
            if (interrupted || failed)
            {
                break;
            }
        }

        return new LayerInspectionResult(
            interrupted || failed ? ReferencesCoverage.Incomplete : coverage,
            interrupted,
            failed);
    }

    private async ValueTask<ReferencesDestinationFacts> ResolveAsync(
        SourceCatalogue catalogue,
        SourceLogicalSource source,
        SourceLayer layer,
        ReferencesAuthoredLink authored,
        ICollection<ReferencesFinding> findings,
        CancellationToken cancellationToken)
    {
        var target = await _destinationResolver.ResolveAsync(
            new ReferencesDestinationInput(
                // The catalogue is created for exactly one selected workspace.
                catalogue.Workspace,
                catalogue,
                source,
                layer,
                authored.RawDestination),
            cancellationToken).ConfigureAwait(false);

        if (target.Finding is { } finding)
        {
            AddFinding(
                findings,
                new ReferencesFindingInput(finding.Code, finding.Cause)
                {
                    Direction = authored.Direction,
                    Source = ToSourceIdentity(authored.Source),
                    Layer = authored.Layer,
                    Path = authored.CanonicalPath,
                    Location = authored.Location,
                    DestinationLocation = authored.DestinationLocation,
                    Candidates = finding.Candidates,
                });
        }

        return target;
    }

    private static ReferencesOccurrence ToOccurrence(
        ReferencesAuthoredLink authored,
        SourceLayer layer,
        ReferencesTarget target)
        => new(
            authored.Direction,
            new ReferencesOccurrenceSource(
                authored.Source.Id,
                layer.CanonicalPath,
                layer.Kind),
            authored.Location,
            authored.DestinationLocation,
            authored.RawDestination,
            authored.Fragment,
            target,
            authored.Provenance);

    private static ReferencesIncomingSelection CreateIncomingSelection(
        SourceUniverseFilterResolution resolution,
        ReferencesSelectionMode mode)
        => new(
            mode,
            resolution.Selectors.Select(selector => new ReferencesSelectorOccurrence(
                selector.Occurrence.Role,
                selector.Occurrence.Value)),
            resolution.Selectors.Select(selector => new ReferencesSelectorResolution(
                selector.Occurrence.Role,
                selector.RoleOccurrence,
                selector.Occurrence.Value,
                selector.Reference.Form,
                selector.Reference.State,
                selector.Reference.Source is { } source ? ToSourceIdentity(source) : null,
                selector.Expansion,
                selector.Reference.Candidates.Select(ToSourceIdentity))),
            resolution.Selection.Sources.Select(ToSourceIdentity),
            []);

    private static ReferencesIncomingSelection RebindInspectedSources(
        ReferencesIncomingSelection existing,
        IEnumerable<ReferencesSourceLayerEvidence> inspectedSources)
    {
        ArgumentNullException.ThrowIfNull(inspectedSources);
        var evidence = inspectedSources
            .OrderBy(value => value.Source.Id, StringComparer.Ordinal)
            .ThenBy(value => value.Source.Path, StringComparer.Ordinal)
            .ThenBy(value => value.Layer)
            .ThenBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
        return new ReferencesIncomingSelection(
            existing.Mode,
            existing.Supplied,
            existing.Resolved,
            existing.EffectiveSources,
            evidence);
    }

    private static ReferencesSelectorOccurrence ToSelectorOccurrence(SourceUniverseSelectorOccurrence occurrence)
        => new(occurrence.Role, occurrence.Value);


    private static ReferencesSourceIdentity ToSourceIdentity(SourceLogicalSource source)
        => new(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);

    private static ReferencesSourceIdentity ToSourceIdentity(ReferencesSource source)
        => new(source.Id, source.Path);

    private static ReferencesSourceIdentity ToSourceIdentity(ReferencesAuthoredLink source)
        => new(source.Source.Id, source.Source.Path);

    private static IReadOnlyList<SourceLayer> ReadLayers(SourceLogicalSource source)
        => source.Overwrite is { } overwrite ? [source.Base, overwrite] : [source.Base];

    private static IReadOnlySet<string> ReadSourcePaths(SourceLogicalSource source)
        => ReadLayers(source)
            .Select(layer => layer.CanonicalPath)
            .Append(source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);

    private static bool TargetMatchesSource(ReferencesTarget target, SourceLogicalSource source)
        => target.Path is { } path
            && ReadLayers(source).Any(layer => string.Equals(layer.CanonicalPath, path, StringComparison.Ordinal));

    private static ReferencesCoverage MergeCoverage(
        ReferencesCoverage current,
        ReferencesCoverage candidate)
    {
        if (current == ReferencesCoverage.Blocked || candidate == ReferencesCoverage.Blocked)
        {
            return ReferencesCoverage.Blocked;
        }

        return current == ReferencesCoverage.Incomplete || candidate == ReferencesCoverage.Incomplete
            ? ReferencesCoverage.Incomplete
            : ReferencesCoverage.Complete;
    }

    private sealed record LayerInspectionResult(
        ReferencesCoverage Coverage,
        bool Interrupted,
        bool Failed);
}

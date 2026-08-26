using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Operation;
using OpenForge.Cli.Core.Commands.References.Models.Request;
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
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using static OpenForge.Cli.Core.Commands.References.Shared.Result.ReferencesFindingFactory;

namespace OpenForge.Cli.Core.Commands.References;

internal sealed class ReferencesOperation
{
    private readonly ReferencesOperationComponents _components;
    private readonly ReferencesLayerInspector _layerInspector;
    private readonly ReferencesDestinationResolver _destinationResolver;

    internal ReferencesOperation(ReferencesOperationComponents components)
    {
        ArgumentNullException.ThrowIfNull(components);
        _components = components;
        _layerInspector = new ReferencesLayerInspector(components);
        _destinationResolver = new ReferencesDestinationResolver(
            components.PhysicalPathResolver,
            components.StrictUtf8Reader,
            components.MarkdownParser);
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
        ReferencesSourceReadContext? sourceContext = null;
        SourceLogicalSource? logicalSource = null;
        SourceUniverseFilterResolution? filterResolution = null;
        var interrupted = false;
        var failed = false;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            sourceContext = await _components
                .SourceBoundaryReader(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            if (sourceContext.Catalogue.IsCancelled)
            {
                AddEvent(findings, ReferencesFindingCode.Interrupted, null);
                interrupted = true;
            }
            ReferencesSourceFindingMapper.AddRootIssues(findings, sourceContext.Catalogue.SelectAll().RootIssues);
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

        if (sourceContext is not null && !interrupted && !failed)
        {
            try
            {
                var resolution = _components.SourceReferenceResolver.Resolve(
                    request.SourceReference,
                    sourceContext.Catalogue);
                if (resolution.State == SourceReferenceResolutionState.Resolved
                    && resolution.Source is { } source)
                {
                    logicalSource = source;
                    selectedSource = ToSource(source);
                    ReferencesSourceFindingMapper.AddSourceCatalogueIssues(
                        findings,
                        sourceContext.Catalogue,
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

        if (sourceContext is not null && request.RequestsIncoming && !interrupted && !failed)
        {
            try
            {
                filterResolution = _components.UniverseFilterResolver.Resolve(
                    new SourceUniverseFilterRequest(
                        sourceContext.Catalogue,
                        sourceContext.DefaultSelectionScope,
                        request.SelectorOccurrences));
                incomingSelection = CreateIncomingSelection(
                    filterResolution,
                    request.SelectorOccurrences.Count == 0
                        ? ReferencesSelectionMode.Default
                        : ReferencesSelectionMode.Filtered);
                ReferencesSourceFindingMapper.AddSelectorFindings(findings, filterResolution);
                ReferencesSourceFindingMapper.AddSourceCatalogueIssues(
                    findings,
                    sourceContext.Catalogue,
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

        if (sourceContext is not null && request.RequestsOutgoing && logicalSource is not null && !interrupted && !failed)
        {
            var inspection = await InspectLayersAsync(
                sourceContext,
                logicalSource,
                ReferencesDirection.Out,
                ReferencesProvenance.SelectedSource,
                sourceContext.Catalogue,
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

        if (sourceContext is not null
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
                        sourceContext,
                        source,
                        layer,
                        ReferencesDirection.In,
                        mode == ReferencesSelectionMode.Default
                            ? ReferencesProvenance.DefaultIncomingScan
                            : ReferencesProvenance.FilteredIncomingScan,
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
                        foreach (var authored in inspection.Inspection.Links)
                        {
                            ReferencesDestinationFacts targetFacts;
                            try
                            {
                                targetFacts = await ResolveAsync(
                                    sourceContext.Catalogue,
                                    source,
                                    layer,
                                    authored,
                                    findings,
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
            incomingSelection = new ReferencesIncomingSelection(
                request.SelectorOccurrences.Count == 0 ? ReferencesSelectionMode.Default : ReferencesSelectionMode.Filtered,
                request.SelectorOccurrences.Select(ToSelectorOccurrence),
                [],
                [],
                []);
        }

        return _components.ResultBuilder.Build(new ReferencesResultInput
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
        ReferencesSourceReadContext context,
        SourceLogicalSource source,
        ReferencesDirection direction,
        ReferencesProvenance provenance,
        SourceCatalogue catalogue,
        ICollection<ReferencesOccurrence> occurrences,
        ICollection<ReferencesFinding> findings,
        CancellationToken cancellationToken)
    {
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
                context,
                source,
                layer,
                direction,
                provenance,
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
                finding.Code,
                authored.Direction,
                ToSourceIdentity(authored.Source),
                authored.Layer,
                authored.CanonicalPath,
                authored.Location,
                authored.DestinationLocation,
                finding.Cause,
                finding.Candidates);
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

    private static ReferencesSource ToSource(SourceLogicalSource source)
    {
        var layers = source.Overwrite is { } overwrite
            ? new ReferencesSourceLayer[]
            {
                new ReferencesSourceLayer(SourceLayerKind.Base, source.Base.CanonicalPath),
                new ReferencesSourceLayer(SourceLayerKind.Overwrite, overwrite.CanonicalPath),
            }
            : new ReferencesSourceLayer[]
            { new ReferencesSourceLayer(SourceLayerKind.Base, source.Base.CanonicalPath) };
        return new ReferencesSource(source.Identity.AutomaticId, source.Identity.CanonicalBasePath, layers);
    }

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

using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Application;

internal sealed class FindApplicabilityResolver(FindLayerInspector layerInspector)
{
    private readonly FindLayerInspector _layerInspector = layerInspector;

    internal async ValueTask<FindApplicabilityEvaluation> EvaluateAsync(
        FindRequest request,
        SourceReadSession sourceSession,
        SourceRouteFacts? routeFacts,
        IReadOnlyList<FindMatch> matches,
        IReadOnlyList<FindLayerInspectionFacts> selectedInspections,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceSession);
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(selectedInspections);

        var requireCompatibility = request.WorkingPaths.Count != 0;
        if (!requireCompatibility && routeFacts is null)
        {
            return new FindApplicabilityEvaluation(matches, []);
        }

        var inspectionByPath = selectedInspections
            .Where(inspection => inspection.Layer.Kind == SourceLayerKind.Base)
            .GroupBy(inspection => inspection.Layer.CanonicalPath, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var inspectedAncestors = new Dictionary<string, FindLayerInspectionFacts>(StringComparer.Ordinal);
        var evaluatedMatches = new List<FindMatch>();
        var findings = new List<FindFinding>();

        foreach (var match in matches)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = sourceSession.Catalogue.FindByPath(match.Path);
            if (source is null)
            {
                if (requireCompatibility)
                {
                    AddFinding(findings, CreateFinding(
                        FindFindingCode.InspectionUnavailable,
                        null,
                        match.Path,
                        "The source could not be established for file applicability."));
                    continue;
                }

                evaluatedMatches.Add(match);
                continue;
            }

            var chainPaths = ReadChainPaths(routeFacts, match.Path, out var unresolved);
            if (unresolved)
            {
                if (requireCompatibility)
                {
                    AddFinding(findings, CreateFinding(
                        FindFindingCode.LayerUnresolved,
                        source,
                        source.Base.CanonicalPath,
                        "The source ancestry could not be established for file applicability."));
                    continue;
                }

                evaluatedMatches.Add(match);
                continue;
            }

            var conditions = new List<SourceApplyToCondition>(chainPaths.Count);
            var unavailable = false;
            foreach (var path in chainPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ancestor = sourceSession.Catalogue.FindByPath(path);
                if (ancestor is null)
                {
                    unavailable = true;
                    AddFinding(findings, CreateFinding(
                        FindFindingCode.LayerUnresolved,
                        source,
                        source.Base.CanonicalPath,
                        "The source ancestry contains a layer that is unavailable for file applicability."));
                    break;
                }

                var inspection = await ReadInspectionAsync(
                    ancestor,
                    sourceSession,
                    inspectionByPath,
                    inspectedAncestors,
                    cancellationToken).ConfigureAwait(false);
                var frontmatter = inspection?.Frontmatter;
                var metadata = frontmatter?.ApplyTo;
                if (metadata is null)
                {
                    unavailable = true;
                    if (requireCompatibility)
                    {
                        AddFinding(findings, CreateFinding(
                            inspection is null || inspection.Findings.Count != 0
                                ? FindFindingCode.InspectionUnavailable
                                : FindFindingCode.FrontmatterUnavailable,
                            ancestor,
                            ancestor.Base.CanonicalPath,
                            "The source applicability metadata could not be inspected."));
                    }

                    break;
                }

                conditions.Add(new SourceApplyToCondition(path, metadata));
            }

            if (unavailable)
            {
                if (!requireCompatibility)
                {
                    evaluatedMatches.Add(match);
                }

                continue;
            }

            var applicability = SourceApplicabilityEvaluator.Evaluate(
                conditions,
                request.NormalizedWorkingPaths);
            if (applicability.State == SourceApplicabilityState.Invalid)
            {
                foreach (var invalidCondition in conditions.Where(
                             condition => condition.Metadata.State == ApplyToMetadataState.Invalid))
                {
                    var invalidSource = sourceSession.Catalogue.FindByPath(invalidCondition.CanonicalSourcePath);
                    AddFinding(findings, CreateFinding(
                        FindFindingCode.FrontmatterUnavailable,
                        invalidSource ?? source,
                        invalidSource?.Base.CanonicalPath ?? source.Base.CanonicalPath,
                        "The source applyTo declaration is malformed."));
                }
            }

            if (requireCompatibility
                && applicability.State is not (SourceApplicabilityState.Matched or SourceApplicabilityState.Unconditioned))
            {
                continue;
            }

            if (!requireCompatibility && applicability.State == SourceApplicabilityState.Unconditioned)
            {
                evaluatedMatches.Add(match);
                continue;
            }

            evaluatedMatches.Add(match.WithApplicability(Project(applicability)));
        }

        return new FindApplicabilityEvaluation(evaluatedMatches, findings);
    }

    private async ValueTask<FindLayerInspectionFacts?> ReadInspectionAsync(
        SourceLogicalSource source,
        SourceReadSession sourceSession,
        IReadOnlyDictionary<string, FindLayerInspectionFacts> selectedInspections,
        IDictionary<string, FindLayerInspectionFacts> inspectedAncestors,
        CancellationToken cancellationToken)
    {
        var path = source.Base.CanonicalPath;
        if (selectedInspections.TryGetValue(path, out var selected))
        {
            return selected;
        }

        if (inspectedAncestors.TryGetValue(path, out var inspected))
        {
            return inspected;
        }

        try
        {
            inspected = await _layerInspector.InspectAsync(
                new FindLayerInspectionInput(
                    source,
                    sourceSession.DocumentReader,
                    source.Base),
                cancellationToken).ConfigureAwait(false);
            inspectedAncestors.Add(path, inspected);
            return inspected;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IReadOnlyList<string> ReadChainPaths(
        SourceRouteFacts? routeFacts,
        string candidatePath,
        out bool unresolved)
    {
        unresolved = false;
        if (routeFacts is null)
        {
            unresolved = true;
            return [];
        }

        var leaf = routeFacts.Topology.FindByPath(candidatePath);
        if (leaf is null)
        {
            return [candidatePath];
        }

        var paths = new List<string>();
        var current = leaf;
        while (true)
        {
            paths.Add(current.Identity.CanonicalBasePath);
            if (current.ParentState == SourceRouteParentState.Ambiguous)
            {
                unresolved = true;
                return [];
            }

            if (current.ParentState == SourceRouteParentState.None)
            {
                break;
            }

            current = routeFacts.Topology.FindByPath(current.ParentPaths[0]);
            if (current is null)
            {
                unresolved = true;
                return [];
            }
        }

        paths.Reverse();
        return paths;
    }

    private static FindMatchApplicability Project(SourceApplicabilityResult result)
    {
        var conditions = result.Conditions
            .Where(condition => condition.Metadata.State != ApplyToMetadataState.Absent)
            .Select(condition => new FindApplicabilityCondition(
                condition.CanonicalSourcePath,
                condition.Metadata.Patterns.Select(pattern => pattern.Text)));
        return new FindMatchApplicability(
            StateName(result.State),
            conditions,
            result.MatchingPaths);
    }

    private static string StateName(SourceApplicabilityState state)
        => state switch
        {
            SourceApplicabilityState.Unconditioned => "unconditioned",
            SourceApplicabilityState.Matched => "matched",
            SourceApplicabilityState.Unmatched => "unmatched",
            SourceApplicabilityState.Pending => "pending",
            SourceApplicabilityState.Invalid => "invalid",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The source applicability state is not defined."),
        };

    private static FindFinding CreateFinding(
        FindFindingCode code,
        SourceLogicalSource? source,
        string path,
        string cause)
    {
        var identity = source is null
            ? null
            : new FindSourceIdentity(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
        return new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            null,
            cause,
            null,
            null,
            identity,
            source is null ? null : SourceLayerKind.Base,
            source is null ? null : path,
            null,
            null,
            []);
    }

    private static void AddFinding(ICollection<FindFinding> findings, FindFinding candidate)
    {
        if (!findings.Any(existing => existing.Code == candidate.Code
                && string.Equals(existing.Source?.Id, candidate.Source?.Id, StringComparison.Ordinal)
                && string.Equals(existing.Source?.Path, candidate.Source?.Path, StringComparison.Ordinal)
                && existing.Layer == candidate.Layer
                && string.Equals(existing.Path, candidate.Path, StringComparison.Ordinal)))
        {
            findings.Add(candidate);
        }
    }
}

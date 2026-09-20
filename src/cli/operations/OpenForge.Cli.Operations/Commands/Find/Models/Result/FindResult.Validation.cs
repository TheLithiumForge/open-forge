using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal sealed partial record FindResult
{
    private static void ValidateStatusAndCoverage(
        CliSemanticStatus status,
        FindCoverage coverage,
        FindPresentationSelection presentation,
        IReadOnlyList<FindFinding> findings)
    {
        var expectedState = status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindCoverageState.Complete,
            CliSemanticStatus.Incomplete => FindCoverageState.Incomplete,
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Failed => FindCoverageState.Failed,
            CliSemanticStatus.Interrupted => FindCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find result status is not defined."),
        };
        if (coverage.State != expectedState)
        {
            throw new ArgumentException("The Find coverage state does not match the result status.", nameof(coverage));
        }

        var contentRequested = presentation.Content.IsRequested;
        if ((coverage.Projection == FindProjectionCoverageState.NotRequested) == contentRequested)
        {
            throw new ArgumentException("Find projection coverage must reflect whether content was requested.", nameof(coverage));
        }

        var stageStatesAgree = StageStatesAgree(
            status,
            coverage.Matching,
            coverage.Projection,
            contentRequested);
        if (!stageStatesAgree)
        {
            throw new ArgumentException("The Find stage coverage does not support the aggregate result status.", nameof(coverage));
        }

        if (status != CliSemanticStatus.Complete
            && (!findings.Any(finding => finding.Status == status)
                || findings.Any(finding => !IsAllowedFindingStatus(status, finding.Status))))
        {
            throw new ArgumentException("The Find finding set does not support the aggregate result status.", nameof(findings));
        }
    }

    internal static bool StageStatesAgree(
        CliSemanticStatus status,
        FindCoverageState matching,
        FindProjectionCoverageState projection,
        bool contentRequested)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention =>
                matching == FindCoverageState.Complete
                    && projection is FindProjectionCoverageState.Complete or FindProjectionCoverageState.NotRequested,
            CliSemanticStatus.Incomplete =>
                matching == FindCoverageState.Incomplete
                    && projection is (FindProjectionCoverageState.Complete
                        or FindProjectionCoverageState.Incomplete
                        or FindProjectionCoverageState.NotRequested)
                    || matching == FindCoverageState.Complete
                        && projection == FindProjectionCoverageState.Incomplete,
            CliSemanticStatus.Invalid =>
                matching == FindCoverageState.NotStarted
                    && projection == (contentRequested
                        ? FindProjectionCoverageState.NotStarted
                        : FindProjectionCoverageState.NotRequested),
            CliSemanticStatus.Blocked =>
                matching == FindCoverageState.Blocked
                    && projection == (contentRequested
                        ? FindProjectionCoverageState.Blocked
                        : FindProjectionCoverageState.NotRequested),
            CliSemanticStatus.Failed =>
                matching == FindCoverageState.Failed
                    && projection == (contentRequested
                        ? FindProjectionCoverageState.Failed
                        : FindProjectionCoverageState.NotRequested)
                    || (matching is FindCoverageState.Complete or FindCoverageState.Incomplete)
                        && projection is (FindProjectionCoverageState.Complete
                            or FindProjectionCoverageState.Incomplete
                            or FindProjectionCoverageState.Failed)
                    || (!contentRequested
                        && matching is (FindCoverageState.Complete or FindCoverageState.Incomplete)
                        && projection == FindProjectionCoverageState.NotRequested),
            CliSemanticStatus.Interrupted =>
                matching == FindCoverageState.Interrupted
                    && projection == (contentRequested
                        ? FindProjectionCoverageState.Interrupted
                        : FindProjectionCoverageState.NotRequested)
                    || (matching is FindCoverageState.Complete or FindCoverageState.Incomplete)
                        && projection is (FindProjectionCoverageState.Complete
                            or FindProjectionCoverageState.Incomplete
                            or FindProjectionCoverageState.Interrupted)
                    || (!contentRequested
                        && matching is (FindCoverageState.Complete or FindCoverageState.Incomplete)
                        && projection == FindProjectionCoverageState.NotRequested),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Find result status is not defined."),
        };

    internal static bool IsAllowedFindingStatus(
        CliSemanticStatus resultStatus,
        CliSemanticStatus findingStatus)
        => resultStatus switch
        {
            CliSemanticStatus.Complete => false,
            CliSemanticStatus.Attention => findingStatus == CliSemanticStatus.Attention,
            CliSemanticStatus.Incomplete => findingStatus is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete,
            CliSemanticStatus.Invalid => findingStatus == CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked => findingStatus is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete or CliSemanticStatus.Blocked,
            CliSemanticStatus.Failed => findingStatus is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete or CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted => findingStatus is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete or CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resultStatus),
                resultStatus,
                "The Find result status is not defined."),
        };

    private static void ValidateNext(
        CliSemanticStatus status,
        IReadOnlyList<FindFinding> findings,
        CliNextAction? next)
    {
        var expected = status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => FindDefinitions.IncompleteNextAction,
            CliSemanticStatus.Invalid => FindDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked when findings
                .Where(finding => finding.Status == CliSemanticStatus.Blocked)
                .All(finding => finding.Code == FindFindingCode.SelectorAmbiguous) => FindDefinitions.SelectorAmbiguousNextAction,
            CliSemanticStatus.Blocked => FindDefinitions.BlockedNextAction,
            CliSemanticStatus.Failed => FindDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => FindDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find result status is not defined."),
        };
        if (!NextEquals(next, expected))
        {
            throw new ArgumentException("The Find next action does not match the result status.", nameof(next));
        }
    }

    private static bool NextEquals(CliNextAction? actual, CliNextAction? expected)
        => actual is null && expected is null
            || actual is not null
                && expected is not null
                && string.Equals(actual.Command, expected.Command, StringComparison.Ordinal)
                && string.Equals(actual.Reason, expected.Reason, StringComparison.Ordinal);

    private static void ValidateMatches(
        FindUniverse universe,
        FindQuery query,
        FindContentSelection content,
        IReadOnlyList<FindMatch> matches)
    {
        for (var index = 0; index < matches.Count; index++)
        {
            if (matches[index].Position != index + 1)
            {
                throw new ArgumentException("Find match positions must be contiguous and ordered.", nameof(matches));
            }

            if (index > 0)
            {
                var previous = matches[index - 1];
                var current = matches[index];
                var idOrder = string.CompareOrdinal(previous.Id, current.Id);
                if (idOrder > 0
                    || idOrder == 0 && string.CompareOrdinal(previous.Path, current.Path) >= 0)
                {
                    throw new ArgumentException("Find matches must use ordinal ID and path order.", nameof(matches));
                }
            }

            foreach (var evidence in matches[index].Evidence)
            {
                if (evidence.Predicate > query.EffectivePredicates.Count)
                {
                    throw new ArgumentException("Find evidence must reference an effective predicate.", nameof(matches));
                }

                var predicate = query.EffectivePredicates[evidence.Predicate - 1];
                if (evidence.Kind != predicate.Kind
                    || !string.Equals(evidence.Query, predicate.SuppliedValue, StringComparison.Ordinal))
                {
                    throw new ArgumentException("Find evidence must agree with its effective predicate.", nameof(matches));
                }
            }

            ValidateProjections(content, matches[index]);
        }

        if (matches
                .Select(match => (match.Id, match.Path))
                .Distinct()
                .Count() != matches.Count)
        {
            throw new ArgumentException("Find matches must retain unique logical identities.", nameof(matches));
        }

        if (universe.MatchedCount is { } matchedCount && matchedCount != matches.Count)
        {
            throw new ArgumentException("The Find matched count must equal the concrete match count.", nameof(universe));
        }
    }

    private static void ValidateProjections(
        FindContentSelection content,
        FindMatch match)
    {
        if (content.Effective.Count == 0)
        {
            if (match.Projections.Count != 0)
            {
                throw new ArgumentException("Find matches cannot carry unrequested projections.", nameof(match));
            }

            return;
        }

        if (match.Projections.Any(projection => !content.Effective.Any(part => ProjectionMatchesPart(projection, part))))
        {
            throw new ArgumentException("Find matches cannot carry projection parts that were not requested.", nameof(match));
        }

        IReadOnlyList<SourceLayerKind>? physicalLayers = null;
        FindMetadata? metadata = null;
        foreach (var part in content.Effective)
        {
            var projections = match.Projections
                .Where(projection => ProjectionMatchesPart(projection, part))
                .ToArray();
            if (part.Kind == FindContentPartKind.Metadata)
            {
                if (projections.Length != 1)
                {
                    throw new ArgumentException("Find metadata projection must occur exactly once per match.", nameof(match));
                }

                metadata = projections[0].Metadata;
                continue;
            }

            var layers = projections
                .Select(projection => projection.Layer
                    ?? throw new ArgumentException("A physical Find projection requires a layer.", nameof(match)))
                .ToArray();
            if (layers.Length is < 1 or > 2
                || layers[0] != SourceLayerKind.Base
                || layers.Length == 2 && layers[1] != SourceLayerKind.Overwrite)
            {
                throw new ArgumentException("Each requested physical Find part requires base-then-overwrite projection cardinality.", nameof(match));
            }

            if (physicalLayers is null)
            {
                physicalLayers = layers;
            }
            else if (!physicalLayers.SequenceEqual(layers))
            {
                throw new ArgumentException("Every requested physical Find part must cover the same source layers.", nameof(match));
            }
        }

        if (metadata is not null
            && physicalLayers is not null
            && !metadata.Layers.Select(layer => layer.Kind).SequenceEqual(physicalLayers))
        {
            throw new ArgumentException("Find metadata and authored projections must cover the same source layers.", nameof(match));
        }
    }

    private static bool ProjectionMatchesPart(
        FindProjection projection,
        FindContentPart part)
        => projection.Part == part.Kind
            && string.Equals(projection.Name, part.Name, StringComparison.Ordinal);

    private static void ValidateProjectionStates(
        FindCoverage coverage,
        FindContentSelection content,
        IReadOnlyList<FindFinding> findings,
        IReadOnlyList<FindMatch> matches)
    {
        if (content.Effective.Count == 0)
        {
            return;
        }

        var projections = matches
            .SelectMany(match => match.Projections.Select(projection => (Match: match, Projection: projection)))
            .ToArray();
        if (coverage.Projection is FindProjectionCoverageState.NotStarted or FindProjectionCoverageState.Blocked)
        {
            if (projections.Length != 0)
            {
                throw new ArgumentException("A projection stage that did not run cannot carry projections.", nameof(matches));
            }

            return;
        }

        var hasIncompleteProjection = projections.Any(value => value.Projection.State is
            FindProjectionState.Unavailable or FindProjectionState.Ambiguous);
        var expectedCoverage = hasIncompleteProjection
            ? FindProjectionCoverageState.Incomplete
            : FindProjectionCoverageState.Complete;
        if (coverage.Projection is not (FindProjectionCoverageState.Failed or FindProjectionCoverageState.Interrupted)
            && coverage.Projection != expectedCoverage)
        {
            throw new ArgumentException("Find projection coverage must reflect every concrete projection state.", nameof(coverage));
        }

        foreach (var (match, projection) in projections)
        {
            if (projection.State == FindProjectionState.Missing
                && !findings.Any(finding => IsProjectionFinding(
                    finding,
                    match,
                    projection,
                    FindFindingCode.ProjectionMissing,
                    requireLayer: false)))
            {
                throw new ArgumentException("A missing Find projection requires its projection-missing finding.", nameof(findings));
            }

            if (projection.State == FindProjectionState.Ambiguous
                && !findings.Any(finding => IsProjectionFinding(
                    finding,
                    match,
                    projection,
                    FindFindingCode.SectionAmbiguous,
                    requireLayer: true)))
            {
                throw new ArgumentException("An ambiguous Find projection requires its section-ambiguous finding.", nameof(findings));
            }

            if (projection.State == FindProjectionState.Unavailable
                && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete
                    && finding.Source is { } source
                    && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
                    && string.Equals(source.Path, match.Path, StringComparison.Ordinal)))
            {
                throw new ArgumentException("An unavailable Find projection requires an applicable incomplete finding.", nameof(findings));
            }
        }
    }

    private static bool IsProjectionFinding(
        FindFinding finding,
        FindMatch match,
        FindProjection projection,
        FindFindingCode code,
        bool requireLayer)
        => finding.Code == code
            && finding.Source is { } source
            && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
            && string.Equals(source.Path, match.Path, StringComparison.Ordinal)
            && (!requireLayer && finding.Layer is null || finding.Layer == projection.Layer)
            && string.Equals(finding.Region?.Name, projection.Name, StringComparison.Ordinal);
}

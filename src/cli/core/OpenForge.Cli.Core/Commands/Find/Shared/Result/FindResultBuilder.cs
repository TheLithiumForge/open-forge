using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultBuilder
{
    internal FindResult Build(FindResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var contentRequested = input.Request.Presentation.Content.IsRequested;
        var orderedMatches = BuildMatches(input.Matches, input.Projections, input.Request.Presentation.Content);
        var findings = new List<FindFinding>(input.Findings);
        foreach (var inspection in input.Inspections)
        {
            foreach (var finding in inspection.Findings)
            {
                if (!findings.Any(existing => ReferenceEquals(existing, finding)))
                {
                    findings.Add(finding);
                }
            }
        }

        AddProjectionFindings(findings, orderedMatches, contentRequested);
        AddStageFindings(findings, input, orderedMatches, contentRequested);

        var status = ReadStatus(input, findings);
        var matchingCoverage = ReadMatchingCoverage(status, input, findings);
        var projectionCoverage = ReadProjectionCoverage(
            status,
            input,
            findings,
            orderedMatches,
            contentRequested);

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked)
        {
            orderedMatches = [];
        }

        findings = RetainAllowedFindings(status, findings);
        EnsureStatusFinding(
            status,
            findings,
            input.TerminalEvent,
            projectionCoverage);
        var orderedFindings = OrderFindings(findings);
        var universe = BuildUniverse(input, orderedMatches.Count, matchingCoverage);
        var coverage = new FindCoverage(
            ReadOverallCoverage(status),
            matchingCoverage,
            projectionCoverage);

        return new FindResult(
            status,
            input.Request.Workspace,
            universe,
            input.Request.Query,
            input.Request.Presentation,
            coverage,
            orderedFindings,
            orderedMatches,
            ReadNextAction(status, orderedFindings));
    }

    private static IReadOnlyList<FindMatch> BuildMatches(
        IReadOnlyList<FindMatch> matches,
        IReadOnlyList<FindProjection> projections,
        FindContentSelection content)
    {
        var ordered = matches
            .GroupBy(match => new MatchKey(match.Id, match.Path))
            .Select(group => group.First())
            .OrderBy(match => match.Id, StringComparer.Ordinal)
            .ThenBy(match => match.Path, StringComparer.Ordinal)
            .ToArray();
        var attached = AttachProjections(ordered, projections, content);

        return ordered
            .Select((match, index) =>
            {
                var key = new MatchKey(match.Id, match.Path);
                var matchProjections = attached.TryGetValue(key, out var values)
                    ? OrderProjections(values, content)
                    : [];
                var evidence = match.Evidence
                    .OrderBy(value => value.Predicate)
                    .ThenBy(value => ReadRegionRank(value.Region))
                    .ThenBy(value => ReadLayerRank(value.Layer))
                    .ThenBy(value => value.Location.ByteOffset)
                    .ThenBy(value => value.Location.ByteLength)
                    .ThenBy(value => value.Occurrence)
                    .ToArray();
                return new FindMatch(
                    index + 1,
                    match.Id,
                    match.Path,
                    match.Description,
                    evidence,
                    matchProjections);
            })
            .ToArray();
    }

    private static IReadOnlyDictionary<MatchKey, IReadOnlyList<FindProjection>> AttachProjections(
        IReadOnlyList<FindMatch> matches,
        IReadOnlyList<FindProjection> projections,
        FindContentSelection content)
    {
        if (content.Effective.Count == 0)
        {
            return new Dictionary<MatchKey, IReadOnlyList<FindProjection>>();
        }

        var attached = matches.ToDictionary(
            match => new MatchKey(match.Id, match.Path),
            _ => new List<FindProjection>());
        var unresolvedMetadata = new List<(int Index, FindProjection Projection)>();
        for (var index = 0; index < projections.Count; index++)
        {
            var projection = projections[index];
            if (!IsRequested(projection, content))
            {
                continue;
            }

            var key = ReadProjectionKey(projection, matches);
            if (key is null)
            {
                if (projection.Part == FindContentPartKind.Metadata)
                {
                    unresolvedMetadata.Add((index, projection));
                }

                continue;
            }

            attached[key.Value].Add(projection);
        }

        foreach (var (index, projection) in unresolvedMetadata)
        {
            var key = ReadNearbyPhysicalKey(index, projections, matches, content)
                ?? ReadFirstMetadataFreeKey(attached);
            if (key is not null)
            {
                attached[key.Value].Add(projection);
            }
        }

        return attached.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<FindProjection>)pair.Value);
    }

    private static MatchKey? ReadProjectionKey(
        FindProjection projection,
        IReadOnlyList<FindMatch> matches)
    {
        if (projection.Part == FindContentPartKind.Metadata
            && projection.Metadata is { } metadata)
        {
            return matches.Any(match => string.Equals(match.Id, metadata.Id, StringComparison.Ordinal)
                    && string.Equals(match.Path, metadata.Path, StringComparison.Ordinal))
                ? new MatchKey(metadata.Id, metadata.Path)
                : null;
        }

        if (projection.Path is null)
        {
            return null;
        }

        var basePath = projection.Path.EndsWith(".overwrite.md", StringComparison.Ordinal)
            ? projection.Path[..^".overwrite.md".Length] + ".md"
            : projection.Path;
        var match = matches.FirstOrDefault(value => string.Equals(value.Path, basePath, StringComparison.Ordinal));
        return match is null ? null : new MatchKey(match.Id, match.Path);
    }

    private static MatchKey? ReadNearbyPhysicalKey(
        int index,
        IReadOnlyList<FindProjection> projections,
        IReadOnlyList<FindMatch> matches,
        FindContentSelection content)
    {
        for (var next = index + 1; next < projections.Count; next++)
        {
            if (!IsRequested(projections[next], content) || projections[next].Path is null)
            {
                continue;
            }

            return ReadProjectionKey(projections[next], matches);
        }

        for (var previous = index - 1; previous >= 0; previous--)
        {
            if (!IsRequested(projections[previous], content) || projections[previous].Path is null)
            {
                continue;
            }

            return ReadProjectionKey(projections[previous], matches);
        }

        return null;
    }

    private static MatchKey? ReadFirstMetadataFreeKey(
        IReadOnlyDictionary<MatchKey, List<FindProjection>> attached)
    {
        foreach (var pair in attached.OrderBy(pair => pair.Key.Id, StringComparer.Ordinal)
                     .ThenBy(pair => pair.Key.Path, StringComparer.Ordinal))
        {
            if (!pair.Value.Any(projection => projection.Part == FindContentPartKind.Metadata))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static IReadOnlyList<FindProjection> OrderProjections(
        IReadOnlyList<FindProjection> projections,
        FindContentSelection content)
        => projections
            .OrderBy(ReadProjectionRank)
            .ThenBy(projection => projection.Part == FindContentPartKind.Section
                && projection.Location is null
                ? 1
                : 0)
            .ThenBy(projection => projection.Part == FindContentPartKind.Section
                ? projection.Location?.ByteOffset ?? long.MaxValue
                : 0)
            .ThenBy(projection => ReadContentPartIndex(projection, content))
            .ThenBy(projection => projection.Path, StringComparer.Ordinal)
            .ToArray();

    private static bool IsRequested(FindProjection projection, FindContentSelection content)
        => content.Effective.Any(part => part.Kind == projection.Part
            && string.Equals(part.Name, projection.Name, StringComparison.Ordinal));

    private static int ReadContentPartIndex(
        FindProjection projection,
        FindContentSelection content)
    {
        for (var index = 0; index < content.Effective.Count; index++)
        {
            var part = content.Effective[index];
            if (part.Kind == projection.Part
                && string.Equals(part.Name, projection.Name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    private static int ReadProjectionRank(FindProjection projection)
    {
        if (projection.Part == FindContentPartKind.Metadata)
        {
            return 0;
        }

        var layerRank = projection.Layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Layer, "The Find projection layer is not defined."),
        };
        var partRank = projection.Part switch
        {
            FindContentPartKind.Frontmatter => 1,
            FindContentPartKind.Headings => 2,
            FindContentPartKind.Body => 3,
            FindContentPartKind.Section => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Find projection part is not defined."),
        };
        return checked(1 + layerRank * 10 + partRank);
    }

    private static void AddProjectionFindings(
        ICollection<FindFinding> findings,
        IReadOnlyList<FindMatch> matches,
        bool contentRequested)
    {
        if (!contentRequested)
        {
            return;
        }

        foreach (var match in matches)
        {
            foreach (var projection in match.Projections)
            {
                if (projection.State == FindProjectionState.Missing
                    && !HasProjectionFinding(findings, match, projection, FindFindingCode.ProjectionMissing))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.ProjectionMissing,
                        "The requested Find section is not present in the source layer."));
                }
                else if (projection.State == FindProjectionState.Ambiguous
                    && !HasProjectionFinding(findings, match, projection, FindFindingCode.SectionAmbiguous))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.SectionAmbiguous,
                        "The requested Find section is ambiguous in the source layer."));
                }
                else if (projection.State == FindProjectionState.Unavailable
                    && !HasIncompleteSourceFinding(findings, match))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.ProjectionUnavailable,
                        "The requested Find projection is unavailable."));
                }
            }
        }
    }

    private static bool HasProjectionFinding(
        IEnumerable<FindFinding> findings,
        FindMatch match,
        FindProjection projection,
        FindFindingCode code)
        => findings.Any(finding => finding.Code == code
            && finding.Source is { } source
            && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
            && string.Equals(source.Path, match.Path, StringComparison.Ordinal)
            && (finding.Layer is null || finding.Layer == projection.Layer)
            && string.Equals(finding.Region?.Name, projection.Name, StringComparison.Ordinal));

    private static bool HasIncompleteSourceFinding(
        IEnumerable<FindFinding> findings,
        FindMatch match)
        => findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete
            && finding.Source is { } source
            && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
            && string.Equals(source.Path, match.Path, StringComparison.Ordinal));

    private static FindFinding CreateProjectionFinding(
        FindMatch match,
        FindProjection projection,
        FindFindingCode code,
        string cause)
    {
        var source = new FindSourceIdentity(match.Id, match.Path);
        var region = projection.Part == FindContentPartKind.Section
            ? new FindRegion(
                FindRegionKind.Section,
                projection.Name,
                $"{FindDefinitions.SectionPrefix}{projection.Name}")
            : null;
        return new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            match.Id,
            cause,
            null,
            null,
            source,
            projection.Layer,
            projection.Path ?? match.Path,
            region,
            projection.Location,
            []);
    }

    private static void AddStageFindings(
        ICollection<FindFinding> findings,
        FindResultInput input,
        IReadOnlyList<FindMatch> matches,
        bool contentRequested)
    {
        if (input.StageCompletion.Matching == FindCoverageState.Incomplete
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.InspectionUnavailable,
                "One or more Find matching facts are unavailable."));
        }

        if (contentRequested
            && input.StageCompletion.Projection == FindProjectionCoverageState.Incomplete
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.ProjectionUnavailable,
                "One or more Find projections are unavailable."));
        }

        if (contentRequested
            && input.StageCompletion.Projection == FindProjectionCoverageState.NotStarted
            && matches.Count != 0
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.ProjectionUnavailable,
                "The requested Find projections were not established."));
        }

        if (input.StageCompletion.Matching == FindCoverageState.Failed
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.OperationFailed,
                "An unexpected failure prevented Find matching from completing."));
        }

        if (input.StageCompletion.Matching == FindCoverageState.Interrupted
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.Interrupted,
                "The Find invocation was interrupted before matching completed."));
        }

        if (input.StageCompletion.Projection == FindProjectionCoverageState.Failed
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.OperationFailed,
                "An unexpected failure prevented Find projection from completing."));
        }

        if (input.StageCompletion.Projection == FindProjectionCoverageState.Interrupted
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.Interrupted,
                "The Find invocation was interrupted before projection completed."));
        }

        if (input.TerminalEvent is { } terminalEvent)
        {
            var code = terminalEvent.Kind == FindTerminalEventKind.Failed
                ? FindFindingCode.OperationFailed
                : FindFindingCode.Interrupted;
            if (!findings.Any(finding => finding.Code == code))
            {
                findings.Add(CreateAggregateFinding(code, terminalEvent.Cause));
            }
        }
    }

    private static FindFinding CreateAggregateFinding(FindFindingCode code, string cause)
        => new(
            code,
            FindDefinitions.ReadFindingStatus(code),
            null,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);

    private static CliSemanticStatus ReadStatus(
        FindResultInput input,
        IReadOnlyList<FindFinding> findings)
    {
        if (input.TerminalEvent is { Kind: FindTerminalEventKind.Failed })
        {
            return CliSemanticStatus.Failed;
        }

        if (input.TerminalEvent is { Kind: FindTerminalEventKind.Interrupted })
        {
            return CliSemanticStatus.Interrupted;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Blocked
            || input.StageCompletion.Projection == FindProjectionCoverageState.Blocked
            || findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Failed
            || input.StageCompletion.Projection == FindProjectionCoverageState.Failed
            || findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Interrupted
            || input.StageCompletion.Projection == FindProjectionCoverageState.Interrupted
            || findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (input.StageCompletion.Matching == FindCoverageState.NotStarted)
        {
            return CliSemanticStatus.Invalid;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Incomplete
            || input.StageCompletion.Projection == FindProjectionCoverageState.Incomplete
            || findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return findings.Any(finding => finding.Status == CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static FindCoverageState ReadMatchingCoverage(
        CliSemanticStatus status,
        FindResultInput input,
        IReadOnlyList<FindFinding> findings)
    {
        return status switch
        {
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindCoverageState.Complete,
            CliSemanticStatus.Incomplete => ReadIncompleteMatchingCoverage(input, findings),
            CliSemanticStatus.Failed => ReadTerminalMatchingCoverage(input.StageCompletion.Matching, FindCoverageState.Failed),
            CliSemanticStatus.Interrupted => ReadTerminalMatchingCoverage(input.StageCompletion.Matching, FindCoverageState.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
    }

    private static FindCoverageState ReadIncompleteMatchingCoverage(
        FindResultInput input,
        IReadOnlyList<FindFinding> findings)
    {
        if (input.StageCompletion.Matching == FindCoverageState.Incomplete)
        {
            return FindCoverageState.Incomplete;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Complete
            && findings.All(finding => finding.Status != CliSemanticStatus.Incomplete
                || finding.Code is FindFindingCode.SectionAmbiguous
                    or FindFindingCode.ProjectionUnavailable))
        {
            return FindCoverageState.Complete;
        }

        return FindCoverageState.Incomplete;
    }

    private static FindCoverageState ReadTerminalMatchingCoverage(
        FindCoverageState stage,
        FindCoverageState terminal)
        => stage is FindCoverageState.Complete or FindCoverageState.Incomplete
            ? stage
            : terminal;

    private static FindProjectionCoverageState ReadProjectionCoverage(
        CliSemanticStatus status,
        FindResultInput input,
        IReadOnlyList<FindFinding> findings,
        IReadOnlyList<FindMatch> matches,
        bool contentRequested)
    {
        if (!contentRequested)
        {
            return FindProjectionCoverageState.NotRequested;
        }

        var stage = input.StageCompletion.Projection;
        return status switch
        {
            CliSemanticStatus.Invalid => FindProjectionCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindProjectionCoverageState.Blocked,
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindProjectionCoverageState.Complete,
            CliSemanticStatus.Incomplete => ReadIncompleteProjectionCoverage(stage, findings, matches),
            CliSemanticStatus.Failed => ReadTerminalProjectionCoverage(stage, FindProjectionCoverageState.Failed),
            CliSemanticStatus.Interrupted => ReadTerminalProjectionCoverage(stage, FindProjectionCoverageState.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
    }

    private static FindProjectionCoverageState ReadIncompleteProjectionCoverage(
        FindProjectionCoverageState stage,
        IReadOnlyList<FindFinding> findings,
        IReadOnlyList<FindMatch> matches)
    {
        if (stage == FindProjectionCoverageState.Incomplete
            || findings.Any(finding => finding.Code is FindFindingCode.ProjectionUnavailable or FindFindingCode.SectionAmbiguous)
            || matches.SelectMany(match => match.Projections)
                .Any(projection => projection.State is FindProjectionState.Unavailable or FindProjectionState.Ambiguous))
        {
            return FindProjectionCoverageState.Incomplete;
        }

        if (stage == FindProjectionCoverageState.NotStarted && matches.Count != 0)
        {
            return FindProjectionCoverageState.Incomplete;
        }

        return FindProjectionCoverageState.Complete;
    }

    private static FindProjectionCoverageState ReadTerminalProjectionCoverage(
        FindProjectionCoverageState stage,
        FindProjectionCoverageState terminal)
        => stage is FindProjectionCoverageState.Complete or FindProjectionCoverageState.Incomplete
            ? stage
            : terminal;

    private static FindCoverageState ReadOverallCoverage(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => FindCoverageState.Complete,
            CliSemanticStatus.Incomplete => FindCoverageState.Incomplete,
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Failed => FindCoverageState.Failed,
            CliSemanticStatus.Interrupted => FindCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };

    private static FindUniverse BuildUniverse(
        FindResultInput input,
        int matchCount,
        FindCoverageState matchingCoverage)
    {
        if (input.Universe is null)
        {
            var include = input.Request.UniverseFilter.Include
                .Select(CreateUnresolvedSelector)
                .ToArray();
            var exclude = input.Request.UniverseFilter.Exclude
                .Select(CreateUnresolvedSelector)
                .ToArray();
            var mode = include.Length == 0 && exclude.Length == 0
                ? FindUniverseMode.Default
                : FindUniverseMode.Filtered;
            return new FindUniverse(
                mode,
                include,
                exclude,
                null,
                null,
                matchCount == 0 ? null : matchCount);
        }

        var inspectedCount = input.Universe.InspectedCount
            ?? ReadInspectedCount(input, matchingCoverage);
        return new FindUniverse(
            input.Universe.Mode,
            input.Universe.Include,
            input.Universe.Exclude,
            input.Universe.CandidateCount,
            inspectedCount,
            matchCount);
    }

    private static FindSelector CreateUnresolvedSelector(string value)
        => new(
            value,
            null,
            FindSelectorResolution.Invalid,
            null,
            null,
            null,
            []);

    private static int? ReadInspectedCount(
        FindResultInput input,
        FindCoverageState matchingCoverage)
    {
        if (input.Universe?.CandidateCount is null
            || matchingCoverage is FindCoverageState.NotStarted or FindCoverageState.Blocked)
        {
            return null;
        }

        var inspected = input.Inspections
            .GroupBy(inspection => new MatchKey(
                inspection.Source.Identity.AutomaticId,
                inspection.Source.Identity.CanonicalBasePath))
            .Count(group => IsInspectedSource(input.Request, group));
        return Math.Min(inspected, input.Universe.CandidateCount.Value);
    }

    private static bool IsInspectedSource(
        FindRequestEcho request,
        IEnumerable<FindLayerInspectionFacts> inspections)
    {
        var facts = inspections.ToArray();
        if (facts.Length == 0)
        {
            return false;
        }

        var source = facts[0].Source;
        var layers = new[] { source.Base, source.Overwrite }
            .Where(layer => layer is not null)
            .Cast<SourceLayer>()
            .ToArray();
        return layers.All(layer => facts.Any(inspection =>
            string.Equals(inspection.Layer.CanonicalPath, layer.CanonicalPath, StringComparison.Ordinal)
            && IsMatchingComplete(request, inspection)));
    }

    private static bool IsMatchingComplete(
        FindRequestEcho request,
        FindLayerInspectionFacts inspection)
    {
        if (inspection.Document?.BodySpan is null)
        {
            return false;
        }

        if (request.Query.EffectivePredicates.Count == 0)
        {
            return true;
        }

        foreach (var predicate in request.Query.EffectivePredicates)
        {
            if (predicate.Kind == FindPredicateKind.Heading)
            {
                continue;
            }

            foreach (var region in request.Query.Within.Tag)
            {
                if (region.Kind is FindRegionKind.Frontmatter or FindRegionKind.Document
                    && inspection.Frontmatter?.Availability != FindFrontmatterAvailability.Complete)
                {
                    return false;
                }

                if (region.Kind is FindRegionKind.Body or FindRegionKind.Section or FindRegionKind.Document
                    && inspection.BodyTags?.Availability != FindBodyTagAvailability.Complete)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static List<FindFinding> RetainAllowedFindings(
        CliSemanticStatus status,
        IEnumerable<FindFinding> findings)
    {
        CliSemanticStatus[] allowed = status switch
        {
            CliSemanticStatus.Complete => [],
            CliSemanticStatus.Attention => [CliSemanticStatus.Attention],
            CliSemanticStatus.Incomplete => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete],
            CliSemanticStatus.Invalid => [CliSemanticStatus.Invalid],
            CliSemanticStatus.Blocked => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Blocked],
            CliSemanticStatus.Failed => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Failed],
            CliSemanticStatus.Interrupted => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Interrupted],
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
        return findings
            .Where(finding => allowed.Contains(finding.Status))
            .ToList();
    }

    private static void EnsureStatusFinding(
        CliSemanticStatus status,
        ICollection<FindFinding> findings,
        FindTerminalEvent? terminalEvent,
        FindProjectionCoverageState projectionCoverage)
    {
        if (status == CliSemanticStatus.Complete
            || findings.Any(finding => finding.Status == status))
        {
            return;
        }

        var (code, cause) = status switch
        {
            CliSemanticStatus.Attention => (
                FindFindingCode.IdentityCollision,
                "The Find result contains an established attention condition."),
            CliSemanticStatus.Incomplete when projectionCoverage == FindProjectionCoverageState.Incomplete => (
                FindFindingCode.ProjectionUnavailable,
                "One or more Find projections are unavailable."),
            CliSemanticStatus.Incomplete => (
                FindFindingCode.InspectionUnavailable,
                "One or more Find source facts are unavailable."),
            CliSemanticStatus.Invalid => (
                FindFindingCode.InvalidInput,
                "The Find input is invalid."),
            CliSemanticStatus.Blocked => (
                FindFindingCode.WorkspaceUnavailable,
                "The Find workspace or source boundary is unavailable."),
            CliSemanticStatus.Failed => (
                FindFindingCode.OperationFailed,
                terminalEvent?.Cause ?? "An unexpected failure prevented Find from completing."),
            CliSemanticStatus.Interrupted => (
                FindFindingCode.Interrupted,
                terminalEvent?.Cause ?? "The Find invocation was interrupted."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
        findings.Add(CreateAggregateFinding(code, cause));
    }

    private static IReadOnlyList<FindFinding> OrderFindings(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => ReadFindingRank(finding.Code))
            .ThenBy(finding => ReadSelectorRoleRank(finding.SelectorRole))
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadOptionalLayerRank(finding.Layer))
            .ThenBy(finding => ReadOptionalRegionRank(finding.Region))
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ThenBy(finding => finding.Location?.ByteLength ?? long.MaxValue)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

    private static int ReadFindingRank(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => 0,
            FindFindingCode.InvalidSelector => 1,
            FindFindingCode.WorkspaceUnavailable => 2,
            FindFindingCode.WorkspaceUnsafe => 3,
            FindFindingCode.SelectorAmbiguous => 4,
            FindFindingCode.SelectorUnsafe => 5,
            FindFindingCode.IdentityCollision => 6,
            FindFindingCode.CandidateUnsafe => 7,
            FindFindingCode.LayerUnresolved => 8,
            FindFindingCode.InspectionUnavailable => 9,
            FindFindingCode.InvalidEncoding => 10,
            FindFindingCode.FrontmatterUnavailable => 11,
            FindFindingCode.SectionAmbiguous => 12,
            FindFindingCode.ProjectionMissing => 13,
            FindFindingCode.ProjectionUnavailable => 14,
            FindFindingCode.OperationFailed => 15,
            FindFindingCode.Interrupted => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };

    private static int ReadSelectorRoleRank(FindSelectorRole? role)
        => role switch
        {
            FindSelectorRole.Include => 0,
            FindSelectorRole.Exclude => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Find selector role is not defined."),
        };

    private static int ReadOptionalLayerRank(SourceLayerKind? layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static int ReadOptionalRegionRank(FindRegion? region)
        => region?.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body or FindRegionKind.Section => 1,
            FindRegionKind.Document => 2,
            null => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(region), region?.Kind, "The Find region kind is not defined."),
        };

    private static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body or FindRegionKind.Section => 1,
            FindRegionKind.Document => throw new ArgumentException("Find evidence cannot use the document region.", nameof(region)),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region.Kind, "The Find region kind is not defined."),
        };

    private static int ReadLayerRank(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<FindFinding> findings)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => FindDefinitions.IncompleteNextAction,
            CliSemanticStatus.Invalid => FindDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked when findings
                .Where(finding => finding.Status == CliSemanticStatus.Blocked)
                .All(finding => finding.Code == FindFindingCode.SelectorAmbiguous)
                => FindDefinitions.SelectorAmbiguousNextAction,
            CliSemanticStatus.Blocked => FindDefinitions.BlockedNextAction,
            CliSemanticStatus.Failed => FindDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => FindDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };

    private readonly record struct MatchKey(string Id, string Path);
}

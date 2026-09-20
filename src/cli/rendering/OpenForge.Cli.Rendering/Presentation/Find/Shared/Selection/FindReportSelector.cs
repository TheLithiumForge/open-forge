using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Presentation.Find.Models;
using OpenForge.Cli.Core.Presentation.Find.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Content.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Find.Shared.Selection;

internal static class FindReportSelector
{
    internal static CliReport<FindData> Select(FindResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var matches = result.Matches
            .Select(match => new FindDataMatch
            {
                Id = match.Id,
                Path = match.Path,
                Description = match.Description,
                Evidence = standard ? match.Evidence.Select(Evidence).ToArray() : null,
                Parts = result.Presentation.Content.IsRequested
                    ? match.Projections.Select(projection => Part(projection, full)).ToArray()
                    : null,
            })
            .ToArray();

        var data = new FindData
        {
            Matches = matches,
            Query = standard ? Query(result.Query) : null,
            SourceSet = full ? SourceSet(result.Universe) : null,
            ContentBlocks = result.Matches.SelectMany(match => ContentBlocks(match, selection.Detail)).ToArray(),
            PrependBlankLine = result.Findings.Count > 0 && matches.Length > 0,
        };

        return new CliReport<FindData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = ShowsConcreteHeadlineFinding(result)
                ? FindWording.FindingCode(result.Findings[0].Code)
                : null,
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(Finding).ToArray(),
            Counts = Counts(result),
            Data = data,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug ? Diagnostics(result) : [],
        };
    }

    private static CliHeadline Headline(FindResult result)
    {
        var query = FindWording.Query(result.Query);
        return result.Status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention when result.Matches.Count == 0
                => new(
                    FindWording.NoMatches(query, query.Length == 0 && result.Universe.CandidateCount == 0),
                    CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete
                => new(FindWording.Completed(result.Matches.Count, query), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(FindWording.Completed(result.Matches.Count, query), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(FindWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(FindWording.CannotSearch(FirstMessage(result)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(FindWording.CannotSearch(FirstMessage(result)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(FindWording.Failed(FirstCause(result)), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(FindWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Find result status is not defined."),
        };
    }

    private static string FirstMessage(FindResult result)
    {
        var finding = result.Findings.FirstOrDefault();
        return finding is null
            ? global::OpenForge.Cli.OutputText.Find.FindText.LabelTheRequestedSearchCouldNotBeStarted()
            : FindWording.FindingMessage(finding).TrimEnd('.');
    }

    private static string FirstCause(FindResult result)
        => result.Findings.FirstOrDefault()?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheOperationFailed();

    private static bool ShowsConcreteHeadlineFinding(FindResult result)
        => result.Findings.Count == 1
            && result.Status is CliSemanticStatus.Invalid
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted;

    private static CliNextAction? Next(FindResult result)
    {
        if (result.Findings.Any(finding => finding.Code == FindFindingCode.InvalidSelector))
        {
            return new CliNextAction(
                "open-forge route list --depth=all",
                FindWording.NextInvalidSelector());
        }

        return result.Status == CliSemanticStatus.Incomplete ? result.Next : null;
    }

    private static IReadOnlyList<CliCount> Counts(FindResult result)
        =>
        [
            new CliCount("matches", global::OpenForge.Cli.OutputText.Find.FindText.LabelMatches(), result.Universe.MatchedCount),
            new CliCount("sourcesInspected", global::OpenForge.Cli.OutputText.Find.FindText.LabelSourcesInspected(), result.Universe.InspectedCount),
            new CliCount("sourcesCandidates", global::OpenForge.Cli.OutputText.Find.FindText.LabelSourceCandidates(), result.Universe.CandidateCount),
        ];

    private static CliFinding Finding(FindFinding finding)
    {
        var path = finding.Path ?? finding.Source?.Path;
        var id = finding.Source?.Id ?? finding.Subject ?? (path is null ? "find" : null);
        var kind = finding.Source is not null || path is not null
            ? CliSubjectKind.Source
            : CliSubjectKind.Identifier;
        var subject = new CliSubject(
            kind,
            path,
            id,
            finding.LocationView is { } location
                ? new CliSourceLocation(location.Line, location.Column)
                : null);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = FindWording.FindingCode(finding.Code),
            Title = FindWording.FindingTitle(finding.Code),
            Message = FindWording.FindingMessage(finding),
            Subject = subject,
            Candidates = finding.Candidates
                .Select(candidate => new CliCandidate(
                    new CliSubject(CliSubjectKind.Source, candidate.Path, candidate.Id),
                    []))
                .ToArray(),
        };
    }

    private static FindDataEvidence Evidence(FindEvidence evidence)
        => new()
        {
            Kind = FindWording.EvidenceKind(evidence.Kind),
            Value = evidence.Authored,
            Region = evidence.Region.CanonicalValue,
            Layer = FindWording.Layer(evidence.LayerValue),
        };

    private static FindDataPart Part(FindProjection projection, bool full)
        => new()
        {
            Part = FindWording.ProjectionPart(projection),
            Name = projection.Name,
            Layer = projection.LayerValue is { } layer ? FindWording.Layer(layer) : null,
            Path = projection.Path,
            State = projection.State == FindProjectionState.Available
                ? null
                : FindWording.ProjectionState(projection.State),
            Text = projection.Part == FindContentPartKind.Metadata
                ? MetadataText(projection.Metadata)
                : projection.Text,
            Headings = projection.Part == FindContentPartKind.Headings
                ? projection.Headings.Select(heading => new FindDataHeading
                {
                    Text = heading.Text,
                    Level = heading.Level,
                    Line = full ? heading.LocationView.Line : null,
                }).ToArray()
                : null,
        };

    private static IEnumerable<CliContentBlock> ContentBlocks(FindMatch match, CliDetail detail)
    {
        foreach (var layer in new[] { "base", "overwrite" })
        {
            var projections = match.Projections
                .Where(projection => projection.Part == FindContentPartKind.Metadata
                    ? layer == "base"
                    : projection.LayerValue is { } projectionLayer
                        && FindWording.Layer(projectionLayer) == layer)
                .ToArray();
            if (projections.Length == 0)
            {
                continue;
            }

            var path = projections.Select(projection => projection.Path)
                .FirstOrDefault(value => value is not null)
                ?? match.Path;
            yield return new CliContentBlock
            {
                Path = path,
                Id = match.Id,
                DelimiterLayer = layer,
                Parts = projections.Select(projection => ContentPart(projection, detail)).ToArray(),
            };
        }
    }

    private static CliContentPart ContentPart(FindProjection projection, CliDetail detail)
    {
        var name = FindWording.ProjectionPart(projection);
        var state = projection.State == FindProjectionState.Available
            ? null
            : FindWording.ProjectionState(projection.State);
        return projection.Part switch
        {
            FindContentPartKind.Metadata => new CliContentPart
            {
                Name = name,
                Kind = CliContentPartKind.Metadata,
                Metadata = MetadataRows(projection.Metadata),
                State = state,
            },
            FindContentPartKind.Headings => new CliContentPart
            {
                Name = name,
                Kind = CliContentPartKind.Headings,
                State = state,
                Headings = projection.Headings.Select(heading => new CliContentHeading
                {
                    Text = heading.Text,
                    Level = heading.Level,
                    Line = detail >= CliDetail.Full ? heading.LocationView.Line : null,
                }).ToArray(),
            },
            FindContentPartKind.Frontmatter
                or FindContentPartKind.Body
                or FindContentPartKind.Section => new CliContentPart
                {
                    Name = name,
                    Kind = CliContentPartKind.Text,
                    State = state,
                    AuthoredText = projection.Text is { } text ? new CliAuthoredSpan(text) : null,
                },
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Find projection part is not defined."),
        };
    }

    private static IReadOnlyList<CliContentMetadata> MetadataRows(FindMetadata? metadata)
    {
        if (metadata is null)
        {
            return [];
        }

        var rows = new List<CliContentMetadata>
        {
            new() { Name = "id", Value = metadata.Id },
        };
        if (metadata.Route is { } route)
        {
            rows.Add(new CliContentMetadata { Name = "route", Value = route });
        }

        return rows;
    }

    private static string? MetadataText(FindMetadata? metadata)
        => metadata is null
            ? null
            : string.Join(
                    "\n",
                    MetadataRows(metadata).Select(row => $"{row.Name}: {row.Value}"))
                + "\n";

    private static FindDataQuery Query(FindQuery query)
        => new()
        {
            Tags = query.Predicates
                .Where(predicate => predicate.Kind == FindPredicateKind.Tag)
                .Select(predicate => predicate.SuppliedValue)
                .ToArray(),
            Headings = query.Predicates
                .Where(predicate => predicate.Kind == FindPredicateKind.Heading)
                .Select(predicate => predicate.SuppliedValue)
                .ToArray(),
            Require = FindWording.Requirement(query.Requirement),
            Within = query.Within.Tag
                .Concat(query.Within.Heading)
                .Select(region => region.CanonicalValue)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
        };

    private static FindDataSourceSet SourceSet(FindUniverse universe)
        => new()
        {
            Mode = universe.Mode switch
            {
                FindUniverseMode.Default => "default",
                FindUniverseMode.Filtered => "filtered",
                _ => throw new ArgumentOutOfRangeException(nameof(universe), universe.Mode, "The Find universe mode is not defined."),
            },
            Include = universe.Include.Select(selector => selector.Value).ToArray(),
            Exclude = universe.Exclude.Select(selector => selector.Value).ToArray(),
            Inspected = universe.InspectedCount,
            Candidates = universe.CandidateCount,
        };

    private static IReadOnlyList<string> Diagnostics(FindResult result)
        =>
        [
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"coverage={FindWording.Coverage(result.Coverage.State)}",
            $"matching={FindWording.Coverage(result.Coverage.Matching)}",
            $"projection={FindWording.ProjectionCoverage(result.Coverage.Projection)}",
            $"matches={result.Matches.Count}",
            $"sourcesInspected={result.Universe.InspectedCount?.ToString() ?? "null"}",
            $"sourcesCandidates={result.Universe.CandidateCount?.ToString() ?? "null"}",
            $"findings={result.Findings.Count}",
            .. result.Findings.Select(finding => $"finding={FindWording.FindingCode(finding.Code)}"),
        ];
}

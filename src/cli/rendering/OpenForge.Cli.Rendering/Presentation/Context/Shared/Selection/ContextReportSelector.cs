using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Context.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Content;
using OpenForge.Cli.Core.Presentation.Shared.Content.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Context.Shared.Selection;

internal static class ContextReportSelector
{
    internal static CliReport<ContextData> Select(ContextResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var standard = selection.Detail >= CliDetail.Standard;
        var full = selection.Detail >= CliDetail.Full;
        var selectedLayers = result.Sources
            .SelectMany(source => source.Layers.Select(layer => SelectLayer(
                result,
                source,
                layer,
                selection.Detail,
                standard,
                full)))
            .ToArray();
        var data = new ContextData
        {
            Sources = selectedLayers.Select(selected => selected.Source).ToArray(),
            ContentBlocks = selectedLayers.Select(selected => selected.Block).ToArray(),
            Summary = standard && result.Counts.Sources is { } sources && result.Counts.Tokens is { } tokens
                ? ContextWording.Summary(sources, tokens)
                : null,
            Links = full ? result.Links.Select(Link).ToArray() : null,
            PathsOnly = result.Presentation.Content.Effective is [{ Kind: ContextContentPartKind.Paths }],
            EmptyAdditions = result.Selection.AdditionsOnly && selectedLayers.Length == 0,
            SourceCount = result.Counts.Sources,
            TokenCount = result.Counts.Tokens,
            LinksFollowed = result.Counts.LinksFollowed,
            LinksNotFollowed = result.Counts.LinksNotFollowed,
        };
        var findings = result.Findings.Select(Finding).ToArray();
        var counts = Counts(result);
        return new CliReport<ContextData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = ShowsConcreteHeadlineFinding(result)
                ? ContextWording.FindingCode(result.Findings[0].Code)
                : null,
            Workspace = result.WorkspacePath is { } path ? new CliWorkspaceEcho(path, result.WorkspaceExplicit) : null,
            Findings = findings,
            Counts = counts,
            Limitations = result.Findings
                .Where(finding => finding.Code == ContextFindingCode.ProjectionUnavailable)
                .Select(finding => new CliLimitation(
                    finding.Part?.CanonicalValue ?? "content",
                    finding.Cause,
                    Subject(finding)))
                .ToArray(),
            Data = data,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? result.Findings.Select(finding => finding.Cause).ToArray()
                : [],
        };
    }

    private static SelectedLayer SelectLayer(
        ContextResult result,
        ContextSource source,
        ContextLayer layer,
        CliDetail detail,
        bool standard,
        bool full)
    {
        var parts = Parts(result, source, layer, detail);
        return new SelectedLayer(
            Source(source, layer, parts, standard, full),
            Block(source, layer, parts, standard, full));
    }

    private static CliHeadline Headline(ContextResult result)
    {
        if (result.Selection.AdditionsOnly && result.Sources.Count == 0
            && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            return new(ContextWording.NoAdditional(), CliHeadlineKind.NothingToDo);
        }

        return result.Status switch
        {
            CliSemanticStatus.Complete => new(ContextWording.Completed(), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(ContextWording.CompletedWithWarnings(), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete => new(ContextWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid or CliSemanticStatus.Blocked => new(
                ContextWording.CannotRead(FirstMessage(result)),
                result.Status == CliSemanticStatus.Invalid ? CliHeadlineKind.CannotStart : CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(ContextWording.Failed(FirstCause(result)), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(ContextWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Context status is not defined."),
        };
    }

    private static string FirstMessage(ContextResult result)
    {
        var finding = result.Findings.FirstOrDefault();
        if (finding is null)
        {
            return global::OpenForge.Cli.OutputText.Context.ContextText.LabelTheRequestedSourcesCouldNotBeResolved();
        }

        return ContextWording.FindingProblem(finding);
    }

    private static string FirstCause(ContextResult result)
        => result.Findings.FirstOrDefault()?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheOperationFailed();

    private static bool ShowsConcreteHeadlineFinding(ContextResult result)
    {
        if (result.Findings.Count != 1)
        {
            return false;
        }

        return result.Status switch
        {
            CliSemanticStatus.Invalid
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted => true,
            _ => false,
        };
    }

    private static CliNextAction? Next(ContextResult result)
    {
        if (result.Findings.Any(finding => finding.Code == ContextFindingCode.InvalidSource))
        {
            return new CliNextAction(
                "open-forge route list --depth=all",
                ContextWording.ChooseSourceNext());
        }

        if (result.Findings.Any(finding => finding.Code is
            ContextFindingCode.ClosureUnavailable
                or ContextFindingCode.LayerUnavailable
                or ContextFindingCode.InvalidEncoding
                or ContextFindingCode.MarkdownUnavailable
                or ContextFindingCode.TargetMissing
                or ContextFindingCode.FragmentMissing
                or ContextFindingCode.LinkEncodingInvalid
                or ContextFindingCode.TargetUnreadable
                or ContextFindingCode.ProjectionUnavailable))
        {
            return new CliNextAction(
                "open-forge doctor",
                ContextWording.InspectUnavailableNext());
        }

        if (result.Findings.Any(finding => finding.Code == ContextFindingCode.TargetCaseMismatch))
        {
            return new CliNextAction(
                "open-forge repair --automatic",
                ContextWording.RepairCaseMismatchNext());
        }

        return null;
    }

    private static IReadOnlyList<CliCount> Counts(ContextResult result)
    {
        var reason = result.Counts.UnavailableReason;
        return
        [
            new CliCount("sources", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSources(), result.Counts.Sources, reason),
            new CliCount("tokens", global::OpenForge.Cli.OutputText.Context.ContextText.LabelTokens(), result.Counts.Tokens, reason),
            new CliCount("bytes", global::OpenForge.Cli.OutputText.Context.ContextText.LabelBytes(), result.Counts.Bytes, reason),
            new CliCount("linksFollowed", global::OpenForge.Cli.OutputText.Context.ContextText.LabelLinksFollowed(), result.Counts.LinksFollowed, reason),
            new CliCount("linksNotFollowed", global::OpenForge.Cli.OutputText.Context.ContextText.LabelLinksNotFollowed(), result.Counts.LinksNotFollowed, reason),
        ];
    }

    private static ContextDataSource Source(
        ContextSource source,
        ContextLayer layer,
        IReadOnlyList<ContextDataPart> parts,
        bool standard,
        bool full)
        => new()
        {
            Path = layer.Path,
            Id = source.Id,
            Layer = ContextWording.Layer(layer.Kind),
            Parts = parts,
            Standard = standard
                ? new ContextDataSourceStandard
                {
                    IncludedBecause = Reasons(source, layer),
                    Route = source.Route,
                    Scope = source.Scope,
                }
                : null,
            Full = full
                ? new ContextDataSourceFull { Order = layer.PathPosition }
                : null,
        };

    private static CliContentBlock Block(
        ContextSource source,
        ContextLayer layer,
        IReadOnlyList<ContextDataPart> parts,
        bool standard,
        bool full)
        => new()
        {
            Path = layer.Path,
            Id = source.Id,
            DelimiterLayer = ContextWording.Layer(layer.Kind),
            Parts = parts
                .Select(ContentPart)
                .ToArray(),
            IncludedBecause = standard
                ? Reasons(source, layer)
                : [],
            Route = full ? source.Route : null,
            Scope = full ? source.Scope : null,
            Order = full ? layer.PathPosition : null,
            Layer = full ? ContextWording.Layer(layer.Kind) : null,
        };

    private static IReadOnlyList<ContextDataPart> Parts(
        ContextResult result,
        ContextSource source,
        ContextLayer layer,
        CliDetail detail)
    {
        var values = new List<ContextDataPart>();
        foreach (var requested in result.Presentation.Content.Effective)
        {
            switch (requested.Kind)
            {
                case ContextContentPartKind.Metadata:
                    values.Add(MetadataPart(source));
                    break;
                case ContextContentPartKind.Paths:
                    values.Add(new ContextDataPart
                    {
                        Kind = ContextDataPartKind.Paths,
                        Part = requested.CanonicalValue,
                        Paths = [layer.Path],
                    });
                    break;
                default:
                    if (layer.Projections.FirstOrDefault(projection => Matches(projection, requested)) is { } projection)
                    {
                        values.Add(ProjectionPart(projection, detail));
                    }

                    break;
            }
        }

        return values;
    }

    private static ContextDataPart MetadataPart(ContextSource source)
    {
        var rows = new List<ContextDataMetadata>();
        if (source.Id is { } id)
        {
            rows.Add(new ContextDataMetadata { Name = global::OpenForge.Cli.OutputText.Context.ContextText.LabelId(), Value = id });
        }

        if (source.Route is not null)
        {
            rows.Add(new ContextDataMetadata { Name = global::OpenForge.Cli.OutputText.Shared.SharedText.LabelRoute(), Value = source.Route });
        }

        if (source.Metadata.Description is { } description)
        {
            rows.Add(new ContextDataMetadata { Name = global::OpenForge.Cli.OutputText.Context.ContextText.LabelDescription(), Value = description });
        }

        if (source.Metadata.Tags.Count > 0)
        {
            rows.Add(new ContextDataMetadata { Name = global::OpenForge.Cli.OutputText.Context.ContextText.LabelTags(), Value = string.Join(", ", source.Metadata.Tags) });
        }

        return new ContextDataPart
        {
            Kind = ContextDataPartKind.Metadata,
            Part = ContextWording.MetadataPartName,
            Text = rows.Count == 0
                ? null
                : string.Join("\n", rows.Select(row => ContentPartsWording.Metadata(row.Name, row.Value))) + "\n",
            Metadata = rows,
        };
    }

    private static ContextDataPart ProjectionPart(ContextProjection projection, CliDetail detail)
        => projection.Part == ContextProjectionPart.Headings
            ? new ContextDataPart
            {
                Kind = ContextDataPartKind.Headings,
                Part = projection.Part.ToString().ToLowerInvariant(),
                Headings = projection.Headings.Select(heading => new ContextDataHeading
                {
                    Text = heading.Text,
                    Level = heading.Level,
                    Line = detail >= CliDetail.Full ? heading.LocationView.Line : null,
                }).ToArray(),
                State = State(projection.State),
            }
            : new ContextDataPart
            {
                Kind = ContextDataPartKind.Text,
                Part = projection.Part == ContextProjectionPart.Section
                    ? $"section:{projection.Name}"
                    : projection.Part.ToString().ToLowerInvariant(),
                Text = projection.Text,
                State = projection.Text is null ? State(projection.State) : null,
            };

    private static CliContentPart ContentPart(ContextDataPart part)
        => part.Kind switch
        {
            ContextDataPartKind.Paths => new CliContentPart
            {
                Name = part.Part,
                Kind = CliContentPartKind.Paths,
                Paths = part.Paths ?? [],
            },
            ContextDataPartKind.Metadata => new CliContentPart
            {
                Name = part.Part,
                Kind = CliContentPartKind.Metadata,
                Metadata = part.Metadata.Select(row => new CliContentMetadata
                {
                    Name = row.Name,
                    Value = row.Value,
                }).ToArray(),
            },
            ContextDataPartKind.Headings => new CliContentPart
            {
                Name = part.Part,
                Kind = CliContentPartKind.Headings,
                Headings = part.Headings is { } headings
                    ? headings.Select(heading => new CliContentHeading
                    {
                        Text = heading.Text,
                        Level = heading.Level,
                        Line = heading.Line,
                    }).ToArray()
                    : [],
            },
            ContextDataPartKind.Text => new CliContentPart
            {
                Name = part.Part,
                Kind = CliContentPartKind.Text,
                State = part.State,
                AuthoredText = part.Text is null ? null : new CliAuthoredSpan(part.Text),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(part), part.Kind, "The Context data part kind is not defined."),
        };

    private static bool Matches(ContextProjection projection, ContextContentPart requested)
        => projection.Part switch
        {
            ContextProjectionPart.Frontmatter => requested.Kind == ContextContentPartKind.Frontmatter,
            ContextProjectionPart.Headings => requested.Kind == ContextContentPartKind.Headings,
            ContextProjectionPart.Body => requested.Kind == ContextContentPartKind.Body,
            ContextProjectionPart.Section => requested.Kind == ContextContentPartKind.Section
                && string.Equals(projection.Name, requested.Name, StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Context projection part is not defined."),
        };

    private static string State(ContextProjectionState state)
        => state switch
        {
            ContextProjectionState.Available => "available",
            ContextProjectionState.Missing => "missing",
            ContextProjectionState.Unavailable => "unavailable",
            ContextProjectionState.Ambiguous => "ambiguous",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Context projection state is not defined."),
        };

    private static ContextDataLink Link(ContextLink link)
        => new()
        {
            From = link.Source.Path,
            Location = new ContextDataLocation { Line = link.LocationView.Line, Column = link.LocationView.Column },
            Destination = link.RawDestination,
            ResolvedPath = link.Target.Path,
            Resolution = ContextWording.Resolution(link.Target.Resolution),
            Followed = link.FollowState == ContextLinkFollowState.Followed,
        };

    private static CliFinding Finding(ContextFinding finding)
    {
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = ContextWording.FindingCode(finding.Code),
            Title = ContextWording.FindingTitle(finding.Code),
            Message = ContextWording.FindingMessage(finding),
            Subject = Subject(finding),
            Candidates = finding.Candidates.Select(candidate => new CliCandidate(
                new CliSubject(CliSubjectKind.Source, candidate.Path, candidate.Id),
                [])).ToArray(),
        };
    }

    private static CliSubject Subject(ContextFinding finding)
    {
        var path = finding.Source?.Path ?? finding.Path ?? finding.Subject;
        var id = finding.Source?.Id ?? finding.Subject;
        return new CliSubject(
            finding.Source is null ? CliSubjectKind.Identifier : CliSubjectKind.Source,
            path,
            id,
            finding.LocationView is { } location ? new CliSourceLocation(location.Line, location.Column) : null);
    }

    private static IReadOnlyList<string> Reasons(ContextSource source, ContextLayer layer)
        => source.InclusionReasons
            .Concat(layer.InclusionReasons)
            .Distinct()
            .Select(ContextWording.Reason)
            .ToArray();

    private sealed record SelectedLayer(ContextDataSource Source, CliContentBlock Block);
}

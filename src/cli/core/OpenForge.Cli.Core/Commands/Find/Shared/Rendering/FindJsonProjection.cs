using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using FindJsonProjectionModel = OpenForge.Cli.Core.Commands.Find.Models.Presentation.FindJsonProjection;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static partial class FindJsonProjection
{
    internal static FindJsonDocument Create(FindResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new FindJsonDocument
        {
            SchemaVersion = FindDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new FindJsonResult
            {
                Universe = Universe(result.Universe, result.Coverage.Matching),
                Query = Query(result.Query),
                Presentation = Presentation(result.Presentation),
                Coverage = Coverage(result.Coverage),
                Findings = result.Findings.Select(Finding).ToArray(),
                Matches = result.Matches.Select(Match).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new FindJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static FindJsonWorkspace Workspace(CliWorkspace workspace)
    {
        return new FindJsonWorkspace
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };
    }

    private static FindJsonUniverse Universe(FindUniverse universe, FindCoverageState matchingCoverage)
    {
        var countsEstablished = matchingCoverage is not (FindCoverageState.NotStarted or FindCoverageState.Blocked);
        return new FindJsonUniverse
        {
            Mode = universe.Mode switch
            {
                FindUniverseMode.Default => "default",
                FindUniverseMode.Filtered => "filtered",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(universe),
                    universe.Mode,
                    "The Find universe mode is not defined."),
            },
            Include = universe.Include.Select(Selector).ToArray(),
            Exclude = universe.Exclude.Select(Selector).ToArray(),
            CandidateCount = countsEstablished ? universe.CandidateCount : null,
            InspectedCount = countsEstablished ? universe.InspectedCount : null,
            MatchedCount = countsEstablished ? universe.MatchedCount : null,
        };
    }

    private static FindJsonSelector Selector(FindSelector selector)
    {
        return new FindJsonSelector
        {
            Value = selector.Value,
            Form = selector.Form is null ? null : SelectorForm(selector.Form.Value),
            Resolution = SelectorResolution(selector.Resolution),
            Identity = selector.Identity is null ? null : Identity(selector.Identity),
            SourceKind = selector.SourceKind is null ? null : SourceKind(selector.SourceKind.Value),
            Expansion = selector.Expansion is null ? null : Expansion(selector.Expansion.Value),
            Candidates = selector.Candidates.Select(Identity).ToArray(),
        };
    }

    private static FindJsonQuery Query(FindQuery query)
    {
        return new FindJsonQuery
        {
            Predicates = query.Predicates.Select(Predicate).ToArray(),
            EffectivePredicates = query.EffectivePredicates.Select(Predicate).ToArray(),
            Require = query.Requirement switch
            {
                FindRequirement.All => FindDefinitions.All,
                FindRequirement.Any => FindDefinitions.Any,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(query),
                    query.Requirement,
                    "The Find requirement is not defined."),
            },
            Within = new FindJsonWithin
            {
                Supplied = query.Within.Supplied.Select(Region).ToArray(),
                Tag = query.Within.Tag.Select(Region).ToArray(),
                Heading = query.Within.Heading.Select(Region).ToArray(),
            },
        };
    }

    private static FindJsonPredicate Predicate(FindPredicate predicate)
    {
        return new FindJsonPredicate
        {
            Kind = predicate.Kind switch
            {
                FindPredicateKind.Tag => "tag",
                FindPredicateKind.Heading => "heading",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(predicate),
                    predicate.Kind,
                    "The Find predicate kind is not defined."),
            },
            Value = predicate.SuppliedValue,
        };
    }

    private static FindJsonPresentation Presentation(FindPresentationSelection presentation)
    {
        return new FindJsonPresentation
        {
            View = new FindJsonView
            {
                Supplied = presentation.SuppliedView is null
                    ? null
                    : View(presentation.SuppliedView.Value),
                Effective = View(presentation.EffectiveView),
            },
            Content = new FindJsonContent
            {
                Supplied = presentation.Content.Supplied.Select(ContentPart).ToArray(),
                Effective = presentation.Content.Effective.Select(ContentPart).ToArray(),
            },
        };
    }

    private static FindJsonCoverage Coverage(FindCoverage coverage)
    {
        return new FindJsonCoverage
        {
            State = CoverageState(coverage.State),
            Matching = CoverageState(coverage.Matching),
            Projection = ProjectionCoverageState(coverage.Projection),
        };
    }

    private static FindJsonFinding Finding(FindFinding finding)
    {
        return new FindJsonFinding
        {
            Code = FindDefinitions.ReadFindingCode(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Subject = finding.Subject,
            Cause = finding.Cause,
            SelectorRole = finding.SelectorRole is null ? null : SelectorRole(finding.SelectorRole.Value),
            SelectorOccurrence = finding.SelectorOccurrence,
            Source = finding.Source is null ? null : Identity(finding.Source),
            Layer = finding.Layer is null ? null : Layer(finding.Layer.Value),
            Path = finding.Path,
            Region = finding.Region is null ? null : Region(finding.Region),
            Location = finding.Location is null ? null : Location(finding.Location),
            Candidates = finding.Candidates.Select(Identity).ToArray(),
        };
    }

    private static FindJsonMatch Match(FindMatch match)
    {
        return new FindJsonMatch
        {
            Position = match.Position,
            Id = match.Id,
            Path = match.Path,
            Description = match.Description,
            Evidence = match.Evidence.Select(Evidence).ToArray(),
            Projections = match.Projections.Select(Projection).ToArray(),
        };
    }

    private static FindJsonEvidence Evidence(FindEvidence evidence)
    {
        return new FindJsonEvidence
        {
            Predicate = evidence.Predicate,
            Kind = evidence.Kind switch
            {
                FindPredicateKind.Tag => "tag",
                FindPredicateKind.Heading => "heading",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(evidence),
                    evidence.Kind,
                    "The Find evidence predicate kind is not defined."),
            },
            Query = evidence.Query,
            Authored = evidence.Authored,
            Region = Region(evidence.Region),
            Layer = Layer(evidence.Layer),
            Path = evidence.Path,
            Location = Location(evidence.Location),
            Occurrence = evidence.Occurrence,
            Heading = evidence.Heading is null
                ? null
                : new FindJsonHeadingEvidence
                {
                    Level = evidence.Heading.Level,
                    Form = HeadingForm(evidence.Heading.Form),
                    Canonical = evidence.Heading.Canonical,
                },
        };
    }

    private static FindJsonProjectionModel Projection(FindProjection projection)
    {
        return new FindJsonProjectionModel
        {
            Part = ProjectionPart(projection.Part),
            Name = ProjectionName(projection),
            Layer = projection.Layer is null ? null : Layer(projection.Layer.Value),
            Path = projection.Path,
            State = ProjectionState(projection.State),
            Metadata = projection.Metadata is null ? null : Metadata(projection.Metadata),
            Text = projection.Text,
            Headings = projection.Headings.Select(ProjectedHeading).ToArray(),
            Location = projection.Location is null ? null : Location(projection.Location),
        };
    }

    private static FindJsonMetadata Metadata(FindMetadata metadata)
    {
        return new FindJsonMetadata
        {
            Position = metadata.Position,
            Id = metadata.Id,
            Path = metadata.Path,
            RouteState = metadata.RouteState switch
            {
                FindRouteState.Routed => "routed",
                FindRouteState.Unrouted => "unrouted",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(metadata),
                    metadata.RouteState,
                    "The Find route state is not defined."),
            },
            Route = metadata.Route,
            Layers = metadata.Layers.Select(MetadataLayer).ToArray(),
        };
    }

    private static FindJsonMetadataLayer MetadataLayer(FindMetadataLayer layer)
    {
        return new FindJsonMetadataLayer
        {
            Kind = Layer(layer.Kind),
            Path = layer.Path,
        };
    }

    private static FindJsonProjectedHeading ProjectedHeading(FindProjectedHeading heading)
    {
        return new FindJsonProjectedHeading
        {
            Text = heading.Text,
            Level = heading.Level,
            Form = HeadingForm(heading.Form),
            Location = Location(heading.Location),
            Canonical = heading.Canonical,
        };
    }

    private static FindJsonIdentity Identity(FindSourceIdentity identity)
    {
        return new FindJsonIdentity
        {
            Id = identity.Id,
            Path = identity.Path,
        };
    }

    private static FindJsonLocation Location(SourceLocation location)
    {
        return new FindJsonLocation
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };
    }

    private static string ContentPart(FindContentPart part)
    {
        return part.Kind switch
        {
            FindContentPartKind.Metadata => FindDefinitions.Metadata,
            FindContentPartKind.Frontmatter => FindDefinitions.Frontmatter,
            FindContentPartKind.Headings => FindDefinitions.Headings,
            FindContentPartKind.Body => FindDefinitions.Body,
            FindContentPartKind.Section => SectionValue(part.Name, nameof(part)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(part),
                part.Kind,
                "The Find content part kind is not defined."),
        };
    }

    private static string? ProjectionName(FindProjection projection)
    {
        return projection.Part switch
        {
            FindContentPartKind.Metadata
                or FindContentPartKind.Frontmatter
                or FindContentPartKind.Headings
                or FindContentPartKind.Body => null,
            FindContentPartKind.Section => SectionName(projection.Name, nameof(projection)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(projection),
                projection.Part,
                "The Find projection part kind is not defined."),
        };
    }

    private static string ProjectionPart(FindContentPartKind part)
    {
        return part switch
        {
            FindContentPartKind.Metadata => FindDefinitions.Metadata,
            FindContentPartKind.Frontmatter => FindDefinitions.Frontmatter,
            FindContentPartKind.Headings => FindDefinitions.Headings,
            FindContentPartKind.Body => FindDefinitions.Body,
            FindContentPartKind.Section => "section",
            _ => throw new ArgumentOutOfRangeException(
                nameof(part),
                part,
                "The Find projection part kind is not defined."),
        };
    }

    private static string Region(FindRegion region)
    {
        return region.Kind switch
        {
            FindRegionKind.Document => FindDefinitions.Document,
            FindRegionKind.Frontmatter => FindDefinitions.Frontmatter,
            FindRegionKind.Body => FindDefinitions.Body,
            FindRegionKind.Section => SectionValue(region.Name, nameof(region)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region.Kind,
                "The Find region kind is not defined."),
        };
    }

    private static string SectionValue(string? name, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, parameterName);
        return $"{FindDefinitions.SectionPrefix}{name}";
    }

    private static string SectionName(string? name, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, parameterName);
        return name;
    }

    private static string SelectorForm(SourceReferenceKind form)
    {
        return form switch
        {
            SourceReferenceKind.SourceId => "id",
            SourceReferenceKind.SourcePath => "path",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The Find selector form is not defined."),
        };
    }

    private static string SelectorResolution(FindSelectorResolution resolution)
    {
        return resolution switch
        {
            FindSelectorResolution.Resolved => "resolved",
            FindSelectorResolution.Invalid => "invalid",
            FindSelectorResolution.Unknown => "unknown",
            FindSelectorResolution.Unsupported => "unsupported",
            FindSelectorResolution.Ambiguous => "ambiguous",
            FindSelectorResolution.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution,
                "The Find selector resolution is not defined."),
        };
    }

    private static string SourceKind(FindSourceKind kind)
    {
        return kind switch
        {
            FindSourceKind.Loader => "loader",
            FindSourceKind.Entrypoint => "entrypoint",
            FindSourceKind.Skill => "skill",
            FindSourceKind.Ordinary => "ordinary",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find source kind is not defined."),
        };
    }

    private static string Expansion(FindSelectorExpansion expansion)
    {
        return expansion switch
        {
            FindSelectorExpansion.Folder => "folder",
            FindSelectorExpansion.Source => "source",
            _ => throw new ArgumentOutOfRangeException(
                nameof(expansion),
                expansion,
                "The Find selector expansion is not defined."),
        };
    }

    private static string View(CliView view)
    {
        return view switch
        {
            CliView.Compact => "compact",
            CliView.Expanded => "expanded",
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "The view is not defined."),
        };
    }

    private static string CoverageState(FindCoverageState state)
    {
        return state switch
        {
            FindCoverageState.NotStarted => "not-started",
            FindCoverageState.Complete => "complete",
            FindCoverageState.Incomplete => "incomplete",
            FindCoverageState.Blocked => "blocked",
            FindCoverageState.Failed => "failed",
            FindCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find coverage state is not defined."),
        };
    }

    private static string ProjectionCoverageState(FindProjectionCoverageState state)
    {
        return state switch
        {
            FindProjectionCoverageState.NotRequested => "not-requested",
            FindProjectionCoverageState.NotStarted => "not-started",
            FindProjectionCoverageState.Complete => "complete",
            FindProjectionCoverageState.Incomplete => "incomplete",
            FindProjectionCoverageState.Blocked => "blocked",
            FindProjectionCoverageState.Failed => "failed",
            FindProjectionCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection coverage state is not defined."),
        };
    }

    private static string SelectorRole(FindSelectorRole role)
    {
        return role switch
        {
            FindSelectorRole.Include => "include",
            FindSelectorRole.Exclude => "exclude",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Find selector role is not defined."),
        };
    }

    private static string Layer(SourceLayerKind layer)
    {
        return layer switch
        {
            SourceLayerKind.Base => "base",
            SourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };
    }

    private static string ProjectionState(FindProjectionState state)
    {
        return state switch
        {
            FindProjectionState.Available => "available",
            FindProjectionState.Missing => "missing",
            FindProjectionState.Unavailable => "unavailable",
            FindProjectionState.Ambiguous => "ambiguous",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection state is not defined."),
        };
    }

    private static string HeadingForm(MarkdownHeadingForm form)
    {
        return form switch
        {
            MarkdownHeadingForm.Atx => "atx",
            MarkdownHeadingForm.Setext => "setext",
            _ => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The Markdown heading form is not defined."),
        };
    }
}

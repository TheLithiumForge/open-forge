using OpenForge.Cli.Core.Commands.Context.Models.Presentation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextJsonProjector
{
    internal static ContextJsonDocument Create(ContextResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);
        return new ContextJsonDocument
        {
            SchemaVersion = ContextDefinitions.SchemaVersion,
            Command = result.Command,
            Status = Status(result.Status),
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new ContextJsonResult
            {
                Selection = Selection(result.Selection),
                Presentation = Presentation(result.Presentation),
                Coverage = Coverage(result.Coverage),
                Paths = result.Paths.Select(PathProjection).ToArray(),
                Links = result.Links.Select(Link).ToArray(),
                Sources = result.Sources.Select(Source).ToArray(),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new ContextJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static ContextJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace.SelectedBy, "The workspace selection method is not defined."),
            },
        };

    private static ContextJsonSelection Selection(ContextSelection selection)
        => new()
        {
            RequestedSources = selection.RequestedSources.Select(RequestedSource).ToArray(),
            StartupIncluded = selection.StartupIncluded,
            AdditionsOnly = selection.AdditionsOnly,
            LinkExpansion = new ContextJsonLinkExpansion
            {
                Mode = selection.LinkExpansion.Mode switch
                {
                    ContextLinkExpansionMode.None => "none",
                    ContextLinkExpansionMode.Bounded => "bounded",
                    ContextLinkExpansionMode.All => "all",
                    _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.LinkExpansion.Mode, "The link expansion mode is not defined."),
                },
                Depth = selection.LinkExpansion.Depth,
            },
            SourceCount = selection.SourceCount,
        };

    private static ContextJsonRequestedSource RequestedSource(ContextRequestedSource source)
        => new()
        {
            Supplied = source.Supplied,
            Form = source.Form switch
            {
                SourceReferenceKind.SourceId => "source-id",
                SourceReferenceKind.SourcePath => "source-path",
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Form, "The source-reference form is not defined."),
            },
            Resolution = SourceResolution(source.Resolution),
            Source = source.Source is null ? null : Identity(source.Source),
            RouteState = source.RouteState is null ? null : RouteState(source.RouteState.Value),
            Candidates = source.Candidates.Select(Identity).ToArray(),
        };

    private static ContextJsonPresentation Presentation(ContextPresentation presentation)
        => new()
        {
            View = new ContextJsonView
            {
                Supplied = presentation.SuppliedView is null ? null : View(presentation.SuppliedView.Value),
                Effective = View(presentation.EffectiveView),
            },
            Content = new ContextJsonContent
            {
                Supplied = presentation.Content.Supplied.Select(part => part.CanonicalValue).ToArray(),
                Effective = presentation.Content.Effective.Select(part => part.CanonicalValue).ToArray(),
            },
        };

    private static ContextJsonCoverage Coverage(ContextCoverage coverage)
        => new()
        {
            State = Coverage(coverage.State),
            Selection = Coverage(coverage.Selection),
            Links = OptionalCoverage(coverage.Links),
            Projection = Coverage(coverage.Projection),
        };

    private static ContextJsonPathProjection PathProjection(ContextPathProjection path)
        => new()
        {
            Position = path.Position,
            SourcePosition = path.SourcePosition,
            Id = path.Id,
            Path = path.Path,
            Layer = Layer(path.Layer),
            InclusionReasons = path.InclusionReasons.Select(InclusionReason).ToArray(),
        };

    private static ContextJsonSource Source(ContextSource source)
        => new()
        {
            Position = source.Position,
            Id = source.Id,
            Path = source.Path,
            RouteState = RouteState(source.RouteState),
            Route = source.Route,
            Scope = source.Scope,
            InclusionReasons = source.InclusionReasons.Select(InclusionReason).ToArray(),
            Layers = source.Layers.Select(SourceLayer).ToArray(),
        };

    private static ContextJsonLayer SourceLayer(ContextLayer layer)
        => new()
        {
            PathPosition = layer.PathPosition,
            Kind = Layer(layer.Kind),
            Path = layer.Path,
            InclusionReasons = layer.InclusionReasons.Select(InclusionReason).ToArray(),
            Projections = layer.Projections.Select(Projection).ToArray(),
        };

    private static ContextJsonInclusionReason InclusionReason(ContextInclusionReason reason)
        => new()
        {
            Kind = reason.Kind switch
            {
                ContextInclusionReasonKind.WorkspaceEntry => "workspace-entry",
                ContextInclusionReasonKind.Loader => "loader",
                ContextInclusionReasonKind.LoadNow => "load-now",
                ContextInclusionReasonKind.KeepInMind => "keep-in-mind",
                ContextInclusionReasonKind.AncestorRequired => "ancestor-required",
                ContextInclusionReasonKind.SelectedSource => "selected-source",
                ContextInclusionReasonKind.ScopeLocal => "scope-local",
                ContextInclusionReasonKind.LinkedSource => "linked-source",
                ContextInclusionReasonKind.OverwriteCompanion => "overwrite-companion",
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason.Kind, "The inclusion reason is not defined."),
            },
            Source = reason.Source is null ? null : Identity(reason.Source),
            Reference = reason.Reference,
            Depth = reason.Depth,
            Location = reason.Location is null ? null : Location(reason.Location),
        };

    private static ContextJsonProjection Projection(ContextProjection projection)
        => new()
        {
            Part = projection.Part switch
            {
                ContextProjectionPart.Frontmatter => "frontmatter",
                ContextProjectionPart.Headings => "headings",
                ContextProjectionPart.Body => "body",
                ContextProjectionPart.Section => "section",
                _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The projection part is not defined."),
            },
            Name = projection.Name,
            State = projection.State switch
            {
                ContextProjectionState.Available => "available",
                ContextProjectionState.Missing => "missing",
                ContextProjectionState.Unavailable => "unavailable",
                ContextProjectionState.Ambiguous => "ambiguous",
                _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.State, "The projection state is not defined."),
            },
            Text = projection.Text,
            Headings = projection.Headings.Select(Heading).ToArray(),
            Location = projection.Location is null ? null : Location(projection.Location),
        };

    private static ContextJsonProjectedHeading Heading(ContextProjectedHeading heading)
        => new()
        {
            Text = heading.Text,
            Level = heading.Level,
            Form = heading.Form switch
            {
                ContextHeadingForm.Atx => "atx",
                ContextHeadingForm.Setext => "setext",
                _ => throw new ArgumentOutOfRangeException(nameof(heading), heading.Form, "The heading form is not defined."),
            },
            Location = Location(heading.Location),
            Canonical = heading.Canonical,
        };

    private static ContextJsonLink Link(ContextLink link)
        => new()
        {
            Depth = link.Depth,
            Source = new ContextJsonLinkSource
            {
                Id = link.Source.Id,
                Path = link.Source.Path,
                Layer = Layer(link.Source.Layer),
            },
            Location = Location(link.Location),
            DestinationLocation = link.DestinationLocation is null ? null : Location(link.DestinationLocation),
            RawDestination = link.RawDestination,
            Fragment = link.Fragment,
            Target = new ContextJsonLinkTarget
            {
                Kind = LinkTargetKind(link.Target.Kind),
                Id = link.Target.Id,
                Path = link.Target.Path,
                Layer = link.Target.Layer is null ? null : Layer(link.Target.Layer.Value),
                Resolution = LinkResolution(link.Target.Resolution),
                Network = link.Target.Network is null ? null : "network-not-attempted",
            },
            Disposition = link.Disposition switch
            {
                ContextLinkDisposition.Selected => "selected",
                ContextLinkDisposition.AlreadySelected => "already-selected",
                ContextLinkDisposition.Cycle => "cycle",
                ContextLinkDisposition.ExternalUnchecked => "external-unchecked",
                ContextLinkDisposition.Unresolved => "unresolved",
                _ => throw new ArgumentOutOfRangeException(nameof(link), link.Disposition, "The link disposition is not defined."),
            },
        };

    private static ContextJsonFinding Finding(ContextFinding finding)
        => new()
        {
            Code = ContextDefinitions.Read(finding.Code).Code,
            Status = Status(finding.Status),
            Subject = finding.Subject,
            Cause = finding.Cause,
            Reference = finding.Reference,
            Source = finding.Source is null ? null : Identity(finding.Source),
            Layer = finding.Layer is null ? null : Layer(finding.Layer.Value),
            Path = finding.Path,
            Part = finding.Part?.CanonicalValue,
            Location = finding.Location is null ? null : Location(finding.Location),
            DestinationLocation = finding.DestinationLocation is null ? null : Location(finding.DestinationLocation),
            Candidates = finding.Candidates.Select(Identity).ToArray(),
        };

    private static ContextJsonSourceIdentity Identity(ContextSourceIdentity identity)
        => new() { Id = identity.Id, Path = identity.Path };

    private static ContextJsonLocation Location(SourceLocation location)
        => new()
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };

    private static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    private static string View(CliView view)
        => view switch
        {
            CliView.Compact => CliPresentationDefinitions.Compact,
            CliView.Expanded => CliPresentationDefinitions.Expanded,
            _ => throw new ArgumentOutOfRangeException(nameof(view), view, "The Context view is not defined."),
        };

    private static string SourceResolution(SourceReferenceResolutionState resolution)
        => resolution switch
        {
            SourceReferenceResolutionState.Resolved => "resolved",
            SourceReferenceResolutionState.Invalid => "invalid",
            SourceReferenceResolutionState.Unknown => "unknown",
            SourceReferenceResolutionState.Ambiguous => "ambiguous",
            SourceReferenceResolutionState.Unsupported => "unsupported",
            SourceReferenceResolutionState.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The source resolution is not defined."),
        };

    private static string RouteState(ContextRouteState state)
        => state switch
        {
            ContextRouteState.Routed => "routed",
            ContextRouteState.Unrouted => "unrouted",
            ContextRouteState.Ambiguous => "ambiguous",
            ContextRouteState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route state is not defined."),
        };

    private static string Layer(ContextSourceLayerKind layer)
        => layer switch
        {
            ContextSourceLayerKind.Base => "base",
            ContextSourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The source layer is not defined."),
        };

    private static string Coverage(ContextCoverageState coverage)
        => coverage switch
        {
            ContextCoverageState.NotStarted => "not-started",
            ContextCoverageState.Complete => "complete",
            ContextCoverageState.Incomplete => "incomplete",
            ContextCoverageState.Blocked => "blocked",
            ContextCoverageState.Failed => "failed",
            ContextCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The coverage state is not defined."),
        };

    private static string OptionalCoverage(ContextOptionalCoverageState coverage)
        => coverage switch
        {
            ContextOptionalCoverageState.NotRequested => "not-requested",
            ContextOptionalCoverageState.NotStarted => "not-started",
            ContextOptionalCoverageState.Complete => "complete",
            ContextOptionalCoverageState.Incomplete => "incomplete",
            ContextOptionalCoverageState.Blocked => "blocked",
            ContextOptionalCoverageState.Failed => "failed",
            ContextOptionalCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The optional coverage state is not defined."),
        };

    private static string LinkTargetKind(ContextLinkTargetKind kind)
        => kind switch
        {
            ContextLinkTargetKind.Local => "local",
            ContextLinkTargetKind.External => "external",
            ContextLinkTargetKind.Unsupported => "unsupported",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The link target kind is not defined."),
        };

    private static string LinkResolution(ContextLinkResolution resolution)
        => resolution switch
        {
            ContextLinkResolution.Complete => "complete",
            ContextLinkResolution.Missing => "missing",
            ContextLinkResolution.FragmentMissing => "fragment-missing",
            ContextLinkResolution.CaseMismatch => "case-mismatch",
            ContextLinkResolution.Malformed => "malformed",
            ContextLinkResolution.Absolute => "absolute",
            ContextLinkResolution.Query => "query",
            ContextLinkResolution.EncodingUnsupported => "encoding-unsupported",
            ContextLinkResolution.OutsideWorkspace => "outside-workspace",
            ContextLinkResolution.PhysicalEscape => "physical-escape",
            ContextLinkResolution.Ambiguous => "ambiguous",
            ContextLinkResolution.Unreadable => "unreadable",
            ContextLinkResolution.Unsupported => "unsupported",
            ContextLinkResolution.ExternalUnchecked => "external-unchecked",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The link resolution is not defined."),
        };
}

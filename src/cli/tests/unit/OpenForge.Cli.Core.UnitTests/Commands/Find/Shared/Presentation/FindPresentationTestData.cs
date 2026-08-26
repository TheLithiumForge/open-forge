using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

internal static class FindPresentationTestData
{
    internal static IReadOnlyList<CliSemanticStatus> Statuses =>
    [
        CliSemanticStatus.Complete,
        CliSemanticStatus.Attention,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
    ];

    internal static FindResult ForStatus(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => CompleteResult(),
            CliSemanticStatus.Attention => AttentionResult(),
            CliSemanticStatus.Incomplete => IncompleteResult(),
            CliSemanticStatus.Invalid => InvalidResult(),
            CliSemanticStatus.Blocked => BlockedResult(),
            CliSemanticStatus.Failed => FailedResult(),
            CliSemanticStatus.Interrupted => InterruptedResult(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };

    internal static FindResult CompleteResult()
        => RichResult(Workspace(), FindRequirement.All, CliView.Expanded, AllContent());

    internal static FindResult CompactResult()
        => RichResult(Workspace(), FindRequirement.All, CliView.Compact, OmittedContent());

    internal static FindResult AnyResult()
        => RichResult(Workspace(), FindRequirement.Any, CliView.Expanded, OmittedContent());

    internal static FindResult AttentionResult()
    {
        var source = Source(withOverwrite: true);
        var query = RichQuery(FindRequirement.All);
        return Build(
            Workspace(),
            new FindUniverseFilter([source.Identity.AutomaticId], []),
            query,
            Presentation(AllContent()),
            Universe(source),
            [Match(source, query)],
            Projections(source),
            [IdentityCollisionFinding()],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Complete),
            null);
    }

    internal static FindResult OrderedFindingsResult()
    {
        var source = Source();
        var content = SectionContent("Target");
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(content),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 1),
            [SafeMatch(source)],
            [Projection(
                source,
                FindContentPartKind.Section,
                "Target",
                SourceLayerKind.Base,
                FindProjectionState.Missing,
                null)],
            [
                IdentityCollisionFinding(),
            ],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Complete),
            null);
    }

    internal static FindResult IncompleteResult()
    {
        var source = Source();
        var content = SectionContent("Target");
        var identity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(content),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 1),
            [SafeMatch(source)],
            [Projection(
                source,
                FindContentPartKind.Section,
                "Target",
                SourceLayerKind.Base,
                FindProjectionState.Ambiguous,
                null)],
            [new FindFinding(
                FindFindingCode.SectionAmbiguous,
                CliSemanticStatus.Incomplete,
                source.Identity.AutomaticId,
                "Several Target headings prevent one exact section projection.",
                null,
                null,
                identity,
                SourceLayerKind.Base,
                source.Base.CanonicalPath,
                new FindRegion(FindRegionKind.Section, "Target", "section:Target"),
                new SourceLocation(12, 1, 120, 10),
                [])],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Incomplete),
            null);
    }

    internal static FindResult InvalidResult()
        => ExplicitMalformedContentResult();

    internal static FindResult MatchingIncompleteResult()
    {
        var source = Source();
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, null, 1),
            [SafeMatch(source)],
            [],
            [Finding(FindFindingCode.InspectionUnavailable, CliSemanticStatus.Incomplete, source)],
            new FindStageCompletion(FindCoverageState.Incomplete, FindProjectionCoverageState.NotRequested),
            null);
    }

    internal static FindResult BlockedResult()
    {
        var selector = new FindUniverseFilter(["docs"], []);
        return Build(
            null,
            selector,
            EmptyQuery(),
            Presentation(OmittedContent()),
            null,
            [],
            [],
            [Finding(FindFindingCode.WorkspaceUnavailable, CliSemanticStatus.Blocked)],
            new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
            null);
    }

    internal static FindResult SelectorAmbiguousBlockedResult()
    {
        var candidates = CollisionCandidates();
        var selector = new FindSelector(
            "docs",
            SourceReferenceKind.SourceId,
            FindSelectorResolution.Ambiguous,
            null,
            null,
            null,
            candidates);
        return Build(
            Workspace(),
            new FindUniverseFilter(["docs"], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Filtered, [selector], [], null, null, null),
            [],
            [],
            [new FindFinding(
                FindFindingCode.SelectorAmbiguous,
                CliSemanticStatus.Blocked,
                "docs",
                "The source selector identifies more than one source.",
                FindSelectorRole.Include,
                1,
                null,
                null,
                null,
                null,
                null,
                candidates)],
            new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
            null);
    }

    internal static FindResult FailedResult()
        => TerminalResult(
            CliSemanticStatus.Failed,
            FindTerminalEventKind.Failed,
            "The Find operation failed at its bounded operating boundary.");

    internal static FindResult InterruptedResult()
        => TerminalResult(
            CliSemanticStatus.Interrupted,
            FindTerminalEventKind.Interrupted,
            "The Find operation was interrupted at its bounded operating boundary.");

    internal static FindResult OmittedContentResult()
        => Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 1),
            [SafeMatch(Source())],
            [],
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
            null);

    internal static FindResult ZeroMatchesResult()
        => Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Default, [], [], 0, 0, 0),
            [],
            [],
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
            null);

    internal static FindResult ExplicitMalformedContentResult()
    {
        var content = new FindContentSelection([], [], isRequested: true);
        var query = EmptyQuery();
        var request = new FindRequestEcho(
            Workspace(),
            new FindUniverseFilter([], []),
            query,
            Presentation(content, suppliedView: null));
        return new FindResultBuilder().Build(new FindResultInput(
            request,
            null,
            [],
            [],
            [],
            [Finding(FindFindingCode.InvalidInput, CliSemanticStatus.Invalid)],
            new FindStageCompletion(FindCoverageState.NotStarted, FindProjectionCoverageState.NotStarted),
            null));
    }

    internal static FindResult ProjectionCoverageNotStartedResult()
        => Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(AllContent()),
            new FindUniverse(FindUniverseMode.Default, [], [], null, null, null),
            [],
            [],
            [Finding(FindFindingCode.InvalidInput, CliSemanticStatus.Invalid)],
            new FindStageCompletion(FindCoverageState.NotStarted, FindProjectionCoverageState.NotStarted),
            null);

    internal static FindResult BlockedProjectionResult()
        => Build(
            null,
            new FindUniverseFilter(["docs"], []),
            EmptyQuery(),
            Presentation(AllContent()),
            null,
            [],
            [],
            [Finding(FindFindingCode.WorkspaceUnavailable, CliSemanticStatus.Blocked)],
            new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.Blocked),
            null);

    internal static FindResult NullViewResult()
        => Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(OmittedContent(), suppliedView: null, effectiveView: CliView.Compact),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 1),
            [SafeMatch(Source())],
            [],
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
            null);

    internal static FindResult JsonFiniteValuesResult()
    {
        var routed = Source(".agents/docs.md", withOverwrite: true);
        var unrouted = Source(".agents/unrouted.md");
        var query = JsonVocabularyQuery();
        var include = JsonSelectors();
        IReadOnlyList<FindSelector> exclude = [JsonExcludeSelector()];
        return Build(
            Workspace(),
            new FindUniverseFilter(include.Select(selector => selector.Value), exclude.Select(selector => selector.Value)),
            query,
            Presentation(AllContent(), suppliedView: CliView.Compact, effectiveView: CliView.Compact),
            new FindUniverse(FindUniverseMode.Filtered, include, exclude, 5, 5, 2),
            [JsonMatch(routed, query, position: 1), JsonMatch(unrouted, query, position: 2)],
            Projections(routed, position: 1, routeState: FindRouteState.Routed)
                .Concat(Projections(unrouted, position: 2, routeState: FindRouteState.Unrouted)),
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Complete),
            null);
    }

    internal static IReadOnlyList<(string Name, FindResult Result)> JsonSelectorResolutionResults()
        =>
        [
            ("invalid", InvalidSelectorResult(
                "bad,selector",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Invalid)),
            ("unknown", InvalidSelectorResult(
                "unknown",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Unknown)),
            ("unsupported", InvalidSelectorResult(
                ".agents/unsupported",
                SourceReferenceKind.SourcePath,
                FindSelectorResolution.Unsupported)),
            ("ambiguous", BlockedSelectorResult(
                "docs",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Ambiguous,
                FindSelectorRole.Include,
                FindFindingCode.SelectorAmbiguous,
                CollisionCandidates())),
            ("unsafe", BlockedSelectorResult(
                ".agents/unsafe.md",
                SourceReferenceKind.SourcePath,
                FindSelectorResolution.Unsafe,
                FindSelectorRole.Exclude,
                FindFindingCode.SelectorUnsafe,
                [])),
        ];

    internal static FindResult SelectorRoleResult()
    {
        var candidates = CollisionCandidates();
        var include = new FindSelector(
            "docs",
            SourceReferenceKind.SourceId,
            FindSelectorResolution.Ambiguous,
            null,
            null,
            null,
            candidates);
        var exclude = new FindSelector(
            ".agents/unsafe.md",
            SourceReferenceKind.SourcePath,
            FindSelectorResolution.Unsafe,
            null,
            null,
            null,
            []);
        return Build(
            Workspace(),
            new FindUniverseFilter([include.Value], [exclude.Value]),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Filtered, [include], [exclude], null, null, null),
            [],
            [],
            [
                new FindFinding(
                    FindFindingCode.SelectorAmbiguous,
                    CliSemanticStatus.Blocked,
                    include.Value,
                    "The include selector is ambiguous.",
                    FindSelectorRole.Include,
                    1,
                    null,
                    null,
                    null,
                    null,
                    null,
                    candidates),
                new FindFinding(
                    FindFindingCode.SelectorUnsafe,
                    CliSemanticStatus.Blocked,
                    exclude.Value,
                    "The exclude selector crosses the workspace boundary.",
                    FindSelectorRole.Exclude,
                    1,
                    null,
                    null,
                    null,
                    null,
                    null,
                    []),
            ],
            new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
            null);
    }

    internal static IReadOnlyList<(string Name, FindResult Result, string State, string Matching, string Projection)> CoverageResults()
        =>
        [
            ("not-started", ProjectionCoverageNotStartedResult(), "not-started", "not-started", "not-started"),
            ("complete", CompleteResult(), "complete", "complete", "complete"),
            ("matching-incomplete", MatchingIncompleteResult(), "incomplete", "incomplete", "not-requested"),
            ("projection-incomplete", IncompleteResult(), "incomplete", "complete", "incomplete"),
            ("blocked", BlockedProjectionResult(), "blocked", "blocked", "blocked"),
            ("failed", TerminalResult(
                CliSemanticStatus.Failed,
                FindTerminalEventKind.Failed,
                "The Find operation failed at its bounded operating boundary.",
                AllContent()), "failed", "complete", "failed"),
            ("interrupted", TerminalResult(
                CliSemanticStatus.Interrupted,
                FindTerminalEventKind.Interrupted,
                "The Find operation was interrupted at its bounded operating boundary.",
                AllContent()), "interrupted", "complete", "interrupted"),
            ("not-requested", OmittedContentResult(), "complete", "complete", "not-requested"),
        ];

    internal static IReadOnlyList<(FindFindingCode Code, FindResult Result)> FindingCodeResults()
        => Enum.GetValues<FindFindingCode>()
            .Select(code => (code, FindingCodeResult(code)))
            .ToArray();

    internal static FindResult ProjectionStateResult(FindProjectionState state)
    {
        var source = Source();
        var content = SectionContent("Target");
        var completion = state is FindProjectionState.Unavailable or FindProjectionState.Ambiguous
            ? new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Incomplete)
            : new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.Complete);
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(content),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 1),
            [SafeMatch(source)],
            [Projection(
                source,
                FindContentPartKind.Section,
                "Target",
                SourceLayerKind.Base,
                state,
                state == FindProjectionState.Available ? "available section" : null)],
            [],
            completion,
            null);
    }

    internal static FindResult HostileResult()
    {
        var source = Source(withOverwrite: true);
        var query = RichQuery(FindRequirement.All);
        var identity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        return Build(
            DiagnosticWorkspace(),
            new FindUniverseFilter([source.Identity.AutomaticId], []),
            query,
            Presentation(OmittedContent()),
            Universe(source),
            [Match(source, query)],
            [],
            [new FindFinding(
                FindFindingCode.InspectionUnavailable,
                CliSemanticStatus.Incomplete,
                "hostile source subject must stay private",
                "base frontmatter overwrite frontmatter base body overwrite body base section overwrite section",
                null,
                null,
                identity,
                SourceLayerKind.Base,
                source.Base.CanonicalPath,
                new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                new SourceLocation(9, 1, 90, 12),
                [])],
            new FindStageCompletion(
                FindCoverageState.Incomplete,
                FindProjectionCoverageState.NotRequested),
            null);
    }

    internal static CliPresentationRequest<FindResult> PresentationRequest(
        FindResult result,
        CliOutputFormat format = CliOutputFormat.Human,
        CliView view = CliView.Expanded,
        CliVerbosity verbosity = CliVerbosity.Normal)
        => new(result, new CliPresentation(format, view, verbosity));

    internal static FindContentSelection OmittedContent()
        => new([], []);

    internal static FindContentSelection AllContent()
    {
        var parts = new FindContentPart[]
        {
            new(FindContentPartKind.Metadata, null, FindDefinitions.Metadata),
            new(FindContentPartKind.Frontmatter, null, FindDefinitions.Frontmatter),
            new(FindContentPartKind.Headings, null, FindDefinitions.Headings),
            new(FindContentPartKind.Body, null, FindDefinitions.Body),
            new(FindContentPartKind.Section, "Target", "section:Target"),
        };
        return new FindContentSelection(parts, parts, isRequested: true);
    }

    internal static FindContentSelection SectionContent(string name)
    {
        var part = new FindContentPart(
            FindContentPartKind.Section,
            name,
            $"{FindDefinitions.SectionPrefix}{name}");
        return new FindContentSelection([part], [part], isRequested: true);
    }

    private static FindResult RichResult(
        CliWorkspace workspace,
        FindRequirement requirement,
        CliView view,
        FindContentSelection content)
    {
        var source = Source(withOverwrite: true);
        var query = RichQuery(requirement);
        return Build(
            workspace,
            new FindUniverseFilter([source.Identity.AutomaticId], []),
            query,
            Presentation(content, view, view),
            Universe(source),
            [Match(source, query)],
            content.Effective.Count == 0 ? [] : Projections(source),
            [],
            new FindStageCompletion(
                FindCoverageState.Complete,
                content.Effective.Count == 0
                    ? FindProjectionCoverageState.NotRequested
                    : FindProjectionCoverageState.Complete),
            null);
    }

    private static FindResult TerminalResult(
        CliSemanticStatus status,
        FindTerminalEventKind eventKind,
        string cause,
        FindContentSelection? content = null)
    {
        var source = Source();
        content ??= OmittedContent();
        var terminal = new FindTerminalEvent(eventKind, cause);
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(content),
            new FindUniverse(FindUniverseMode.Default, [], [], 1, 1, 0),
            [SafeMatch(source)],
            content.Effective.Count == 0 ? [] : Projections(source),
            [],
            new FindStageCompletion(
                FindCoverageState.Complete,
                content.Effective.Count == 0
                    ? FindProjectionCoverageState.NotRequested
                    : eventKind == FindTerminalEventKind.Failed
                        ? FindProjectionCoverageState.Failed
                        : FindProjectionCoverageState.Interrupted),
            terminal);
    }

    private static FindResult FindingCodeResult(FindFindingCode code)
    {
        if (code == FindFindingCode.IdentityCollision)
        {
            return AttentionResult();
        }

        if (code == FindFindingCode.ProjectionMissing)
        {
            return ProjectionStateResult(FindProjectionState.Missing);
        }

        if (code == FindFindingCode.SectionAmbiguous)
        {
            return ProjectionStateResult(FindProjectionState.Ambiguous);
        }

        if (code == FindFindingCode.ProjectionUnavailable)
        {
            return ProjectionStateResult(FindProjectionState.Unavailable);
        }

        var status = FindDefinitions.ReadFindingStatus(code);
        var matching = status switch
        {
            CliSemanticStatus.Invalid => FindCoverageState.NotStarted,
            CliSemanticStatus.Blocked => FindCoverageState.Blocked,
            CliSemanticStatus.Incomplete => FindCoverageState.Incomplete,
            _ => FindCoverageState.Complete,
        };
        return Build(
            Workspace(),
            new FindUniverseFilter([], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Default, [], [], 0, null, 0),
            [],
            [],
            [new FindFinding(
                code,
                status,
                null,
                $"The typed {code} finding is retained for JSON mapping evidence.",
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                [])],
            new FindStageCompletion(
                matching,
                FindProjectionCoverageState.NotRequested),
            null);
    }

    private static FindResult Build(
        CliWorkspace? workspace,
        FindUniverseFilter filter,
        FindQuery query,
        FindPresentationSelection presentation,
        FindUniverse? universe,
        IEnumerable<FindMatch> matches,
        IEnumerable<FindProjection> projections,
        IEnumerable<FindFinding> findings,
        FindStageCompletion completion,
        FindTerminalEvent? terminal)
        => new FindResultBuilder().Build(new FindResultInput(
            new FindRequestEcho(workspace, filter, query, presentation),
            universe,
            [],
            matches,
            projections,
            findings,
            completion,
            terminal));

    private static FindPresentationSelection Presentation(
        FindContentSelection content,
        CliView? suppliedView = CliView.Expanded,
        CliView effectiveView = CliView.Expanded)
        => new(suppliedView, effectiveView, content);

    private static FindQuery EmptyQuery()
        => new(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
                [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]));

    private static FindQuery RichQuery(FindRequirement requirement)
    {
        var predicates = new FindPredicate[]
        {
            new(FindPredicateKind.Tag, LongTag, LongTag),
            new(FindPredicateKind.Heading, LongHeading, LongHeading),
        };
        return new FindQuery(
            predicates,
            predicates,
            requirement,
            new FindRegionSelection(
                [
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                ],
                [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
                [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]));
    }

    private static FindQuery JsonVocabularyQuery()
    {
        var predicates = new FindPredicate[]
        {
            new(FindPredicateKind.Tag, "Tag", "Tag"),
            new(FindPredicateKind.Heading, "Heading", "Heading"),
        };
        return new FindQuery(
            predicates,
            predicates,
            FindRequirement.Any,
            new FindRegionSelection(
                [
                    new FindRegion(FindRegionKind.Document, null, FindDefinitions.Document),
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                    new FindRegion(FindRegionKind.Section, "Target", "section:Target"),
                ],
                [new FindRegion(FindRegionKind.Document, null, FindDefinitions.Document)],
                [new FindRegion(FindRegionKind.Section, "Target", "section:Target")]));
    }

    private static IReadOnlyList<FindSelector> JsonSelectors()
        =>
        [
            new FindSelector(
                "loader",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Resolved,
                Identity("loader", ".agents/loader.md"),
                FindSourceKind.Loader,
                FindSelectorExpansion.Folder,
                []),
            new FindSelector(
                ".agents/documents/_documents.md",
                SourceReferenceKind.SourcePath,
                FindSelectorResolution.Resolved,
                Identity("documents", ".agents/documents/_documents.md"),
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                []),
            new FindSelector(
                "skills/demo",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Resolved,
                Identity("skills/demo", ".agents/skills/demo/SKILL.md"),
                FindSourceKind.Skill,
                FindSelectorExpansion.Folder,
                []),
            new FindSelector(
                ".agents/docs.md",
                SourceReferenceKind.SourcePath,
                FindSelectorResolution.Resolved,
                Identity("docs", ".agents/docs.md"),
                FindSourceKind.Ordinary,
                FindSelectorExpansion.Source,
                []),
            new FindSelector(
                "unrouted",
                SourceReferenceKind.SourceId,
                FindSelectorResolution.Resolved,
                Identity("unrouted", ".agents/unrouted.md"),
                FindSourceKind.Ordinary,
                FindSelectorExpansion.Source,
                []),
        ];

    private static FindSelector JsonExcludeSelector()
        => new(
            ".agents/guide.md",
            SourceReferenceKind.SourcePath,
            FindSelectorResolution.Resolved,
            Identity("guide", ".agents/guide.md"),
            FindSourceKind.Ordinary,
            FindSelectorExpansion.Source,
            []);

    private static FindResult InvalidSelectorResult(
        string value,
        SourceReferenceKind? form,
        FindSelectorResolution resolution)
    {
        var selector = new FindSelector(value, form, resolution, null, null, null, []);
        return Build(
            Workspace(),
            new FindUniverseFilter([value], []),
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Filtered, [selector], [], null, null, null),
            [],
            [],
            [SelectorFinding(
                FindFindingCode.InvalidSelector,
                CliSemanticStatus.Invalid,
                value,
                FindSelectorRole.Include,
                1,
                [])],
            new FindStageCompletion(FindCoverageState.NotStarted, FindProjectionCoverageState.NotRequested),
            null);
    }

    private static FindResult BlockedSelectorResult(
        string value,
        SourceReferenceKind form,
        FindSelectorResolution resolution,
        FindSelectorRole role,
        FindFindingCode code,
        IReadOnlyList<FindSourceIdentity> candidates)
    {
        var selector = new FindSelector(value, form, resolution, null, null, null, candidates);
        var include = role == FindSelectorRole.Include ? new[] { selector } : Array.Empty<FindSelector>();
        var exclude = role == FindSelectorRole.Exclude ? new[] { selector } : Array.Empty<FindSelector>();
        var filter = new FindUniverseFilter(
            include.Select(item => item.Value),
            exclude.Select(item => item.Value));
        return Build(
            Workspace(),
            filter,
            EmptyQuery(),
            Presentation(OmittedContent()),
            new FindUniverse(FindUniverseMode.Filtered, include, exclude, null, null, null),
            [],
            [],
            [SelectorFinding(
                code,
                CliSemanticStatus.Blocked,
                value,
                role,
                1,
                candidates)],
            new FindStageCompletion(FindCoverageState.Blocked, FindProjectionCoverageState.NotRequested),
            null);
    }

    private static FindSourceIdentity Identity(string id, string path)
        => new(id, path);

    private static FindUniverse Universe(SourceLogicalSource source)
    {
        var identity = new FindSourceIdentity(
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath);
        var selector = new FindSelector(
            source.Identity.AutomaticId,
            SourceReferenceKind.SourceId,
            FindSelectorResolution.Resolved,
            identity,
            FindSourceKind.Ordinary,
            FindSelectorExpansion.Source,
            []);
        return new FindUniverse(
            FindUniverseMode.Filtered,
            [selector],
            [],
            1,
            1,
            1);
    }

    private static FindMatch Match(SourceLogicalSource source, FindQuery query)
    {
        var basePath = source.Base.CanonicalPath;
        var overwritePath = source.Overwrite?.CanonicalPath
            ?? throw new InvalidOperationException("The rich Find fixture requires an overwrite layer.");
        return new FindMatch(
            1,
            source.Identity.AutomaticId,
            basePath,
            "A description with a tab\t, quote \" and slash \",",
            [
                new FindEvidence(
                    1,
                    FindPredicateKind.Tag,
                    query.EffectivePredicates[0].SuppliedValue,
                    query.EffectivePredicates[0].SuppliedValue,
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    SourceLayerKind.Base,
                    basePath,
                    new SourceLocation(2, 4, 12, 8),
                    1,
                    null),
                new FindEvidence(
                    1,
                    FindPredicateKind.Tag,
                    query.EffectivePredicates[0].SuppliedValue,
                    query.EffectivePredicates[0].SuppliedValue,
                    new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                    SourceLayerKind.Overwrite,
                    overwritePath,
                    new SourceLocation(2, 4, 14, 8),
                    1,
                    null),
                new FindEvidence(
                    2,
                    FindPredicateKind.Heading,
                    query.EffectivePredicates[1].SuppliedValue,
                    query.EffectivePredicates[1].SuppliedValue,
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                    SourceLayerKind.Base,
                    basePath,
                    new SourceLocation(8, 1, 80, 20),
                    1,
                    new FindHeadingEvidence(2, MarkdownHeadingForm.Atx, true)),
                new FindEvidence(
                    2,
                    FindPredicateKind.Heading,
                    query.EffectivePredicates[1].SuppliedValue,
                    query.EffectivePredicates[1].SuppliedValue,
                    new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body),
                    SourceLayerKind.Overwrite,
                    overwritePath,
                    new SourceLocation(8, 1, 82, 20),
                    1,
                    new FindHeadingEvidence(2, MarkdownHeadingForm.Setext, false)),
            ],
            []);
    }

    private static FindMatch SafeMatch(SourceLogicalSource source)
        => new(
            1,
            source.Identity.AutomaticId,
            source.Identity.CanonicalBasePath,
            "A safe terminal match retained before the terminal event.",
            [],
            []);

    private static FindMatch JsonMatch(SourceLogicalSource source, FindQuery query, int position)
    {
        var path = source.Base.CanonicalPath;
        var predicate = query.EffectivePredicates[0];
        return new FindMatch(
            position,
            source.Identity.AutomaticId,
            path,
            "A filtered JSON match.",
            [new FindEvidence(
                1,
                predicate.Kind,
                predicate.SuppliedValue,
                predicate.SuppliedValue,
                new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter),
                SourceLayerKind.Base,
                path,
                new SourceLocation(2, 1, 12, 3),
                1,
                null)],
            []);
    }

    private static IReadOnlyList<FindProjection> Projections(
        SourceLogicalSource source,
        int position = 1,
        FindRouteState routeState = FindRouteState.Routed)
    {
        var basePath = source.Base.CanonicalPath;
        var metadataLayers = new List<FindMetadataLayer>
        {
            new(SourceLayerKind.Base, basePath),
        };
        if (source.Overwrite is { } overwrite)
        {
            metadataLayers.Add(new FindMetadataLayer(SourceLayerKind.Overwrite, overwrite.CanonicalPath));
        }

        var projections = new List<FindProjection>
        {
            new(
                FindContentPartKind.Metadata,
                null,
                null,
                null,
                FindProjectionState.Available,
                new FindMetadata(
                    position,
                    source.Identity.AutomaticId,
                    basePath,
                    routeState,
                    routeState == FindRouteState.Routed ? source.Identity.AutomaticId : null,
                    metadataLayers),
                null,
                [],
                null),
            Projection(source, FindContentPartKind.Frontmatter, null, SourceLayerKind.Base, FindProjectionState.Available, "base frontmatter"),
            Projection(source, FindContentPartKind.Headings, null, SourceLayerKind.Base, FindProjectionState.Available, null),
            Projection(source, FindContentPartKind.Body, null, SourceLayerKind.Base, FindProjectionState.Available, "base body"),
            Projection(source, FindContentPartKind.Section, "Target", SourceLayerKind.Base, FindProjectionState.Available, "base section"),
        };
        if (source.Overwrite is not null)
        {
            projections.AddRange(
            [
                Projection(source, FindContentPartKind.Frontmatter, null, SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite frontmatter"),
                Projection(source, FindContentPartKind.Headings, null, SourceLayerKind.Overwrite, FindProjectionState.Available, null),
                Projection(source, FindContentPartKind.Body, null, SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite body"),
                Projection(source, FindContentPartKind.Section, "Target", SourceLayerKind.Overwrite, FindProjectionState.Available, "overwrite section"),
            ]);
        }

        return projections;
    }

    private static FindProjection Projection(
        SourceLogicalSource source,
        FindContentPartKind part,
        string? name,
        SourceLayerKind layer,
        FindProjectionState state,
        string? text)
    {
        var path = layer == SourceLayerKind.Base
            ? source.Base.CanonicalPath
            : source.Overwrite?.CanonicalPath
                ?? throw new InvalidOperationException("The Find projection fixture requires its overwrite layer.");
        var location = new SourceLocation(
            layer == SourceLayerKind.Base ? 5 : 15,
            1,
            layer == SourceLayerKind.Base ? 40 : 140,
            text?.Length ?? 0);
        FindProjectedHeading[] headings = part == FindContentPartKind.Headings && state == FindProjectionState.Available
            ? [new FindProjectedHeading(
                "Projected heading",
                2,
                layer == SourceLayerKind.Base ? MarkdownHeadingForm.Atx : MarkdownHeadingForm.Setext,
                new SourceLocation(
                    layer == SourceLayerKind.Base ? 8 : 18,
                    1,
                    layer == SourceLayerKind.Base ? 80 : 180,
                    18),
                layer == SourceLayerKind.Base)]
            : [];
        return new FindProjection(
            part,
            name,
            layer,
            path,
            state,
            null,
            state == FindProjectionState.Available && part != FindContentPartKind.Headings ? text : null,
            headings,
            state == FindProjectionState.Available && part is not FindContentPartKind.Headings ? location : null);
    }

    private static FindFinding IdentityCollisionFinding()
        => new(
            FindFindingCode.IdentityCollision,
            CliSemanticStatus.Attention,
            "docs",
            "Several logical sources share one automatic source ID.",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            CollisionCandidates());

    private static FindFinding SelectorFinding(
        FindFindingCode code,
        CliSemanticStatus status,
        string subject,
        FindSelectorRole role,
        int occurrence,
        IReadOnlyList<FindSourceIdentity> candidates)
        => new(
            code,
            status,
            subject,
            $"The {role.ToString().ToLowerInvariant()} selector retains its {code.ToString().ToLowerInvariant()} resolution.",
            role,
            occurrence,
            null,
            null,
            null,
            null,
            null,
            candidates);

    private static FindFinding Finding(
        FindFindingCode code,
        CliSemanticStatus status,
        SourceLogicalSource? source = null)
    {
        var sourceIdentity = source is null
            ? null
            : new FindSourceIdentity(source.Identity.AutomaticId, source.Identity.CanonicalBasePath);
        SourceLayerKind? layer = source is null ? null : SourceLayerKind.Base;
        var path = source?.Base.CanonicalPath;
        return new FindFinding(
            code,
            status,
            source?.Identity.AutomaticId,
            $"A bounded {FindDefinitions.ReadFindingCode(code)} condition retained for presentation evidence.",
            null,
            null,
            sourceIdentity,
            layer,
            path,
            null,
            source is null ? null : new SourceLocation(1, 1, 0, 1),
            []);
    }

    private static IReadOnlyList<FindSourceIdentity> CollisionCandidates()
        =>
        [
            new FindSourceIdentity("docs", ".agents/docs.md"),
            new FindSourceIdentity("docs", ".agents/docs/_docs.md"),
        ];

    private static SourceLogicalSource Source(bool withOverwrite = false)
        => Source(".agents/docs.md", withOverwrite);

    private static SourceLogicalSource Source(string basePath, bool withOverwrite = false)
    {
        var root = Workspace().PhysicalRoot;
        var id = SourceIdentity.DeriveId(basePath)
            ?? throw new InvalidOperationException("The Find fixture source must have a derived identity.");
        var baseLayer = new SourceLayer(
            basePath,
            Physical(root, basePath),
            SourceDocumentForm.Markdown,
            SourceLayerKind.Base);
        var overwrite = withOverwrite
            ? new SourceLayer(
                $"{basePath[..^3]}.overwrite.md",
                Physical(root, $"{basePath[..^3]}.overwrite.md"),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, basePath),
            baseLayer,
            overwrite);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-presentation-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static CliWorkspace DiagnosticWorkspace()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "find-\"diagnostic\\workspace-" + new string('x', 360));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(root, logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private static string LongTag
        => $"Tag-{new string('t', 600)}";

    private static string LongHeading
        => $"Heading-{new string('h', 600)}\\\"";
}

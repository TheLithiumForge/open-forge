using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Commands.References.Shared.Documents.Parsing;
using OpenForge.Cli.Core.Commands.References.Shared.Extraction;
using OpenForge.Cli.Core.Commands.References.Shared.Inspection;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.References;

public sealed class ReferencesDomainModelTests
{
    [Fact(DisplayName = "References definitions expose exactly the accepted finding vocabulary"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void FindingVocabularyIsFiniteAndOrdered()
    {
        Assert.Equal(25, ReferencesDefinitions.FindingCodes.Count);
        Assert.Equal(
            [
                "references.invalid-input",
                "references.invalid-source",
                "references.invalid-direction",
                "references.invalid-filter",
                "references.workspace-unavailable",
                "references.workspace-unsafe",
                "references.source-ambiguous",
                "references.source-unsafe",
                "references.selector-ambiguous",
                "references.selector-unsafe",
                "references.identity-collision",
                "references.candidate-unsafe",
                "references.layer-unresolved",
                "references.inspection-unavailable",
                "references.invalid-encoding",
                "references.generated-region-unavailable",
                "references.destination-malformed",
                "references.destination-unsupported",
                "references.target-missing",
                "references.fragment-missing",
                "references.target-unsafe",
                "references.target-ambiguous",
                "references.target-unreadable",
                "references.operation-failed",
                "references.interrupted",
            ],
            ReferencesDefinitions.FindingCodes.Select(ReferencesDefinitions.ReadMachineName));
    }

    [Fact(DisplayName = "References requests preserve global selector order and reject selectors for outgoing-only work"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void RequestValidatesDirectionAndSelectorPositions()
    {
        var workspace = new CliWorkspace("/tmp/references-workspace", "/tmp/references-workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 1),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, "docs/private", 2),
        };
        var request = new ReferencesRequest(workspace, "docs", ReferencesDirection.In, selectors);

        Assert.Equal(selectors, request.SelectorOccurrences);
        Assert.Throws<ArgumentException>(() => new ReferencesRequest(
            workspace,
            "docs",
            ReferencesDirection.Out,
            selectors));
        Assert.Throws<ArgumentException>(() => new ReferencesRequest(
            workspace,
            "docs",
            ReferencesDirection.In,
            [new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 2)]));
    }

    [Fact(DisplayName = "References extraction preserves accepted links, locations, and generated-region exclusion"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ExtractionUsesNeutralMarkdownFactsOnly()
    {
        const string sourceText =
            "[inline](target.md)\n"
            + "[reference][target]\n"
            + "<https://example.invalid>\n"
            + "![image](image.png)\n"
            + "\n"
            + "[target]: <target>\n"
            + "\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "[generated](generated.md)\n"
            + "<!-- open-forge:generated-index:end -->\n";
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity("docs", ".agents/docs.md"),
            new SourceLayer(".agents/docs.md", "/tmp/references-workspace/.agents/docs.md", SourceDocumentForm.Markdown, SourceLayerKind.Base));
        var facts = new MarkdownDocumentParser().Parse(sourceText);
        var extracted = new ReferencesLinkExtractor().Extract(
            source,
            source.Base,
            source.Base.CanonicalPath,
            facts,
            ReferencesDirection.Out,
            ReferencesProvenance.SelectedSource);

        Assert.True(extracted.Links.Count == 3, string.Join("|", extracted.Links.Select(link => link.RawDestination)));
        Assert.Equal(["target.md", "target", "https://example.invalid"], extracted.Links.Select(link => link.RawDestination));
        Assert.Equal(ReferencesDirection.Out, extracted.Links[0].Direction);
        Assert.True(extracted.Links[0].Location.ByteLength > 0);
        Assert.Null(extracted.Links[2].DestinationLocation);
        Assert.DoesNotContain(extracted.Links, link => link.RawDestination == "generated.md");
    }

    public static IEnumerable<object[]> FindingStatuses()
        => Enum.GetValues<ReferencesFindingCode>()
            .Select(code => new object[]
            {
                code,
                code switch
                {
                    ReferencesFindingCode.InvalidInput
                        or ReferencesFindingCode.InvalidSource
                        or ReferencesFindingCode.InvalidDirection
                        or ReferencesFindingCode.InvalidFilter => CliSemanticStatus.Invalid,
                    ReferencesFindingCode.WorkspaceUnavailable
                        or ReferencesFindingCode.WorkspaceUnsafe
                        or ReferencesFindingCode.SourceAmbiguous
                        or ReferencesFindingCode.SourceUnsafe
                        or ReferencesFindingCode.SelectorAmbiguous
                        or ReferencesFindingCode.SelectorUnsafe
                        or ReferencesFindingCode.TargetUnsafe
                        or ReferencesFindingCode.TargetAmbiguous => CliSemanticStatus.Blocked,
                    ReferencesFindingCode.IdentityCollision
                        or ReferencesFindingCode.DestinationMalformed
                        or ReferencesFindingCode.DestinationUnsupported
                        or ReferencesFindingCode.TargetMissing
                        or ReferencesFindingCode.FragmentMissing => CliSemanticStatus.Attention,
                    ReferencesFindingCode.CandidateUnsafe
                        or ReferencesFindingCode.LayerUnresolved
                        or ReferencesFindingCode.InspectionUnavailable
                        or ReferencesFindingCode.InvalidEncoding
                        or ReferencesFindingCode.GeneratedRegionUnavailable
                        or ReferencesFindingCode.TargetUnreadable => CliSemanticStatus.Incomplete,
                    ReferencesFindingCode.OperationFailed => CliSemanticStatus.Failed,
                    ReferencesFindingCode.Interrupted => CliSemanticStatus.Interrupted,
                    _ => throw new ArgumentOutOfRangeException(nameof(code), code, null),
                },
            });

    [Fact(DisplayName = "References findings map every accepted code to its exact status"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void FindingStatusMappingIsExact()
    {
        foreach (var row in FindingStatuses())
        {
            var code = Assert.IsType<ReferencesFindingCode>(row[0]);
            var status = Assert.IsType<CliSemanticStatus>(row[1]);
            Assert.Equal(status, ReferencesDefinitions.ReadStatus(code));
            Assert.Equal(status, CreateFinding(code).Status);
        }
    }

    [Fact(DisplayName = "References generated-region status is conditionally incomplete or blocked only"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void GeneratedRegionStatusOverrideIsBounded()
    {
        var blocked = CreateFinding(
            ReferencesFindingCode.GeneratedRegionUnavailable,
            CliSemanticStatus.Blocked);

        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Throws<ArgumentException>(() => CreateFinding(
            ReferencesFindingCode.GeneratedRegionUnavailable,
            CliSemanticStatus.Attention));
        Assert.Throws<ArgumentException>(() => CreateFinding(
            ReferencesFindingCode.TargetMissing,
            CliSemanticStatus.Blocked));
    }

    [Fact(DisplayName = "References extraction retains links and reports an invalid generated boundary"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ExtractionRetainsLinksWhenGeneratedBoundaryIsInvalid()
    {
        const string sourceText =
            "# Document\n\n"
            + "[authored](authored.md)\n\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "[candidate](candidate.md)\n"
            + "<!-- open-forge:generated-index:end -->\n"
            + "## Later\n";
        var source = CreateLogicalSource();
        var document = new MarkdownDocumentParser().Parse(sourceText);
        Assert.Equal(MarkdownGeneratedRegionState.Invalid, document.GeneratedRegion.State);

        var extracted = new ReferencesLinkExtractor().Extract(
            source,
            source.Base,
            source.Base.CanonicalPath,
            document,
            ReferencesDirection.Out,
            ReferencesProvenance.SelectedSource);

        Assert.True(extracted.Established);
        Assert.False(extracted.Blocked);
        Assert.Equal(["authored.md", "candidate.md"], extracted.Links.Select(link => link.RawDestination));
        var finding = Assert.Single(extracted.Findings);
        Assert.Equal(ReferencesFindingCode.GeneratedRegionUnavailable, finding.Code);
        Assert.False(finding.Blocked);
    }

    [Fact(DisplayName = "References extraction blocks only when the Markdown body boundary is unavailable"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ExtractionBlocksOnlyWhenBodyBoundaryIsUnavailable()
    {
        var source = CreateLogicalSource();
        const string noBodyBoundary = "---\nopen-forge:\n  tags: [One]\n# Missing terminator\n";
        var document = new MarkdownDocumentParser().Parse(noBodyBoundary);
        Assert.Equal(MarkdownGeneratedRegionState.Unavailable, document.GeneratedRegion.State);

        var blocked = new ReferencesLinkExtractor().Extract(
            source,
            source.Base,
            source.Base.CanonicalPath,
            document,
            ReferencesDirection.Out,
            ReferencesProvenance.SelectedSource);
        Assert.False(blocked.Established);
        Assert.True(blocked.Blocked);
        Assert.Single(blocked.Findings, value => value.Code == ReferencesFindingCode.GeneratedRegionUnavailable);
    }

    [Fact(DisplayName = "References occurrence and selection models enforce direction and order invariants"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ModelsRejectContradictoryDirectionAndSelectorFacts()
    {
        var source = new ReferencesOccurrenceSource("docs", ".agents/docs.md", SourceLayerKind.Base);
        var location = new SourceLocation(1, 1, 0, 1);
        var target = new ReferencesTarget(
            ReferencesTargetKind.Local,
            "docs",
            ".agents/docs.md",
            SourceLayerKind.Base,
            ReferencesTargetResolution.Complete,
            null);

        Assert.Throws<ArgumentOutOfRangeException>(() => new ReferencesOccurrence(
            ReferencesDirection.Both,
            source,
            location,
            null,
            "docs.md",
            null,
            target,
            ReferencesProvenance.SelectedSource));
        Assert.Throws<ArgumentException>(() => new ReferencesOccurrence(
            ReferencesDirection.In,
            source,
            location,
            null,
            "docs.md",
            null,
            target,
            ReferencesProvenance.SelectedSource));
        Assert.Throws<ArgumentException>(() => new ReferencesIncomingSelection(
            ReferencesSelectionMode.Default,
            [new ReferencesSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs")],
            [],
            [],
            []));
        Assert.Throws<ArgumentException>(() => new ReferencesFinding(
            ReferencesFindingCode.InvalidFilter,
            ReferencesDirection.In,
            "docs",
            "cause",
            SourceUniverseSelectorRole.Include,
            null,
            null,
            null,
            null,
            null,
            null,
            [],
            null));
    }

    [Fact(DisplayName = "References operation converts an unexpected boundary failure into failed partial evidence"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public async Task UnexpectedBoundaryFailureStopsWithFailedIncompleteSections()
    {
        SourcePhysicalPathResolver sourcePathResolver = static (_, _) => throw new InvalidOperationException("not reached");
        var sourceReferenceResolver = new SourceReferenceResolver(sourcePathResolver);
        ReferencesMarkdownParser markdownParser = static _ => throw new InvalidOperationException("not reached");
        var operation = new ReferencesOperation(
            new ReferencesSourceResolver(
                static (_, _) => throw new InvalidOperationException("boundary failure"),
                sourceReferenceResolver,
                new SourceUniverseFilterResolver(sourceReferenceResolver)),
            new ReferencesLayerInspector(
                static (_, _, _) => throw new InvalidOperationException("not reached"),
                markdownParser),
            new ReferencesDestinationResolver(
                static (_, _) => throw new InvalidOperationException("not reached"),
                static (_, _, _) => throw new InvalidOperationException("not reached"),
                markdownParser),
            new ReferencesResultBuilder());
        var request = new ReferencesRequest(
            new CliWorkspace("/tmp/references-failed", "/tmp/references-failed", CliWorkspaceSelectionMethod.CurrentDirectory),
            "docs",
            ReferencesDirection.Both,
            []);

        var result = await operation.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(ReferencesCoverage.Incomplete, result.Incoming!.Coverage);
        Assert.Equal(ReferencesCoverage.Incomplete, result.Outgoing!.Coverage);
        Assert.Empty(result.Incoming.Occurrences);
        Assert.Empty(result.Outgoing.Occurrences);
        Assert.Single(result.Findings, finding => finding.Code == ReferencesFindingCode.OperationFailed);
        Assert.Equal("open-forge references --verbose", result.Next!.Command);
    }

    [Theory(DisplayName = "References incoming requests admit interleaved duplicate selectors"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData(false), InlineData(true)]
    public void IncomingRequestsAdmitInterleavedDuplicates(bool both)
    {
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 1),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, ".agents/skip.md", 2),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 3),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, "./.agents/skip.md", 4),
        };
        var request = new ReferencesRequest(
            new CliWorkspace("/tmp/references-filtered", "/tmp/references-filtered", CliWorkspaceSelectionMethod.CurrentDirectory),
            "docs",
            both ? ReferencesDirection.Both : ReferencesDirection.In,
            selectors);

        Assert.True(request.RequestsIncoming);
        Assert.Equal(both, request.RequestsOutgoing);
        Assert.Equal([1, 2, 3, 4], request.SelectorOccurrences.Select(row => row.Position));
        Assert.Equal(["docs", ".agents/skip.md", "docs", "./.agents/skip.md"], request.SelectorOccurrences.Select(row => row.Value));
    }

    [Theory(DisplayName = "References reader failure retains unresolved incoming selector rows"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData(false), InlineData(true)]
    public async Task EarlyReaderFailureRetainsIncomingSelectors(bool both)
    {
        SourcePhysicalPathResolver sourcePathResolver = static (_, _) => throw new InvalidOperationException("not reached");
        var sourceReferenceResolver = new SourceReferenceResolver(sourcePathResolver);
        ReferencesMarkdownParser markdownParser = static _ => throw new InvalidOperationException("not reached");
        var operation = new ReferencesOperation(
            new ReferencesSourceResolver(
                static (_, _) => throw new InvalidOperationException("boundary failure"),
                sourceReferenceResolver,
                new SourceUniverseFilterResolver(sourceReferenceResolver)),
            new ReferencesLayerInspector(
                static (_, _, _) => throw new InvalidOperationException("not reached"),
                markdownParser),
            new ReferencesDestinationResolver(
                static (_, _) => throw new InvalidOperationException("not reached"),
                static (_, _, _) => throw new InvalidOperationException("not reached"),
                markdownParser),
            new ReferencesResultBuilder());
        var selectors = new[]
        {
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 1),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, ".agents/skip.md", 2),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 3),
            new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, "./.agents/skip.md", 4),
        };
        var request = new ReferencesRequest(
            new CliWorkspace("/tmp/references-failed", "/tmp/references-failed", CliWorkspaceSelectionMethod.CurrentDirectory),
            "docs",
            both ? ReferencesDirection.Both : ReferencesDirection.In,
            selectors);

        var result = await operation.ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        var finding = Assert.Single(result.Findings);
        Assert.Equal(ReferencesFindingCode.OperationFailed, finding.Code);
        Assert.Null(finding.Direction);
        Assert.Null(finding.Subject);
        Assert.NotNull(result.Next);
        Assert.Equal("open-forge references --verbose", result.Next.Command);
        Assert.Null(result.Source);
        var selection = Assert.IsType<ReferencesIncomingSelection>(result.IncomingSelection);
        Assert.Equal(ReferencesSelectionMode.Filtered, selection.Mode);
        Assert.Equal(
            [
                (SourceUniverseSelectorRole.Include, "docs"),
                (SourceUniverseSelectorRole.Exclude, ".agents/skip.md"),
                (SourceUniverseSelectorRole.Include, "docs"),
                (SourceUniverseSelectorRole.Exclude, "./.agents/skip.md"),
            ],
            selection.Supplied.Select(row => (row.Role, row.Value)));
        Assert.Equal(
            [
                (SourceUniverseSelectorRole.Include, 1, "docs", SourceReferenceKind.SourceId),
                (SourceUniverseSelectorRole.Exclude, 1, ".agents/skip.md", SourceReferenceKind.SourcePath),
                (SourceUniverseSelectorRole.Include, 2, "docs", SourceReferenceKind.SourceId),
                (SourceUniverseSelectorRole.Exclude, 2, "./.agents/skip.md", SourceReferenceKind.SourcePath),
            ],
            selection.Resolved.Select(row => (row.Role, row.Occurrence, row.Supplied, row.Form)));
        Assert.All(selection.Resolved, row =>
        {
            Assert.Equal(SourceReferenceResolutionState.Unknown, row.Resolution);
            Assert.Null(row.Source);
            Assert.Null(row.Expansion);
            Assert.Empty(row.Candidates);
        });
        Assert.Empty(selection.EffectiveSources);
        Assert.Empty(selection.InspectedSources);
        var incoming = Assert.IsType<ReferencesSection>(result.Incoming);
        Assert.Equal(ReferencesCoverage.Incomplete, incoming.Coverage);
        Assert.Equal(0, incoming.OccurrenceCount);
        Assert.Empty(incoming.Occurrences);
        if (both)
        {
            var outgoing = Assert.IsType<ReferencesSection>(result.Outgoing);
            Assert.Equal(ReferencesCoverage.Incomplete, outgoing.Coverage);
            Assert.Equal(0, outgoing.OccurrenceCount);
            Assert.Empty(outgoing.Occurrences);
        }
        else
        {
            Assert.Null(result.Outgoing);
        }
    }

    private static ReferencesFinding CreateFinding(
        ReferencesFindingCode code,
        CliSemanticStatus? statusOverride = null)
        => new(
            code,
            null,
            null,
            "cause",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            [],
            statusOverride);

    private static SourceLogicalSource CreateLogicalSource()
        => new(
            new SourceLogicalIdentity("docs", ".agents/docs.md"),
            new SourceLayer(
                ".agents/docs.md",
                "/tmp/references-workspace/.agents/docs.md",
                SourceDocumentForm.Markdown,
                SourceLayerKind.Base));
}

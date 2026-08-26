using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindOperationIntegrationRedTests
{
    [Theory(DisplayName = "Direct Find operation covers inventory, matching, projections, empty results, and attention without writes")]
    [InlineData("bare-inventory")]
    [InlineData("predicate-match")]
    [InlineData("projection")]
    [InlineData("zero-candidates")]
    [InlineData("zero-matches")]
    [InlineData("attention")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task DirectOperationCoversBarePredicateProjectionZeroAndAttentionResultsWithoutWrites(
        string scenario)
    {
        using var workspace = FindOperationIntegrationWorkspace.New();
        var before = workspace.SnapshotBytes();
        var request = workspace.CreateRequest(scenario);
        var result = await FindOperationFactory.Create()
            .ExecuteAsync(request, TestContext.Current.CancellationToken);

        AssertDirectScenario(scenario, result);
        Assert.Equal(before, workspace.SnapshotBytes());
    }

    [Theory(DisplayName = "Repeated and pre-cancelled Find operation calls stay deterministic, fresh, and read-only")]
    [InlineData("repeated")]
    [InlineData("pre-cancelled")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task RepeatedAndPreCancelledInvocationsRemainDeterministicFreshAndReadOnly(
        string scenario)
    {
        using var workspace = FindOperationIntegrationWorkspace.New();
        var before = workspace.SnapshotBytes();
        var operation = FindOperationFactory.Create();

        if (scenario == "repeated")
        {
            var request = workspace.CreateRequest("predicate-match");
            var first = await operation.ExecuteAsync(
                request,
                TestContext.Current.CancellationToken);
            var second = await operation.ExecuteAsync(
                request,
                TestContext.Current.CancellationToken);

            Assert.NotSame(first, second);
            Assert.NotSame(first.Universe, second.Universe);
            Assert.NotSame(first.Coverage, second.Coverage);
            Assert.NotSame(first.Matches, second.Matches);
            Assert.Equal(ResultFingerprint(first), ResultFingerprint(second));
        }
        else if (scenario == "pre-cancelled")
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var result = await operation.ExecuteAsync(
                workspace.CreateRequest("bare-inventory"),
                cancellation.Token);

            Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
            Assert.Equal(FindCoverageState.Interrupted, result.Coverage.State);
            Assert.Equal(FindCoverageState.Interrupted, result.Coverage.Matching);
            Assert.Equal(
                FindProjectionCoverageState.NotRequested,
                result.Coverage.Projection);
            Assert.Null(result.Universe.CandidateCount);
            Assert.Null(result.Universe.InspectedCount);
            Assert.Null(result.Universe.MatchedCount);
            Assert.Empty(result.Matches);
            Assert.Contains(
                result.Findings,
                finding => finding.Code == FindFindingCode.Interrupted
                    && finding.Status == CliSemanticStatus.Interrupted);
            var next = Assert.IsType<CliNextAction>(result.Next);
            Assert.Equal("open-forge find", next.Command);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(scenario), scenario);
        }

        Assert.Equal(before, workspace.SnapshotBytes());
    }

    private static void AssertDirectScenario(string scenario, FindResult result)
    {
        switch (scenario)
        {
            case "bare-inventory":
                AssertComplete(result, 3, 3, 3, FindProjectionCoverageState.NotRequested);
                Assert.Equal(FindUniverseMode.Default, result.Universe.Mode);
                Assert.Equal(
                    new[] { "docs", "loader", "unrouted" },
                    result.Matches.Select(match => match.Id).ToArray());
                Assert.Equal(
                    new[]
                    {
                        ".agents/docs/_docs.md",
                        ".agents/loader.md",
                        ".agents/unrouted.md",
                    },
                    result.Matches.Select(match => match.Path).ToArray());
                Assert.All(result.Matches, match => Assert.Empty(match.Evidence));
                break;
            case "predicate-match":
                AssertPredicateMatch(result);
                break;
            case "projection":
                AssertProjection(result);
                break;
            case "zero-candidates":
                AssertComplete(result, 0, 0, 0, FindProjectionCoverageState.NotRequested);
                Assert.Equal(FindUniverseMode.Filtered, result.Universe.Mode);
                Assert.Single(result.Universe.Include);
                Assert.Single(result.Universe.Exclude);
                Assert.Equal("docs", result.Universe.Include[0].Value);
                Assert.Equal("docs", result.Universe.Exclude[0].Value);
                Assert.Empty(result.Matches);
                break;
            case "zero-matches":
                AssertComplete(result, 3, 3, 0, FindProjectionCoverageState.NotRequested);
                Assert.Empty(result.Matches);
                Assert.Empty(result.Findings);
                Assert.Equal("NeverPresent", result.Query.Predicates.Single().SuppliedValue);
                break;
            case "attention":
                AssertAttention(result);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario);
        }
    }

    private static void AssertComplete(
        FindResult result,
        int candidateCount,
        int inspectedCount,
        int matchedCount,
        FindProjectionCoverageState projectionCoverage)
    {
        Assert.Equal("find", result.Command);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.State);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.Matching);
        Assert.Equal(projectionCoverage, result.Coverage.Projection);
        Assert.Equal(candidateCount, result.Universe.CandidateCount);
        Assert.Equal(inspectedCount, result.Universe.InspectedCount);
        Assert.Equal(matchedCount, result.Universe.MatchedCount);
        Assert.Empty(result.Findings);
        Assert.Null(result.Next);
    }

    private static void AssertPredicateMatch(FindResult result)
    {
        AssertComplete(result, 1, 1, 1, FindProjectionCoverageState.NotRequested);
        Assert.Equal(FindUniverseMode.Filtered, result.Universe.Mode);
        var include = Assert.Single(result.Universe.Include);
        Assert.Equal(SourceReferenceKind.SourceId, include.Form);
        Assert.Equal(FindSelectorResolution.Resolved, include.Resolution);
        Assert.Equal(FindSourceKind.Entrypoint, include.SourceKind);
        Assert.Equal(FindSelectorExpansion.Folder, include.Expansion);
        var identity = Assert.IsType<FindSourceIdentity>(include.Identity);
        Assert.Equal("docs", identity.Id);
        Assert.Equal(".agents/docs/_docs.md", identity.Path);
        Assert.Empty(include.Candidates);
        Assert.Empty(result.Universe.Exclude);

        Assert.Equal(FindRequirement.All, result.Query.Requirement);
        Assert.Equal(
            new[] { "Architecture", "Overwrite Heading" },
            result.Query.Predicates.Select(predicate => predicate.SuppliedValue).ToArray());
        Assert.Equal(
            new[] { "Architecture", "Overwrite Heading" },
            result.Query.EffectivePredicates
                .Select(predicate => predicate.SuppliedValue)
                .ToArray());

        var match = Assert.Single(result.Matches);
        Assert.Equal(1, match.Position);
        Assert.Equal("docs", match.Id);
        Assert.Equal(".agents/docs/_docs.md", match.Path);
        Assert.Equal("Docs base", match.Description);
        Assert.Empty(match.Projections);

        var tag = Assert.Single(match.Evidence, evidence => evidence.Kind == FindPredicateKind.Tag);
        Assert.Equal(1, tag.Predicate);
        Assert.Equal("Architecture", tag.Query);
        Assert.Equal("Architecture", tag.Authored);
        Assert.Equal(FindRegionKind.Frontmatter, tag.Region.Kind);
        Assert.Equal(SourceLayerKind.Base, tag.Layer);
        Assert.Equal(".agents/docs/_docs.md", tag.Path);
        Assert.Null(tag.Heading);

        var heading = Assert.Single(
            match.Evidence,
            evidence => evidence.Kind == FindPredicateKind.Heading);
        Assert.Equal(2, heading.Predicate);
        Assert.Equal("Overwrite Heading", heading.Query);
        Assert.Equal("Overwrite Heading", heading.Authored);
        Assert.Equal(FindRegionKind.Body, heading.Region.Kind);
        Assert.Equal(SourceLayerKind.Overwrite, heading.Layer);
        Assert.Equal(".agents/docs/_docs.overwrite.md", heading.Path);
        var headingFacts = Assert.IsType<FindHeadingEvidence>(heading.Heading);
        Assert.Equal(1, headingFacts.Level);
        Assert.Equal(MarkdownHeadingForm.Atx, headingFacts.Form);
        Assert.True(headingFacts.Canonical);
    }

    private static void AssertProjection(FindResult result)
    {
        Assert.Equal("find", result.Command);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(FindCoverageState.Incomplete, result.Coverage.State);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.Matching);
        Assert.Equal(FindProjectionCoverageState.Incomplete, result.Coverage.Projection);
        Assert.Equal(1, result.Universe.CandidateCount);
        Assert.Equal(1, result.Universe.InspectedCount);
        Assert.Equal(1, result.Universe.MatchedCount);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == FindFindingCode.ProjectionUnavailable
                && finding.Status == CliSemanticStatus.Incomplete);
        Assert.NotNull(result.Next);

        Assert.Equal(
            new[] { "section:Target", "metadata", "headings" },
            result.Presentation.Content.Supplied.Select(part => part.CanonicalValue).ToArray());
        Assert.Equal(
            new[] { "metadata", "headings", "section:Target" },
            result.Presentation.Content.Effective.Select(part => part.CanonicalValue).ToArray());

        var match = Assert.Single(result.Matches);
        Assert.Equal(5, match.Projections.Count);

        var metadata = Assert.Single(
            match.Projections,
            projection => projection.Part == FindContentPartKind.Metadata);
        Assert.Equal(FindProjectionState.Unavailable, metadata.State);
        Assert.Null(metadata.Metadata);
        Assert.Null(metadata.Text);
        Assert.Null(metadata.Location);

        var headings = match.Projections
            .Where(projection => projection.Part == FindContentPartKind.Headings)
            .ToArray();
        Assert.Equal(2, headings.Length);
        Assert.Equal(
            new SourceLayerKind?[] { SourceLayerKind.Base, SourceLayerKind.Overwrite },
            headings.Select(projection => projection.Layer).ToArray());
        Assert.Equal(
            new[] { "Base Heading", "Target" },
            headings[0].Headings.Select(heading => heading.Text).ToArray());
        Assert.Equal(
            new[] { "Overwrite Heading", "Target" },
            headings[1].Headings.Select(heading => heading.Text).ToArray());
        Assert.All(headings, projection => Assert.Equal(FindProjectionState.Available, projection.State));

        var sections = match.Projections
            .Where(projection => projection.Part == FindContentPartKind.Section)
            .ToArray();
        Assert.Equal(2, sections.Length);
        Assert.Equal(
            new SourceLayerKind?[] { SourceLayerKind.Base, SourceLayerKind.Overwrite },
            sections.Select(projection => projection.Layer).ToArray());
        Assert.All(sections, projection =>
        {
            Assert.Equal(FindProjectionState.Available, projection.State);
            Assert.Contains("## Target", projection.Text, StringComparison.Ordinal);
            Assert.NotNull(projection.Location);
        });
    }

    private static void AssertAttention(FindResult result)
    {
        Assert.Equal("find", result.Command);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.State);
        Assert.Equal(FindCoverageState.Complete, result.Coverage.Matching);
        Assert.Equal(FindProjectionCoverageState.Complete, result.Coverage.Projection);
        Assert.Equal(3, result.Universe.CandidateCount);
        Assert.Equal(3, result.Universe.InspectedCount);
        Assert.Equal(1, result.Universe.MatchedCount);
        Assert.Null(result.Next);

        var finding = Assert.Single(
            result.Findings,
            candidate => candidate.Code == FindFindingCode.ProjectionMissing);
        Assert.Equal(CliSemanticStatus.Attention, finding.Status);
        Assert.Equal(FindRegionKind.Section, finding.Region?.Kind);
        Assert.Equal("Missing", finding.Region?.Name);

        var match = Assert.Single(result.Matches);
        Assert.Equal("docs", match.Id);
        var sections = match.Projections
            .Where(projection => projection.Part == FindContentPartKind.Section)
            .ToArray();
        Assert.Equal(2, sections.Length);
        Assert.All(sections, projection =>
        {
            Assert.Equal(FindProjectionState.Missing, projection.State);
            Assert.Null(projection.Text);
            Assert.Null(projection.Location);
        });
    }

    private static string ResultFingerprint(FindResult result)
    {
        var universe = string.Join(
            ";",
            result.Universe.Mode,
            Format(result.Universe.CandidateCount),
            Format(result.Universe.InspectedCount),
            Format(result.Universe.MatchedCount),
            string.Join(",", result.Universe.Include.Select(SelectorFingerprint)),
            string.Join(",", result.Universe.Exclude.Select(SelectorFingerprint)));
        var query = string.Join(
            ";",
            result.Query.Requirement,
            string.Join(",", result.Query.Predicates.Select(PredicateFingerprint)),
            string.Join(",", result.Query.EffectivePredicates.Select(PredicateFingerprint)),
            string.Join(",", result.Query.Within.Supplied.Select(RegionFingerprint)),
            string.Join(",", result.Query.Within.Tag.Select(RegionFingerprint)),
            string.Join(",", result.Query.Within.Heading.Select(RegionFingerprint)));
        var presentation = string.Join(
            ";",
            result.Presentation.SuppliedView?.ToString() ?? "null",
            result.Presentation.EffectiveView,
            string.Join(",", result.Presentation.Content.Supplied.Select(ContentFingerprint)),
            string.Join(",", result.Presentation.Content.Effective.Select(ContentFingerprint)));
        var coverage = string.Join(
            ";",
            result.Coverage.State,
            result.Coverage.Matching,
            result.Coverage.Projection);
        var findings = string.Join("|", result.Findings.Select(FindingFingerprint));
        var matches = string.Join("|", result.Matches.Select(MatchFingerprint));
        var next = result.Next is null
            ? "null"
            : $"{result.Next.Command}|{result.Next.Reason}";

        return string.Join(
            "\n",
            result.Command,
            result.Status,
            result.Workspace?.LexicalRoot ?? "null",
            result.Workspace?.PhysicalRoot ?? "null",
            universe,
            query,
            presentation,
            coverage,
            findings,
            matches,
            next);
    }

    private static string SelectorFingerprint(FindSelector selector)
    {
        return string.Join(
            ";",
            selector.Value,
            selector.Form?.ToString() ?? "null",
            selector.Resolution,
            selector.Identity is null ? "null" : IdentityFingerprint(selector.Identity),
            selector.SourceKind?.ToString() ?? "null",
            selector.Expansion?.ToString() ?? "null",
            string.Join(",", selector.Candidates.Select(IdentityFingerprint)));
    }

    private static string PredicateFingerprint(FindPredicate predicate)
    {
        return string.Join(
            ";",
            predicate.Kind,
            predicate.SuppliedValue,
            predicate.ComparisonValue);
    }

    private static string RegionFingerprint(FindRegion region)
    {
        return string.Join(
            ";",
            region.Kind,
            region.Name ?? "null",
            region.CanonicalValue);
    }

    private static string ContentFingerprint(FindContentPart part)
    {
        return string.Join(
            ";",
            part.Kind,
            part.Name ?? "null",
            part.CanonicalValue);
    }

    private static string IdentityFingerprint(FindSourceIdentity identity)
    {
        return $"{identity.Id};{identity.Path}";
    }

    private static string FindingFingerprint(FindFinding finding)
    {
        return string.Join(
            ";",
            finding.Code,
            finding.Status,
            finding.Subject ?? "null",
            finding.Cause,
            finding.SelectorRole?.ToString() ?? "null",
            Format(finding.SelectorOccurrence),
            finding.Source is null ? "null" : IdentityFingerprint(finding.Source),
            finding.Layer?.ToString() ?? "null",
            finding.Path ?? "null",
            finding.Region is null ? "null" : RegionFingerprint(finding.Region),
            finding.Location is null ? "null" : LocationFingerprint(finding.Location),
            string.Join(",", finding.Candidates.Select(IdentityFingerprint)));
    }

    private static string MatchFingerprint(FindMatch match)
    {
        return string.Join(
            ";",
            match.Position,
            match.Id,
            match.Path,
            match.Description ?? "null",
            string.Join(",", match.Evidence.Select(EvidenceFingerprint)),
            string.Join(",", match.Projections.Select(ProjectionFingerprint)));
    }

    private static string EvidenceFingerprint(FindEvidence evidence)
    {
        return string.Join(
            ";",
            evidence.Predicate,
            evidence.Kind,
            evidence.Query,
            evidence.Authored,
            RegionFingerprint(evidence.Region),
            evidence.Layer,
            evidence.Path,
            LocationFingerprint(evidence.Location),
            evidence.Occurrence,
            evidence.Heading is null ? "null" : HeadingFingerprint(evidence.Heading));
    }

    private static string ProjectionFingerprint(FindProjection projection)
    {
        return string.Join(
            ";",
            projection.Part,
            projection.Name ?? "null",
            projection.Layer?.ToString() ?? "null",
            projection.Path ?? "null",
            projection.State,
            projection.Metadata is null ? "null" : MetadataFingerprint(projection.Metadata),
            projection.Text ?? "null",
            string.Join(",", projection.Headings.Select(HeadingFingerprint)),
            projection.Location is null ? "null" : LocationFingerprint(projection.Location));
    }

    private static string MetadataFingerprint(FindMetadata metadata)
    {
        return string.Join(
            ";",
            metadata.Position,
            metadata.Id,
            metadata.Path,
            metadata.RouteState,
            metadata.Route ?? "null",
            string.Join(",", metadata.Layers.Select(layer => $"{layer.Kind}:{layer.Path}")));
    }

    private static string HeadingFingerprint(FindHeadingEvidence heading)
    {
        return string.Join(";", heading.Level, heading.Form, heading.Canonical);
    }

    private static string HeadingFingerprint(FindProjectedHeading heading)
    {
        return string.Join(
            ";",
            heading.Text,
            heading.Level,
            heading.Form,
            LocationFingerprint(heading.Location),
            heading.Canonical);
    }

    private static string LocationFingerprint(SourceLocation location)
    {
        return string.Join(
            ";",
            location.Line,
            location.Column,
            location.ByteOffset,
            location.ByteLength);
    }

    private static string Format(int? value)
    {
        return value?.ToString(CultureInfo.InvariantCulture) ?? "null";
    }

    private sealed class FindOperationIntegrationWorkspace : IDisposable
    {
        private static readonly UTF8Encoding StrictUtf8 = new(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);

        private const string BaseDocument = """
            ---
            open-forge:
              description: Docs base
              tags:
                - Architecture
            ---
            # Base Heading

            ## Target

            Base section bytes.
            """;

        private const string OverwriteDocument = """
            ---
            open-forge:
              description: Docs overwrite
              tags:
                - Layered
            ---
            # Overwrite Heading

            ## Target

            Overwrite section bytes.
            """;

        private const string UnroutedDocument = """
            ---
            open-forge:
              description: Unrouted source
              tags:
                - Unrouted
            ---
            # Unrouted Heading

            This source is outside the selected entrypoint scope.
            """;

        private const string LoaderDocument = """
            # Open Forge Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Docs](docs/_docs.md) - #LoadNow
            <!-- open-forge:generated-index:end -->
            """;

        private readonly TemporaryWorkspace _temporary;

        private FindOperationIntegrationWorkspace()
        {
            _temporary = TemporaryWorkspace.Create("find-operation-integration");
            _temporary.CreateDirectory(".agents");
            WriteUtf8(".agents/loader.md", LoaderDocument);
            WriteUtf8(".agents/docs/_docs.md", BaseDocument);
            WriteUtf8(".agents/docs/_docs.overwrite.md", OverwriteDocument);
            WriteUtf8(".agents/unrouted.md", UnroutedDocument);
        }

        internal static FindOperationIntegrationWorkspace New()
        {
            return new FindOperationIntegrationWorkspace();
        }

        private CliWorkspace Workspace => new(
            _temporary.Path,
            _temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);

        internal FindRequest CreateRequest(string scenario)
        {
            var (include, exclude) = scenario switch
            {
                "predicate-match" or "projection" => (["docs"], Array.Empty<string>()),
                "zero-candidates" => (["docs"], ["docs"]),
                _ => (Array.Empty<string>(), Array.Empty<string>()),
            };
            var predicates = scenario switch
            {
                "predicate-match" or "projection" => new[]
                {
                    new FindPredicate(FindPredicateKind.Tag, "Architecture", "Architecture"),
                    new FindPredicate(FindPredicateKind.Heading, "Overwrite Heading", "Overwrite Heading"),
                },
                "zero-matches" => new[]
                {
                    new FindPredicate(FindPredicateKind.Tag, "NeverPresent", "NeverPresent"),
                },
                "attention" => new[]
                {
                    new FindPredicate(FindPredicateKind.Tag, "Architecture", "Architecture"),
                },
                _ => Array.Empty<FindPredicate>(),
            };
            var content = scenario switch
            {
                "projection" => (
                    new[]
                    {
                        new FindContentPart(
                            FindContentPartKind.Section,
                            "Target",
                            "section:Target"),
                        new FindContentPart(FindContentPartKind.Metadata, null, "metadata"),
                        new FindContentPart(FindContentPartKind.Headings, null, "headings"),
                    },
                    new[]
                    {
                        new FindContentPart(FindContentPartKind.Metadata, null, "metadata"),
                        new FindContentPart(FindContentPartKind.Headings, null, "headings"),
                        new FindContentPart(
                            FindContentPartKind.Section,
                            "Target",
                            "section:Target"),
                    }),
                "attention" => (
                    new[]
                    {
                        new FindContentPart(
                            FindContentPartKind.Section,
                            "Missing",
                            "section:Missing"),
                    },
                    new[]
                    {
                        new FindContentPart(
                            FindContentPartKind.Section,
                            "Missing",
                            "section:Missing"),
                    }),
                _ => (Array.Empty<FindContentPart>(), Array.Empty<FindContentPart>()),
            };

            return new FindRequest(
                Workspace,
                new FindUniverseFilter(include, exclude),
                new FindQuery(
                    predicates,
                    predicates,
                    FindRequirement.All,
                    NaturalRegions()),
                new FindPresentationSelection(
                    suppliedView: null,
                    effectiveView: CliView.Expanded,
                    new FindContentSelection(content.Item1, content.Item2)));
        }

        internal IReadOnlyDictionary<string, string> SnapshotBytes()
        {
            return _temporary.SnapshotHashes();
        }

        private void WriteUtf8(string relativePath, string contents)
        {
            _temporary.WriteBytes(relativePath, StrictUtf8.GetBytes(contents));
        }

        private static FindRegionSelection NaturalRegions()
        {
            return new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
                [new FindRegion(FindRegionKind.Body, null, "body")]);
        }

        public void Dispose()
        {
            _temporary.Dispose();
        }
    }
}

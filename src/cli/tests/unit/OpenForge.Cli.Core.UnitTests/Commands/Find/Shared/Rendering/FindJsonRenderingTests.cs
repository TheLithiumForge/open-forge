using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;
using FindJsonProjectionModel = OpenForge.Cli.Core.Commands.Find.Models.Presentation.FindJsonProjection;
using FindJsonProjectionRenderer = OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindJsonProjection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Rendering;

public sealed class FindJsonRenderingTests
{
    [Fact(DisplayName = "Find JSON projection creates the exact envelope and nested member order"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionCreatesExactEnvelopeOrder()
    {
        var result = FindPresentationTestData.CompleteResult();

        var document = FindJsonProjectionRenderer.Create(result);

        Assert.Equal(1, document.SchemaVersion);
        Assert.Equal("find", document.Command);
        AssertProperties<FindJsonDocument>("SchemaVersion", "Command", "Status", "Workspace", "Result", "Next");
        AssertProperties<FindJsonWorkspace>("Path", "SelectedBy");
        AssertProperties<FindJsonResult>("Universe", "Query", "Presentation", "Coverage", "Findings", "Matches");
        AssertProperties<FindJsonUniverse>("Mode", "Include", "Exclude", "CandidateCount", "InspectedCount", "MatchedCount");
        AssertProperties<FindJsonSelector>("Value", "Form", "Resolution", "Identity", "SourceKind", "Expansion", "Candidates");
        AssertProperties<FindJsonIdentity>("Id", "Path");
        AssertProperties<FindJsonQuery>("Predicates", "EffectivePredicates", "Require", "Within");
        AssertProperties<FindJsonPredicate>("Kind", "Value");
        AssertProperties<FindJsonWithin>("Supplied", "Tag", "Heading");
        AssertProperties<FindJsonPresentation>("View", "Content");
        AssertProperties<FindJsonView>("Supplied", "Effective");
        AssertProperties<FindJsonContent>("Supplied", "Effective");
        AssertProperties<FindJsonCoverage>("State", "Matching", "Projection");
        AssertProperties<FindJsonFinding>("Code", "Status", "Subject", "Cause", "SelectorRole", "SelectorOccurrence", "Source", "Layer", "Path", "Region", "Location", "Candidates");
        AssertProperties<FindJsonLocation>("Line", "Column", "ByteOffset", "ByteLength");
        AssertProperties<FindJsonMatch>("Position", "Id", "Path", "Description", "Evidence", "Projections");
        AssertProperties<FindJsonEvidence>("Predicate", "Kind", "Query", "Authored", "Region", "Layer", "Path", "Location", "Occurrence", "Heading");
        AssertProperties<FindJsonHeadingEvidence>("Level", "Form", "Canonical");
        AssertProperties<FindJsonProjectionModel>("Part", "Name", "Layer", "Path", "State", "Metadata", "Text", "Headings", "Location");
        AssertProperties<FindJsonMetadata>("Position", "Id", "Path", "RouteState", "Route", "Layers");
        AssertProperties<FindJsonMetadataLayer>("Kind", "Path");
        AssertProperties<FindJsonProjectedHeading>("Text", "Level", "Form", "Location", "Canonical");
        AssertProperties<FindJsonNext>("Command", "Reason");
    }

    [Theory(DisplayName = "Find JSON projection maps every finite semantic status"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEveryStatus(string statusValue)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var document = FindJsonProjectionRenderer.Create(result);

        Assert.Equal(statusValue.ToLowerInvariant(), document.Status);
        var expected = statusValue switch
        {
            "Complete" => ("complete", "complete", "complete", "complete", (string?)null,
                (string?)null),
            "Attention" => ("attention", "complete", "complete", "complete", (string?)null,
                (string?)null),
            "Incomplete" => ("incomplete", "incomplete", "complete", "incomplete",
                "open-forge doctor", "Inspect the unavailable source or projection facts before relying on this Find result."),
            "Invalid" => ("invalid", "not-started", "not-started", "not-started",
                "open-forge find --help", "Correct the named Find input, then rerun the request."),
            "Blocked" => ("blocked", "blocked", "blocked", "not-requested",
                "open-forge doctor", "Inspect the blocked workspace or source boundary before rerunning Find."),
            "Failed" => ("failed", "failed", "complete", "not-requested",
                "open-forge find --verbose", "Report the failure and retry the same request with bounded diagnostics."),
            "Interrupted" => ("interrupted", "interrupted", "complete", "not-requested",
                "open-forge find", "Rerun the same Find request."),
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The Find status is not defined."),
        };
        Assert.Equal(expected.Item1, document.Status);
        Assert.Equal(expected.Item2, document.Result.Coverage.State);
        Assert.Equal(expected.Item3, document.Result.Coverage.Matching);
        Assert.Equal(expected.Item4, document.Result.Coverage.Projection);
        Assert.Equal(expected.Item5, document.Next?.Command);
        Assert.Equal(expected.Item6, document.Next?.Reason);
    }

    [Fact(DisplayName = "Find JSON rendering serializes the concrete document for compact and expanded view requests with defined compact evidence omission"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonViewsRetainCoreAndOmitSupportingEvidence()
    {
        var result = FindPresentationTestData.CompleteResult();
        var compact = FindPresentationTestData.PresentationRequest(
            result,
            CliOutputFormat.Json,
            CliView.Compact);
        var expanded = FindPresentationTestData.PresentationRequest(
            result,
            CliOutputFormat.Json,
            CliView.Expanded);

        var compactText = FindJsonRenderer.Render(compact);
        var expandedText = FindJsonRenderer.Render(expanded);

        Assert.True(JsonViewComparison.RetainsResult(compactText, expandedText, ["matches.*.evidence"]));
    }

    [Fact(DisplayName = "Find JSON projection preserves a null supplied view and compact effective view"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesNullSuppliedView()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.NullViewResult());

        Assert.Null(document.Result.Presentation.View.Supplied);
        Assert.Equal("compact", document.Result.Presentation.View.Effective);
    }

    [Theory(DisplayName = "Find JSON projection preserves null, empty-array, and established-count rules for every status"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesExactStatusCountsAndNullRules(string statusValue)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var document = FindJsonProjectionRenderer.Create(result);

        var expected = statusValue switch
        {
            "Complete" => (1, 1, 1, 0, 1, true),
            "Attention" => (1, 1, 1, 1, 1, true),
            "Incomplete" => (1, 1, 1, 1, 1, true),
            "Invalid" => ((int?)null, (int?)null, (int?)null, 1, 0, true),
            "Blocked" => ((int?)null, (int?)null, (int?)null, 1, 0, false),
            "Failed" => (1, 1, 1, 1, 1, true),
            "Interrupted" => (1, 1, 1, 1, 1, true),
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The Find status is not defined."),
        };
        Assert.NotNull(document.Result);
        Assert.NotNull(document.Result.Findings);
        Assert.NotNull(document.Result.Matches);
        Assert.NotNull(document.Result.Universe.Include);
        Assert.NotNull(document.Result.Universe.Exclude);
        Assert.Equal(expected.Item1, document.Result.Universe.CandidateCount);
        Assert.Equal(expected.Item2, document.Result.Universe.InspectedCount);
        Assert.Equal(expected.Item3, document.Result.Universe.MatchedCount);
        Assert.Equal(expected.Item4, document.Result.Findings.Length);
        Assert.Equal(expected.Item5, document.Result.Matches.Length);
        Assert.Equal(expected.Item6, document.Workspace is not null);
        Assert.All(document.Result.Findings, finding =>
        {
            Assert.False(string.IsNullOrWhiteSpace(finding.Code));
            Assert.False(string.IsNullOrWhiteSpace(finding.Status));
            Assert.NotNull(finding.Candidates);
        });
        Assert.All(document.Result.Matches, match =>
        {
            Assert.True(match.Position > 0);
            Assert.NotNull(match.Evidence);
            Assert.NotNull(match.Projections);
        });
    }

    [Theory(DisplayName = "Find JSON projection maps every matching and projection coverage value exactly"),
        MemberData(nameof(CoverageCases)), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEveryCoverageValueExactly(
        string name,
        object resultValue,
        string expectedState,
        string expectedMatching,
        string expectedProjection)
    {
        Assert.False(string.IsNullOrWhiteSpace(name));
        var result = Assert.IsType<FindResult>(resultValue);
        var document = FindJsonProjectionRenderer.Create(result);

        Assert.Equal(expectedState, document.Result.Coverage.State);
        Assert.Equal(expectedMatching, document.Result.Coverage.Matching);
        Assert.Equal(expectedProjection, document.Result.Coverage.Projection);
    }

    [Fact(DisplayName = "Find JSON projection echoes an explicitly malformed content request with not-started projection coverage"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionRetainsMalformedContentPresence()
    {
        var result = FindPresentationTestData.ExplicitMalformedContentResult();
        Assert.True(result.Presentation.Content.IsRequested);
        Assert.Empty(result.Presentation.Content.Supplied);
        Assert.Empty(result.Presentation.Content.Effective);

        var document = FindJsonProjectionRenderer.Create(result);

        Assert.Equal([], document.Result.Presentation.Content.Supplied);
        Assert.Equal([], document.Result.Presentation.Content.Effective);
        Assert.Equal("not-started", document.Result.Coverage.Projection);
    }

    [Fact(DisplayName = "Find JSON projection preserves exact zero counts and empty result arrays"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesZeroMatchCountsAndEmptyArrays()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.ZeroMatchesResult());

        Assert.Equal(0, document.Result.Universe.CandidateCount);
        Assert.Equal(0, document.Result.Universe.InspectedCount);
        Assert.Equal(0, document.Result.Universe.MatchedCount);
        Assert.Empty(document.Result.Universe.Include);
        Assert.Empty(document.Result.Universe.Exclude);
        Assert.Empty(document.Result.Findings);
        Assert.Empty(document.Result.Matches);
    }

    [Fact(DisplayName = "Find JSON projection maps every content part and section name separately from its discriminator"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsContentPartAndSectionName()
    {
        var result = FindPresentationTestData.CompleteResult();

        var document = FindJsonProjectionRenderer.Create(result);

        var projections = Assert.Single(document.Result.Matches).Projections;
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            projections.Select(projection => projection.Part));
        Assert.Equal(
            [null, null, null, null, "Target", null, null, null, "Target"],
            projections.Select(projection => projection.Name));
        Assert.Equal(
            ["available", "available", "available", "available", "available", "available", "available", "available", "available"],
            projections.Select(projection => projection.State));
    }

    [Fact(DisplayName = "Find JSON projection maps every projection state and preserves discriminator-only payload boundaries"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEveryProjectionState()
    {
        foreach (var state in Enum.GetValues<FindProjectionState>())
        {
            var document = FindJsonProjectionRenderer.Create(
                FindPresentationTestData.ProjectionStateResult(state));
            var projection = Assert.Single(Assert.Single(document.Result.Matches).Projections);

            Assert.Equal(state.ToString().ToLowerInvariant(), projection.State);
            Assert.Equal("section", projection.Part);
            Assert.Equal("Target", projection.Name);
            Assert.Equal("base", projection.Layer);
            Assert.Equal(".agents/docs.md", projection.Path);
            if (state == FindProjectionState.Available)
            {
                Assert.Equal("available section", projection.Text);
                Assert.NotNull(projection.Location);
            }
            else
            {
                Assert.Null(projection.Metadata);
                Assert.Null(projection.Text);
                Assert.Empty(projection.Headings);
                Assert.Null(projection.Location);
            }
        }
    }

    [Fact(DisplayName = "Find JSON projection preserves every rich non-selector finding field"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesRichFindingPayload()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.IncompleteResult());

        var finding = Assert.Single(document.Result.Findings);
        Assert.Equal("find.section-ambiguous", finding.Code);
        Assert.Equal("incomplete", finding.Status);
        Assert.Equal("docs", finding.Subject);
        Assert.Equal(
            "Several Target headings prevent one exact section projection.",
            finding.Cause);
        Assert.Null(finding.SelectorRole);
        Assert.Null(finding.SelectorOccurrence);
        Assert.NotNull(finding.Source);
        Assert.Equal("docs", finding.Source!.Id);
        Assert.Equal(".agents/docs.md", finding.Source.Path);
        Assert.Equal("base", finding.Layer);
        Assert.Equal(".agents/docs.md", finding.Path);
        Assert.Equal("section:Target", finding.Region);
        Assert.NotNull(finding.Location);
        Assert.Equal((12, 1, 120L, 10L), (
            finding.Location!.Line,
            finding.Location.Column,
            finding.Location.ByteOffset,
            finding.Location.ByteLength));
        Assert.Empty(finding.Candidates);
    }

    [Fact(DisplayName = "Find JSON projection preserves source locations, authored evidence, metadata layers, and projected headings"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesLocationsEvidenceAndLayers()
    {
        var result = FindPresentationTestData.CompleteResult();
        var typedMatch = Assert.Single(result.Matches);

        var document = FindJsonProjectionRenderer.Create(result);

        var match = Assert.Single(document.Result.Matches);
        Assert.Equal(typedMatch.Path, match.Path);
        Assert.Equal(typedMatch.Evidence.Count, match.Evidence.Length);
        Assert.Equal(["tag", "tag", "heading", "heading"], match.Evidence.Select(evidence => evidence.Kind));
        Assert.Equal(["base", "overwrite", "base", "overwrite"], match.Evidence.Select(evidence => evidence.Layer));
        Assert.All(match.Evidence, evidence =>
        {
            Assert.NotNull(evidence.Location);
            Assert.True(evidence.Location.ByteOffset >= 0);
            Assert.True(evidence.Location.ByteLength >= 0);
        });
        Assert.Equal(["base", "overwrite"], match.Projections[0].Metadata!.Layers.Select(layer => layer.Kind));
        Assert.NotEmpty(match.Projections[2].Headings);
        Assert.NotNull(match.Projections[2].Headings[0].Location);
        Assert.Equal("atx", match.Projections[2].Headings[0].Form);
        Assert.Equal("setext", match.Projections[6].Headings[0].Form);
        Assert.Equal("base body", match.Projections[3].Text);
        Assert.Equal("overwrite body", match.Projections[7].Text);
        Assert.Equal(
            [
                (2, 4, 12L, 8L),
                (2, 4, 14L, 8L),
                (8, 1, 80L, 20L),
                (8, 1, 82L, 20L),
            ],
            match.Evidence.Select(evidence => (
                evidence.Location.Line,
                evidence.Location.Column,
                evidence.Location.ByteOffset,
                evidence.Location.ByteLength)));
        var baseLocation = match.Projections[1].Location
            ?? throw new InvalidOperationException("The base frontmatter projection requires a location.");
        Assert.Equal((5, 1, 40L, 16L), (
            baseLocation.Line,
            baseLocation.Column,
            baseLocation.ByteOffset,
            baseLocation.ByteLength));
        var overwriteLocation = match.Projections[7].Location
            ?? throw new InvalidOperationException("The overwrite body projection requires a location.");
        Assert.Equal((15, 1, 140L, 14L), (
            overwriteLocation.Line,
            overwriteLocation.Column,
            overwriteLocation.ByteOffset,
            overwriteLocation.ByteLength));
    }

    [Fact(DisplayName = "Find JSON projection maps the complete finite selector, query, view, region, layer, heading, and projection vocabulary"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEveryRichFiniteValue()
    {
        var document = FindJsonProjectionRenderer.Create(FindPresentationTestData.CompleteResult());
        var selector = Assert.Single(document.Result.Universe.Include);
        var predicateKinds = document.Result.Query.Predicates.Select(predicate => predicate.Kind).ToArray();
        var match = Assert.Single(document.Result.Matches);
        var metadata = match.Projections[0].Metadata
            ?? throw new InvalidOperationException("The rich JSON fixture requires metadata.");

        Assert.Equal("filtered", document.Result.Universe.Mode);
        Assert.Equal("docs", selector.Value);
        Assert.Equal("id", selector.Form);
        Assert.Equal("resolved", selector.Resolution);
        Assert.Equal("ordinary", selector.SourceKind);
        Assert.Equal("source", selector.Expansion);
        Assert.Empty(selector.Candidates);
        Assert.Equal(["tag", "heading"], predicateKinds);
        Assert.Equal(["frontmatter"], document.Result.Query.Within.Tag);
        Assert.Equal(["body"], document.Result.Query.Within.Heading);
        Assert.Equal("all", document.Result.Query.Require);
        Assert.Equal("expanded", document.Result.Presentation.View.Effective);
        Assert.Equal("expanded", document.Result.Presentation.View.Supplied);
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section:Target"],
            document.Result.Presentation.Content.Effective);
        Assert.Equal(
            ["base", "overwrite"],
            match.Evidence.Select(evidence => evidence.Layer).Distinct().ToArray());
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            match.Projections.Select(projection => projection.Part));
        Assert.Equal("Target", match.Projections[4].Name);
        Assert.Equal("routed", metadata.RouteState);
        Assert.Equal("docs", metadata.Route);
        Assert.Equal(["base", "overwrite"], metadata.Layers.Select(layer => layer.Kind));
    }

    [Fact(DisplayName = "Find JSON projection maps every finite selector, region, view, route, layer, and content value from typed domain facts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsCompleteFiniteVocabulary()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.JsonFiniteValuesResult());

        Assert.Equal("filtered", document.Result.Universe.Mode);
        Assert.Equal(5, document.Result.Universe.CandidateCount);
        Assert.Equal(5, document.Result.Universe.InspectedCount);
        Assert.Equal(2, document.Result.Universe.MatchedCount);
        var selectors = document.Result.Universe.Include;
        Assert.Equal(
            ["resolved", "resolved", "resolved", "resolved", "resolved"],
            selectors.Select(selector => selector.Resolution));
        Assert.Equal(
            ["id", "path", "id", "path", "id"],
            selectors.Select(selector => selector.Form));
        Assert.Equal(
            ["loader", "entrypoint", "skill", "ordinary", "ordinary"],
            selectors.Select(selector => selector.SourceKind));
        Assert.Equal(
            ["folder", "folder", "folder", "source", "source"],
            selectors.Select(selector => selector.Expansion));
        Assert.Equal([0, 0, 0, 0, 0], selectors.Select(selector => selector.Candidates.Length));
        Assert.Equal(
            ["loader", "documents", "skills/demo", "docs", "unrouted"],
            selectors.Select(selector => selector.Identity!.Id));
        Assert.Equal(
            [
                ".agents/loader.md",
                ".agents/documents/_documents.md",
                ".agents/skills/demo/SKILL.md",
                ".agents/docs.md",
                ".agents/unrouted.md",
            ],
            selectors.Select(selector => selector.Identity!.Path));

        var exclude = Assert.Single(document.Result.Universe.Exclude);
        Assert.Equal("guide", exclude.Identity!.Id);
        Assert.Equal(".agents/guide.md", exclude.Identity.Path);
        Assert.Equal("path", exclude.Form);
        Assert.Equal("resolved", exclude.Resolution);
        Assert.Equal("ordinary", exclude.SourceKind);
        Assert.Equal("source", exclude.Expansion);
        Assert.Empty(exclude.Candidates);
        Assert.DoesNotContain(exclude.Identity.Id, selectors.Select(selector => selector.Identity!.Id));

        Assert.Equal(["tag", "heading"], document.Result.Query.Predicates.Select(predicate => predicate.Kind));
        Assert.Equal(["any"], new[] { document.Result.Query.Require });
        Assert.Equal(
            ["document", "frontmatter", "body", "section:Target"],
            document.Result.Query.Within.Supplied);
        Assert.Equal(["document"], document.Result.Query.Within.Tag);
        Assert.Equal(["section:Target"], document.Result.Query.Within.Heading);
        Assert.Equal("compact", document.Result.Presentation.View.Supplied);
        Assert.Equal("compact", document.Result.Presentation.View.Effective);
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section:Target"],
            document.Result.Presentation.Content.Supplied);
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section:Target"],
            document.Result.Presentation.Content.Effective);

        var matches = document.Result.Matches;
        Assert.Equal([1, 2], matches.Select(match => match.Position));
        Assert.Equal(["docs", "unrouted"], matches.Select(match => match.Id));
        Assert.Equal([".agents/docs.md", ".agents/unrouted.md"], matches.Select(match => match.Path));
        Assert.Equal(["routed", "unrouted"], matches.Select(match => match.Projections[0].Metadata!.RouteState));
        Assert.Equal(["docs", null], matches.Select(match => match.Projections[0].Metadata!.Route));
        Assert.Equal(
            [
                ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
                ["metadata", "frontmatter", "headings", "body", "section"],
            ],
            matches.Select(match => match.Projections.Select(projection => projection.Part).ToArray()));
        Assert.Equal(
            ["base", "overwrite"],
            matches[0].Projections[0].Metadata!.Layers.Select(layer => layer.Kind));
        Assert.Equal(["base"], matches[1].Projections[0].Metadata!.Layers.Select(layer => layer.Kind));
    }

    [Theory(DisplayName = "Find JSON projection maps every unresolved selector resolution with exact role, null, finding, and count rules"),
        MemberData(nameof(SelectorResolutionCases)),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEverySelectorResolution(string name, object resultValue)
    {
        var expected = name switch
        {
            "invalid" => (Status: "invalid", Role: "include", Form: "id", Resolution: "invalid", Code: "find.invalid-selector", Candidates: 0),
            "unknown" => (Status: "invalid", Role: "include", Form: "id", Resolution: "unknown", Code: "find.invalid-selector", Candidates: 0),
            "unsupported" => (Status: "invalid", Role: "include", Form: "path", Resolution: "unsupported", Code: "find.invalid-selector", Candidates: 0),
            "ambiguous" => (Status: "blocked", Role: "include", Form: "id", Resolution: "ambiguous", Code: "find.selector-ambiguous", Candidates: 2),
            "unsafe" => (Status: "blocked", Role: "exclude", Form: "path", Resolution: "unsafe", Code: "find.selector-unsafe", Candidates: 0),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "The Find selector resolution case is not defined."),
        };
        var result = Assert.IsType<FindResult>(resultValue);

        var document = FindJsonProjectionRenderer.Create(result);

        Assert.Equal(expected.Status, document.Status);
        var selectors = document.Result.Universe.Include
            .Select(selector => (Role: "include", Selector: selector))
            .Concat(document.Result.Universe.Exclude
                .Select(selector => (Role: "exclude", Selector: selector)))
            .ToArray();
        Assert.Equal([expected.Role], selectors.Select(value => value.Role));
        var selector = Assert.Single(selectors).Selector;
        Assert.Equal(expected.Form, selector.Form);
        Assert.Equal(expected.Resolution, selector.Resolution);
        Assert.Null(selector.Identity);
        Assert.Null(selector.SourceKind);
        Assert.Null(selector.Expansion);
        Assert.Equal(expected.Candidates, selector.Candidates.Length);

        Assert.Null(document.Result.Universe.CandidateCount);
        Assert.Null(document.Result.Universe.InspectedCount);
        Assert.Null(document.Result.Universe.MatchedCount);

        var finding = Assert.Single(document.Result.Findings);
        Assert.Equal(expected.Code, finding.Code);
        Assert.Equal(expected.Status, finding.Status);
        Assert.Equal(expected.Role, finding.SelectorRole);
        Assert.Equal(1, finding.SelectorOccurrence);
        Assert.Null(finding.Source);
        Assert.Null(finding.Layer);
        Assert.Null(finding.Path);
        Assert.Null(finding.Region);
        Assert.Null(finding.Location);
        Assert.Equal(expected.Candidates, finding.Candidates.Length);
    }

    [Fact(DisplayName = "Find JSON projection maps include and exclude finding roles from typed selector facts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsSelectorRoles()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.SelectorRoleResult());

        Assert.Equal(
            ["find.selector-ambiguous", "find.selector-unsafe"],
            document.Result.Findings.Select(finding => finding.Code));
        Assert.Equal(["blocked", "blocked"], document.Result.Findings.Select(finding => finding.Status));
        Assert.Equal(["include", "exclude"], document.Result.Findings.Select(finding => finding.SelectorRole));
        Assert.Equal([1, 1], document.Result.Findings.Select(finding => finding.SelectorOccurrence));
        Assert.Equal([2, 0], document.Result.Findings.Select(finding => finding.Candidates.Length));
    }

    [Fact(DisplayName = "Find selector ambiguity JSON exposes the exact rerun command and reason"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionUsesSelectorAmbiguityNextAction()
    {
        var document = FindJsonProjectionRenderer.Create(
            FindPresentationTestData.SelectorAmbiguousBlockedResult());

        Assert.Equal("blocked", document.Status);
        Assert.NotNull(document.Next);
        Assert.Equal("open-forge find", document.Next!.Command);
        Assert.Equal(
            "Replace every ambiguous selector with one listed exact path, then rerun the same request.",
            document.Next.Reason);
    }

    [Fact(DisplayName = "Find JSON projection maps every finite finding code from typed result facts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonProjectionMapsEveryFindingCode()
    {
        var expected = new Dictionary<FindFindingCode, (string Code, string Status)>
        {
            [FindFindingCode.InvalidInput] = ("find.invalid-input", "invalid"),
            [FindFindingCode.InvalidSelector] = ("find.invalid-selector", "invalid"),
            [FindFindingCode.WorkspaceUnavailable] = ("find.workspace-unavailable", "blocked"),
            [FindFindingCode.WorkspaceUnsafe] = ("find.workspace-unsafe", "blocked"),
            [FindFindingCode.SelectorAmbiguous] = ("find.selector-ambiguous", "blocked"),
            [FindFindingCode.SelectorUnsafe] = ("find.selector-unsafe", "blocked"),
            [FindFindingCode.IdentityCollision] = ("find.identity-collision", "attention"),
            [FindFindingCode.CandidateUnsafe] = ("find.candidate-unsafe", "incomplete"),
            [FindFindingCode.LayerUnresolved] = ("find.layer-unresolved", "incomplete"),
            [FindFindingCode.InspectionUnavailable] = ("find.inspection-unavailable", "incomplete"),
            [FindFindingCode.InvalidEncoding] = ("find.invalid-encoding", "incomplete"),
            [FindFindingCode.FrontmatterUnavailable] = ("find.frontmatter-unavailable", "incomplete"),
            [FindFindingCode.SectionAmbiguous] = ("find.section-ambiguous", "incomplete"),
            [FindFindingCode.ProjectionMissing] = ("find.projection-missing", "attention"),
            [FindFindingCode.ProjectionUnavailable] = ("find.projection-unavailable", "incomplete"),
            [FindFindingCode.OperationFailed] = ("find.operation-failed", "failed"),
            [FindFindingCode.Interrupted] = ("find.interrupted", "interrupted"),
        };

        foreach (var (code, result) in FindPresentationTestData.FindingCodeResults())
        {
            var document = FindJsonProjectionRenderer.Create(result);
            var finding = Assert.Single(
                document.Result.Findings,
                value => value.Code == expected[code].Code);

            Assert.Equal(expected[code].Code, finding.Code);
            Assert.Equal(expected[code].Status, finding.Status);
        }
    }

    public static IEnumerable<object[]> CoverageCases()
        => FindPresentationTestData.CoverageResults()
            .Select(value => new object[]
            {
                value.Name,
                value.Result,
                value.State,
                value.Matching,
                value.Projection,
            });

    public static IEnumerable<object[]> SelectorResolutionCases()
        => FindPresentationTestData.JsonSelectorResolutionResults()
            .Select(value => new object[]
            {
                value.Name,
                value.Result,
            });

    private static void AssertProperties<T>(params string[] names)
        => Assert.Equal(names, typeof(T).GetProperties().Select(property => property.Name));
}

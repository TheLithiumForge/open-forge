using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Rendering;

public sealed class FindHumanRenderingTests
{
    [Theory(DisplayName = "Find human presentation dispatches every semantic status through the typed result"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HumanDispatcherCoversEveryStatus(string statusValue)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var rendered = FindHumanRenderer.Render(
            FindPresentationTestData.PresentationRequest(result, view: CliView.Compact));

        Assert.Equal(ExpectedCompactSummary(statusValue), FirstLine(rendered));
    }

    [Theory(DisplayName = "Find compact renderer emits the exact summary, safe rows, findings, and one status-specific next action"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void CompactRendererCoversEveryStatusAndSafeRows(string statusValue)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var rendered = FindCompactRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Equal(ExpectedCompactSummary(statusValue), FirstLine(rendered));
        if (statusValue == "Blocked")
        {
            Assert.Contains("find.workspace-unavailable", rendered, StringComparison.Ordinal);
        }
        if (result.Matches.Count != 0)
        {
            Assert.Contains("docs\t.agents/docs.md", rendered, StringComparison.Ordinal);
        }

        AssertExpectedNext(rendered, ExpectedCompactNext(statusValue));
    }

    [Theory(DisplayName = "Find expanded renderer emits workspace, query, universe, evidence, projections, and exact next placement for every status"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRendererCoversEveryStatusAndNextPlacement(string statusValue)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Contains("Workspace:", rendered, StringComparison.Ordinal);
        Assert.Contains("Selected by:", rendered, StringComparison.Ordinal);
        Assert.Contains("Filters:", rendered, StringComparison.Ordinal);
        Assert.Contains("Source universe:", rendered, StringComparison.Ordinal);
        Assert.Contains("Coverage:", rendered, StringComparison.Ordinal);
        if (result.Presentation.Content.IsRequested)
        {
            Assert.Contains("Projection coverage:", rendered, StringComparison.Ordinal);
        }
        Assert.StartsWith($"Found {result.Matches.Count} matching ", rendered, StringComparison.Ordinal);
        AssertExactNext(rendered, ExpectedCompactNext(statusValue));
    }

    [Theory(DisplayName = "Find compact output keeps zero-match and safe terminal rows observable"),
        InlineData("zero"),
        InlineData("incomplete"),
        InlineData("failed"),
        InlineData("interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void CompactRendererKeepsZeroAndSafeRows(string scenario)
    {
        var result = scenario switch
        {
            "zero" => FindPresentationTestData.ZeroMatchesResult(),
            "incomplete" => FindPresentationTestData.IncompleteResult(),
            "failed" => FindPresentationTestData.FailedResult(),
            "interrupted" => FindPresentationTestData.InterruptedResult(),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The compact row scenario is not defined."),
        };

        var rendered = FindCompactRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Equal(
            scenario switch
            {
                "zero" => "result=complete\tcoverage=complete\tuniverse=default\tmatches=0",
                "incomplete" => "result=incomplete\tcoverage=incomplete\tprojection=incomplete\tuniverse=default\tmatches=1",
                "failed" => "result=failed\tcoverage=failed\tuniverse=default\tmatches=1",
                "interrupted" => "result=interrupted\tcoverage=interrupted\tuniverse=default\tmatches=1",
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The compact row scenario is not defined."),
            },
            FirstLine(rendered));
        if (scenario == "zero")
        {
            Assert.Equal("No matches.", Assert.Single(Lines(rendered), line => line == "No matches."));
        }

        AssertExpectedNext(
            rendered,
            scenario switch
            {
                "zero" => null,
                "incomplete" => "Next: open-forge doctor",
                "failed" => "Next: open-forge find --verbose",
                "interrupted" => "Next: open-forge find",
                _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The compact row scenario is not defined."),
            });
    }

    [Fact(DisplayName = "Find compact rendering preserves fixed finding order and never emits more than the typed top-level next action"),
     Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void CompactRendererPreservesFindingOrderAndNextAction()
    {
        var result = FindPresentationTestData.OrderedFindingsResult();

        var rendered = FindCompactRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Equal(
            "result=attention\tcoverage=complete\tprojection=complete\tuniverse=default\tmatches=1",
            FirstLine(rendered));
        AssertInOrder(rendered, "find.identity-collision", "find.projection-missing");
        AssertExpectedNext(rendered, null);

        var omitted = FindCompactRenderer.Render(FindPresentationTestData.OmittedContentResult(), CliHumanStyle.Plain);
        Assert.DoesNotContain("find.projection-missing", omitted, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Find compact frontmatter findings name distinct known source paths"),
     Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void CompactFrontmatterFindingsNameDistinctKnownSourcePaths()
    {
        var result = FindPresentationTestData.SourceSpecificFrontmatterFindingsResult();

        var rendered = FindCompactRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Contains(
            "  .agents/first.md",
            rendered,
            StringComparison.Ordinal);
        Assert.Contains(
            "  .agents/second.md",
            rendered,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Subject: none", rendered, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Find expanded query and universe evidence distinguish default and filtered selections"),
        InlineData("default"),
        InlineData("filtered"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRendererCoversDefaultAndFilteredUniverse(string universeKind)
    {
        var result = universeKind == "default"
            ? FindPresentationTestData.ZeroMatchesResult()
            : FindPresentationTestData.CompleteResult();

        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Contains(
            universeKind == "filtered" ? "Mode:       filtered" : "Mode:       default",
            rendered,
            StringComparison.Ordinal);
        Assert.StartsWith($"Found {result.Matches.Count} matching ", rendered, StringComparison.Ordinal);
        Assert.Equal(
            universeKind == "filtered" ? FindUniverseMode.Filtered : FindUniverseMode.Default,
            result.Universe.Mode);
    }

    [Theory(DisplayName = "Find human projections cover every content kind, projection state, and physical layer"),
        InlineData("Available"),
        InlineData("Missing"),
        InlineData("Unavailable"),
        InlineData("Ambiguous"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HumanRendererCoversContentProjectionStatesAndLayers(string stateValue)
    {
        var state = Enum.Parse<FindProjectionState>(stateValue);
        var result = FindPresentationTestData.ProjectionStateResult(state);

        var rendered = FindHumanRenderer.Render(
            FindPresentationTestData.PresentationRequest(result, view: CliView.Expanded));

        var typedProjection = Assert.Single(Assert.Single(result.Matches).Projections);
        var block = ReadProjectionBlock(rendered, "Projection: section:Target");

        Assert.Contains("Projection: section:Target", block, StringComparison.Ordinal);
        Assert.Contains("Layer: base", block, StringComparison.Ordinal);
        Assert.Contains("Path: .agents/docs.md", block, StringComparison.Ordinal);
        Assert.Contains($"State: {stateValue.ToLowerInvariant()}", block, StringComparison.Ordinal);
        Assert.Equal(state, typedProjection.State);

        if (state == FindProjectionState.Available)
        {
            var text = typedProjection.Text
                ?? throw new InvalidOperationException("An available projection requires typed text.");
            var location = typedProjection.Location
                ?? throw new InvalidOperationException("An available projection requires a typed location.");
            Assert.Contains("Text:", block, StringComparison.Ordinal);
            Assert.Contains(text, block, StringComparison.Ordinal);
            Assert.Contains("Location:", block, StringComparison.Ordinal);
            Assert.Contains($"line {location.Line}", block, StringComparison.Ordinal);
        }
        else
        {
            Assert.Null(typedProjection.Metadata);
            Assert.Null(typedProjection.Text);
            Assert.Empty(typedProjection.Headings);
            Assert.Null(typedProjection.Location);
            Assert.DoesNotContain("Metadata:", block, StringComparison.Ordinal);
            Assert.DoesNotContain("Text:", block, StringComparison.Ordinal);
            Assert.DoesNotContain("Headings:", block, StringComparison.Ordinal);
            Assert.DoesNotContain("Location:", block, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Find human rendering distinguishes omitted content from explicitly malformed content"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HumanRendererDistinguishesContentPresenceStates()
    {
        var omitted = FindPresentationTestData.OmittedContentResult();
        var malformed = FindPresentationTestData.ExplicitMalformedContentResult();

        var omittedText = FindHumanRenderer.Render(
            FindPresentationTestData.PresentationRequest(omitted, view: CliView.Compact));
        var malformedText = FindHumanRenderer.Render(
            FindPresentationTestData.PresentationRequest(malformed, view: CliView.Compact));

        Assert.False(omitted.Presentation.Content.IsRequested);
        Assert.True(malformed.Presentation.Content.IsRequested);
        Assert.Equal(
            "result=complete\tcoverage=complete\tuniverse=default\tmatches=1",
            FirstLine(omittedText));
        Assert.Equal(
            "result=invalid\tcoverage=not-started\tprojection=not-started\tuniverse=default\tmatches=0",
            FirstLine(malformedText));
        Assert.DoesNotContain("projection=", omittedText, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Find expanded next-line wording is exact, status-specific, and absent for complete and attention"),
        InlineData("Complete", ""),
        InlineData("Attention", ""),
        InlineData("Incomplete", "Next: open-forge doctor"),
        InlineData("Invalid", "Next: open-forge find --help"),
        InlineData("Blocked", "Next: open-forge doctor"),
        InlineData("Failed", "Next: open-forge find --verbose"),
        InlineData("Interrupted", "Next: open-forge find"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRendererUsesExactNextText(string statusValue, string expectedNext)
    {
        var result = FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));

        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);

        AssertExpectedNext(rendered, expectedNext.Length == 0 ? null : expectedNext);
    }

    [Fact(DisplayName = "Find human escaping preserves the complete authored heading and tag spelling beyond 512 characters"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HumanEscapingPreservesLongAuthoredValues()
    {
        var result = FindPresentationTestData.CompleteResult();
        var tag = result.Query.EffectivePredicates[0].SuppliedValue;
        var heading = result.Query.EffectivePredicates[1].SuppliedValue;

        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Contains(tag, rendered, StringComparison.Ordinal);
        Assert.Contains(EscapeHumanValue(heading), rendered, StringComparison.Ordinal);
        Assert.True(tag.Length > 512);
        Assert.True(heading.Length > 512);
    }

    [Fact(DisplayName = "Find human projection blocks retain canonical order, layer boundaries, typed text, headings, and locations"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HumanProjectionBlocksRetainTypedOrderAndPayloadBoundaries()
    {
        var result = FindPresentationTestData.CompleteResult();
        var match = Assert.Single(result.Matches);

        Assert.Equal(
            [
                FindContentPartKind.Metadata,
                FindContentPartKind.Frontmatter,
                FindContentPartKind.Headings,
                FindContentPartKind.Body,
                FindContentPartKind.Section,
                FindContentPartKind.Frontmatter,
                FindContentPartKind.Headings,
                FindContentPartKind.Body,
                FindContentPartKind.Section,
            ],
            match.Projections.Select(projection => projection.Part));
        Assert.Equal(
            [SourceLayerKind.Base, SourceLayerKind.Base, SourceLayerKind.Base, SourceLayerKind.Base,
                SourceLayerKind.Overwrite, SourceLayerKind.Overwrite, SourceLayerKind.Overwrite, SourceLayerKind.Overwrite],
            match.Projections.Skip(1).Select(projection => projection.Layer));
        Assert.NotNull(match.Projections[0].Metadata);
        Assert.NotNull(match.Projections[2].Headings.Single().Location);
        Assert.NotNull(match.Projections[3].Location);
        Assert.NotNull(match.Projections[5].Location);

        var rendered = FindHumanRenderer.Render(
            FindPresentationTestData.PresentationRequest(result, view: CliView.Expanded));

        Assert.Equal(
            [
                "Projection: metadata|Layer: none|Path: none",
                "Projection: frontmatter|Layer: base|Path: .agents/docs.md",
                "Projection: headings|Layer: base|Path: .agents/docs.md",
                "Projection: body|Layer: base|Path: .agents/docs.md",
                "Projection: section:Target|Layer: base|Path: .agents/docs.md",
                "Projection: frontmatter|Layer: overwrite|Path: .agents/docs.overwrite.md",
                "Projection: headings|Layer: overwrite|Path: .agents/docs.overwrite.md",
                "Projection: body|Layer: overwrite|Path: .agents/docs.overwrite.md",
                "Projection: section:Target|Layer: overwrite|Path: .agents/docs.overwrite.md",
            ],
            ReadProjectionSignatures(rendered));
        AssertInOrder(
            rendered,
            "Projection: metadata",
            "Layer: none",
            "Path: none",
            "State: available",
            "Metadata:",
            "base frontmatter",
            "Projection: headings",
            "Projected heading",
            "base body",
            "Location:",
            "base section",
            "overwrite frontmatter",
            "overwrite body",
            "overwrite section");

        AssertProjectionBlock(
            rendered,
            "Projection: metadata",
            "Layer: none",
            "Path: none",
            "State: available",
            "Route state: routed",
            "Route: docs",
            ".agents/docs.overwrite.md");
        AssertProjectionBlock(
            rendered,
            "Projection: frontmatter",
            "Layer: base",
            "Path: .agents/docs.md",
            "State: available",
            "Location:",
            "base frontmatter");
        AssertProjectionBlock(
            rendered,
            "Projection: headings",
            "Layer: base",
            "Path: .agents/docs.md",
            "State: available",
            "Projected heading",
            "line 8");
        AssertProjectionBlock(
            rendered,
            "Projection: body",
            "Layer: base",
            "Path: .agents/docs.md",
            "State: available",
            "Location:",
            "base body");
        AssertProjectionBlock(
            rendered,
            "Projection: section:Target",
            "Layer: base",
            "Path: .agents/docs.md",
            "State: available",
            "Location:",
            "base section");
        AssertProjectionBlockAt(
            rendered,
            "Projection: frontmatter",
            2,
            "Layer: overwrite",
            "Path: .agents/docs.overwrite.md",
            "State: available",
            "Location:",
            "overwrite frontmatter");
        AssertProjectionBlockAt(
            rendered,
            "Projection: headings",
            2,
            "Layer: overwrite",
            "Path: .agents/docs.overwrite.md",
            "State: available",
            "Projected heading",
            "line 18");
        AssertProjectionBlockAt(
            rendered,
            "Projection: body",
            2,
            "Layer: overwrite",
            "Path: .agents/docs.overwrite.md",
            "State: available",
            "Location:",
            "overwrite body");
        AssertProjectionBlockAt(
            rendered,
            "Projection: section:Target",
            2,
            "Layer: overwrite",
            "Path: .agents/docs.overwrite.md",
            "State: available",
            "Location:",
            "overwrite section");
    }

    [Fact(DisplayName = "Find expanded rendering preserves exact rich query, source-universe, evidence, and projection facts"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRenderingPreservesExactRichFacts()
    {
        var result = FindPresentationTestData.CompleteResult();
        var match = Assert.Single(result.Matches);
        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);

        var tag = result.Query.EffectivePredicates[0].SuppliedValue;
        var heading = result.Query.EffectivePredicates[1].SuppliedValue;
        var description = match.Description
            ?? throw new InvalidOperationException("The rich Find fixture requires a description.");

        Assert.Contains("Selected by: --workspace", rendered, StringComparison.Ordinal);
        Assert.Contains($"Tag:     {tag}", rendered, StringComparison.Ordinal);
        Assert.Contains($"Heading: {EscapeHumanValue(heading)}", rendered, StringComparison.Ordinal);
        Assert.Contains("Require: all", rendered, StringComparison.Ordinal);
        Assert.Contains("Tag:     frontmatter", rendered, StringComparison.Ordinal);
        Assert.Contains("Heading: body", rendered, StringComparison.Ordinal);
        Assert.Contains("Mode:       filtered", rendered, StringComparison.Ordinal);
        Assert.Contains("Include:    docs", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("Exclude:    omitted", rendered, StringComparison.Ordinal);
        Assert.Contains("Inspected: 1 of 1 candidates", rendered, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", rendered, StringComparison.Ordinal);
        Assert.Contains("Projection coverage: complete", rendered, StringComparison.Ordinal);
        Assert.Contains("Found 1 matching source.", rendered, StringComparison.Ordinal);
        Assert.Contains("Path: .agents/docs.md", rendered, StringComparison.Ordinal);
        Assert.Contains($"Description: {EscapeHumanValue(description)}", rendered, StringComparison.Ordinal);
        Assert.Contains("Target", rendered, StringComparison.Ordinal);

        Assert.Equal(
            [FindPredicateKind.Tag, FindPredicateKind.Tag, FindPredicateKind.Heading, FindPredicateKind.Heading],
            match.Evidence.Select(evidence => evidence.Kind));
        Assert.Equal(
            [FindRegionKind.Frontmatter, FindRegionKind.Frontmatter, FindRegionKind.Body, FindRegionKind.Body],
            match.Evidence.Select(evidence => evidence.Region.Kind));
        Assert.Equal(
            [SourceLayerKind.Base, SourceLayerKind.Overwrite, SourceLayerKind.Base, SourceLayerKind.Overwrite],
            match.Evidence.Select(evidence => evidence.Layer));
        Assert.Equal([2, 2, 8, 8], match.Evidence.Select(evidence => evidence.Location.Line));
        Assert.Equal(
            [
                ".agents/docs.md",
                ".agents/docs.overwrite.md",
                ".agents/docs.md",
                ".agents/docs.overwrite.md",
            ],
            match.Evidence.Select(evidence => evidence.Path));
        Assert.Equal([1, 1, 1, 1], match.Evidence.Select(evidence => evidence.Occurrence));

        Assert.Equal(
            [
                $"{EscapeHumanValue(match.Evidence[0].Authored)} — frontmatter, base",
                $"{EscapeHumanValue(match.Evidence[1].Authored)} — frontmatter, overwrite",
                $"{EscapeHumanValue(match.Evidence[2].Authored)} — heading, base, line 8",
                $"{EscapeHumanValue(match.Evidence[3].Authored)} — heading, overwrite, line 8",
            ],
            ReadEvidenceLines(rendered));

        AssertInOrder(
            rendered,
            "Found 1 matching source.",
            "Coverage: complete",
            "Projection coverage: complete",
            "Matched:",
            "Filters:",
            "Source universe:",
            "Projection: metadata",
            "Projection: frontmatter",
            "Projection: headings",
            "Projection: body",
            "Projection: section:Target",
            "overwrite frontmatter");
    }

    [Fact(DisplayName = "Find expanded rendering places the exact required Next line after match explanation and before projection blocks"),
     Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRenderingPlacesNextBetweenMatchAndProjectionBlocks()
    {
        var rendered = FindExpandedRenderer.Render(
            FindPresentationTestData.IncompleteResult(), CliHumanStyle.Plain);

        var lines = Lines(rendered);
        var matchPath = IndexOfLine(lines, line => line.Contains("Path: .agents/docs.md", StringComparison.Ordinal));
        var matchDescription = IndexOfLine(lines, line => line.Contains("Description:", StringComparison.Ordinal));
        var matched = IndexOfLine(lines, line => line.Trim() == "Matched:");
        var next = IndexOfLine(lines, line => line == "Next: open-forge doctor");
        var projection = IndexOfLine(lines, line => line.Contains("Projection: section:Target", StringComparison.Ordinal));

        Assert.True(matchPath < matchDescription, "The match path must precede its description.");
        Assert.True(matchDescription < matched, "The match explanation must precede evidence.");
        Assert.True(matched < next, "The exact Next line must follow the complete match explanation.");
        Assert.True(next < projection, "The exact Next line must precede selected projection blocks.");
        AssertExactNext(rendered, "Next: open-forge doctor");
    }

    [Fact(DisplayName = "Find selector ambiguity emits the exact alternate compact next action"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void CompactRenderingUsesSelectorAmbiguityNextAction()
    {
        var result = FindPresentationTestData.SelectorAmbiguousBlockedResult();

        var rendered = FindCompactRenderer.Render(result, CliHumanStyle.Plain);

        Assert.Equal(
            "result=blocked\tcoverage=blocked\tuniverse=filtered\tmatches=0",
            FirstLine(rendered));
        AssertExactNext(
             rendered,
             "Next: open-forge find");
    }

    [Fact(DisplayName = "Find expanded rendering places the selector-ambiguity next action after its finding and match boundary"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void ExpandedRenderingUsesSelectorAmbiguityNextAction()
    {
        var result = FindPresentationTestData.SelectorAmbiguousBlockedResult();

        var rendered = FindExpandedRenderer.Render(result, CliHumanStyle.Plain);
        var lines = Lines(rendered);
        const string expectedNext = "Next: open-forge find";
        var finding = IndexOfLine(
            lines,
            line => line.Contains("find.selector-ambiguous", StringComparison.Ordinal));
        var matches = IndexOfLine(lines, line => line.Trim() == "Found 0 matching sources.");
        var next = IndexOfLine(lines, line => line == expectedNext);

        Assert.Equal(1, lines.Count(line => line == expectedNext));
        Assert.True(Math.Max(finding, matches) < next);
        AssertExactNext(rendered, expectedNext);
    }

    private static string ExpectedCompactSummary(string statusValue)
        => statusValue switch
        {
            "Complete" => "result=complete\tcoverage=complete\tprojection=complete\tuniverse=filtered\tmatches=1",
            "Attention" => "result=attention\tcoverage=complete\tprojection=complete\tuniverse=filtered\tmatches=1",
            "Incomplete" => "result=incomplete\tcoverage=incomplete\tprojection=incomplete\tuniverse=default\tmatches=1",
            "Invalid" => "result=invalid\tcoverage=not-started\tprojection=not-started\tuniverse=default\tmatches=0",
            "Blocked" => "result=blocked\tcoverage=blocked\tuniverse=filtered\tmatches=0",
            "Failed" => "result=failed\tcoverage=failed\tuniverse=default\tmatches=1",
            "Interrupted" => "result=interrupted\tcoverage=interrupted\tuniverse=default\tmatches=1",
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The Find status is not defined."),
        };

    private static string? ExpectedCompactNext(string statusValue)
        => statusValue switch
        {
            "Complete" or "Attention" => null,
            "Incomplete" => "Next: open-forge doctor",
            "Invalid" => "Next: open-forge find --help",
            "Blocked" => "Next: open-forge doctor",
            "Failed" => "Next: open-forge find --verbose",
            "Interrupted" => "Next: open-forge find",
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The Find status is not defined."),
        };

    private static string FirstLine(string rendered)
        => rendered.Split('\n', StringSplitOptions.None)[0].TrimEnd('\r');

    private static void AssertExpectedNext(string rendered, string? expected)
        => AssertExactNext(rendered, expected);

    private static void AssertExactNext(string rendered, string? expected)
    {
        string[] expectedLines = expected is null ? [] : [expected];
        Assert.Equal(
            expectedLines,
            Lines(rendered).Where(line => line.StartsWith("Next:", StringComparison.Ordinal)));
    }

    private static void AssertInOrder(string text, params string[] values)
    {
        var previous = -1;
        foreach (var value in values)
        {
            var current = text.IndexOf(value, previous + 1, StringComparison.Ordinal);
            Assert.True(current >= 0, $"Expected '{value}' in rendered output.");
            Assert.True(current > previous, $"Expected '{value}' after the preceding projection value.");
            previous = current;
        }
    }

    private static void AssertProjectionBlock(
        string rendered,
        string projection,
        params string[] requiredValues)
    {
        var block = ReadProjectionBlock(rendered, projection);
        foreach (var value in requiredValues)
        {
            Assert.Contains(value, block, StringComparison.Ordinal);
        }
    }

    private static void AssertProjectionBlockAt(
        string rendered,
        string projection,
        int occurrence,
        params string[] requiredValues)
    {
        var block = ReadProjectionBlock(rendered, projection, occurrence);
        foreach (var value in requiredValues)
        {
            Assert.Contains(value, block, StringComparison.Ordinal);
        }
    }

    private static string ReadProjectionBlock(
        string rendered,
        string projection,
        int occurrence = 1)
    {
        var start = -projection.Length;
        for (var index = 0; index < occurrence; index++)
        {
            start = rendered.IndexOf(
                projection,
                start + projection.Length,
                StringComparison.Ordinal);
        }

        Assert.True(start >= 0, $"Expected projection block '{projection}'.");
        var nextStart = rendered.IndexOf("Projection:", start + projection.Length, StringComparison.Ordinal);
        return nextStart >= 0
            ? rendered[start..nextStart]
            : rendered[start..];
    }

    private static List<string> ReadProjectionSignatures(string rendered)
    {
        var lines = Lines(rendered);
        var starts = lines
            .Select((line, index) => (Line: line.TrimStart(), Index: index))
            .Where(value => value.Line.StartsWith("Projection:", StringComparison.Ordinal))
            .ToArray();
        var signatures = new List<string>(starts.Length);
        for (var index = 0; index < starts.Length; index++)
        {
            var end = index + 1 < starts.Length ? starts[index + 1].Index : lines.Length;
            var block = lines[starts[index].Index..end].Select(line => line.Trim()).ToArray();
            var layer = Assert.Single(block, line => line.StartsWith("Layer:", StringComparison.Ordinal));
            var path = Assert.Single(block, line => line.StartsWith("Path:", StringComparison.Ordinal));
            signatures.Add($"{starts[index].Line}|{layer}|{path}");
        }

        return signatures;
    }

    private static IReadOnlyList<string> ReadEvidenceLines(string rendered)
    {
        var lines = Lines(rendered);
        var matched = IndexOfLine(lines, line => line.Trim() == "Matched:");
        var projection = IndexOfLine(
            lines,
            (line, index) => index > matched && line == "Search details:");
        return [.. lines[(matched + 1)..projection]
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())];
    }

    private static string[] Lines(string rendered)
        => rendered.Split(Environment.NewLine, StringSplitOptions.None);

    private static int IndexOfLine(
        string[] lines,
        Func<string, bool> predicate)
    {
        for (var index = 0; index < lines.Length; index++)
        {
            if (predicate(lines[index]))
            {
                return index;
            }
        }

        Assert.Fail("The expected human-rendering line was not found.");
        return -1;
    }

    private static int IndexOfLine(
        string[] lines,
        Func<string, int, bool> predicate)
    {
        for (var index = 0; index < lines.Length; index++)
        {
            if (predicate(lines[index], index))
            {
                return index;
            }
        }

        Assert.Fail("The expected human-rendering line was not found.");
        return -1;
    }

    private static string EscapeHumanValue(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\t", "\\u0009", StringComparison.Ordinal);
}

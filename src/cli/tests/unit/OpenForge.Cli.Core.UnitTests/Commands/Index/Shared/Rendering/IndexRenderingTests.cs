using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Presentation.Index.Models;
using OpenForge.Cli.Core.Presentation.Index.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Rendering;

public sealed class IndexRenderingTests
{
    [Trait("Boundary", "Output")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void HumanDetailsRetainExplicitWorkspaceAndPartialEffects(bool standard)
    {
        var detail = standard ? CliDetail.Standard : CliDetail.Minimal;
        var current = IndexTestData.Result();
        var currentText = Text(current, detail);
        Assert.Contains(current.Workspace!.LexicalRoot, currentText, StringComparison.Ordinal);
        Assert.StartsWith("The Entries section is current in 1 file. Nothing to do.\n", currentText);
        var failed = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Unknown)],
            findings: [IndexTestData.Finding(IndexFindingCode.WriteFailed)],
            recovery: new IndexRecovery(IndexRecoveryState.Unknown, null));
        var before = Json(failed);
        var text = Text(failed, detail);
        Assert.Contains(".agents/memory/_memory.md  final state unknown", text);
        Assert.DoesNotContain("Nothing was changed.", text);
        Assert.DoesNotContain("Nothing was written.", text);
        Assert.Equal(before, Json(failed));
    }

    [Trait("Boundary", "Output")]
    [Theory, InlineData("new\n"), InlineData("new\r\n"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DryRunPreservesFinalExpectedNewlineTokens(string expected)
    {
        var result = IndexTestData.Result(regions: [IndexTestData.Update(beforeBody: "old\r\ntail", expectedBody: expected)], mode: IndexMode.DryRun);
        var selected = Select(result, CliDetail.Standard);
        var document = CliTextRenderer.Render(selected, CliTextStyle.Plain, IndexPresentation.Rendering.DataTextRenderer);
        Assert.Equal(new[] { "- old\r\n", "- tail", "+ " + expected },
            document.Spans.Where(span => span.Authored).Select(span => span.Content));
        Assert.Contains("No files were changed.\n", document.Content);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void UnparseablePriorEntriesRemainUnknown()
    {
        var result = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Verified, beforeEntryCount: null)],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, null));
        var text = Text(result);
        Assert.Contains("unknown -> 1 entries", text);
        Assert.DoesNotContain("0 -> 1 entries", text);
        using var json = JsonDocument.Parse(Json(result));
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").GetProperty("changes")[0].GetProperty("before").ValueKind);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FullJsonRetainsTypedSelectionRegionsAndRecovery()
    {
        var result = IndexTestData.Result(regions: [IndexTestData.Update()], mode: IndexMode.DryRun);
        using var json = JsonDocument.Parse(Json(result, CliDetail.Full));
        var root = json.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("index", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var data = root.GetProperty("data");
        Assert.Equal("dry-run", data.GetProperty("mode").GetString());
        Assert.Equal("explicit-sources", data.GetProperty("selection").GetProperty("origin").GetString());
        Assert.Equal("rooted", data.GetProperty("selection").GetProperty("scope").GetString());
        Assert.Equal("update", data.GetProperty("regions")[0].GetProperty("action").GetString());
        Assert.Equal("not-requested", data.GetProperty("regions")[0].GetProperty("outcome").GetString());
        Assert.Equal("not-required", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void JsonSupportsEverySemanticStatus()
    {
        var results = new[]
        {
            IndexTestData.Result(),
            IndexTestData.Result(regions: [IndexTestData.Update(IndexRegionOutcome.Unknown)],
                findings: [IndexTestData.Finding(IndexFindingCode.WriteFailed)], recovery: new IndexRecovery(IndexRecoveryState.Unknown, null)),
            IndexTestData.Result(regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
                findings: [IndexTestData.Finding(IndexFindingCode.RecoveryArtifactRetained)],
                recovery: new IndexRecovery(IndexRecoveryState.Retained, IndexTestData.RecoveryPath("retained.zip"))),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.DiscoveryIncomplete)]),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.InvalidInput)]),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.TargetUnsafe)]),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.Interrupted)]),
        };
        var expected = new[] { "completed", "failed", "completed-with-warnings", "incomplete", "invalid-input", "blocked", "cancelled" };
        for (var index = 0; index < results.Length; index++)
        {
            using var json = JsonDocument.Parse(Json(results[index]));
            Assert.Equal(expected[index], json.RootElement.GetProperty("status").GetString());
            Assert.Equal("index", json.RootElement.GetProperty("command").GetString());
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index output distinguishes optional metadata warnings from skipped partial sources")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void OptionalAndSkippedMetadataRenderTruthfulWarnings()
    {
        var optionalSource = IndexTestData.Source("root/child", ".agents/root/child.md");
        var optional = IndexTestData.Result(
            findings:
            [
                new IndexFinding(
                    IndexFindingCode.MetadataOptional,
                    sourceOccurrence: null,
                    source: optionalSource,
                    cause: "Optional authored metadata is missing.",
                    candidates: []),
            ]);
        var optionalText = Text(optional);

        Assert.Contains("The Entries section is current", optionalText, StringComparison.Ordinal);
        Assert.Contains("Optional metadata is missing for .agents/root/child.md; observed values were used.", optionalText, StringComparison.Ordinal);
        Assert.DoesNotContain("open-forge cleanup", optionalText, StringComparison.Ordinal);

        var safe = IndexTestData.Source("alpha", ".agents/alpha/_alpha.md");
        var skipped = IndexTestData.Source("beta", ".agents/beta/_beta.md");
        var partial = IndexTestData.Result(
            regions:
            [
                IndexRegion.Update(
                    safe,
                    new IndexRegionUpdate
                    {
                        BeforeEntryCount = 1,
                        ExpectedEntryCount = 1,
                        Change = new IndexChange("old\n", "new\n"),
                        Outcome = IndexRegionOutcome.Verified,
                    }),
                IndexRegion.NotEstablished(skipped),
            ],
            findings:
            [
                new IndexFinding(
                    IndexFindingCode.MetadataSkipped,
                    sourceOccurrence: null,
                    source: skipped,
                    cause: "Malformed authored metadata was skipped.",
                    candidates: [])
                {
                    Details = new IndexFindingDetails
                    {
                        ParentPath = skipped.Path,
                        MetadataProblem = IndexMetadataProblem.Invalid,
                    },
                },
            ],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, null));
        var partialText = Text(partial);

        Assert.Contains("Updated the Entries section in 1 of 1 file; other sources were skipped.", partialText, StringComparison.Ordinal);
        Assert.Contains("Skipped .agents/beta/_beta.md because its authored frontmatter metadata is malformed", partialText, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void HelpUsesGlobalFlagVocabulary()
    {
        var help = IndexHelpSections.Create();
        Assert.Contains(help.Sections, section => section.Heading == "Syntax" && section.Body.Contains("open-forge index", StringComparison.Ordinal));
        var options = Assert.Single(help.Sections, section => section.Heading == "Global options").Body;
        Assert.Contains("--detail", options);
        Assert.Contains("--format", options);
        Assert.DoesNotContain("--view", options);
        Assert.DoesNotContain("--verbose", options);
    }

    private static CliSelectedReport<IndexData> Select(IndexResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = IndexPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }
    private static string Text(IndexResult result, CliDetail detail = CliDetail.Standard)
        => CliTextRenderer.Render(Select(result, detail), CliTextStyle.Plain, IndexPresentation.Rendering.DataTextRenderer).Content;
    private static string Json(IndexResult result, CliDetail detail = CliDetail.Standard)
        => CliJsonRenderer.Render(Select(result, detail), IndexPresentation.Rendering.DataJsonTypeInfo);
}

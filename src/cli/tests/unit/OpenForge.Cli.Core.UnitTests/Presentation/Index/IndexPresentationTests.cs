using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Presentation.Index.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Index;

public sealed class IndexPresentationTests
{
    private static readonly IndexLogicalSource Source = new("memory", ".agents/memory/_memory.md", IndexLogicalSourceScope.Rooted);

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void FolderHeadlineAnswersTheMinimalFindingAndFullAndJsonRetainItsIdentity()
    {
        var result = Result([], findings:
        [
            new IndexFinding(IndexFindingCode.InvalidSource, 1, null, "The operand denotes a folder.", [])
            {
                Details = new IndexFindingDetails { Operand = ".agents/memory", InputProblem = IndexInputProblem.Folder, CorrectedSourceId = "memory" },
            },
        ]);
        var minimal = Select(result, CliDetail.Minimal);
        Assert.Equal("Cannot index .agents/memory: it is a folder, not a source.\nNext: open-forge index memory\n", Text(minimal));
        Assert.Contains("[index.invalid-source]", Text(Select(result, CliDetail.Full)), StringComparison.Ordinal);
        using var json = JsonDocument.Parse(Json(minimal));
        Assert.Equal("index.invalid-source", Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("errors").GetInt32());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void MinimalCurrentPreviewIsExactlyOneLine()
    {
        var selected = Select(Result([IndexRegion.Unchanged(Source, 2, 2)], IndexMode.DryRun), CliDetail.Minimal);
        Assert.Equal("The Entries section is current in 1 file. Nothing to do.\n", Text(selected));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void MinimalCurrentResultIsExactlyOneLineAndJsonRetainsWorkspace()
    {
        var selected = Select(Result([IndexRegion.Unchanged(Source, 2, 2)]), CliDetail.Minimal);
        Assert.Equal("The Entries section is current in 1 file. Nothing to do.\n", Text(selected));
        using var json = JsonDocument.Parse(Json(selected));
        Assert.Equal(3, json.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
        Assert.Equal("current-directory", json.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("filesChecked").GetInt32());
        Assert.False(json.RootElement.GetProperty("data").TryGetProperty("unchanged", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void FullInvalidInputDoesNotPrintAnEmptySelectionLine()
    {
        var result = Result([], findings:
        [
            new IndexFinding(IndexFindingCode.InvalidSource, 1, null, "The source ID does not identify a retained logical source.", [])
            {
                Details = new IndexFindingDetails
                {
                    Operand = "missing",
                    InputProblem = IndexInputProblem.UnknownId,
                },
            },
        ]);

        var text = Text(Select(result, CliDetail.Full));

        Assert.DoesNotContain("Selection: \n", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void FilesCheckedAlwaysEqualsUpdatedPlusCurrent()
    {
        var currentSource = new IndexLogicalSource("patterns", ".agents/patterns/_patterns.md", IndexLogicalSourceScope.Rooted);
        var results = new[]
        {
            Result([IndexRegion.Unchanged(Source, 1, 1)]),
            Result([Change(IndexRegionOutcome.Verified)], recovery: new IndexRecovery(IndexRecoveryState.Removed, null)),
            Result([Change(IndexRegionOutcome.NotRequested)], IndexMode.DryRun),
            Result(
                [Change(IndexRegionOutcome.Unknown), IndexRegion.Unchanged(currentSource, 1, 1)],
                findings: [new IndexFinding(IndexFindingCode.WriteFailed, null, Source, "The write failed.", [])],
                recovery: new IndexRecovery(IndexRecoveryState.Unknown, null)),
            Result(
                [IndexRegion.NotEstablished(Source), IndexRegion.Unchanged(currentSource, 1, 1)],
                findings: [new IndexFinding(IndexFindingCode.MetadataIncomplete, null, Source, "Metadata was unavailable.", [])]),
            Result(
                [Change(IndexRegionOutcome.NotStarted)],
                findings: [new IndexFinding(IndexFindingCode.Interrupted, null, Source, "Index was cancelled.", [])]),
        };

        foreach (var result in results)
        {
            var counts = Select(result, CliDetail.Minimal).Report.Counts
                .ToDictionary(count => count.Name, count => count.Value);

            Assert.Equal(counts["filesUpdated"] + counts["filesCurrent"], counts["filesChecked"]);
        }
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void UpdatedReceiptAppearsOnceAndMatchesJsonEffectIdentity(int detailValue)
    {
        var detail = (CliDetail)detailValue;
        var result = Result([Change(IndexRegionOutcome.Verified)], recovery: new IndexRecovery(IndexRecoveryState.Removed, null));
        var selected = Select(result, detail);
        if (detail == CliDetail.Debug) Assert.Contains("status=completed", selected.Report.Diagnostics);
        else Assert.Empty(selected.Report.Diagnostics);
        var text = Text(selected);
        Assert.StartsWith("Updated the Entries section in 1 of 1 file.\n", text);
        Assert.Equal(1, text.Split("0 -> 1 entries", StringSplitOptions.None).Length - 1);
        using var json = JsonDocument.Parse(Json(selected));
        Assert.Equal(Source.Path, json.RootElement.GetProperty("effects")[0].GetProperty("path").GetString());
        Assert.Equal(Source.Path, json.RootElement.GetProperty("data").GetProperty("changes")[0].GetProperty("path").GetString());
        var counts = json.RootElement.GetProperty("counts");
        Assert.Equal(counts.GetProperty("filesChecked").GetInt32(),
            counts.GetProperty("filesUpdated").GetInt32() + counts.GetProperty("filesCurrent").GetInt32());
        Assert.Equal(detail >= CliDetail.Standard, json.RootElement.GetProperty("data").TryGetProperty("unchanged", out _));
        Assert.Equal(detail >= CliDetail.Full, json.RootElement.GetProperty("data").TryGetProperty("regions", out _));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void DryRunUsesExactCatalogueAndPreservesAuthoredDiffBytes()
    {
        var result = Result([Change(IndexRegionOutcome.NotRequested, "\r\n- old \"é😀\"\r\n\r\n", "- new\n")], IndexMode.DryRun);
        var selected = Select(result, CliDetail.Standard);
        var document = CliTextRenderer.Render(selected, CliTextStyle.Color, IndexPresentation.Rendering.DataTextRenderer);
        Assert.StartsWith("Would update the Entries section in 1 of 1 file.\n", document.Content);
        Assert.Contains("No files were changed.\n", document.Content);
        Assert.Contains("--- .agents/memory/_memory.md  (Entries section)\n", document.Content);
        var authored = document.Spans.Where(span => span.Authored).Select(span => span.Content).ToArray();
        Assert.Equal(new[] { "- - old \"é😀\"\r\n", "+ - new\n" }, authored);
        Assert.DoesNotContain(authored, line => line.Contains("\u001b", StringComparison.Ordinal));
        using var json = JsonDocument.Parse(Json(selected));
        Assert.Equal(authored, json.RootElement.GetProperty("data").GetProperty("changes")[0].GetProperty("diff")
            .EnumerateArray().Select(line => line.GetString()).ToArray());
    }

    [Trait("Boundary", "Output")]
    [Theory, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AllStalePreviewStylesAlignedSubjectPathsAndPreservesPlainAuthoredSpans(int detailValue)
    {
        var regions = new[] { "a", "aa" }.Select(id => IndexRegion.Update(
            new IndexLogicalSource(id, $".agents/{id}.md", IndexLogicalSourceScope.Rooted),
            new IndexRegionUpdate
            {
                BeforeEntryCount = 0,
                ExpectedEntryCount = 1,
                Change = new IndexChange("", "- new\r\n"),
                Outcome = IndexRegionOutcome.NotRequested,
            })).ToArray();
        var detail = (CliDetail)detailValue;
        var selected = Select(Result(regions, IndexMode.DryRun), detail);
        var colored = CliTextRenderer.Render(selected, CliTextStyle.Color, IndexPresentation.Rendering.DataTextRenderer);
        var plain = CliTextRenderer.Render(selected, CliTextStyle.Plain, IndexPresentation.Rendering.DataTextRenderer);
        Assert.Contains("  \u001b[1m.agents/a.md\u001b[22m   0 -> 1 entries\n"
            + "  \u001b[1m.agents/aa.md\u001b[22m  0 -> 1 entries\n", colored.Content, StringComparison.Ordinal);
        Assert.Contains("  .agents/a.md   0 -> 1 entries\n  .agents/aa.md  0 -> 1 entries\n", plain.Content, StringComparison.Ordinal);
        Assert.Equal(plain.Content, colored.Content.Replace("\u001b[1m", "", StringComparison.Ordinal)
            .Replace("\u001b[22m", "", StringComparison.Ordinal));
        Assert.EndsWith("No files were changed.\n", plain.Content, StringComparison.Ordinal);
        Assert.Equal(1, plain.Content.Split("No files were changed.", StringSplitOptions.None).Length - 1);
        if (detail == CliDetail.Standard)
        {
            Assert.EndsWith("+ - new\r\nNo files were changed.\n", plain.Content, StringComparison.Ordinal);
        }
        else if (detail >= CliDetail.Full)
        {
            Assert.EndsWith("Selection: .agents/a.md, .agents/aa.md\n"
                + "No recovery bundle was needed.\nNo files were changed.\n", plain.Content, StringComparison.Ordinal);
        }

        var authored = colored.Spans.Where(span => span.Authored).Select(span => span.Content).ToArray();
        Assert.Equal(detail >= CliDetail.Standard ? new[] { "+ - new\r\n", "+ - new\r\n" } : [], authored);
        Assert.Equal(plain.Spans.Where(span => span.Authored).Select(span => span.Content), authored);
        Assert.All(authored, span => Assert.DoesNotContain("\u001b", span, StringComparison.Ordinal));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void PartialUnknownIsListedOnceAndNeverCountsAsCurrent()
    {
        var unknown = Change(IndexRegionOutcome.Unknown);
        var currentSource = new IndexLogicalSource("patterns", ".agents/patterns/_patterns.md", IndexLogicalSourceScope.Rooted);
        var result = Result([unknown, IndexRegion.Unchanged(currentSource, 1, 1)],
            findings: [new IndexFinding(IndexFindingCode.WriteFailed, null, Source, "The write failed.", [])],
            recovery: new IndexRecovery(IndexRecoveryState.Unknown, null));
        var selected = Select(result, CliDetail.Minimal);
        var text = Text(selected);
        Assert.StartsWith("Index stopped after 0 of 2 files.\n", text);
        Assert.Equal(1, text.Split("final state unknown", StringSplitOptions.None).Length - 1);
        Assert.DoesNotContain("0 -> 1 entries", text, StringComparison.Ordinal);
        using var json = JsonDocument.Parse(Json(selected));
        var counts = json.RootElement.GetProperty("counts");
        Assert.Equal(1, counts.GetProperty("filesChecked").GetInt32());
        Assert.Equal(0, counts.GetProperty("filesUpdated").GetInt32());
        Assert.Equal(1, counts.GetProperty("filesCurrent").GetInt32());
        Assert.Equal("unknown", json.RootElement.GetProperty("effects")[0].GetProperty("outcome").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void BlockedHeadlineNamesParentAndFindingNamesMalformedLeaf()
    {
        var child = new IndexLogicalSource("skills/pdf", ".agents/skills/pdf/SKILL.md", IndexLogicalSourceScope.Rooted);
        var parent = new IndexLogicalSource("skills", ".agents/skills/_skills.md", IndexLogicalSourceScope.Rooted);
        var current = Enumerable.Range(0, 19).Select(index => IndexRegion.Unchanged(
            new IndexLogicalSource($"current{index}", $".agents/current{index}/_current{index}.md", IndexLogicalSourceScope.Rooted), 1, 1));
        var result = Result(new[] { IndexRegion.NotEstablished(parent) }.Concat(current).ToArray(), findings:
        [
            new IndexFinding(IndexFindingCode.MetadataUnsafe, null, child, "The frontmatter block is not closed.", [])
            {
                Details = new IndexFindingDetails { ParentPath = parent.Path, MetadataProblem = IndexMetadataProblem.UnclosedFrontmatter, Line = 1, Column = 1 },
            },
        ]);
        var text = Text(Select(result, CliDetail.Minimal));
        Assert.Equal("Cannot rebuild the Entries section of .agents/skills/_skills.md.\n"
            + $"Workspace: {result.WorkspacePath}\n"
            + "  Error  .agents/skills/pdf/SKILL.md:1:1  Frontmatter is invalid\n"
            + "         The frontmatter block is not closed.\n"
            + "  Nothing was written. The other 19 sections are current.\n"
            + "Next: open-forge doctor\n", text);
        using var json = JsonDocument.Parse(Json(Select(result, CliDetail.Full)));
        Assert.Empty(json.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Contains(json.RootElement.GetProperty("data").GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("path").GetString() == parent.Path && region.GetProperty("outcome").GetString() == "not-established");
    }

    private static IndexRegion Change(IndexRegionOutcome outcome, string before = "", string after = "- new\n")
        => IndexRegion.Update(Source, new IndexRegionUpdate
        {
            BeforeEntryCount = 0,
            ExpectedEntryCount = 1,
            Change = new IndexChange(before, after),
            Outcome = outcome,
        });

    private static IndexResult Result(IReadOnlyList<IndexRegion> regions, IndexMode mode = IndexMode.Apply,
        IReadOnlyList<IndexFinding>? findings = null, IndexRecovery? recovery = null)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "index-presentation-evidence"));
        return new IndexResult(new IndexResultFormation
        {
            Workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory),
            Mode = mode,
            Selection = regions.Count == 0 ? IndexSelection.NotEstablished(IndexSelectionOrigin.ExplicitSources)
                : new IndexSelection(IndexSelectionOrigin.ExplicitSources, IndexSelectionScope.Rooted, regions.Select(region => region.Source)),
            Regions = regions,
            Findings = findings ?? [],
            Recovery = recovery ?? IndexRecovery.NotRequired,
        });
    }

    private static CliSelectedReport<IndexData> Select(IndexResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = IndexPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }

    private static string Text(CliSelectedReport<IndexData> selected)
        => CliTextRenderer.Render(selected, CliTextStyle.Plain, IndexPresentation.Rendering.DataTextRenderer).Content;
    private static string Json(CliSelectedReport<IndexData> selected)
        => CliJsonRenderer.Render(selected, IndexPresentation.Rendering.DataJsonTypeInfo);
}

using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
public sealed class IndexCompletionPresentationTests
{
    [Theory, InlineData(false), InlineData(true)]
    public void NavigationRenameIsOneHumanRowWithTruthfulFileEffects(bool preview)
    {
        var source = IndexTestData.Source();
        var outcome = preview ? IndexRegionOutcome.NotRequested : IndexRegionOutcome.Verified;
        var result = new IndexResult(new()
        {
            Workspace = IndexTestData.Workspace(),
            Mode = preview ? IndexMode.DryRun : IndexMode.Apply,
            Selection = IndexTestData.Selection(source),
            Regions = [IndexRegion.Unchanged(source, 1, 1)],
            Findings = [],
            Recovery = IndexRecovery.NotRequired,
            Entrypoints = [new(".agents/guidance/invoices/_invoices.md", outcome)],
            Deletions = [new(".agents/guidance/invoices/_billing.md", outcome)],
        });
        var selection = new CliSelection(CliDetail.Minimal, null);
        var report = IndexPresentation.Rendering.Selector(result, selection);
        Assert.Equal(preview ? "Would rename 1 navigation file." : "Renamed 1 navigation file.", report.Headline.Sentence);
        Assert.Equal(0, Assert.Single(report.Counts, count => count.Name == "entrypointsCreated").Value);
        Assert.Equal(1, Assert.Single(report.Counts, count => count.Name == "navigationFilesRenamed").Value);
        var selected = IndexPresentation.Rendering.SelectText!(CliReportTrimmer.Trim(report, selection, IndexPresentation.Rendering.Shape));
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, IndexPresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains(preview ? "would be renamed from _billing.md, your rules would be kept" : "renamed from _billing.md, your rules were kept", text);
        Assert.DoesNotContain("entrypoint created", text);
        Assert.DoesNotContain("deleted", text);
        Assert.Equal(2, report.Effects.Count);
    }

    [Theory(DisplayName = "Index joins canonical renames with independent creates and metadata without double counting")]
    [InlineData(false, 1, 1, 1, "Created 1 entrypoint and renamed 1 navigation file and completed metadata in 1 file.")]
    [InlineData(true, 1, 1, 1, "Would create 1 entrypoint and rename 1 navigation file and complete metadata in 1 file.")]
    [InlineData(false, 2, 0, 0, "Renamed 2 navigation files.")]
    [InlineData(true, 2, 0, 0, "Would rename 2 navigation files.")]
    public void MixedCompletion(bool preview, int renamed, int created, int metadata, string expected)
    {
        var source = IndexTestData.Source();
        var outcome = preview ? IndexRegionOutcome.NotRequested : IndexRegionOutcome.Verified;
        var result = new IndexResult(new()
        {
            Workspace = IndexTestData.Workspace(),
            Mode = preview ? IndexMode.DryRun : IndexMode.Apply,
            Selection = IndexTestData.Selection(source),
            Regions = [IndexRegion.Unchanged(source, 1, 1)],
            Findings = [],
            Recovery = !preview && metadata > 0 ? new(IndexRecoveryState.Removed, null) : IndexRecovery.NotRequired,
            Entrypoints = [.. Enumerable.Range(0, renamed).Select(index => new IndexEntrypointCompletion($".agents/guidance/new-{index}/_new-{index}.md", outcome)),
                .. Enumerable.Range(0, created).Select(index => new IndexEntrypointCompletion($".agents/guidance/created-{index}/_created-{index}.md", outcome))],
            Deletions = [.. Enumerable.Range(0, renamed).Select(index => new IndexFileDeletion($".agents/guidance/new-{index}/_old-{index}.md", outcome))],
            Metadata = [.. Enumerable.Range(0, metadata).Select(index => new IndexMetadataCompletion($".agents/guidance/note-{index}.md", ["tags"], null, ["Guidance"], outcome))],
        });
        var report = IndexPresentation.Rendering.Selector(result, new(CliDetail.Full));
        Assert.Equal(expected, report.Headline.Sentence);
        Assert.Equal(created, Assert.Single(report.Counts, count => count.Name == "entrypointsCreated").Value);
        Assert.Equal(renamed, Assert.Single(report.Counts, count => count.Name == "navigationFilesRenamed").Value);
        var keys = report.Counts.Select(count => count.Name).ToArray();
        Assert.Equal("filesSkipped", keys[^2]);
        Assert.Equal("navigationFilesRenamed", keys[^1]);
    }

    [Theory(DisplayName = "Index completion arrays and counts are additive at every detail with honest zero creates")]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void EveryDetail(int detail, bool preview)
    {
        var source = IndexTestData.Source();
        var result = new IndexResult(new()
        {
            Workspace = IndexTestData.Workspace(),
            Mode = preview ? IndexMode.DryRun : IndexMode.Apply,
            Selection = IndexTestData.Selection(source),
            Regions = [IndexRegion.Unchanged(source, 1, 1)],
            Findings = [],
            Recovery = preview ? IndexRecovery.NotRequired : new(IndexRecoveryState.Removed, null),
            Metadata = [new(".agents/memory/note.md", ["tags"], null, ["Memory"], preview ? IndexRegionOutcome.NotRequested : IndexRegionOutcome.Verified)],
        });
        var rendering = IndexPresentation.Rendering;
        var selection = new CliSelection((CliDetail)detail, null);
        var report = rendering.SelectText?.Invoke(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape))
            ?? throw new InvalidOperationException();
        using var json = JsonDocument.Parse(CliJsonRenderer.Render(report, rendering.DataJsonTypeInfo));
        Assert.Equal(3, json.RootElement.GetProperty("schemaVersion").GetInt32());
        var metadata = Assert.Single(json.RootElement.GetProperty("data").GetProperty("metadata").EnumerateArray());
        Assert.Equal("tags", metadata.GetProperty("fields")[0].GetString());
        Assert.Equal(JsonValueKind.Null, metadata.GetProperty("description").ValueKind);
        Assert.Equal(preview ? "planned" : "done", metadata.GetProperty("outcome").GetString());
        Assert.Empty(json.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray());
        Assert.Empty(json.RootElement.GetProperty("data").GetProperty("changes").EnumerateArray());
        var counts = json.RootElement.GetProperty("counts");
        Assert.Equal(1, counts.GetProperty("metadataCompleted").GetInt32());
        Assert.Equal(0, counts.GetProperty("entrypointsCreated").GetInt32());
        Assert.Equal(0, counts.GetProperty("filesSkipped").GetInt32());
        Assert.Equal(1, counts.GetProperty("filesChecked").GetInt32());
        var text = CliTextRenderer.Render(report, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
        Assert.Contains(preview ? "Would complete metadata in 1 file." : "Completed metadata in 1 file.", text, StringComparison.Ordinal);
        Assert.Contains(preview ? "metadata would be completed, your content would be kept" : "metadata completed, your content was kept", text, StringComparison.Ordinal);
        if (preview) Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Malformed-only Index admits an incomplete no-write result without independent updates")]
    public void MalformedOnlyIsIncomplete()
    {
        var source = IndexTestData.Source();
        var result = IndexTestData.Result([IndexRegion.Unchanged(source, 0, 0)],
            [new IndexFinding(IndexFindingCode.MetadataSkipped, null, source, "The frontmatter block is not closed.", [])]);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var selection = new CliSelection(CliDetail.Standard, null);
        var report = IndexPresentation.Rendering.Selector(result, selection);
        Assert.Equal($"Fix the frontmatter in .agents/memory/_memory.md, then run open-forge index --workspace {CliCommandText.Quote(IndexTestData.Workspace().LexicalRoot)}.", report.Next?.Command);
        Assert.Equal(CliSeverity.Error, Assert.Single(report.Findings).Severity);
    }
}

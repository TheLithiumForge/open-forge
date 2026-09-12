using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Presentation;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Rendering;

public sealed class IndexRenderingTests
{
    [Fact(DisplayName = "Index dry-run diff retains every LF CRLF empty and final-newline token without truncation"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DiffRetainsExactBodyTokens()
    {
        Assert.Equal([string.Empty], IndexDiffRenderer.Tokenize(string.Empty));
        Assert.Equal(["one\r\n", "two\n", "tail"], IndexDiffRenderer.Tokenize("one\r\ntwo\ntail"));
        var region = IndexTestData.Update(
            beforeBody: "one\r\ntail",
            expectedBody: "new\r\n");

        var rendered = IndexDiffRenderer.Render(region);

        Assert.Equal(
            "@@ {\"id\":\"memory\",\"path\":\".agents/memory/_memory.md\",\"scope\":\"rooted\"} @@\n"
            + "- one\r\n"
            + "- tail\n"
            + "+ new\r\n",
            rendered);
    }

    [Fact(DisplayName = "Index human dry-run output preserves the final expected LF and CRLF body tokens"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void HumanDryRunPreservesFinalExpectedNewlineTokens()
    {
        foreach (var expectedBody in new[] { "new\n", "new\r\n" })
        {
            var region = IndexTestData.Update(
                beforeBody: "old\r\n",
                expectedBody: expectedBody);
            var result = IndexTestData.Result(
                regions: [region],
                mode: IndexMode.DryRun);

            var rendered = IndexHumanRenderer.Render(IndexTestData.Presentation(result));

            var exactDiff = IndexDiffRenderer.Render(region);
            Assert.EndsWith(expectedBody, exactDiff, StringComparison.Ordinal);
            Assert.Contains(exactDiff + Environment.NewLine + "No files changed (--dry-run).", rendered, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Index human output never claims no effects when failed typed regions show applied or unknown outcomes"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void FailedOutputReportsObservedEffectsTruthfully()
    {
        var result = IndexTestData.Result(
            regions: [IndexTestData.Update(IndexRegionOutcome.Unknown)],
            findings: [IndexTestData.Finding(IndexFindingCode.WriteFailed)],
            recovery: new IndexRecovery(IndexRecoveryState.Unknown, null));

        var rendered = IndexHumanRenderer.Render(IndexTestData.Presentation(result));

        Assert.Contains("1 files may have changed", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("No files changed.", rendered, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Index human output reports an unparseable prior generated-entry count as unknown"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void UpdatedRegionWithUnparseablePriorEntriesRendersUnknownCount()
    {
        var result = IndexTestData.Result(
            regions: [IndexTestData.Update(
                outcome: IndexRegionOutcome.Verified,
                beforeEntryCount: null)],
            recovery: new IndexRecovery(IndexRecoveryState.Removed, residualPath: null));

        var rendered = IndexHumanRenderer.Render(IndexTestData.Presentation(result));

        Assert.Contains("unknown -> 1 entries (verified)", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(": 0 -> 1 entries", rendered, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Index JSON projection preserves exact command-local shape finite values ordering and null presence"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void JsonProjectionPreservesExactTypedShape()
    {
        var result = IndexTestData.Result(
            regions: [IndexTestData.Update()],
            mode: IndexMode.DryRun);

        var document = IndexJsonProjection.Create(result);

        Assert.Equal((1, "index", "complete"), (document.SchemaVersion, document.Command, document.Status));
        Assert.Equal("dry-run", document.Result.Mode);
        Assert.Equal("explicit-sources", document.Result.Selection.Origin);
        Assert.Equal("rooted", document.Result.Selection.Scope);
        var region = Assert.Single(document.Result.Regions);
        Assert.Equal(("update", "not-requested"), (region.Action, region.Outcome));
        var change = Assert.IsType<IndexJsonChange>(region.Change);
        Assert.Equal(("old\n", "new\n"), (change.BeforeBody, change.ExpectedBody));
        Assert.Equal("not-required", document.Result.Recovery.State);
        Assert.Null(document.Result.Recovery.ResidualPath);
        Assert.Empty(document.Result.Findings);
        Assert.Equal((1, 1, 0, 0, 0), (
            document.Result.Counts.Regions,
            document.Result.Counts.Updates,
            document.Result.Counts.Unchanged,
            document.Result.Counts.Applied,
            document.Result.Counts.Verified));
        Assert.Null(document.Next);
    }

    [Fact(DisplayName = "Index JSON renderer emits one complete document for every typed semantic status"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void JsonRendererSupportsEveryTypedStatus()
    {
        var retainedPath = IndexTestData.RecoveryPath("retained.zip");
        var results = new[]
        {
            IndexTestData.Result(),
            IndexTestData.Result(
                regions: [IndexTestData.Update(IndexRegionOutcome.Verified)],
                findings: [IndexTestData.Finding(IndexFindingCode.RecoveryArtifactRetained)],
                recovery: new IndexRecovery(IndexRecoveryState.Retained, retainedPath)),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.DiscoveryIncomplete)]),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.InvalidInput)]),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.TargetUnsafe)]),
            IndexTestData.Result(
                regions: [IndexTestData.Update(IndexRegionOutcome.Unknown)],
                findings: [IndexTestData.Finding(IndexFindingCode.WriteFailed)],
                recovery: new IndexRecovery(IndexRecoveryState.Unknown, residualPath: null)),
            IndexTestData.Result(findings: [IndexTestData.Finding(IndexFindingCode.Interrupted)]),
        };

        Assert.Equal(
            Enum.GetValues<CliSemanticStatus>(),
            results.Select(result => result.Status).OrderBy(status => status));
        foreach (var result in results)
        {
            var presentation = CliPresentationStage.Create(
                result,
                new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal));

            using var document = JsonDocument.Parse(IndexJsonRenderer.Render(presentation));

            Assert.Equal(
                CliStatusDefinitions.Read(result.Status).MachineName,
                document.RootElement.GetProperty("status").GetString());
            Assert.Equal("index", document.RootElement.GetProperty("command").GetString());
        }
    }

    [Fact(DisplayName = "Index help and diagnostics retain exact public spellings and bounded streams guidance"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void HelpAndDiagnosticsUseExactPublicVocabulary()
    {
        var help = IndexHelpSections.Create();
        var diagnostics = IndexDiagnosticRenderer.Render(
            IndexTestData.Presentation(
                IndexTestData.Result(mode: IndexMode.DryRun),
                verbosity: CliVerbosity.Verbose));

        Assert.Contains(help.Sections, section => section.Heading == "Syntax" && section.Body.Contains("open-forge index", StringComparison.Ordinal));
        Assert.Contains("status=complete", diagnostics, StringComparison.Ordinal);
        Assert.Contains("mode=dry-run", diagnostics, StringComparison.Ordinal);
        Assert.Contains("recovery=not-required", diagnostics, StringComparison.Ordinal);
        var globalOptions = Assert.Single(help.Sections, section => section.Heading == "Global options");
        Assert.Contains("--view <compact|expanded>", globalOptions.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("--view=<compact|expanded>", globalOptions.Body, StringComparison.Ordinal);
    }
}

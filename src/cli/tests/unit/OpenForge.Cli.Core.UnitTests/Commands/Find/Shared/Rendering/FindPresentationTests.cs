using System.Text.Json;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Find;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Rendering;

public sealed class FindPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find minimal text is only the aligned ID and path rows"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void MinimalTextContainsOnlyRows()
    {
        var output = Render(CurrentDirectory(FindPresentationTestData.OmittedContentResult()), CliDetail.Minimal);

        Assert.Equal("  docs  .agents/docs.md\n", output.PrimaryContent);
        Assert.DoesNotContain("result=", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Workspace:", output.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find standard text adds the headline and description column"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void StandardTextAddsHeadlineAndDescription()
    {
        var output = Render(FindPresentationTestData.AnyResult(), CliDetail.Standard);

        Assert.Contains("1 source matches --tag ", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("--heading ", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("docs  .agents/docs.md", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("A description with a tab\\t", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("result=", output.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find full text places evidence and search details after the rows"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void FullTextAddsEvidenceAndSearchDetails()
    {
        var output = Render(FindPresentationTestData.CompleteResult(), CliDetail.Full);

        var row = output.PrimaryContent.IndexOf("docs  .agents/docs.md", StringComparison.Ordinal);
        var evidence = output.PrimaryContent.IndexOf("matched tag", StringComparison.Ordinal);
        var details = output.PrimaryContent.IndexOf("Search details:", StringComparison.Ordinal);
        Assert.True(row >= 0);
        Assert.True(evidence > row);
        Assert.True(details > evidence);
        Assert.Contains("=== .agents/docs.md (docs) ===", output.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains("=== .agents/docs.overwrite.md (docs, overwrite) ===", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("result=", output.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find warning text places the blank separator between findings and rows"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void WarningPrecedesRowsWithBlankLine()
    {
        var output = Render(FindPresentationTestData.AttentionResult(), CliDetail.Standard);

        var finding = output.PrimaryContent.IndexOf("Source identity collides", StringComparison.Ordinal);
        var row = output.PrimaryContent.IndexOf("docs  .agents/docs.md", StringComparison.Ordinal);
        Assert.True(finding >= 0);
        Assert.True(row > finding);
        Assert.Contains("\n\n  docs", output.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find invalid input uses the catalogue headline and stderr target"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void InvalidInputUsesCatalogueHeadline()
    {
        var output = Render(FindPresentationTestData.InvalidResult(), CliDetail.Minimal);

        Assert.Equal(CliOutputTarget.StandardError, output.PrimaryTarget);
        Assert.Contains("Cannot search:", output.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", output.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find JSON exposes only selected data with the native match shape"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void JsonUsesNativeDataShapeByDetail()
    {
        using var minimal = JsonDocument.Parse(Render(FindPresentationTestData.OmittedContentResult(), CliDetail.Minimal, CliFormat.Json).PrimaryContent);
        using var standard = JsonDocument.Parse(Render(FindPresentationTestData.AnyResult(), CliDetail.Standard, CliFormat.Json).PrimaryContent);
        using var full = JsonDocument.Parse(Render(FindPresentationTestData.CompleteResult(), CliDetail.Full, CliFormat.Json).PrimaryContent);

        var minimalData = minimal.RootElement.GetProperty("data");
        Assert.Equal("docs", minimalData.GetProperty("matches")[0].GetProperty("id").GetString());
        Assert.False(minimalData.TryGetProperty("query", out _));
        Assert.False(minimalData.TryGetProperty("sourceSet", out _));
        Assert.False(minimalData.GetProperty("matches")[0].TryGetProperty("evidence", out _));
        Assert.False(minimalData.GetProperty("matches")[0].TryGetProperty("parts", out _));

        var standardData = standard.RootElement.GetProperty("data");
        Assert.True(standardData.TryGetProperty("query", out _));
        Assert.True(standardData.GetProperty("matches")[0].TryGetProperty("evidence", out _));
        Assert.False(standardData.TryGetProperty("sourceSet", out _));

        var fullData = full.RootElement.GetProperty("data");
        Assert.True(fullData.TryGetProperty("sourceSet", out _));
        Assert.True(fullData.GetProperty("matches")[0].TryGetProperty("parts", out _));
        Assert.False(fullData.GetRawText().Contains("result=", StringComparison.Ordinal));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find debug diagnostics remain bounded and do not include authored payload"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void DebugDiagnosticsAreBounded()
    {
        var output = Render(
            FindPresentationTestData.HostileResult(),
            CliDetail.Minimal,
            CliFormat.Text,
            CliDetail.Debug);

        Assert.NotNull(output.DiagnosticContent);
        Assert.InRange(output.DiagnosticContent!.Length, 1, 4096);
        Assert.Contains("status=incomplete", output.DiagnosticContent, StringComparison.Ordinal);
        Assert.Contains("findings=1", output.DiagnosticContent, StringComparison.Ordinal);
        Assert.DoesNotContain("hostile source subject must stay private", output.DiagnosticContent, StringComparison.Ordinal);
        Assert.DoesNotContain("base frontmatter", output.DiagnosticContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Find help keeps the catalogue sections and shared result stream wording"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HelpUsesNativeFindSections()
    {
        var help = FindPresentation.CreateHelp();

        Assert.Equal(
            [
                "Syntax",
                "Source references",
                "Predicates and regions",
                "Content and views",
                "Inherited global options",
                "Results and streams",
                "Examples",
                "Related commands",
                "Notes",
            ],
            help.Sections.Select(section => section.Heading));
        var results = Assert.Single(help.Sections, section => section.Heading == "Results and streams").Body;
        Assert.Contains("completed-with-warnings", results, StringComparison.Ordinal);
        Assert.Contains("130", results, StringComparison.Ordinal);
        Assert.Contains("stderr", results, StringComparison.OrdinalIgnoreCase);
    }

    private static CliRenderedOutput Render(
        FindResult result,
        CliDetail detail,
        CliFormat format = CliFormat.Text,
        CliDetail? diagnosticDetail = null)
        => CliRenderingStage.Render(
            FindPresentationTestData.PresentationRequest(result, format, detail, diagnosticDetail),
            FindPresentation.Rendering);

    private static FindResult CurrentDirectory(FindResult result)
    {
        var workspace = result.Workspace ?? throw new InvalidOperationException("The fixture must carry a workspace.");
        return new FindResult(
            result.Status,
            new CliWorkspace(workspace.LexicalRoot, workspace.PhysicalRoot, CliWorkspaceSelectionMethod.CurrentDirectory),
            result.Universe,
            result.Query,
            result.Presentation,
            result.Coverage,
            result.Findings,
            result.Matches,
            result.Next);
    }
}

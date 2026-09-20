using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

public sealed class ReferencesApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed References help retains exact grammar and a read-only success disposition"),
     Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task LeafHelpRetainsExactGrammarAndNoWriteDisposition()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["references", "--help"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge references", result.Output, StringComparison.Ordinal);
        Assert.Contains("<source-reference>", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "References debug JSON preserves command data and bounds each diagnostic value"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task VerboseJsonPreservesPrimaryDocument()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        string[] arguments = ["references", "docs", "--direction=out", "--format", "json"];
        var plain = await CliHostCapture.RunAsync(arguments, workspace.Path);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--detail", "debug"], workspace.Path);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);

        // The detail level applies to JSON as well as text (C7), so the two documents are not
        // identical: debug adds what the level selects and keeps everything minimal already
        // answered. The requested rows themselves are never shortened by a level.
        using var plainDocument = JsonDocument.Parse(plain.Output);
        using var verboseDocument = JsonDocument.Parse(verbose.Output);
        var plainData = plainDocument.RootElement.GetProperty("data");
        var verboseData = verboseDocument.RootElement.GetProperty("data");
        Assert.Equal(["source", "direction", "outgoing"], plainData.EnumerateObject().Select(member => member.Name));
        Assert.Equal(["source", "direction", "outgoing", "coverage"], verboseData.EnumerateObject().Select(member => member.Name));
        Assert.Equal(
            plainData.GetProperty("source").GetRawText(),
            verboseData.GetProperty("source").GetRawText());
        Assert.Equal(
            plainData.GetProperty("outgoing").GetArrayLength(),
            verboseData.GetProperty("outgoing").GetArrayLength());
        var plainRow = plainData.GetProperty("outgoing").EnumerateArray().First();
        var verboseRow = verboseData.GetProperty("outgoing").EnumerateArray().First();
        Assert.Equal(["location", "destination", "resolvedPath", "state"], plainRow.EnumerateObject().Select(member => member.Name));
        Assert.Equal(["location", "destination", "resolvedPath", "state", "layer"], verboseRow.EnumerateObject().Select(member => member.Name));
        Assert.All(
            new[] { "location", "destination", "resolvedPath", "state" },
            member => Assert.Equal(plainRow.GetProperty(member).GetRawText(), verboseRow.GetProperty(member).GetRawText()));
        Assert.Equal(string.Empty, plain.Error);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        var diagnostics = verbose.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, diagnostics.Length);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.InRange(diagnostic.Length, 1, 240);
            Assert.DoesNotContain('\n', diagnostic);
            Assert.DoesNotContain('\r', diagnostic);
        });
        Assert.Equal("status=completed", diagnostics[0]);
        Assert.Contains("incoming=0", diagnostics);
        Assert.Contains("outgoing=3", diagnostics);
        Assert.Contains("scanned=0", diagnostics);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed public root help exposes one direct References leaf and names it once"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task RootHelpExposesOneReferencesLeaf()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(["--help"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var referencesLines = result.Output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.TrimStart().StartsWith("references", StringComparison.Ordinal))
            .ToArray();
        var referenceLine = Assert.Single(referencesLines);
        Assert.Contains("references", referenceLine, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "References composed application uses the real workspace and default both expanded result"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task BareInvocationUsesDefaultBothExpandedResult()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["references", "docs"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.StartsWith(".agents/docs.md", result.Output, StringComparison.Ordinal);
        Assert.Contains("  in   .agents/alpha.md:3:1", result.Output, StringComparison.Ordinal);
        Assert.Contains("  out  :3:1   target.md#overview", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Direction:", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Inspected:", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "References composed application honors exact in and out directions and filters only incoming work"), Trait("Feature", "references"), Trait("Evidence", "Integration"),
     InlineData("in"),
     InlineData("out")]
    public static async Task ExplicitDirectionControlsEvaluatedSections(string direction)
    {
        using var workspace = CreateWorkspace();
        string[] arguments = direction == "in"
            ? ["references", "docs", "--direction=in", "--include", "alpha", "--exclude=beta"]
            : ["references", "docs", "--direction=out"];
        var result = await CliHostCapture.RunAsync([.. arguments, "--detail", "standard"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        if (direction == "in")
        {
            Assert.Contains("1 in, 0 out", result.Output, StringComparison.Ordinal);
            Assert.Contains("  in   .agents/alpha.md:3:1", result.Output, StringComparison.Ordinal);
            Assert.DoesNotContain("  out  ", result.Output, StringComparison.Ordinal);
            Assert.Contains(
                "The incoming scan used only alpha and skipped beta.",
                result.Output,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("0 in, 3 out", result.Output, StringComparison.Ordinal);
            Assert.Contains("  out  :3:1   target.md#overview", result.Output, StringComparison.Ordinal);
            Assert.DoesNotContain("  in   ", result.Output, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "References composed application preserves supplied filter occurrence order and duplicates in JSON"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task JsonPreservesOrderedDuplicateSelectors()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            [
                "references", "docs", "--direction=in", "--format", "json", "--detail", "standard",
                "--include", "alpha", "--exclude=beta", "--include:alpha", "--exclude", "beta",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var filters = document.RootElement.GetProperty("data").GetProperty("filters");
        Assert.Equal(
            ["alpha", "alpha"],
            filters.GetProperty("include").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            ["beta", "beta"],
            filters.GetProperty("exclude").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            [".agents/alpha.md"],
            document.RootElement.GetProperty("data").GetProperty("incoming").EnumerateArray()
                .Select(occurrence => occurrence.GetProperty("path").GetString()));
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "References composed application reports typed semantic invalidity and blocked workspace without domain writes"),
     Trait("Feature", "references"), Trait("Evidence", "Integration"),
     InlineData("invalid-direction", 4, "invalid-input", "references.invalid-direction"),
     InlineData("filter-with-out", 4, "invalid-input", "references.invalid-filter"),
     InlineData("blocked-workspace", 5, "blocked", "references.workspace-unavailable")]
    public static async Task TypedInvalidAndBlockedJourneysRemainDistinct(
        string scenario,
        int expectedExit,
        string expectedStatus,
        string expectedFinding)
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing-workspace");
        string[] arguments = scenario switch
        {
            "invalid-direction" => ["references", "docs", "--format", "json", "--direction=incoming"],
            "filter-with-out" => ["references", "docs", "--format", "json", "--direction=out", "--include=alpha"],
            "blocked-workspace" => ["references", "docs", "--format", "json", "--workspace", missing],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The References integration scenario is not defined."),
        };
        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        if (scenario == "blocked-workspace")
        {
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
            Assert.False(Directory.Exists(missing));
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "References compact and expanded human projections preserve one typed occurrence set"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task HumanViewsPreserveTypedOccurrenceParity()
    {
        using var workspace = CreateWorkspace();
        var compact = await CliHostCapture.RunAsync(
            ["references", "docs", "--detail=minimal"],
            workspace.Path);
        var expanded = await CliHostCapture.RunAsync(
            ["references", "docs", "--detail=standard"],
            workspace.Path);

        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.Error, expanded.Error);
        Assert.Contains("target.md#overview", compact.Output, StringComparison.Ordinal);
        Assert.Contains("target.md#overview", expanded.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("->", compact.Output, StringComparison.Ordinal);
        Assert.Contains("-> .agents/target.md", expanded.Output, StringComparison.Ordinal);
        Assert.Contains("2 in, 3 out", expanded.Output, StringComparison.Ordinal);
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("references-application");
        workspace.WriteText(".agents/loader.md", "# Loader\n");
        workspace.WriteText(
            ".agents/docs.md",
            "# Docs\n\n[alpha](target.md#overview)\n[external](https://example.invalid/x)\n");
        workspace.WriteText(
            ".agents/docs.overwrite.md",
            "# Docs Override\n\n[duplicate](target.md#overview)\n");
        workspace.WriteText(".agents/target.md", "# Target\n\n## Overview\n");
        workspace.WriteText(".agents/alpha.md", "# Alpha\n\n[docs](docs.md)\n");
        workspace.WriteText(".agents/beta.md", "# Beta\n\n[docs](docs.md)\n");
        return workspace;
    }
}

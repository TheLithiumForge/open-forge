using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

public sealed class ReferencesApplicationIntegrationTests
{
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
        Assert.Single(referencesLines);
        Assert.Contains("references", referencesLines[0], StringComparison.Ordinal);
    }

    [Fact(DisplayName = "References composed application uses the real workspace and default both expanded result"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task BareInvocationUsesDefaultBothExpandedResult()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["references", "docs"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("Workspace:", result.Output, StringComparison.Ordinal);
        Assert.Contains("Direction: both", result.Output, StringComparison.Ordinal);
        Assert.Contains("Incoming", result.Output, StringComparison.Ordinal);
        Assert.Contains("Outgoing", result.Output, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "References composed application honors exact in and out directions and filters only incoming work"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    [InlineData("in")]
    [InlineData("out")]
    public async Task ExplicitDirectionControlsEvaluatedSections(string direction)
    {
        using var workspace = CreateWorkspace();
        string[] arguments = direction == "in"
            ? ["references", "docs", "--direction=in", "--include", "alpha", "--exclude=beta"]
            : ["references", "docs", "--direction=out"];
        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains($"Direction: {direction}", result.Output, StringComparison.Ordinal);
        if (direction == "in")
        {
            Assert.Contains("Incoming", result.Output, StringComparison.Ordinal);
            Assert.DoesNotContain("Outgoing", result.Output, StringComparison.Ordinal);
            Assert.Contains("alpha", result.Output, StringComparison.Ordinal);
            Assert.Contains("beta", result.Output, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("Outgoing", result.Output, StringComparison.Ordinal);
            Assert.DoesNotContain("Incoming", result.Output, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "References composed application preserves supplied filter occurrence order and duplicates in JSON"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task JsonPreservesOrderedDuplicateSelectors()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(
            [
                "references", "docs", "--direction=in", "--json",
                "--include", "alpha", "--exclude=beta", "--include:alpha", "--exclude", "beta",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var supplied = document.RootElement
            .GetProperty("result")
            .GetProperty("incomingSelection")
            .GetProperty("supplied")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(4, supplied.Length);
        Assert.Equal(
            [("include", "alpha"), ("exclude", "beta"), ("include", "alpha"), ("exclude", "beta")],
            supplied.Select(value =>
                (value.GetProperty("role").GetString()!, value.GetProperty("value").GetString()!)));
    }

    [Theory(DisplayName = "References composed application reports typed semantic invalidity and blocked workspace without domain writes"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    [InlineData("invalid-direction", 4, "invalid", "references.invalid-direction")]
    [InlineData("filter-with-out", 4, "invalid", "references.invalid-filter")]
    [InlineData("blocked-workspace", 5, "blocked", "references.workspace-unavailable")]
    public async Task TypedInvalidAndBlockedJourneysRemainDistinct(
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
            "invalid-direction" => ["references", "docs", "--json", "--direction=incoming"],
            "filter-with-out" => ["references", "docs", "--json", "--direction=out", "--include=alpha"],
            "blocked-workspace" => ["references", "docs", "--json", "--workspace", missing],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The References integration scenario is not defined."),
        };
        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        if (scenario == "blocked-workspace")
        {
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
            Assert.False(Directory.Exists(missing));
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "References compact and expanded human projections preserve one typed occurrence set"), Trait("Feature", "references"), Trait("Evidence", "Integration")]
    public async Task HumanViewsPreserveTypedOccurrenceParity()
    {
        using var workspace = CreateWorkspace();
        var compact = await CliHostCapture.RunAsync(
            ["references", "docs", "--view=compact"],
            workspace.Path);
        var expanded = await CliHostCapture.RunAsync(
            ["references", "docs", "--view=expanded"],
            workspace.Path);

        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.Error, expanded.Error);
        Assert.Contains("Level 1", compact.Output, StringComparison.Ordinal);
        Assert.Contains("raw destination:", expanded.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("target:", expanded.Output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("target.md", compact.Output, StringComparison.Ordinal);
        Assert.Contains("target.md", expanded.Output, StringComparison.Ordinal);
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

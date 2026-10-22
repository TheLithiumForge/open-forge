using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

[Trait("Feature", "included-file-reporting"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class IncludedFileReportingIntegrationTests
{
    [Theory(DisplayName = "Status and Doctor report listed, unlisted, missing and hand-written includes without changes")]
    [InlineData("status")]
    [InlineData("doctor")]
    public async Task ReportsIncludedFiles(string command)
    {
        using var workspace = Seed("""
            - [On](../on.md)
            - [Missing](../missing.md)
            - [Hand-written](../hand.md)
            """, "{\"roots\":[\"on.md\",\"off.md\",\"missing.md\"]}");
        workspace.CreateFile("on.md", "# On\n");
        workspace.CreateFile("off.md", "# Off\n");
        workspace.CreateFile("hand.md", "# Hand-written\n");
        var before = workspace.SnapshotHashes();
        foreach (var format in new[] { "text", "json" })
        {
            var run = await CliHostCapture.RunAsync([command, "--format", format, "--detail", "full"], workspace.Path);
            Assert.Equal(command == "status" ? 3 : 2, run.ExitCode);
            if (format == "json")
            {
                using var report = JsonDocument.Parse(run.Output);
                if (command == "status")
                {
                    var includes = report.RootElement.GetProperty("data").GetProperty("includedFiles");
                    Assert.Equal(1, includes.GetProperty("listed").GetInt32());
                    Assert.Equal(1, includes.GetProperty("unlisted").GetInt32());
                    Assert.Equal(1, includes.GetProperty("missing").GetInt32());
                    Assert.Equal(1, includes.GetProperty("handWritten").GetInt32());
                }
                else
                {
                    var findings = report.RootElement.GetProperty("findings").EnumerateArray().ToArray();
                    AssertFinding(findings, "workspace.root-missing", "missing.md", "error");
                    AssertFinding(findings, "workspace.detached", "off.md", "info");
                }
            }
            else if (command == "status")
                Assert.Contains("Included files: 1 listed, 1 unlisted, 1 missing, 1 hand-written loader row.", run.Output, StringComparison.Ordinal);
            else
            {
                Assert.Contains("The included file is missing.", run.Output, StringComparison.Ordinal);
                Assert.Contains("The registered include has no loader row, so agents do not read it.", run.Output, StringComparison.Ordinal);
                Assert.DoesNotContain("route init", run.Output, StringComparison.Ordinal);
            }
        }
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = command,
            Arguments = [command],
            ExitCode = command == "status" ? 3 : 2,
        }, testName: $"{nameof(ReportsIncludedFiles)}-{command}");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Startup counts required included catalogues once and keeps conditions pending")]
    public async Task MeasuresRequiredIncludedFiles()
    {
        using var workspace = Seed("""
            - [Required](../required.txt) - #LoadNow
            - [Same file](../required.txt) - #KeepInMind
            - [Pending](../pending.txt) - #LoadNow - applies to `src/**`
            - [Catalogue](../docs/_docs.md) - #LoadNow
            - [Same overwrite](../docs/child.overwrite.md) - #LoadNow
            """);
        workspace.CreateFile("required.txt", "Always read.\n");
        workspace.CreateFile("pending.txt", "Wait for working files.\n");
        workspace.CreateFile("docs/_docs.md", "# Documents\n\n## Entries\n\n- [Child](child.md) - #KeepInMind\n- [Pending child](pending.md) - #LoadNow - applies to `tests/**`\n");
        workspace.CreateFile("docs/child.md", "# Child\n");
        workspace.CreateFile("docs/child.overwrite.md", "# Child changes\n");
        workspace.CreateFile("docs/pending.md", "# Pending\n");
        var run = await CliHostCapture.RunAsync(["status", "--format", "json"], workspace.Path);
        Assert.Equal(3, run.ExitCode);
        using var report = JsonDocument.Parse(run.Output);
        Assert.Equal("incomplete", report.RootElement.GetProperty("status").GetString());
        var data = report.RootElement.GetProperty("data");
        var includes = data.GetProperty("includedFiles");
        Assert.Equal(4, includes.GetProperty("startup").GetProperty("files").GetInt64());
        Assert.Equal(new[] { "docs/pending.md", "pending.txt" }, includes.GetProperty("pending").EnumerateArray().Select(value => value.GetString()));
        var startup = data.GetProperty("context").GetProperty("startup").GetProperty("current");
        Assert.Equal(7, startup.GetProperty("files").GetInt64());
        var expectedBytes = new[] { "required.txt", "docs/_docs.md", "docs/child.md", "docs/child.overwrite.md" }
            .Sum(path => new FileInfo(workspace.Combine(path)).Length);
        Assert.Equal(expectedBytes, includes.GetProperty("startup").GetProperty("bytes").GetInt64());
        var text = await CliHostCapture.RunAsync(["status"], workspace.Path);
        Assert.Contains("Startup outside .agents/:", text.Output, StringComparison.Ordinal);
        Assert.Contains("Pending included file: pending.txt.", text.Output, StringComparison.Ordinal);
        var full = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.Path);
        using var fullReport = JsonDocument.Parse(full.Output);
        var continuitySources = fullReport.RootElement.GetProperty("data").GetProperty("context")
            .GetProperty("mayLoadAgainSources").EnumerateArray().ToArray();
        Assert.Contains(continuitySources, source => source.GetProperty("sourceId").GetString() == "./required.txt");
        var child = Assert.Single(continuitySources, source => source.GetProperty("sourceId").GetString() == "./docs/child.md");
        Assert.Equal(2, child.GetProperty("layers").GetArrayLength());
    }

    [Fact(DisplayName = "Doctor warns when a registered include became a directory")]
    public async Task WarnsAboutRejectedRegistration()
    {
        using var workspace = Seed("- [Former file](../former.md)", "{\"roots\":[\"former.md\"]}");
        workspace.CreateDirectory("former.md");
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(["doctor", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(2, run.ExitCode);
        using var report = JsonDocument.Parse(run.Output);
        var findings = report.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        AssertFinding(findings, "workspace.path-invalid", "former.md", "warning");
        Assert.DoesNotContain(findings, finding => finding.GetProperty("code").GetString() == "workspace.root-missing");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Status reports unlisted registrations even when the loader is absent")]
    public async Task ReportsRegistrationsWithoutLoader()
    {
        using var workspace = TemporaryWorkspace.Create("included-without-loader");
        workspace.CreateFile(".agents/open-forge.json", "{\"roots\":[\"off.md\"]}");
        workspace.CreateFile("off.md", "# Off\n");
        var text = await CliHostCapture.RunAsync(["status"], workspace.Path);
        Assert.Contains("Included files: 0 listed, 1 unlisted, 0 missing", text.Output, StringComparison.Ordinal);
        var run = await CliHostCapture.RunAsync(["status", "--format", "json"], workspace.Path);
        using var report = JsonDocument.Parse(run.Output);
        Assert.Equal(1, report.RootElement.GetProperty("data").GetProperty("includedFiles").GetProperty("unlisted").GetInt32());
    }

    [Theory(DisplayName = "Empty roots preserve existing Status and Doctor text and JSON reports")]
    [InlineData("status")]
    [InlineData("doctor")]
    public async Task EmptyRootsPreserveReports(string command)
    {
        using var workspace = Seed();
        var baseline = File.ReadAllText(workspace.Combine(".agents/open-forge.json"));
        var settings = JsonNode.Parse(baseline) ?? throw new InvalidOperationException("The fixture needs settings.");
        settings["roots"] = new JsonArray();
        foreach (var format in new[] { "text", "json" })
        {
            foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
            {
                workspace.ReplaceText(".agents/open-forge.json", baseline);
                var absent = await CliHostCapture.RunAsync([command, "--format", format, "--detail", detail], workspace.Path);
                workspace.ReplaceText(".agents/open-forge.json", settings.ToJsonString());
                var empty = await CliHostCapture.RunAsync([command, "--format", format, "--detail", detail], workspace.Path);
                Assert.Equal(absent.ExitCode, empty.ExitCode);
                Assert.Equal(absent.Output, empty.Output);
                Assert.Equal(absent.Error, empty.Error);
            }
        }
    }

    private static TemporaryWorkspace Seed(string rows = "", string? settings = null)
    {
        var workspace = TemporaryWorkspace.Create("included-reporting");
        workspace.CreateFile("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.CreateFile(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(
            "- [Guidance](guidance/_guidance.md) - #LoadNow\n" + rows + "\n")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the loader.", StringComparison.Ordinal));
        workspace.CreateFile(".agents/guidance/_guidance.md", OpenForgeDocumentSeed.Metadata("Guidance", ["LoadNow"],
            "# Guidance\n\n## Axioms\n\n- inherited - No local axioms apply.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        var authored = JsonNode.Parse(settings ?? "{}") ?? throw new InvalidOperationException("The fixture needs settings.");
        authored["removedCategories"] = JsonNode.Parse("[\"directives\",\"maps\",\"memory\",\"patterns\",\"skills\",\"templates\"]");
        workspace.CreateFile(".agents/open-forge.json", authored.ToJsonString());
        return workspace;
    }

    private static void AssertFinding(JsonElement[] findings, string code, string path, string severity)
    {
        var finding = Assert.Single(findings, finding => finding.GetProperty("code").GetString() == code
            && finding.GetProperty("subject").GetProperty("path").GetString() == path);
        Assert.Equal(severity, finding.GetProperty("severity").GetString());
    }
}

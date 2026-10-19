using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class DoctorLinkScopeIntegrationTests
{
    [Theory(DisplayName = "Doctor reports unknown settings versions as information while honoring understood keys")]
    [InlineData(99)]
    [InlineData(0)]
    public async Task ReportsUnknownSettingsVersion(int version)
    {
        using var workspace = Seed();
        var authored = new JsonObject
        {
            ["schemaVersion"] = JsonValue.Create(version),
            ["ignoreLinksFrom"] = new JsonArray(JsonValue.Create(".agents/guidance/archive")),
            ["future"] = JsonValue.Create(7),
        };
        workspace.CreateFile(".agents/open-forge.json", authored.ToJsonString());
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(["doctor", "--format=json", "--detail=full"], workspace.Path);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        using var report = JsonDocument.Parse(run.Output);
        var findings = report.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        var finding = Assert.Single(findings, finding => finding.GetProperty("code").GetString() == "workspace.settings-version-unknown");
        Assert.Equal("info", finding.GetProperty("severity").GetString());
        Assert.Equal("Workspace settings version is not recognized", finding.GetProperty("title").GetString());
        var message = $".agents/open-forge.json declares schemaVersion {version}, which this CLI does not recognize. This CLI uses the settings keys it knows.";
        Assert.Equal(message, finding.GetProperty("message").GetString());
        Assert.Empty(finding.GetProperty("actions").EnumerateArray());
        Assert.Equal(version.ToString(CultureInfo.InvariantCulture), finding.GetProperty("evidence")[0].GetProperty("value").GetString());
        Assert.DoesNotContain(findings, finding => finding.GetProperty("code").GetString() == "reference.target-missing");
        var text = await CliHostCapture.RunAsync(["doctor", "--detail=full"], workspace.Path);
        Assert.Equal(0, text.ExitCode);
        Assert.Contains(message, text.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Doctor checks incoming links into an ignored archive and pins ignored-count wording")]
    public async Task ChecksIncomingLinks()
    {
        using var workspace = Seed();
        workspace.CreateFile(".agents/guidance/working.md", OpenForgeDocumentSeed.Metadata("Working note", ["Guidance"], "# Working\n\n[Missing](archive/missing.md)\n"));
        workspace.CreateFile(".agents/open-forge.json", "{\"ignoreLinksFrom\":[\".agents/guidance/archive\"]}");
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var run = await CliHostCapture.RunAsync(["doctor", "--format=json", "--detail=full"], workspace.Path);
        using var report = JsonDocument.Parse(run.Output);
        Assert.Single(report.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "reference.target-missing"
            && finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/guidance/working.md");
        var text = await CliHostCapture.RunAsync(["doctor"], workspace.Path);
        Assert.Contains("2 links ignored", text.Output, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Doctor names a stale link-scope entry and its exact next step")]
    [InlineData("ignoreLinksFrom")]
    [InlineData("ignoreLinksTo")]
    public async Task ReportsStaleEntry(string key)
    {
        using var workspace = Seed();
        var settings = key == "ignoreLinksFrom"
            ? "{\"ignoreLinksFrom\":[\".agents/guidance/archive\",\".agents/gone\"]}"
            : "{\"ignoreLinksFrom\":[\".agents/guidance/archive\"],\"ignoreLinksTo\":[\".agents/gone\"]}";
        workspace.CreateFile(".agents/open-forge.json", settings);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var run = await CliHostCapture.RunAsync(["doctor", "--format=json", "--detail=full"], workspace.Path);
        using var report = JsonDocument.Parse(run.Output);
        var finding = Assert.Single(report.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "workspace.link-ignore-path-missing");
        Assert.Equal($"{key} entry .agents/gone no longer exists.", finding.GetProperty("message").GetString());
        Assert.Equal("Ignored link path is missing", finding.GetProperty("title").GetString());
        Assert.Equal("warning", finding.GetProperty("severity").GetString());
        Assert.Equal($"Remove or update {key} entry .agents/gone in .agents/open-forge.json.", finding.GetProperty("actions")[0].GetProperty("command").GetString());
        var text = await CliHostCapture.RunAsync(["doctor"], workspace.Path);
        Assert.Contains($"Remove or update {key} entry .agents/gone in .agents/open-forge.json.", text.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Context honors ignored links while References still lists every link")]
    public async Task ContextAndReferencesUseTheirDeclaredScope()
    {
        using var workspace = Seed();
        workspace.CreateFile(".agents/open-forge.json", "{\"ignoreLinksFrom\":[\".agents/guidance/archive\"]}");
        var context = await CliHostCapture.RunAsync(["context", "guidance/archive/note", "--follow-links=all", "--format=json", "--detail=full"], workspace.Path);
        using var report = JsonDocument.Parse(context.Output);
        Assert.DoesNotContain(report.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString()?.Contains("target-missing", StringComparison.Ordinal) == true);
        var references = await CliHostCapture.RunAsync(["references", "guidance/archive/note", "--direction=out", "--format=json", "--detail=full"], workspace.Path);
        using var links = JsonDocument.Parse(references.Output);
        Assert.Equal("../../missing.md", Assert.Single(links.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray()).GetProperty("destination").GetString());
    }

    private static TemporaryWorkspace Seed()
    {
        var workspace = TemporaryWorkspace.Create("doctor-link-scope");
        workspace.CreateFile("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.CreateFile(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.", StringComparison.Ordinal));
        workspace.CreateFile(".agents/guidance/_guidance.md", OpenForgeDocumentSeed.Metadata("Guidance", ["Guidance"], "# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- [Archive](archive/_archive.md) - #Guidance\n"));
        workspace.CreateFile(".agents/guidance/archive/_archive.md", OpenForgeDocumentSeed.Metadata("Archive", ["Guidance"], "# Archive\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- [Archived note](note.md) - #Guidance\n"));
        workspace.CreateFile(".agents/guidance/archive/note.md", OpenForgeDocumentSeed.Metadata("Archived note", ["Guidance"], "# Note\n\n[Missing](../../missing.md)\n"));
        return workspace;
    }

    [Theory(DisplayName = "Doctor ignores outward archive links and selected targets while checking incoming archive links")]
    [InlineData("ignoreLinksFrom", ".agents/guidance/archive", ".agents/guidance/archive/note.md", "../../missing.md")]
    [InlineData("ignoreLinksFrom", ".agents/guidance/archive/note.md", ".agents/guidance/archive/note.md", "../../missing.md")]
    [InlineData("ignoreLinksTo", ".agents/guidance/archive", ".agents/guidance/note.md", "archive/missing.md")]
    public async Task ExcludesSelectedLinks(string key, string entry, string source, string destination)
    {
        using var workspace = TemporaryWorkspace.Create("doctor-link-scope");
        workspace.CreateFile("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.CreateFile(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.", StringComparison.Ordinal));
        workspace.CreateFile(".agents/guidance/_guidance.md", OpenForgeDocumentSeed.Metadata("Guidance", ["Guidance"], "\n# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        workspace.CreateFile(source, OpenForgeDocumentSeed.Metadata("An authored note", ["Guidance"], $"\n# Note\n\n[Missing]({destination})\n"));
        workspace.CreateDirectory(".agents/guidance/archive");
        workspace.CreateFile(".agents/guidance/archive/_archive.md", OpenForgeDocumentSeed.Metadata("Archive", ["Guidance"], "\n# Archive\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        workspace.CreateFile(".agents/open-forge.json", $"{{\"{key}\":[\"{entry}\"]}}");
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(["doctor", "--format=json", "--detail=full"], workspace.Path);
        using var report = JsonDocument.Parse(run.Output);
        Assert.DoesNotContain(report.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true
            && finding.GetProperty("severity").GetString() is "warning" or "error");
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
        if (entry.EndsWith(".md", StringComparison.Ordinal))
            Assert.Contains("1 link ignored", (await CliHostCapture.RunAsync(["doctor"], workspace.Path)).Output, StringComparison.Ordinal);
    }
}

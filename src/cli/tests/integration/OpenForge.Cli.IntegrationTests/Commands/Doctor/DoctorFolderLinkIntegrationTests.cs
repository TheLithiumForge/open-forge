using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "folder-links"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class DoctorFolderLinkIntegrationTests
{
    [Theory(DisplayName = "Doctor and Repair accept folder links and require exact manual edits for missing folders and fragments"),
        InlineData("doctor"), InlineData("repair")]
    public async Task FolderFindingsStayManual(string command)
    {
        using var workspace = await SeedAsync();
        var before = workspace.SnapshotHashes();
        var arguments = new List<string> { command, "--workspace", workspace.Path, "--format", "json", "--detail", "full" };
        if (command == "repair") arguments.AddRange(["--automatic", "--dry-run"]);
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
        Assert.Equal(2, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        var folder = Assert.Single(findings, finding => finding.GetProperty("message").GetString() ==
            "The linked folder was not found: ../../missing/.");
        var fragment = Assert.Single(findings, finding => finding.GetProperty("message").GetString() ==
            "The linked folder has no headings: ../../src/cli/#heading.");
        var missingAction = command == "doctor"
            ? "Update or remove this link in .agents/guidance/map.md at line 10, column 1."
            : "Update or remove this link in .agents/guidance/map.md at line 10, column 9.";
        var fragmentAction = command == "doctor"
            ? "Remove the heading fragment from this link in .agents/guidance/map.md at line 11, column 1."
            : "Remove the heading fragment from this link in .agents/guidance/map.md at line 11, column 11.";
        Assert.Equal(missingAction,
            Assert.Single(folder.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
        Assert.Equal(fragmentAction,
            Assert.Single(fragment.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
        Assert.Equal("manual-decision", folder.GetProperty("resolution").GetString());
        Assert.Equal("manual-decision", fragment.GetProperty("resolution").GetString());
        Assert.Equal(0, folder.GetProperty("candidates").GetArrayLength());
        Assert.Equal(0, fragment.GetProperty("candidates").GetArrayLength());
        Assert.DoesNotContain(findings, finding => finding.GetProperty("resolution").GetString() is "safe-exact" or "guided-choice");
        if (command == "doctor")
        {
            Assert.Equal("reference.target-missing", folder.GetProperty("code").GetString());
            Assert.Equal("reference.destination-malformed", fragment.GetProperty("code").GetString());
            Assert.Equal(6, document.RootElement.GetProperty("counts").GetProperty("linksValid").GetInt32());
            Assert.Equal(1, document.RootElement.GetProperty("counts").GetProperty("externalLinksNotChecked").GetInt32());
        }
        else Assert.Equal(0, document.RootElement.GetProperty("effects").GetArrayLength());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Doctor and Repair accept a folder reached through a contained directory symlink"),
        InlineData("doctor"), InlineData("repair")]
    public async Task ContainedSymlinkFolderIsValid(string command)
    {
        using var workspace = await SeedAsync();
        workspace.CreateDirectorySymbolicLink("alias", "src/cli");
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Folder](../../alias/)\n"));
        var arguments = new List<string> { command, "--workspace", workspace.Path, "--format", "json", "--detail", "full" };
        if (command == "repair") arguments.AddRange(["--automatic", "--dry-run"]);
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
        Assert.Equal(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.Output);
        Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("severity").GetString() is "warning" or "error");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Repair folder-only reports give manual edits at every text detail without suggesting file choices"),
        InlineData("minimal"), InlineData("standard")]
    public async Task FolderOnlyRepairGuidanceIsManual(string detail)
    {
        using var workspace = await SeedAsync();
        var run = await CliHostCapture.RunAsync(["repair", "--automatic", "--dry-run", "--detail", detail], workspace.Path);
        Assert.Equal(2, run.ExitCode);
        Assert.Contains("The linked folder was not found: ../../missing/.", run.Output, StringComparison.Ordinal);
        Assert.Contains("The linked folder has no headings: ../../src/cli/#heading.", run.Output, StringComparison.Ordinal);
        Assert.Contains("Next: Update or remove this link in .agents/guidance/map.md at line", run.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("select a possible target", run.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--relink", run.Output, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Doctor and Repair folder fragments have one location-specific instruction at every text detail"),
        InlineData("doctor", "minimal", 11), InlineData("doctor", "standard", 11), InlineData("doctor", "full", 11),
        InlineData("repair", "minimal", 21), InlineData("repair", "standard", 21), InlineData("repair", "full", 21)]
    public async Task FolderFragmentGuidanceNamesTheEdit(string command, string detail, int column)
    {
        using var workspace = await SeedAsync();
        workspace.ReplaceText(".agents/guidance/map.md", Note("Map", "# Map\n\nSome text [Heading](../../src/cli/#heading)\n"));
        var arguments = new List<string> { command, "--detail", detail };
        if (command == "repair") arguments.AddRange(["--automatic", "--dry-run"]);
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
        Assert.Equal(2, run.ExitCode);
        var lines = run.Output.Split('\n').Select(line => line.Trim()).ToArray();
        Assert.Contains("The linked folder has no headings: ../../src/cli/#heading.", lines);
        var action = string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"Remove the heading fragment from this link in .agents/guidance/map.md at line 8, column {column}.");
        Assert.Equal(command == "doctor" ? 0 : 1, lines.Count(line => line == action));
        Assert.Single(lines, line => line == $"Next: {action}");
        Assert.DoesNotContain("Update or remove this link", run.Output, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Folder reference problems preserve managed sources and give command-specific ownership guidance"), InlineData("doctor"), InlineData("repair")]
    public async Task ManagedSourceProtectionStillApplies(string command)
    {
        using var workspace = await SeedAsync();
        workspace.WriteText(".agents/open-forge.lock.json", "{\"schemaVersion\":1,\"framework\":{\"source\":{\"id\":\"builtin\",\"version\":null},\"paths\":[\".agents/guidance/map.md\"],\"regions\":[]},\"extensions\":[],\"libraries\":[]}");
        var arguments = new List<string> { command, "--format", "json", "--detail", "full" };
        if (command == "repair") arguments.AddRange(["--automatic", "--dry-run"]);
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
        using var document = JsonDocument.Parse(run.Output);
        var findings = document.RootElement.GetProperty("findings").EnumerateArray()
            .Where(finding => finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/guidance/map.md").ToArray();
        Assert.NotEmpty(findings);
        if (command == "doctor")
            Assert.All(findings, finding => Assert.EndsWith("through the owner of .agents/guidance/map.md.",
                Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString(), StringComparison.Ordinal));
        else
            Assert.All(findings, finding => Assert.Contains(finding.GetProperty("actions").EnumerateArray(),
                action => action.GetProperty("command").GetString()?.StartsWith("open-forge update", StringComparison.Ordinal) == true));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<TemporaryWorkspace> SeedAsync()
    {
        var workspace = TemporaryWorkspace.Create("folder-doctor-repair");
        workspace.WriteText("AGENTS.md", "Read `.agents/loader.md`.\n");
        workspace.WriteText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.", StringComparison.Ordinal) + "\n");
        workspace.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- [Decoy](decoy.md) - #Guidance\n- [Map](map.md) - #Guidance\n"));
        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Folder](../../src/cli/)\n[Folder](../../src/cli)\n[Decoy](../../missing/)\n[Heading](../../src/cli/#heading)\n[Binary](../../asset.bin)\n[Web](https://example.invalid/folder/#heading)\n"));
        workspace.WriteText(".agents/guidance/decoy.md", Note("Decoy", "# Decoy\n"));
        workspace.WriteBytes("asset.bin", [0, 255, 128]);
        workspace.CreateDirectory("src/cli");
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], workspace.Path)).ExitCode);
        return workspace;
    }

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], body);
}

using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class FolderLinkDoctorJourneyTests
{
    [Fact(DisplayName = "Doctor follows an authored folder link from valid to missing and back after a manual edit"),
        Trait("Feature", "folder-links"), Trait("Evidence", "EndToEnd")]
    public async Task FolderLinkDiagnosisAndManualEdit()
    {
        using var workspace = PublishedJourneyWorkspace.Create("folder-link-doctor-journey");
        workspace.WriteText("AGENTS.md", "Read `.agents/loader.md`.\n");
        workspace.WriteText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Guidance](guidance/_guidance.md) - #Guidance")
            .Replace("# Open Forge Loader", "# Open Forge Loader\n\n## Axioms\n\n- Follow the selected rules.", StringComparison.Ordinal) + "\n");
        workspace.WriteText(".agents/guidance/_guidance.md", Note("Guidance", "# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- [Map](map.md) - #Guidance\n"));
        workspace.WriteText("src/cli/keep.txt", "Folder target.\n");
        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[CLI](../../src/cli/)\n"));
        Assert.Equal(0, (await workspace.RunAsync("index")).ExitCode);

        var before = workspace.SnapshotState();
        var valid = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(0, valid.ExitCode);
        Assert.Equal(string.Empty, valid.StandardError);
        Assert.Equal(before, workspace.SnapshotState());

        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[CLI](../../missing/)\n"));
        before = workspace.SnapshotState();
        var missing = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(2, missing.ExitCode);
        using (var document = JsonDocument.Parse(missing.StandardOutput))
        {
            var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString() == "reference.target-missing");
            Assert.Equal("The linked folder was not found: ../../missing/.", finding.GetProperty("message").GetString());
            Assert.Equal("Update or remove this link in .agents/guidance/map.md at line 8, column 1.",
                Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("command").GetString());
            Assert.Equal("manual-decision", finding.GetProperty("resolution").GetString());
        }
        Assert.Equal(before, workspace.SnapshotState());

        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[CLI](../../src/cli)\n"));
        var repaired = await workspace.RunAsync("doctor");
        Assert.Equal(0, repaired.ExitCode);
        Assert.Contains("No problems found.", repaired.StandardOutput, StringComparison.Ordinal);
    }

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], body);
}

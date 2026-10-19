using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class RepairLinkScopeIntegrationTests
{
    [Fact(DisplayName = "Repair leaves a link to an ignored missing target untouched")]
    public async Task IgnoresTargets()
    {
        using var fixture = new RepairRenameGitWorkspace();
        var note = OpenForgeDocumentSeed.Metadata("Working note", ["Guidance"], "# Working\n\n[Missing](archive/missing.md)\n");
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/working.md"] = note,
            [".agents/guidance/archive/_archive.md"] = OpenForgeDocumentSeed.Metadata("Archive", ["Guidance"], "# Archive\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n"),
            [".agents/open-forge.json"] = "{\"ignoreLinksTo\":[\".agents/guidance/archive\"]}",
        }, unborn: true);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], fixture.Root)).ExitCode);
        var before = File.ReadAllBytes(fixture.PathFor(".agents/open-forge.json"));
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], fixture.Root);
        Assert.True(repair.ExitCode == 0, repair.Output + repair.Error);
        Assert.Equal(note, File.ReadAllText(fixture.PathFor(".agents/guidance/working.md")));
        Assert.Equal(before, File.ReadAllBytes(fixture.PathFor(".agents/open-forge.json")));
    }

    [Fact(DisplayName = "Repair skips ignored outward links and fixes incoming links when an ignored file moves")]
    public async Task RepairsIncomingAndPreservesIgnoredOutwardLinks()
    {
        using var fixture = new RepairRenameGitWorkspace();
        static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], body);
        const string original = ".agents/guidance/archive/old.md";
        const string successor = ".agents/guidance/archive/new.md";
        const string incoming = ".agents/guidance/working.md";
        var archive = Note("Archived rounding rules", "# Rounding\n\n[Missing](../missing.md)\n");
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [original] = archive,
            [incoming] = Note("Working notes", "# Working\n\n[Rounding](archive/old.md)\n"),
            [".agents/open-forge.json"] = "{\"ignoreLinksFrom\":[\".agents/guidance/archive\"]}",
        }, unborn: true);
        Assert.Equal(0, (await CliHostCapture.RunAsync(["index"], fixture.Root)).ExitCode);
        fixture.Move(original, successor);
        var repair = await CliHostCapture.RunAsync(["repair", "--automatic"], fixture.Root);
        Assert.True(repair.ExitCode == 0, repair.Output + repair.Error);
        Assert.Equal(archive, File.ReadAllText(fixture.PathFor(successor)));
        Assert.Contains("archive/new.md", File.ReadAllText(fixture.PathFor(incoming)), StringComparison.Ordinal);
        Assert.DoesNotContain("archive/old.md", File.ReadAllText(fixture.PathFor(incoming)), StringComparison.Ordinal);
    }
}

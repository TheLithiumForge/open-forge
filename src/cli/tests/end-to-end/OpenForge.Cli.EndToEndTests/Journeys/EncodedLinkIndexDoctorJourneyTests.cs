using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class EncodedLinkIndexDoctorJourneyTests
{
    [Fact(DisplayName = "Index spaced names in installed managed and user-owned entrypoints then reach a clean Doctor"),
        Trait("Feature", "reference-encoding"), Trait("Evidence", "EndToEnd")]
    public async Task IndexSpacedNamesThenDoctor()
    {
        using var workspace = PublishedJourneyWorkspace.Create("index-encoded-links");
        workspace.ExpectCoreInstall();
        var install = await workspace.RunAsync("install", "--frontmatter", "scoped", "--automatic");
        Assert.Equal(0, install.ExitCode);
        workspace.WriteText(".agents/guidance/old guide.md", Note("Old guide", "# Old guide\n\n## Heading\n"));
        workspace.WriteText(".agents/guidance/hash#name.md", Note("Hash", "# Hash\n"));
        workspace.WriteText(".agents/guidance/café(one).md", Note("Cafe", "# Cafe\n"));
        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Spaced](<old guide.md>)\n[Literal](café(one).md)\n[Encoded](caf%C3%A9%28one%29.md)\n[Hash](%68ash%23name.md)\n"));
        workspace.WriteText(".agents/guidance/topics/_topics.md", Note("Topics", "# Topics\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n"));
        workspace.WriteText(".agents/guidance/topics/spaced topic.md", Note("Spaced topic", "# Spaced topic\n"));
        Assert.Equal(0, (await workspace.RunAsync("index", "--dry-run")).ExitCode);
        Assert.Equal(0, (await workspace.RunAsync("index")).ExitCode);
        Assert.Contains("(old%20guide.md)", File.ReadAllText(Path.Combine(workspace.Path, ".agents", "guidance", "_guidance.md")), StringComparison.Ordinal);
        Assert.Contains("(spaced%20topic.md)", File.ReadAllText(Path.Combine(workspace.Path, ".agents", "guidance", "topics", "_topics.md")), StringComparison.Ordinal);
        Assert.Contains("(hash%23name.md)", File.ReadAllText(Path.Combine(workspace.Path, ".agents", "guidance", "_guidance.md")), StringComparison.Ordinal);
        var offered = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        using (var findings = JsonDocument.Parse(offered.StandardOutput))
        {
            var correction = Assert.Single(findings.RootElement.GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true);
            Assert.Equal("reference.same-target-encoding", correction.GetProperty("code").GetString());
            Assert.Equal("safe-exact", correction.GetProperty("resolution").GetString());
        }
        Assert.Equal(0, (await workspace.RunAsync("repair", "--automatic", "--dry-run")).ExitCode);
        Assert.Equal(0, (await workspace.RunAsync("repair", "--automatic")).ExitCode);
        var mapPath = Path.Combine(workspace.Path, ".agents", "guidance", "map.md");
        Assert.Contains("[Hash](hash%23name.md)", File.ReadAllText(mapPath), StringComparison.Ordinal);
        Assert.Contains("[Spaced](<old guide.md>)", File.ReadAllText(mapPath), StringComparison.Ordinal);
        var before = workspace.SnapshotState();
        var doctor = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(0, doctor.ExitCode);
        Assert.Equal(string.Empty, doctor.StandardError);
        using var document = JsonDocument.Parse(doctor.StandardOutput);
        Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true);
        Assert.Equal(before, workspace.SnapshotState());
        var repair = await workspace.RunAsync("repair", "--automatic");
        Assert.Equal(0, repair.ExitCode);
        Assert.Contains("Nothing to repair.", repair.StandardOutput, StringComparison.Ordinal);

        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n![Guide](old%20guide.md#HEADING)\n"));
        var imageDoctor = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(2, imageDoctor.ExitCode);
        using var image = JsonDocument.Parse(imageDoctor.StandardOutput);
        var manual = Assert.Single(image.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true);
        Assert.Equal("reference.fragment-missing", manual.GetProperty("code").GetString());
        Assert.Equal("manual-decision", manual.GetProperty("resolution").GetString());
        var imageBefore = workspace.SnapshotState();
        Assert.Equal(2, (await workspace.RunAsync("repair", "--automatic")).ExitCode);
        Assert.Equal(imageBefore, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Installed workspace entity filename spellings agree on identity with both alias files present"),
        Trait("Feature", "reference-encoding"), Trait("Evidence", "EndToEnd")]
    public async Task EntityFilenamesRetainIdentity()
    {
        using var workspace = PublishedJourneyWorkspace.Create("entity-filename-links");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync("install", "--frontmatter", "scoped", "--automatic")).ExitCode);
        workspace.WriteText(".agents/guidance/entity&amp;.md", Note("Entity", "# Entity\n"));
        workspace.WriteText(".agents/guidance/entity&.md", Note("Alias", "# Alias\n"));
        workspace.WriteText(".agents/guidance/map.md", Note("Map", "# Map\n\n[Index](entity%26amp%3B.md)\n[Literal](entity%26amp;.md)\n"));
        Assert.Equal(0, (await workspace.RunAsync("index", "--dry-run")).ExitCode);
        Assert.Equal(0, (await workspace.RunAsync("index")).ExitCode);
        var before = workspace.SnapshotState();
        var references = await workspace.RunAsync("references", "guidance/map", "--direction", "out", "--format", "json", "--detail", "full");
        Assert.Equal(0, references.ExitCode);
        using var parsed = JsonDocument.Parse(references.StandardOutput);
        var outgoing = parsed.RootElement.GetProperty("data").GetProperty("outgoing").EnumerateArray().ToArray();
        Assert.Equal(2, outgoing.Length);
        Assert.All(outgoing, link => Assert.Equal(".agents/guidance/entity&amp;.md", link.GetProperty("resolvedPath").GetString()));
        var doctor = await workspace.RunAsync("doctor", "--format", "json", "--detail", "full");
        Assert.Equal(0, doctor.ExitCode);
        using var findings = JsonDocument.Parse(doctor.StandardOutput);
        Assert.DoesNotContain(findings.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()?.StartsWith("reference.", StringComparison.Ordinal) == true);
        var repair = await workspace.RunAsync("repair", "--automatic");
        Assert.Equal(0, repair.ExitCode);
        Assert.Contains("Nothing to repair.", repair.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
    }

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], body);
}

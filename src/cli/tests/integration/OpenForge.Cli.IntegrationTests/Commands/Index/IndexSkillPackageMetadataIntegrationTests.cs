using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class IndexSkillPackageMetadataIntegrationTests
{
    private const string SkillPath = ".agents/skills/release-notes/SKILL.md";
    private const string ReferencePath = ".agents/skills/release-notes/references/style.md";
    private const string SkillBody = "# Release notes\n\nWrite release notes from merged changes.\n\nSee [style](references/style.md).\n";

    [Fact(DisplayName = "Index fills missing metadata in a user-owned Skill package and Doctor then finds nothing to fix")]
    public async Task FillsSkillPackageMetadata()
    {
        using var workspace = Seed();
        workspace.CreateFile(SkillPath, SkillBody);
        workspace.CreateFile(ReferencePath, "# Style\n\nShort sentences.\n");

        var index = await CliHostCapture.RunAsync(["index"], workspace.Path);

        Assert.Equal(0, index.ExitCode);
        Assert.Equal($"---\nname: \"release-notes\"\ndescription: \"Release notes\"\n---\n\n{SkillBody}", File.ReadAllText(workspace.Combine(SkillPath)));
        var reference = File.ReadAllText(workspace.Combine(ReferencePath));
        Assert.Contains("description: Style", reference, StringComparison.Ordinal);
        Assert.Contains("tags: [Skill]", reference, StringComparison.Ordinal);
        Assert.EndsWith("# Style\n\nShort sentences.\n", reference, StringComparison.Ordinal);
        Assert.Contains("release-notes", File.ReadAllText(workspace.Combine(".agents/skills/_skills.md")), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(".agents/skills/release-notes/references/_references.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/skills/release-notes/_release-notes.md")));

        var doctor = await CliHostCapture.RunAsync(["doctor", "--workspace", workspace.Path, "--format", "json"], workspace.Path);
        using var document = JsonDocument.Parse(doctor.Output);
        Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("subject").GetProperty("path").GetString()?.StartsWith(".agents/skills/release-notes/", StringComparison.Ordinal) == true);
        var repeated = await CliHostCapture.RunAsync(["index", "--dry-run"], workspace.Path);
        Assert.Contains("Nothing to do.", repeated.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Index keeps a SKILL.md with unclosed frontmatter unchanged and still completes the other files")]
    public async Task KeepsMalformedSkillUnchanged()
    {
        using var workspace = Seed();
        const string malformed = "---\nname: release-notes\n# Release notes\n";
        workspace.CreateFile(SkillPath, malformed);
        workspace.CreateFile(".agents/skills/other/SKILL.md", "# Other\n\nAnother Skill.\n");

        var index = await CliHostCapture.RunAsync(["index"], workspace.Path);

        Assert.Equal(malformed, File.ReadAllText(workspace.Combine(SkillPath)));
        Assert.StartsWith("---\nname: \"other\"\ndescription: \"Other\"\n---\n", File.ReadAllText(workspace.Combine(".agents/skills/other/SKILL.md")), StringComparison.Ordinal);
        Assert.Contains(SkillPath, index.Output, StringComparison.Ordinal);
    }

    private static TemporaryWorkspace Seed()
    {
        var workspace = TemporaryWorkspace.Create("index skill package metadata");
        workspace.CreateFile("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.CreateFile(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Skills](skills/_skills.md) - #Skill") + "\n");
        workspace.CreateFile(".agents/skills/_skills.md", OpenForgeDocumentSeed.Metadata("Skills", ["Skill"],
            "# Skills\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- none - No entries - #Empty\n"));
        return workspace;
    }
}

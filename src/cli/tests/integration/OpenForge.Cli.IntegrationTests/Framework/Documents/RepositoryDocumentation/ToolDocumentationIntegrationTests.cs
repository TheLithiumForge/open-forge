using OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation;

public sealed class ToolDocumentationIntegrationTests
{
    private const string SourceSkill = ".agents/skills/open-forge-cli";
    private const string ShippedSkill = "src/open-forge/.agents/skills/open-forge-cli";

    [Fact(DisplayName = "Changed Tool documentation has complete local links through the production Markdown checker")]
    [Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public async Task LocalLinks()
    {
        var diagnostics = await RepositoryMarkdownChecker.CheckAsync(RepositoryRoot(),
        [
            "docs/cli.md", "src/docusaurus/docs/cli/index.md", "src/docusaurus/docs/cli/tools.md",
            SourceSkill + "/SKILL.md", SourceSkill + "/references/tools.md",
            ShippedSkill + "/SKILL.md", ShippedSkill + "/references/tools.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-e.md",
            ".agents/memory/working/cli-development/tool-setup/evidence/windows-journeys.md",
        ], TestContext.Current.CancellationToken);
        Assert.True(diagnostics.Count == 0, string.Join(Environment.NewLine, diagnostics));
    }

    [Theory(DisplayName = "The source and shipped CLI Skill Tool instructions use identical bytes")]
    [InlineData("SKILL.md"), InlineData("references/tools.md"), Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public void SkillCopies(string relative)
    {
        var root = RepositoryRoot();
        Assert.Equal(File.ReadAllBytes(Path.Combine(root, SourceSkill, relative)), File.ReadAllBytes(Path.Combine(root, ShippedSkill, relative)));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("The repository solution is missing.");
    }
}

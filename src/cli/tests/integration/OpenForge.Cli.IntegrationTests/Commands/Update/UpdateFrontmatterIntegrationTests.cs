using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update-frontmatter"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class UpdateFrontmatterIntegrationTests
{
    private const string ManagedDocument = ".agents/guidance/_guidance.md";

    [Fact(DisplayName = "Root workspace Update repeats without effects or byte changes")]
    public async Task RootWorkspaceUpdateIsNoOp()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-root-repeat");
        await EstablishScopedAsync(workspace);
        SetForm(workspace, FrontmatterForm.Root);
        var converted = await workspace.ExecuteAsync(workspace.Request());
        AssertComplete(converted);
        AssertForm(workspace.ReadText(ManagedDocument), FrontmatterForm.Root);
        var before = workspace.SnapshotHashes();

        var repeated = await workspace.ExecuteAsync(workspace.Request());

        AssertComplete(repeated);
        Assert.Empty(repeated.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "A hand-edited form setting replaces unedited owned Framework files through ordinary Update")]
    public async Task HandEditedSettingReplacesUneditedOwnedFrameworkFiles()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-form-setting");
        workspace.CreateDirectory(".agents/guidance");
        await EstablishScopedAsync(workspace);
        const string localPath = ".agents/guidance/local-note.md";
        workspace.RegisterTestCleanupPath(localPath);
        const string local = "---\nopen-forge:\n  description: Local note\n  tags: [Workspace]\n---\n\n# Local note\n";
        workspace.WriteText(localPath, local);
        SetForm(workspace, FrontmatterForm.Root);
        var settingsBefore = workspace.ReadBytes(WorkspaceSettingsDefinitions.RelativePath);

        var result = await workspace.ExecuteAsync(workspace.Request());

        AssertComplete(result);
        var effect = Assert.Single(result.Effects, effect => effect.Path == ManagedDocument);
        Assert.Equal(UpdatePhysicalEffectAction.Replace, effect.Action);
        Assert.Equal(UpdatePhysicalEffectOutcome.Verified, effect.Outcome);
        AssertForm(workspace.ReadText(ManagedDocument), FrontmatterForm.Root);
        Assert.Equal(local, workspace.ReadText(localPath));
        Assert.Equal(settingsBefore, workspace.ReadBytes(WorkspaceSettingsDefinitions.RelativePath));
    }

    [Fact(DisplayName = "Scoped Framework destinations compare against rendered payload identity")]
    public async Task ScopedMappedTargetsUseRenderedIdentity()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-rendered-scoped-map");
        workspace.RegisterTestCleanupPath(".agents/memory/team");
        workspace.CreateDirectory(".agents/memory/team/working");
        await EstablishScopedAsync(workspace);
        const string mappedPath = ".agents/memory/team/working/_working.md";
        workspace.WriteText(".agents/memory/team/_team.md", OpenForgeDocumentSeed.Metadata(
            "Team memory", ["Memory"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Team",
                Entries = "- none - No entries - #Empty",
            })));
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The mapped fixture requires the embedded Framework payload.");
        var source = payload.Find(".agents/memory/working/_working.md")
            ?? throw new InvalidOperationException("The mapped fixture requires the Working entrypoint.");
        workspace.WriteText(mappedPath, Encoding.UTF8.GetString(source.Bytes.AsSpan()));
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))?.AsObject()
            ?? throw new InvalidOperationException("The fixture requires ownership.");
        var paths = ownership["framework"]?["paths"]?.AsArray()
            ?? throw new InvalidOperationException("The fixture requires Framework paths.");
        ((IList<JsonNode?>)paths).Add(JsonValue.Create(mappedPath));
        workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
        SetForm(workspace, FrontmatterForm.Root);

        var result = await workspace.ExecuteAsync(workspace.Request());

        AssertComplete(result);
        AssertForm(workspace.ReadText(mappedPath), FrontmatterForm.Root);
        Assert.Contains(result.Comparisons, comparison => comparison.RelativePath == mappedPath
            && comparison.SourceAssetPath == source.Path
            && comparison.IntendedState == UpdateComparisonIntendedState.Changed);
        var before = workspace.SnapshotHashes();
        var repeated = await workspace.ExecuteAsync(workspace.Request());
        AssertComplete(repeated);
        Assert.Empty(repeated.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Update adoption creates a new reference entrypoint in the workspace form")]
    public async Task UpdateAdoptionCreatesEntrypointInWorkspaceForm()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-root-adoption");
        workspace.RegisterTestCleanupPath(".agents/skills");
        workspace.CreateDirectory(".agents/skills");
        await EstablishScopedAsync(workspace);
        SetForm(workspace, FrontmatterForm.Root);
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(workspace, "root-adopted",
            "---\nname: root-adopted\ndescription: Adopted local skill\n---\n\n# Local Skill\n", "# Local Skill\n");
        var originalSkill = workspace.ReadBytes(skill.SkillPath);

        var result = await workspace.ExecuteAsync(workspace.Request());

        AssertComplete(result);
        AssertForm(workspace.ReadText(skill.ReferencesCataloguePath), FrontmatterForm.Root);
        Assert.Equal(originalSkill, workspace.ReadBytes(skill.SkillPath));
        Assert.Contains(result.Migrations, migration => migration.Path == skill.ReferencesCataloguePath);
    }

    private static async Task EstablishScopedAsync(UpdateIntegrationWorkspace workspace)
    {
        workspace.CreateDirectory(".agents");
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{\"schemaVersion\":1,\"frontmatter\":\"scoped\"}\n");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
    }

    private static void SetForm(UpdateIntegrationWorkspace workspace, FrontmatterForm form)
        => workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Encoding.UTF8.GetString(
            WorkspaceSettingsCodec.SetFrontmatter(workspace.ReadBytes(WorkspaceSettingsDefinitions.RelativePath), form)
                ?? workspace.ReadBytes(WorkspaceSettingsDefinitions.RelativePath)));

    private static void AssertForm(string contents, FrontmatterForm expected)
    {
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(contents), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(expected, facts.Syntax.AuthoredForm);
    }

    private static void AssertComplete(OpenForge.Cli.Core.Commands.Update.Models.Result.UpdateResult result)
    {
        Assert.True(result.Status == CliSemanticStatus.Complete,
            string.Join("\n", result.Findings.Select(finding => $"{finding.Code}: {finding.Target}: {finding.Cause}")));
        Assert.Empty(result.Findings);
    }
}

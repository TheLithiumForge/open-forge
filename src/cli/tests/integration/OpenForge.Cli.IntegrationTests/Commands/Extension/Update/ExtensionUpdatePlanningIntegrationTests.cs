using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdatePlanningIntegrationTests
{
    [Fact(DisplayName = "Root Extension Update repeats without effects or divergence"), Trait("Boundary", "OS"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task RootDeliveryRepeatsWithoutDivergence()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-root-repeat");
        await workspace.SeedFrameworkAsync();
        SetFrontmatter(workspace, "root");
        using var source = ExtensionInstallCatalogue.Create("extension-update-root-repeat-source");
        const string target = ".agents/toolkit/_toolkit.md";
        source.AddPackage("toolkit", [], (target, CatalogueDocument()), (".agents/toolkit/note.md", Document("Note")));
        await InstallAllAsync(workspace, source);
        var sourceBefore = source.Snapshot();
        var before = workspace.Snapshot();

        for (var iteration = 0; iteration < 2; iteration++)
        {
            var run = await workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
            Assert.Equal(0, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Complete, run.Status);
            using var result = JsonDocument.Parse(run.StandardOutput);
            Assert.Empty(result.RootElement.GetProperty("effects").EnumerateArray());
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(sourceBefore, source.Snapshot());
        }
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(workspace.ReadText(target)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(FrontmatterForm.Root, facts.Syntax.AuthoredForm);
    }

    [Fact(DisplayName = "Changing the form setting uses ordinary Extension Update replacement"), Trait("Boundary", "OS"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task SettingChangeUsesOrdinaryReplacement()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-form-change");
        await workspace.SeedFrameworkAsync();
        SetFrontmatter(workspace, "scoped");
        using var source = ExtensionInstallCatalogue.Create("extension-update-form-change-source");
        const string target = ".agents/guidance/form-change.md";
        source.AddPackage("toolkit", [], (target, Document("Form change")));
        await InstallAllAsync(workspace, source);
        var sourceBefore = source.Snapshot();
        SetFrontmatter(workspace, "root");

        var run = await workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var result = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(result.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == target && effect.GetProperty("action").GetString() == "replaced");
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(workspace.ReadText(target)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(FrontmatterForm.Root, facts.Syntax.AuthoredForm);
        Assert.Equal(sourceBefore, source.Snapshot());
        var before = workspace.Snapshot();
        var repeated = await workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        using var repeatResult = JsonDocument.Parse(repeated.StandardOutput);
        Assert.Empty(repeatResult.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "A retained shared owner blocks form reconciliation without writes"), Trait("Boundary", "OS"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task SharedOwnerConflictRemainsProtected()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-root-shared-owner");
        await workspace.SeedFrameworkAsync();
        SetFrontmatter(workspace, "scoped");
        using var source = ExtensionInstallCatalogue.Create("extension-update-root-shared-owner-source");
        const string target = ".agents/guidance/shared.md";
        source.AddPackage("alpha", [], (target, Document("Shared")));
        source.AddPackage("beta", [], (target, Document("Shared")));
        await InstallAllAsync(workspace, source);
        SetFrontmatter(workspace, "root");
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(["extension", "update", "alpha", "--source", source.Path, "--automatic", "--format", "json"]);

        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var result = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(result.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Contains(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-update.ownership-conflict");
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    private static void SetFrontmatter(ExtensionInstallIntegrationWorkspace workspace, string form)
    {
        const string settingsPath = ".agents/open-forge.json";
        var exists = File.Exists(workspace.Combine(settingsPath));
        var settings = exists
            ? JsonNode.Parse(workspace.ReadText(settingsPath))?.AsObject()
                ?? throw new InvalidOperationException("The fixture requires valid workspace settings.")
            : new JsonObject { ["schemaVersion"] = 1 };
        settings["frontmatter"] = form;
        if (exists)
        {
            workspace.ReplaceText(settingsPath, settings.ToJsonString());
        }
        else
        {
            workspace.CreateOccupant(settingsPath, settings.ToJsonString());
        }
    }

    [Fact(DisplayName = "Extension Update blocks invalid root rendering and preserves the installed target"), Trait("Boundary", "OS"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task InvalidRootRenderingBlocksAllDelivery()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-root-collision");
        await workspace.SeedFrameworkAsync();
        SetFrontmatter(workspace, "scoped");
        using var source = ExtensionInstallCatalogue.Create("extension-update-root-collision-source");
        source.AddPackage("toolkit", [], (".agents/guidance/collision.md",
            "---\ndescription: Foreign metadata\nopen-forge:\n  description: Toolkit\n  tags: [Extension]\n---\n# Toolkit\n"));
        await InstallAllAsync(workspace, source);
        SetFrontmatter(workspace, "root");
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);

        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var result = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(result.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Contains(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-update.target-unsafe"
            && finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/guidance/collision.md");
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update dry-run resolves the selected dependency closure in order without effects"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task DryRunResolvesDependencyClosureInOrderWithoutEffects()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-dry-run-closure");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-dry-run-closure-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("base", ".agents/base/_base.md", Document("Base v2"));
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension update", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.NotEmpty(root.GetProperty("effects").EnumerateArray());
        var packagePaths = root.GetProperty("effects")
            .EnumerateArray()
            .Select(effect => effect.GetProperty("path").GetString())
            .Where(path => path is ".agents/base/_base.md" or ".agents/toolkit/_toolkit.md")
            .ToArray();
        Assert.Equal([".agents/base/_base.md", ".agents/toolkit/_toolkit.md"], packagePaths);
        Assert.All(
            root.GetProperty("effects").EnumerateArray(),
            effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update normal dry-run plans changed current content without applying it"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task NormalDryRunPlansChangedCurrentContentWithoutMutation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-divergence");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-divergence-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("User divergence"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.DoesNotContain(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.managed-divergence");
        Assert.NotEmpty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(Document("User divergence"), workspace.ReadText(".agents/toolkit/_toolkit.md"));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Installed generated catalogue does not claim an update"), InlineData(false), InlineData(true)]
    public async Task InstalledGeneratedCatalogueDoesNotClaimAnUpdate(bool dryRun)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-installed-generated-catalogue-{dryRun}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-installed-generated-catalogue-source-{dryRun}");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", CatalogueDocument()),
            (".agents/toolkit/note.md", Document("Note")));

        var install = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);
        Assert.Equal(0, install.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, install.Status);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.Contains("- [Note](note.md)", workspace.ReadText(".agents/toolkit/_toolkit.md"), StringComparison.Ordinal);

        var destinationBytes = File.ReadAllBytes(workspace.Combine(".agents/toolkit/_toolkit.md"));
        var ownershipBytes = File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var arguments = new List<string>
        {
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json", "--detail", "full",
        };
        if (dryRun)
        {
            arguments.Add("--dry-run");
        }

        var run = await workspace.RunAsync([.. arguments]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Empty(root.GetProperty("data").GetProperty("sections").EnumerateArray());
        Assert.Equal(0, root.GetProperty("counts").GetProperty("sectionsUpdated").GetInt32());
        Assert.Equal(destinationBytes, File.ReadAllBytes(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.Equal(ownershipBytes, File.ReadAllBytes(workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath)));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");

    private static string CatalogueDocument()
        => OpenForgeDocumentSeed.Metadata(
            "Toolkit",
            ["Extension"],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Toolkit",
                Entries = "- none - No entries - #Empty",
            }));
}

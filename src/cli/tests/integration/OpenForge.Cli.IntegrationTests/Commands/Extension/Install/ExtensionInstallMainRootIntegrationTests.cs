using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

[Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionInstallMainRootIntegrationTests
{
    private const string Fixture = "extension-main-root";
    private const string MainRoot = "docs/forge";
    private const string PackageId = "toolkit";
    private const string Note = "package-note" + SourceFormClassifier.MarkdownExtension;
    private const string PackagedNote = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string InstalledNote = MainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string Skill = WorkspaceLayout.NativeSkillsPath + "/" + Fixture + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string Description = "Extension layout fixture";
    private const string Body = "# Package note\n\nPreserved body.\n";

    [Theory(DisplayName = "Extension Install maps custom content and keeps native Skills through an unchanged repeat")]
    [InlineData(false), InlineData(true)]
    public async Task InstallAndRepeat(bool rootForm)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(Fixture, MainRoot);
        workspace.CreateDirectory(WorkspaceSettingsDefinitions.DirectoryName);
        var form = rootForm ? WorkspaceSettingsDefinitions.FrontmatterRootName : WorkspaceSettingsDefinitions.FrontmatterScopedName;
        workspace.CreateOccupant(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","{{WorkspaceSettingsDefinitions.FrontmatterProperty}}":"{{form}}"}
            """);
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(Fixture);
        source.AddPackage(PackageId, [], (PackagedNote, OpenForgeDocumentSeed.Metadata(Description, [], Body)),
            (Skill, OpenForgeDocumentSeed.Skill(Fixture, Description)));
        var sourceBytes = source.ReadPayloadBytes(PackageId, PackagedNote);
        var settingsBytes = File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath));
        string[] arguments = [ExtensionDefinitions.ExtensionGroup.Name, ExtensionInstallDefinitions.InstallCommand.Name, PackageId,
            ExtensionInstallDefinitions.Source.Name, source.Path, ExtensionInstallDefinitions.Automatic.Name];
        var installed = await workspace.RunAsync(arguments);
        Assert.True(installed.Status == CliSemanticStatus.Complete, installed.StandardOutput + installed.StandardError);
        Assert.True(File.Exists(workspace.Combine(InstalledNote)));
        Assert.False(File.Exists(workspace.Combine(PackagedNote)));
        Assert.Equal(source.ReadPayloadBytes(PackageId, Skill), File.ReadAllBytes(workspace.Combine(Skill)));
        var parsed = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(workspace.ReadText(InstalledNote)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped, parsed.Syntax.AuthoredForm);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        var owned = Assert.Single(ownership.Document.Extensions);
        Assert.Contains(InstalledNote, owned.Paths);
        Assert.Contains(Skill, owned.Paths);
        Assert.DoesNotContain(PackagedNote, owned.Paths);
        var installedBytes = File.ReadAllBytes(workspace.Combine(InstalledNote));
        var repeated = await workspace.RunAsync(arguments);
        Assert.True(repeated.Status == CliSemanticStatus.Complete, repeated.StandardOutput + repeated.StandardError);
        Assert.Equal(installedBytes, File.ReadAllBytes(workspace.Combine(InstalledNote)));
        Assert.Equal(settingsBytes, File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(sourceBytes, source.ReadPayloadBytes(PackageId, PackagedNote));
    }
}

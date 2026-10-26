using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

[Trait("Feature", "extension-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionRemoveMainRootIntegrationTests
{
    private const string Fixture = "extension-remove-layout";
    private const string MainRoot = "docs/forge";
    private const string PackageId = "toolkit";
    private const string Note = "package-note" + SourceFormClassifier.MarkdownExtension;
    private const string PackagedNote = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string InstalledNote = MainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string Skill = WorkspaceLayout.NativeSkillsPath + "/" + Fixture + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string UserNote = MainRoot + "/personal" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "Extension removal layout fixture";
    private const string Body = "# Preserved workspace note\n";

    [Theory(DisplayName = "Extension Remove deletes mapped owned content and native Skills while preserving the configured root")]
    [InlineData(WorkspaceSettingsDefinitions.FrontmatterRootName), InlineData(WorkspaceSettingsDefinitions.FrontmatterScopedName)]
    public async Task RemoveAndRepeat(string form)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(Fixture, MainRoot);
        workspace.CreateDirectory(WorkspaceSettingsDefinitions.DirectoryName);
        workspace.CreateOccupant(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","{{WorkspaceSettingsDefinitions.FrontmatterProperty}}":"{{form}}"}
            """);
        await workspace.SeedFrameworkAsync();
        workspace.CreateOccupant(UserNote, OpenForgeDocumentSeed.Metadata(Description, [], Body));
        var userBytes = File.ReadAllBytes(workspace.Combine(UserNote));
        using var source = ExtensionInstallCatalogue.Create(Fixture);
        source.AddPackage(PackageId, [], (PackagedNote, OpenForgeDocumentSeed.Metadata(Description, [], Body)),
            (Skill, OpenForgeDocumentSeed.Skill(Fixture, Description)));
        var installed = await workspace.RunAsync([ExtensionDefinitions.ExtensionGroup.Name, ExtensionInstallDefinitions.InstallCommand.Name,
            PackageId, ExtensionInstallDefinitions.Source.Name, source.Path, ExtensionInstallDefinitions.Automatic.Name]);
        Assert.True(installed.Status == CliSemanticStatus.Complete, installed.StandardOutput + installed.StandardError);
        var sourceBytes = source.ReadPayloadBytes(PackageId, PackagedNote);
        string[] arguments = [ExtensionDefinitions.ExtensionGroup.Name, ExtensionRemoveDefinitions.RemoveCommand.Name,
            PackageId, ExtensionRemoveDefinitions.Automatic.Name];
        var removed = await workspace.RunAsync(arguments);
        Assert.True(removed.Status == CliSemanticStatus.Complete, removed.StandardOutput + removed.StandardError);
        Assert.False(File.Exists(workspace.Combine(InstalledNote)));
        Assert.False(File.Exists(workspace.Combine(Skill)));
        Assert.Equal(userBytes, File.ReadAllBytes(workspace.Combine(UserNote)));
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(MainRoot, settings.Document.MainRoot);
        Assert.Contains(PackageId, settings.Document.RemovedExtensions);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Empty(ownership.Document.Extensions);
        var settingsBytes = File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var repeated = await workspace.RunAsync(arguments);
        Assert.True(repeated.Status == CliSemanticStatus.Complete, repeated.StandardOutput + repeated.StandardError);
        Assert.Equal(settingsBytes, File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(userBytes, File.ReadAllBytes(workspace.Combine(UserNote)));
        Assert.Equal(sourceBytes, source.ReadPayloadBytes(PackageId, PackagedNote));
    }
}

using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

[Trait("Feature", "extension-update"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionUpdateMainRootIntegrationTests
{
    private const string Fixture = "extension-update-layout";
    private const string MainRoot = "docs/forge";
    private const string PackageId = "toolkit";
    private const string Note = "package-note" + SourceFormClassifier.MarkdownExtension;
    private const string PackagedNote = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string InstalledNote = MainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string Description = "Extension update layout fixture";
    private const string OriginalBody = "# Original package note\n";
    private const string UpdatedBody = "# Updated package note\n";

    [Theory(DisplayName = "Extension Update uses the mapped destination and preserves package coordinates through a repeat")]
    [InlineData(false), InlineData(true)]
    public async Task UpdateAndRepeat(bool rootForm)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(Fixture, MainRoot);
        workspace.CreateDirectory(WorkspaceSettingsDefinitions.DirectoryName);
        var form = rootForm ? WorkspaceSettingsDefinitions.FrontmatterRootName : WorkspaceSettingsDefinitions.FrontmatterScopedName;
        workspace.CreateOccupant(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","{{WorkspaceSettingsDefinitions.FrontmatterProperty}}":"{{form}}"}
            """);
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(Fixture);
        source.AddPackage(PackageId, [], (PackagedNote, OpenForgeDocumentSeed.Metadata(Description, [], OriginalBody)));
        var seeded = await workspace.RunAsync([ExtensionDefinitions.ExtensionGroup.Name, ExtensionInstallDefinitions.InstallCommand.Name,
            PackageId, ExtensionInstallDefinitions.Source.Name, source.Path, ExtensionInstallDefinitions.Automatic.Name]);
        Assert.True(seeded.Status == CliSemanticStatus.Complete, seeded.StandardOutput + seeded.StandardError);
        var original = File.ReadAllBytes(workspace.Combine(InstalledNote));
        source.ReplacePayload(PackageId, PackagedNote, OpenForgeDocumentSeed.Metadata(Description, [], UpdatedBody));
        var sourceBytes = source.ReadPayloadBytes(PackageId, PackagedNote);
        string[] arguments = [ExtensionDefinitions.ExtensionGroup.Name, ExtensionUpdateDefinitions.UpdateCommand.Name, PackageId,
            ExtensionUpdateDefinitions.Source.Name, source.Path, ExtensionUpdateDefinitions.Automatic.Name];
        var updated = await workspace.RunAsync(arguments);
        Assert.True(updated.Status == CliSemanticStatus.Complete, updated.StandardOutput + updated.StandardError);
        var bytes = File.ReadAllBytes(workspace.Combine(InstalledNote));
        Assert.False(original.AsSpan().SequenceEqual(bytes));
        Assert.False(File.Exists(workspace.Combine(PackagedNote)));
        var parsed = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(workspace.ReadText(InstalledNote)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped, parsed.Syntax.AuthoredForm);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Contains(InstalledNote, Assert.Single(ownership.Document.Extensions).Paths);
        var repeated = await workspace.RunAsync(arguments);
        Assert.True(repeated.Status == CliSemanticStatus.Complete, repeated.StandardOutput + repeated.StandardError);
        Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(InstalledNote)));
        Assert.Equal(sourceBytes, source.ReadPayloadBytes(PackageId, PackagedNote));
    }
}

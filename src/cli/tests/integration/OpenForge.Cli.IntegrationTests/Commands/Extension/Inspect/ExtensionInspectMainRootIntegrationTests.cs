using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Inspect;

[Trait("Feature", "extension-inspect"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ExtensionInspectMainRootIntegrationTests
{
    private const string Fixture = "extension-observers-layout";
    private const string MainRoot = "docs/forge";
    private const string PackageId = "toolkit";
    private const string Note = "package-note" + SourceFormClassifier.MarkdownExtension;
    private const string PackagedNote = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string InstalledNote = MainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string Skill = WorkspaceLayout.NativeSkillsPath + "/" + Fixture + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string Description = "Extension observation layout fixture";
    private const string Body = "# Package note\n";
    private const string InvalidSettings = "{";

    [Theory(DisplayName = "Extension Inspect and List compare mapped content and fixed Skills without changing bytes")]
    [InlineData(WorkspaceSettingsDefinitions.FrontmatterRootName), InlineData(WorkspaceSettingsDefinitions.FrontmatterScopedName)]
    public async Task InspectAndList(string form)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(Fixture, MainRoot);
        workspace.CreateDirectory(WorkspaceSettingsDefinitions.DirectoryName);
        workspace.CreateOccupant(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","{{WorkspaceSettingsDefinitions.FrontmatterProperty}}":"{{form}}"}
            """);
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(Fixture);
        source.AddPackage(PackageId, [], (PackagedNote, OpenForgeDocumentSeed.Metadata(Description, [], Body)),
            (Skill, OpenForgeDocumentSeed.Skill(Fixture, Description)));
        var installed = await workspace.RunAsync([ExtensionDefinitions.ExtensionGroup.Name, ExtensionInstallDefinitions.InstallCommand.Name,
            PackageId, ExtensionInstallDefinitions.Source.Name, source.Path, ExtensionInstallDefinitions.Automatic.Name]);
        Assert.True(installed.Status == CliSemanticStatus.Complete, installed.StandardOutput + installed.StandardError);
        var before = Directory.GetFiles(workspace.Workspace.LexicalRoot, "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
        var sourceBefore = Directory.GetFiles(source.Path, "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
        var inspectRequest = new ExtensionInspectRequest(workspace.Workspace, PackageId, source.Path);
        var inspect = await ExtensionInspectOperationFactory.Create().ExecuteAsync(inspectRequest, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, inspect.Status);
        Assert.Equal(ExtensionInspectComparisonState.Complete, inspect.Comparison.State);
        Assert.Equal(new[] { InstalledNote, Skill }.Order(), inspect.Comparison.Paths.Select(path => path.Path).Order());
        Assert.All(inspect.Comparison.Paths, path => Assert.Equal(ExtensionInspectPathRelation.Unchanged, path.Relation));
        var payload = Assert.IsType<ExtensionInspectAvailablePackage>(inspect.Available.Package).Payload;
        var note = Assert.Single(payload, file => file.TargetPath == InstalledNote);
        Assert.Equal(ExtensionPackageLayout.ContentPathPrefix + PackagedNote, note.Path);
        Assert.Contains(payload, file => file.TargetPath == Skill && file.Path == ExtensionPackageLayout.ContentPathPrefix + Skill);
        var listRequest = new ExtensionListRequest
        {
            Workspace = workspace.Workspace,
            Selection = ExtensionListSelection.Create(installedFlag: true, availableFlag: false),
            ExplicitSource = source.Path,
        };
        var list = await ExtensionListOperationFactory.Create().ExecuteAsync(listRequest, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, list.Status);
        var row = Assert.Single(list.Installed);
        Assert.Equal(ExtensionListInstalledCoverage.Complete, row.Coverage);
        Assert.All(row.Files, file => Assert.Equal(ExtensionListInstalledFileState.Current, file.State));
        Assert.Equal(new[] { InstalledNote, Skill }.Order(), row.Files.Select(file => file.Path).Order());
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(workspace.Workspace.LexicalRoot, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
        foreach (var (path, bytes) in sourceBefore) Assert.Equal(bytes, File.ReadAllBytes(path));

        File.WriteAllText(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath), InvalidSettings);
        var unavailable = await ExtensionInspectOperationFactory.Create().ExecuteAsync(inspectRequest, TestContext.Current.CancellationToken);
        Assert.Equal(ExtensionInspectComparisonState.Incomplete, unavailable.Comparison.State);
        Assert.Empty(unavailable.Comparison.Current.Fingerprints);
        Assert.Empty(unavailable.Comparison.Intended.Fingerprints);
        Assert.All(unavailable.Comparison.Paths, path => Assert.Equal(ExtensionInspectPathRelation.Unavailable, path.Relation));
        Assert.All(Assert.IsType<ExtensionInspectAvailablePackage>(unavailable.Available.Package).Payload, file => Assert.Null(file.TargetPath));
        var unavailableList = await ExtensionListOperationFactory.Create().ExecuteAsync(listRequest, TestContext.Current.CancellationToken);
        Assert.Equal(ExtensionListInstalledCoverage.Incomplete, Assert.Single(unavailableList.Installed).Coverage);
        foreach (var (path, bytes) in before.Where(pair => pair.Key != workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)))
            Assert.Equal(bytes, File.ReadAllBytes(path));
    }
}

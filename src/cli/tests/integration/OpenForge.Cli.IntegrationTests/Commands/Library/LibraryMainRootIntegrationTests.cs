using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Reading;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library;

[Trait("Feature", "library-mutation"), Trait("Evidence", "Integration"), Trait("Boundary", "Filesystem")]
public sealed class LibraryMainRootIntegrationTests
{
    private const string MainRoot = "docs/forge";
    private const string Category = "topics";
    private const string Name = "review";
    private const string SourceId = Category + "/" + Name;
    private const string Suffix = SourceId + SourceFormClassifier.MarkdownExtension;
    private const string Metadata = "---\n" + FrameworkMetadataFieldReader.Description + ": Review knowledge\n"
        + FrameworkMetadataFieldReader.Tags + ": [Knowledge]\n---\n# Review\n";
    private const string Entries = "\n## " + MarkdownEntriesSectionReader.EntriesHeadingText + "\n";
    private const string Loader = "# Loader\n" + Entries;
    private const string InvalidSettings = "{";

    [Fact]
    public async Task PlanningUsesConsumerIdentityAndFixedSkillsWithoutRewritingSuffixes()
    {
        using var workspace = Prepare();
        var before = workspace.Snapshot();
        var request = workspace.Attach(LibraryMode.DryRun) with { DestinationRoot = LibraryDestinationRoot.Create(MainRoot) };
        var operation = new LibraryAttachOperation(workspace.Permissions);
        var planned = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, planned.Status);
        var mapping = Assert.Single(planned.Result.Projection.Mappings);
        Assert.Equal(Suffix, mapping.SourcePath);
        Assert.Equal(MainRoot + "/" + Suffix, mapping.DestinationPath);
        Assert.Equal(SourceId, mapping.SourceId);
        Assert.Equal(SourceId, Assert.Single(planned.Result.Source.EligiblePaths).SourceId);
        Assert.Contains(planned.Result.Plan.GeneratedRegions,
            region => region.Path == MainRoot + "/" + Category + "/" + SourceFormClassifier.CanonicalEntrypointFileName(Category));
        Assert.Equal(before, workspace.Snapshot());

        var skills = await operation.ExecuteAsync(request with
        {
            DestinationRoot = LibraryDestinationRoot.Create(WorkspaceLayout.NativeSkillsPath),
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, skills.Status);
        Assert.Equal(WorkspaceLayout.NativeSkillsPath + "/" + Suffix, Assert.Single(skills.Result.Plan.Links).Path);
        Assert.Equal(before, workspace.Snapshot());

        var oldRoot = await operation.ExecuteAsync(request with
        {
            DestinationRoot = LibraryDestinationRoot.Create(WorkspaceLayout.DefaultMainRoot),
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, oldRoot.Status);
        Assert.Contains(oldRoot.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.PermissionRequired);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact]
    public async Task ReadOnlyObserversKeepLiteralRegistrationAndDoNotInventIdentityForInvalidSettings()
    {
        using var workspace = Prepare();
        workspace.RecordAt(MainRoot, Suffix);
        var destination = MainRoot + "/" + Suffix;
        var relativeTarget = Path.GetRelativePath(Path.GetDirectoryName(workspace.Absolute(destination))!,
            workspace.Absolute(LibraryMutationWorkspace.SourceRoot + "/" + Suffix));
        workspace.RawLink(destination, relativeTarget);
        var before = workspace.Snapshot();
        var inspectRequest = new LibraryInspectRequest { Workspace = workspace.Workspace, LibraryId = workspace.Attach().LibraryId };
        var listRequest = new LibraryListRequest { Workspace = workspace.Workspace };
        var inspected = await new LibraryInspectOperation().ExecuteAsync(inspectRequest, TestContext.Current.CancellationToken);
        var listed = await new LibraryListOperation().ExecuteAsync(listRequest, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, inspected.Status);
        Assert.Equal(SourceId, Assert.Single(inspected.Result.Projection.Comparisons).SourceId);
        var library = Assert.Single(listed.Result.Libraries);
        Assert.Equal(MainRoot, library.DestinationRoot);
        Assert.Equal(LibraryMutationWorkspace.SourceRoot, library.SourceRoot);
        Assert.Equal(Suffix, Assert.Single(library.Paths).SourcePath);
        Assert.Equal(SourceId, Assert.Single(library.Paths).SourceId);
        Assert.Equal(before, workspace.Snapshot());

        workspace.Replace(WorkspaceSettingsDefinitions.RelativePath, InvalidSettings);
        var invalidBefore = workspace.Snapshot();
        var unavailableInspect = await new LibraryInspectOperation().ExecuteAsync(inspectRequest, TestContext.Current.CancellationToken);
        var unavailableList = await new LibraryListOperation().ExecuteAsync(listRequest, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, unavailableInspect.Status);
        Assert.Equal(CliSemanticStatus.Incomplete, unavailableList.Status);
        Assert.Contains(unavailableInspect.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.LayoutUnavailable);
        Assert.Contains(unavailableList.Result.Findings, finding => finding.Code == LibraryListFindingCode.LayoutUnavailable);
        Assert.Null(Assert.Single(unavailableInspect.Result.Projection.Comparisons).SourceId);
        Assert.Null(Assert.Single(Assert.Single(unavailableList.Result.Libraries).Paths).SourceId);
        Assert.Equal(invalidBefore, workspace.Snapshot());
    }

    [Fact]
    public async Task MismatchedLoaderBlocksBeforeAnyMutation()
    {
        using var workspace = Prepare();
        workspace.Write(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Loader);
        var before = workspace.Snapshot(includeRuntime: true);
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply) with { DestinationRoot = LibraryDestinationRoot.Create(MainRoot) },
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.LayoutUnavailable);
        Assert.Equal(before, workspace.Snapshot(includeRuntime: true));
    }

    private static LibraryMutationWorkspace Prepare()
    {
        var workspace = new LibraryMutationWorkspace(createLoader: false);
        workspace.Write(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}"}
            """);
        workspace.Write(MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Loader);
        workspace.Write(MainRoot + "/" + Category + "/" + SourceFormClassifier.CanonicalEntrypointFileName(Category), Metadata + Entries);
        workspace.Write(LibraryMutationWorkspace.SourceRoot + "/" + Suffix, Metadata);
        return workspace;
    }
}

using OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexOutsideIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Index preserves an authored conditioned leaving row without maintaining its registered destination")]
    public async Task LeavingRowIsRetained()
    {
        using var fixture = new OutsideMaintenanceWorkspace(anchor: true);
        var destination = Path.GetRelativePath(OutsideMaintenanceWorkspace.Route, OutsideMaintenanceWorkspace.OtherNote).Replace(Path.DirectorySeparatorChar, '/');
        fixture.Files.ReplaceText(OutsideMaintenanceWorkspace.Anchor,
            OpenForge.Cli.TestSupport.GeneratedLoaderDocumentBuilder.Build($"- [Independent foreign policy]({destination}) - #Authored - applies to `foreign/**`") + "\n");
        var parser = new OpenForge.Cli.Core.Framework.Documents.Markdown.MarkdownDocumentParser();
        var beforeDocument = parser.Parse(System.Text.Encoding.UTF8.GetString(fixture.Read(OutsideMaintenanceWorkspace.Anchor)));
        var beforeRow = Assert.Single(OpenForge.Cli.Core.Framework.Sources.Loading.SourceGeneratedEntriesParser.Parse(beforeDocument).Entries);
        var beforeBytes = System.Text.Encoding.UTF8.GetBytes(beforeDocument.Source[beforeRow.Span.Start..beforeRow.Span.End]);
        var other = fixture.Read(OutsideMaintenanceWorkspace.OtherNote);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + OutsideMaintenanceWorkspace.Anchor], IndexMode.Apply), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var afterDocument = parser.Parse(System.Text.Encoding.UTF8.GetString(fixture.Read(OutsideMaintenanceWorkspace.Anchor)));
        var afterRow = Assert.Single(OpenForge.Cli.Core.Framework.Sources.Loading.SourceGeneratedEntriesParser.Parse(afterDocument).Entries, entry => entry.Destination == destination);
        Assert.Equal(beforeBytes, System.Text.Encoding.UTF8.GetBytes(afterDocument.Source[afterRow.Span.Start..afterRow.Span.End]));
        Assert.Equal(other, fixture.Read(OutsideMaintenanceWorkspace.OtherNote));
        Assert.DoesNotContain(result.Metadata, item => item.Path == OutsideMaintenanceWorkspace.OtherNote);
    }

    [Fact(DisplayName = "Explicit outside Index cannot bypass an authored file exclusion after completion declines it")]
    public async Task ExcludedSourceIsReadOnly()
    {
        using var fixture = new OutsideMaintenanceWorkspace();
        fixture.Files.ReplaceText(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{OutsideMaintenanceWorkspace.Route}}"],"{{OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RemovedFilesProperty}}":["{{OutsideMaintenanceWorkspace.Note}}"]}""");
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + OutsideMaintenanceWorkspace.Note], IndexMode.DryRun), Token);
        Assert.Contains(result.Findings, finding => finding.Code == IndexFindingCode.SourceUnsafe);
        Assert.Empty(result.Regions);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
    }

    [Fact(DisplayName = "A listed missing outside directory is refused without granting scaffold admission")]
    public async Task ListedMissingDirectory()
    {
        using var fixture = new OutsideMaintenanceWorkspace(listed: true);
        Directory.Move(fixture.Files.Combine(OutsideMaintenanceWorkspace.Route), fixture.Files.Combine("held-team"));
        try
        {
            var before = fixture.Files.SnapshotHashes(includeRuntime: true);
            var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace, [], IndexMode.DryRun), Token);
            Assert.Contains(result.Findings, finding => finding.Code == IndexFindingCode.SourceUnsafe);
            Assert.Empty(result.Regions);
            Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        }
        finally
        {
            Directory.Move(fixture.Files.Combine("held-team"), fixture.Files.Combine(OutsideMaintenanceWorkspace.Route));
        }
    }

    [Theory(DisplayName = "Unrelated missing or unsafe registrations neither expand nor block explicit outside Index maintenance")]
    [InlineData(false), InlineData(true)]
    public async Task UnrelatedRegistration(bool wrongKind)
    {
        using var fixture = new OutsideMaintenanceWorkspace();
        const string unrelated = "unavailable";
        fixture.Files.ReplaceText(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{OutsideMaintenanceWorkspace.Route}}","{{unrelated}}"]}""");
        if (wrongKind) fixture.Files.CreateFile(unrelated, OutsideMaintenanceWorkspace.Body);
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + OutsideMaintenanceWorkspace.Note], IndexMode.DryRun), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        Assert.All(result.Entrypoints, entrypoint => Assert.StartsWith(OutsideMaintenanceWorkspace.Route + "/", entrypoint.Path, StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Outside alias operands cannot redirect Index into another physical coordinate")]
    [InlineData(false), InlineData(true)]
    public async Task AliasRefusal(bool escape)
    {
        using var fixture = new OutsideMaintenanceWorkspace();
        var aliasDirectory = OutsideMaintenanceWorkspace.Route + "/a-alias";
        fixture.Files.CreateDirectorySymbolicLink(aliasDirectory,
            fixture.Files.Combine(escape ? OutsideMaintenanceWorkspace.Other : OutsideMaintenanceWorkspace.Route + "/nested"));
        var alias = aliasDirectory + "/note" + SourceFormClassifier.MarkdownExtension;
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + alias], IndexMode.DryRun), Token);
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Regions);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
    }

    [Fact(DisplayName = "A file include never admits Index maintenance or sibling discovery")]
    public async Task FileIncludeIsReadOnly()
    {
        using var fixture = new OutsideMaintenanceWorkspace();
        const string path = "foreign/note.md";
        fixture.Files.CreateFile(path, OutsideMaintenanceWorkspace.Body);
        fixture.Files.ReplaceText(fixture.Loader, OpenForge.Cli.TestSupport.GeneratedLoaderDocumentBuilder.Build("- [Foreign](../foreign/note.md) - #Authored"));
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + path], IndexMode.DryRun), Token);
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Regions);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Bare and Loader Index maintain only listed outside roots and preserve conditioned rows")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false), InlineData(OutsideMaintenanceWorkspace.CustomRoot, false, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false), InlineData(OutsideMaintenanceWorkspace.CustomRoot, true, true)]
    public async Task ListedRoots(string mainRoot, bool listed, bool explicitLoader)
    {
        using var fixture = new OutsideMaintenanceWorkspace(mainRoot, listed: listed);
        var note = fixture.Read(OutsideMaintenanceWorkspace.Note);
        var other = fixture.Read(OutsideMaintenanceWorkspace.OtherNote);
        var loader = fixture.Read(fixture.Loader);
        var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
            explicitLoader ? [SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Loader] : [], IndexMode.Apply), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(listed, File.Exists(fixture.Files.Combine(OutsideMaintenanceWorkspace.Anchor)));
        Assert.Equal(other, fixture.Read(OutsideMaintenanceWorkspace.OtherNote));
        Assert.Equal(loader, fixture.Read(fixture.Loader));
        if (!listed) Assert.Equal(note, fixture.Read(OutsideMaintenanceWorkspace.Note));
        Assert.DoesNotContain(result.Regions, region => region.Source.Path.StartsWith(OutsideMaintenanceWorkspace.Other + "/", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Exact outside leaves and catalogues complete their unlisted chain with or without Loader")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false), InlineData(OutsideMaintenanceWorkspace.CustomRoot, true, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true), InlineData(OutsideMaintenanceWorkspace.CustomRoot, false, false)]
    public async Task ExplicitUnlisted(string mainRoot, bool loader, bool catalogue)
    {
        using var fixture = new OutsideMaintenanceWorkspace(mainRoot, loader: loader);
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var other = fixture.Read(OutsideMaintenanceWorkspace.OtherNote);
        var loaderBytes = loader ? fixture.Read(fixture.Loader) : [];
        var path = catalogue ? OutsideMaintenanceWorkspace.Anchor : OutsideMaintenanceWorkspace.Note;
        var references = new[] { SourceWorkspaceRelativePath.LiteralReferencePrefix + path };
        var operation = IndexOperationFactory.Create();
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, references, IndexMode.DryRun), Token);
        Assert.True(preview.Status == CliSemanticStatus.Complete, string.Join("\n", preview.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        Assert.Contains(preview.Entrypoints, entrypoint => entrypoint.Path == OutsideMaintenanceWorkspace.Anchor);
        Assert.Equal(OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.RoutedMetadataDefaults.Tags(
            OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models.RoutedMetadataCategory.Workspace),
            Assert.Single(preview.Metadata, item => item.Path == OutsideMaintenanceWorkspace.Note).Tags);
        var applied = await operation.ExecuteAsync(new(fixture.Workspace, references, IndexMode.Apply), Token);
        Assert.True(applied.Status == CliSemanticStatus.Complete, string.Join("\n", applied.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.True(File.Exists(fixture.Files.Combine(OutsideMaintenanceWorkspace.Anchor)));
        Assert.Equal(other, fixture.Read(OutsideMaintenanceWorkspace.OtherNote));
        if (loader) Assert.Equal(loaderBytes, fixture.Read(fixture.Loader));
        else Assert.False(File.Exists(fixture.Files.Combine(fixture.Loader)));
        Assert.True(fixture.Read(OutsideMaintenanceWorkspace.Note).AsSpan().EndsWith(System.Text.Encoding.UTF8.GetBytes(OutsideMaintenanceWorkspace.Body)));
        var after = fixture.Files.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(new(fixture.Workspace, references, IndexMode.Apply), Token);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Metadata);
        Assert.Empty(repeat.Entrypoints);
        Assert.Equal(0, repeat.Counts.Updates);
        Assert.Equal(after, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Index validates retained outside settings directory and anchor facts under its lease")]
    [InlineData(0), InlineData(1), InlineData(2)]
    public async Task RetainedObservation(int change)
    {
        using var fixture = new OutsideMaintenanceWorkspace(anchor: true);
        var read = await new IndexProjectionReader(new PhysicalPathResolver()).ReadAsync(new(fixture.Workspace,
            [SourceWorkspaceRelativePath.LiteralReferencePrefix + OutsideMaintenanceWorkspace.Anchor], IndexMode.DryRun), Token);
        var observation = Assert.IsType<OpenForge.Cli.Core.Framework.Settings.Models.Observation.WorkspaceLayoutRead>(read.Projection?.LayoutRead);
        if (change == 0) fixture.Files.ReplaceText(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath, "{}");
        if (change == 1) Directory.Move(fixture.Files.Combine(OutsideMaintenanceWorkspace.Route), fixture.Files.Combine("moved"));
        if (change == 2) fixture.Files.ReplaceText(OutsideMaintenanceWorkspace.Anchor, OutsideMaintenanceWorkspace.Body);
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new(fixture.Workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), Token);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        try
        {
            Assert.NotEqual(MutationValidationState.Valid,
                (await new WorkspaceLayoutRevalidator(new PhysicalPathResolver()).ValidateAsync(lease, observation, Token)).State);
        }
        finally
        {
            if (change == 1) Directory.Move(fixture.Files.Combine("moved"), fixture.Files.Combine(OutsideMaintenanceWorkspace.Route));
        }
    }
}

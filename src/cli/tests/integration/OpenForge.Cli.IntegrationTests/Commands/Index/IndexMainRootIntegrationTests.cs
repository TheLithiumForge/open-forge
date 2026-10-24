using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexMainRootIntegrationTests
{
    private const string MainRoot = "handbook/forge";
    private const string OtherRoot = "handbook/other";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string RouteDirectory = "notes";
    private const string CatalogueName = "_notes.md";
    private const string Catalogue = MainRoot + "/" + RouteDirectory + "/" + CatalogueName;
    private const string Note = MainRoot + "/" + RouteDirectory + "/note.md";
    private const string PrivateRouteName = "private";
    private const string PrivateCatalogueName = "_private.md";
    private const string PrivateDirectory = MainRoot + "/" + PrivateRouteName;
    private const string PrivateCatalogue = PrivateDirectory + "/" + PrivateCatalogueName;
    private const string PrivateNote = PrivateDirectory + "/secret.md";
    private const string PrivateNested = PrivateDirectory + "/nested/_nested.md";
    private const string Description = "Workspace notes";
    private const string PrivateDescription = "Private notes";
    private const string Tag = "Docs";
    private const string Stale = "- stale";
    private const string Body = "# Authored note\r\n\r\nKeep these bytes.\r\n";
    private const string Owner = "fixture";

    public enum ChangedEvidence { Settings, DefaultLoader, Host, SelectedLoader, Ownership }

    [Theory(DisplayName = "Index rejects changed custom-layout evidence before target or recovery effects")]
    [InlineData(ChangedEvidence.Settings)]
    [InlineData(ChangedEvidence.DefaultLoader)]
    [InlineData(ChangedEvidence.Host)]
    [InlineData(ChangedEvidence.SelectedLoader)]
    [InlineData(ChangedEvidence.Ownership)]
    public async Task ChangedLayoutStopsApplication(ChangedEvidence changed)
    {
        using var temporary = TemporaryWorkspace.Create("index-main-root-stale");
        using var dataHome = TestDataHome.Create("index-main-root-stale-home");
        Seed(temporary);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new IndexRequest(workspace, [], IndexMode.Apply);
        var paths = new PhysicalPathResolver();
        var reader = new IndexProjectionReader(paths);
        var read = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.NotNull(read.Projection);
        Assert.NotNull(read.Projection.LayoutRead);
        var plan = new IndexPlanBuilder().Build(new IndexPlanningInput
        {
            Request = request,
            Projection = read.Projection,
            LayoutRead = read.Projection.LayoutRead,
        });
        Assert.True(plan.IsExecutable);
        Assert.False(plan.IsNoOp);
        Assert.Same(read.Projection.LayoutRead, plan.LayoutRead);
        switch (changed)
        {
            case ChangedEvidence.Settings:
                temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot));
                break;
            case ChangedEvidence.DefaultLoader:
                temporary.WriteText(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Body);
                break;
            case ChangedEvidence.Host:
                temporary.WriteText(WorkspaceLayoutNames.AgentHostFileName, Body);
                break;
            case ChangedEvidence.SelectedLoader:
                temporary.ReplaceText(Loader, Body);
                break;
            case ChangedEvidence.Ownership:
                temporary.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(changed));
        }
        var before = temporary.SnapshotHashes();
        var preserved = new[] { Catalogue, Note }.ToDictionary(path => path, path => File.ReadAllBytes(temporary.Combine(path)));
        var validator = new FileExpectationValidator(paths);
        var revalidator = new MutationRevalidator(validator);
        var application = new IndexApplicationOperation(new WorkspaceLockManager(WorkspaceLockStoreRoot.FromAbsolutePath(dataHome.LockStoreRoot)),
            revalidator, new FileChangeApplier(revalidator, validator), reader);

        var result = await application.ExecuteAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(IndexFindingCode.TargetChanged, Assert.Single(result.Findings).Code);
        Assert.Equal(IndexRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, temporary.SnapshotHashes());
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
        Assert.All(result.Regions.Where(region => region.Action == IndexRegionAction.Update),
            region => Assert.Equal(IndexRegionOutcome.NotStarted, region.Outcome));
    }

    [Fact(DisplayName = "Custom-root Index preserves private routes and their fixed ownership record")]
    public async Task PrivateRouteRemainsPrivate()
    {
        using var temporary = TemporaryWorkspace.Create("index-main-root-private");
        using var dataHome = TestDataHome.Create("index-main-root-private-home");
        Seed(temporary);
        temporary.ReplaceText(Loader, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Description}]({RouteDirectory}/{CatalogueName}) - #{Tag}\n- [{PrivateDescription}]({PrivateRouteName}/{PrivateCatalogueName}) - #{Tag}"));
        temporary.WriteText(PrivateCatalogue, OpenForgeDocumentSeed.Metadata(PrivateDescription, [Tag], OpenForgeDocumentSeed.GeneratedEntries(Stale)));
        temporary.WriteText(PrivateNote, Body);
        temporary.WriteText(PrivateNested, Body);
        temporary.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = new FrameworkOwnership(new OwnedSource(Owner, null), [], [])
            {
                GitIgnoredRoutes = [new SourceSharingRoute(PrivateDirectory, PrivateCatalogue)],
            },
        }));
        var preserved = new[] { WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath, PrivateNote, PrivateNested }
            .ToDictionary(path => path, path => File.ReadAllBytes(temporary.Combine(path)));
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var operation = IndexOperationFactory.Create(WorkspaceLockStoreRoot.FromAbsolutePath(dataHome.LockStoreRoot));

        var result = await operation.ExecuteAsync(new(workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Contains(result.Regions, region => region.Source.Path == Catalogue && region.Outcome == IndexRegionOutcome.Verified);
        Assert.DoesNotContain(result.Regions, region => region.Source.Path == PrivateNote || region.Source.Path == PrivateNested);
        var entries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(File.ReadAllText(temporary.Combine(PrivateCatalogue))));
        Assert.Equal(SourceGeneratedEntriesState.Complete, entries.State);
        Assert.Empty(entries.Entries);
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
        var before = temporary.SnapshotHashes();
        var explicitResult = await operation.ExecuteAsync(new(workspace, [PrivateNote], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(IndexFindingCode.InvalidSource, Assert.Single(explicitResult.Findings).Code);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static void Seed(TemporaryWorkspace workspace)
    {
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries($"- [{Description}]({RouteDirectory}/{CatalogueName}) - #{Tag}"));
        workspace.WriteText(Catalogue, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(Stale)));
        workspace.WriteText(Note, OpenForgeDocumentSeed.Metadata(Description, [Tag], Body));
    }

    private static string Settings(string root)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}""";
}

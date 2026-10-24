using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteMoveMainRootIntegrationTests
{
    private const string MainRoot = "handbook/context";
    private const string OtherRoot = "handbook/other";
    private const string RouteName = "notes";
    private const string LeafName = "old";
    private const string DestinationName = "new";
    private const string SourceId = RouteName + "/" + LeafName;
    private const string DestinationId = RouteName + "/" + DestinationName;
    private const string SourcePath = MainRoot + "/" + SourceId + SourceFormClassifier.MarkdownExtension;
    private const string DestinationPath = MainRoot + "/" + DestinationId + SourceFormClassifier.MarkdownExtension;
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string ParentPath = MainRoot + "/" + RouteName + "/" + SourceFormClassifier.IndexFileName;
    private const string Tag = "Docs";
    private const string Description = "Move fixture";
    private const string FrameworkId = "fixture";
    private const string ChangedBody = "# Changed after planning\n";
    private const string SkillName = "workshop";
    private const string ReferencesFolder = "references";
    private const string SkillsRootCatalogue = WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.IndexFileName;
    private const string SkillParent = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.IndexFileName;
    private const string SkillReferences = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + ReferencesFolder;
    private const string SkillCatalogue = SkillReferences + "/" + SourceFormClassifier.UnderscoreReferencesFileName;
    private const string SkillDestinationId = WorkspaceLayoutNames.SkillsDirectoryName + "/" + SkillName + "/" + ReferencesFolder + "/" + DestinationName;
    private const string SkillDestinationPath = SkillReferences + "/" + DestinationName + SourceFormClassifier.MarkdownExtension;

    public enum LayoutChange { Settings, Loader, DefaultLoader, Host }

    [Fact(DisplayName = "Route Move verifies a custom-root source after moving into a native Skills route")]
    public async Task NativeSkillsDestinationVerifies()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(NativeSkillsDestinationVerifies));
        Seed(temporary);
        temporary.ReplaceText(LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Description}]({RouteName}/{SourceFormClassifier.IndexFileName}) - #{Tag}\n" +
            $"- [{SkillName}](../../{SkillsRootCatalogue}) - #{Tag}"));
        temporary.WriteText(SkillsRootCatalogue, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries($"- [{SkillName}]({SkillName}/{SourceFormClassifier.IndexFileName}) - #{Tag}")));
        temporary.WriteText(SkillParent, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries($"- [{ReferencesFolder}]({ReferencesFolder}/{SourceFormClassifier.UnderscoreReferencesFileName}) - #{Tag}")));
        temporary.WriteText(SkillCatalogue, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        temporary.WriteText(WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName,
            OpenForgeDocumentSeed.Skill(SkillName, Description, ChangedBody));
        var request = Request(temporary, SkillDestinationId);
        using var lockStore = WorkspaceLockTestStore.Create(nameof(NativeSkillsDestinationVerifies));
        _ = lockStore.Track(request.Workspace);
        var bytes = File.ReadAllBytes(temporary.Combine(SourcePath));
        try
        {
            var result = await RouteMoveOperationFactory.Create(lockStore.StoreRoot).ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
            Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(SkillDestinationPath)));
            Assert.False(File.Exists(temporary.Combine(SourcePath)));
        }
        finally
        {
            if (File.Exists(temporary.Combine(SkillDestinationPath))) File.Delete(temporary.Combine(SkillDestinationPath));
        }
    }

    [Fact(DisplayName = "Route Move plans stable IDs and literal paths in a nested custom root")]
    public async Task CustomRootPlansUseTheObservedLayout()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(CustomRootPlansUseTheObservedLayout));
        Seed(temporary);
        var before = temporary.SnapshotHashes();
        var plan = await Plan(temporary, DestinationId);
        var literal = await Plan(temporary, SourceWorkspaceRelativePath.LiteralReferencePrefix + DestinationPath);
        var literalSource = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            Request(temporary, DestinationId, SourceWorkspaceRelativePath.LiteralReferencePrefix + SourcePath),
            TestContext.Current.CancellationToken);
        Assert.Empty(literalSource.Formation.Findings);
        Assert.Equal(SourcePath, Assert.IsType<RouteMovePlan>(literalSource.Plan).Preview.Source.Path);
        Assert.Equal(MainRoot, plan.LayoutRead.Layout?.MainRoot);
        Assert.Same(plan.LayoutRead, plan.Projection.Destination.Inventory.Subject.LayoutRead);
        Assert.Same(plan.LayoutRead.Settings, plan.Projection.Destination.Inventory.Settings);
        Assert.Equal(DestinationId, plan.Preview.Destination.Id);
        Assert.Equal(DestinationPath, plan.Preview.Destination.Path);
        Assert.Equal(RouteName, plan.Preview.Destination.ParentId);
        Assert.Equal(plan.Preview.Effects.Select(effect => effect.Path), literal.Preview.Effects.Select(effect => effect.Path));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move keeps an old-root source operand literal")]
    public async Task OldRootSourceIsNotRedirected()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(OldRootSourceIsNotRedirected));
        Seed(temporary);
        var oldPath = WorkspaceLayout.DefaultMainRoot + "/" + SourceId + SourceFormClassifier.MarkdownExtension;
        temporary.WriteText(oldPath, ChangedBody);
        var before = temporary.SnapshotHashes();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            Request(temporary, DestinationId, SourceWorkspaceRelativePath.LiteralReferencePrefix + oldPath),
            TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.InvalidSubject, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Route Move never redirects a literal old-root path or admits a case alias")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LiteralDestinationsOutsideTheContentRootsAreRefused(bool caseAlias)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(LiteralDestinationsOutsideTheContentRootsAreRefused));
        Seed(temporary);
        var root = caseAlias ? MainRoot.ToUpperInvariant() : WorkspaceLayout.DefaultMainRoot;
        var destination = root + "/" + DestinationId + SourceFormClassifier.MarkdownExtension;
        var before = temporary.SnapshotHashes();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            Request(temporary, SourceWorkspaceRelativePath.LiteralReferencePrefix + destination), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.InvalidDestination, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + destination, build.Formation.Findings[0].Target);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Route Move rejects changed layout evidence while holding the workspace lease")]
    [InlineData(LayoutChange.Settings)]
    [InlineData(LayoutChange.Loader)]
    [InlineData(LayoutChange.DefaultLoader)]
    [InlineData(LayoutChange.Host)]
    public async Task ChangedLayoutEvidencePreventsRevalidation(LayoutChange change)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(ChangedLayoutEvidencePreventsRevalidation));
        Seed(temporary);
        var plan = await Plan(temporary, DestinationId);
        using var lockStore = WorkspaceLockTestStore.Create(nameof(ChangedLayoutEvidencePreventsRevalidation));
        _ = lockStore.Track(plan.Request.Workspace);
        var acquired = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(plan.Request.Workspace, RouteMoveDefinitions.CommandIdentity, Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        switch (change)
        {
            case LayoutChange.Settings: temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot)); break;
            case LayoutChange.Loader: temporary.ReplaceText(LoaderPath, ChangedBody); break;
            case LayoutChange.DefaultLoader: temporary.WriteText(SourceLogicalPath.LoaderPath, ChangedBody); break;
            case LayoutChange.Host: temporary.WriteText(WorkspaceLayoutNames.AgentHostFileName, ChangedBody); break;
            default: throw new ArgumentOutOfRangeException(nameof(change));
        }
        var before = temporary.SnapshotHashes();
        var sourceBytes = File.ReadAllBytes(temporary.Combine(SourcePath));
        var result = await new RouteMovePlanRevalidator(RouteMoveIntegrationWorkspace.CreatePlanBuilder())
            .RevalidateAsync(plan, lease, TestContext.Current.CancellationToken);
        Assert.Equal(RouteMovePlanRevalidationState.Changed, result.State);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(sourceBytes, File.ReadAllBytes(temporary.Combine(SourcePath)));
        Assert.False(File.Exists(temporary.Combine(DestinationPath)));
    }

    private static async Task<RouteMovePlan> Plan(TemporaryWorkspace temporary, string destination)
    {
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            Request(temporary, destination), TestContext.Current.CancellationToken);
        Assert.Empty(build.Formation.Findings);
        return Assert.IsType<RouteMovePlan>(build.Plan);
    }

    private static RouteMoveRequest Request(TemporaryWorkspace temporary, string destination, string source = SourceId)
        => new(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            source, destination, RouteMoveMode.Apply);

    private static void Seed(TemporaryWorkspace temporary)
    {
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        temporary.WriteText(LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Description}]({RouteName}/{SourceFormClassifier.IndexFileName}) - #{Tag}"));
        temporary.WriteText(ParentPath, OpenForgeDocumentSeed.Metadata(Description, [Tag],
            OpenForgeDocumentSeed.GeneratedEntries($"- [{Description}]({LeafName}{SourceFormClassifier.MarkdownExtension}) - #{Tag}")));
        temporary.WriteText(SourcePath, OpenForgeDocumentSeed.Metadata(Description, [Tag], ChangedBody));
        temporary.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with { Framework = new(new(FrameworkId, null), [], []) }));
    }

    private static string Settings(string root)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}""";
}

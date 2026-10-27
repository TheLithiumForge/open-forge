using OpenForge.Cli.Core.Framework.Sources.Loading;
using System.Text.Json.Nodes;
using System.IO.Compression;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteMoveOutsideSafetyIntegrationTests
{
    [Theory(DisplayName = "Outside maintenance refuses cross registration primary native and case alias destinations")]
    [InlineData(false, 0), InlineData(true, 0), InlineData(false, 1), InlineData(true, 1)]
    [InlineData(false, 2), InlineData(true, 2), InlineData(false, 3), InlineData(true, 3), InlineData(false, 4), InlineData(true, 4)]
    public async Task Boundaries(bool custom, int kind)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var destination = kind switch
        {
            0 => workspace.MainRoot + "/notes/new.md",
            1 => "docs/other/new.md",
            2 => "DOCS/team/right/new.md",
            3 => "docs/team-other/right/new.md",
            4 => workspace.MainRoot + "/skills/workshop/references/new.md",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        workspace.ChangeSettings(settings => settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray(RouteMoveOutsideWorkspace.Root, "docs/other"));
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(destination: destination), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.InvalidDestination, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Canonical and compatibility registration anchors cannot be moved through base or overwrite operands")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task Anchors(bool custom, bool compatibility)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom, compatibility);
        workspace.Write(SourceOverwritePath.ReadAdjacentPath(workspace.Anchor), "Anchor companion.\n");
        foreach (var source in new[] { workspace.Anchor, SourceOverwritePath.ReadAdjacentPath(workspace.Anchor) })
        {
            var before = workspace.Snapshot();
            var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(source: source), TestContext.Current.CancellationToken);
            Assert.Null(build.Plan);
            Assert.Equal(RouteMoveFindingCode.InvalidSubject, Assert.Single(build.Formation.Findings).Code);
            Assert.Equal(before, workspace.Snapshot());
        }
    }

    [Theory(DisplayName = "Every additional recognized outside root anchor remains protected")]
    [InlineData(false, SourceFormClassifier.UnderscoreIndexFileName), InlineData(true, SourceFormClassifier.UnderscoreIndexFileName)]
    [InlineData(false, SourceFormClassifier.ReferencesFileName), InlineData(true, SourceFormClassifier.ReferencesFileName)]
    [InlineData(false, SourceFormClassifier.UnderscoreReferencesFileName), InlineData(true, SourceFormClassifier.UnderscoreReferencesFileName)]
    public async Task OtherAnchors(bool custom, string name)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var original = workspace.Read(workspace.Anchor);
        File.Delete(workspace.Absolute(workspace.Anchor));
        var anchor = RouteMoveOutsideWorkspace.Root + "/" + name;
        workspace.Write(anchor, original);
        workspace.Write(SourceOverwritePath.ReadAdjacentPath(anchor), "Anchor companion.\n");
        foreach (var source in new[] { RouteMoveOutsideWorkspace.Root, anchor, SourceOverwritePath.ReadAdjacentPath(anchor) })
        {
            var before = workspace.Snapshot();
            var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(source: source), TestContext.Current.CancellationToken);
            Assert.Null(build.Plan);
            Assert.NotEmpty(build.Formation.Findings);
            Assert.Equal(before, workspace.Snapshot());
        }
    }

    [Theory(DisplayName = "Required foreign prose rewrites refuse the whole outside plan")]
    [InlineData(false), InlineData(true)]
    public async Task ForeignIncoming(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        workspace.Write("foreign.md", "[Incoming](docs/team/left/old%20note.md)\n");
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        var finding = Assert.Single(build.Formation.Findings);
        Assert.Equal(RouteMoveFindingCode.ReferenceUnsafe, finding.Code);
        Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "A retained Loader include cannot authorize an outside move to rewrite the Loader")]
    [InlineData(false), InlineData(true)]
    public async Task ForeignIncludedRow(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var link = Path.GetRelativePath(workspace.MainRoot, RouteMoveOutsideWorkspace.Leaf).Replace(Path.DirectorySeparatorChar, '/').Replace(" ", "%20", StringComparison.Ordinal);
        workspace.Write(workspace.Loader, OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.GeneratedEntries($"- [Included]({link}) - #Docs #{SourceLoadingClosureResolver.LoadNowTag} - applies to `src/**`"));
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.ReferenceUnsafe, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Outside recovery retains original literal targets while cancellation or anchor drift prevents every effect")]
    [InlineData(false, 0), InlineData(true, 0), InlineData(false, 1), InlineData(true, 1)]
    [InlineData(false, 2), InlineData(true, 2), InlineData(false, 3), InlineData(true, 3)]
    public async Task RecoveryAndEffectGuard(bool custom, int change)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var originals = new[] { RouteMoveOutsideWorkspace.Leaf, SourceOverwritePath.ReadAdjacentPath(RouteMoveOutsideWorkspace.Leaf),
            workspace.LeftCatalogue, workspace.RightCatalogue, RouteMoveOutsideWorkspace.Left + "/incoming.md" }
            .ToDictionary(path => path, workspace.Bytes, StringComparer.Ordinal);
        var built = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteMovePlan>(built.Plan);
        var operationId = Guid.NewGuid();
        var acquired = await workspace.LockStore.AcquireAsync(new WorkspaceLockRequest(workspace.Workspace, RouteMoveDefinitions.CommandIdentity, operationId), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(new RouteMoveHeldApplication(plan, operationId, lease), TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            Assert.StartsWith(WorkspaceRuntimePaths.RecoveryRoot(workspace.Workspace) + Path.DirectorySeparatorChar, preparation.BundlePath, StringComparison.Ordinal);
            Assert.Equal(originals.Keys.Order(StringComparer.Ordinal), preparation.Entries.Select(entry => entry.TargetPath).Order(StringComparer.Ordinal));
            using (var archive = ZipFile.OpenRead(preparation.BundlePath))
                foreach (var entry in preparation.Entries)
                {
                    using var stream = Assert.Single(archive.Entries, payload => payload.FullName == entry.PriorPayload).Open();
                    using var bytes = new MemoryStream();
                    await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
                    Assert.Equal(originals[entry.TargetPath], bytes.ToArray());
                }
            using var cancellation = new CancellationTokenSource();
            switch (change)
            {
                case 0: cancellation.Cancel(); break;
                case 1: workspace.Write(RouteMoveOutsideWorkspace.Root + "/" + SourceFormClassifier.IndexFileName, RouteMoveOutsideWorkspace.Catalogue(string.Empty)); break;
                case 2:
                    File.WriteAllBytes(workspace.Absolute(WorkspaceOwnershipDefinitions.RelativePath), WorkspaceOwnershipCodec.Write(
                        WorkspaceOwnershipDocument.Empty with { Framework = new(new("changed", null), [RouteMoveOutsideWorkspace.Leaf], []) }));
                    break;
                case 3: workspace.ChangeSettings(settings => settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray()); break;
                default: throw new ArgumentOutOfRangeException(nameof(change));
            }
            var before = workspace.Snapshot();
            var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication().ApplyAsync(new RouteMoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            }, cancellation.Token);
            Assert.NotEmpty(progress.Receipts);
            Assert.All(progress.Receipts, receipt => Assert.Equal(FilesystemEffectState.NotStarted, receipt switch
            {
                RouteMoveFileChangeReceipt file => file.Receipt.EffectState,
                RouteMoveDirectoryCreationReceipt directory => directory.Receipt.EffectState,
                RouteMoveDirectoryDeletionReceipt directory => directory.Receipt.State.EffectState,
                _ => throw new InvalidOperationException("Unknown move receipt."),
            }));
            Assert.Equal(before, workspace.Snapshot());
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.False(File.Exists(workspace.Absolute(RouteMoveOutsideWorkspace.Destination)));
        }
        finally { File.Delete(preparation.BundlePath); }
    }

    [Theory(DisplayName = "Outside malformed row suffixes and file only registration never acquire move authority")]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task RequiredPolicy(bool custom, bool fileOnly)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        if (fileOnly)
            workspace.ChangeSettings(settings =>
            {
                settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray();
                settings[WorkspaceSettingsDefinitions.RootsProperty] = new JsonArray(RouteMoveOutsideWorkspace.Leaf);
            });
        else workspace.Write(workspace.LeftCatalogue, workspace.Read(workspace.LeftCatalogue).Replace("`src/**`", "`src/**` unparsed suffix", StringComparison.Ordinal));
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.NotEmpty(build.Formation.Findings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Outside managed leaves remain refused")]
    [InlineData(false), InlineData(true)]
    public async Task Ownership(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        File.WriteAllBytes(workspace.Absolute(WorkspaceOwnershipDefinitions.RelativePath), WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with { Framework = new(new("fixture", null), [RouteMoveOutsideWorkspace.Leaf], []) }));
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.OwnershipClaimed, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Outside layout source and authored row drift invalidate the complete leased move")]
    [InlineData(false, 0), InlineData(true, 0), InlineData(false, 1), InlineData(true, 1), InlineData(false, 2), InlineData(true, 2), InlineData(false, 3), InlineData(true, 3)]
    public async Task Revalidation(bool custom, int change)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var built = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.True(built.Plan is not null, string.Join(Environment.NewLine, built.Formation.Findings.Select(finding => finding.Cause)));
        var plan = Assert.IsType<RouteMovePlan>(built.Plan);
        var acquired = await workspace.LockStore.AcquireAsync(new WorkspaceLockRequest(workspace.Workspace, RouteMoveDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        switch (change)
        {
            case 0: workspace.ChangeSettings(settings => settings[WorkspaceSettingsDefinitions.OutsideRoutesProperty] = new JsonArray()); break;
            case 1: workspace.Write(RouteMoveOutsideWorkspace.Leaf, RouteMoveOutsideWorkspace.Markdown("# Changed source\n")); break;
            case 2: workspace.Write(workspace.LeftCatalogue, workspace.Read(workspace.LeftCatalogue).Replace("`src/**`", "`other/**`", StringComparison.Ordinal)); break;
            case 3:
                workspace.Temporary.MoveFile(RouteMoveOutsideWorkspace.Leaf, RouteMoveOutsideWorkspace.Left + "/retained.md");
                workspace.Temporary.CreateFileSymbolicLink(RouteMoveOutsideWorkspace.Leaf, "retained.md");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(change));
        }
        var before = workspace.Snapshot();
        var result = await new RouteMovePlanRevalidator(RouteMoveIntegrationWorkspace.CreatePlanBuilder()).RevalidateAsync(plan, lease, TestContext.Current.CancellationToken);
        Assert.Equal(RouteMovePlanRevalidationState.Changed, result.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Physical aliases cannot supply outside source or destination authority")]
    [InlineData(false), InlineData(true)]
    public async Task Aliases(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var alias = RouteMoveOutsideWorkspace.Left + "/alias.md";
        workspace.Temporary.CreateFileSymbolicLink(alias, Path.GetFileName(RouteMoveOutsideWorkspace.Leaf));
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(source: alias), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.NotEmpty(build.Formation.Findings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "A contained directory alias cannot supply an outside destination parent")]
    [InlineData(false), InlineData(true)]
    public async Task DestinationAlias(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var alias = RouteMoveOutsideWorkspace.Right + "/alias";
        workspace.Temporary.CreateDirectorySymbolicLink(alias, "../../..");
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(destination: alias + "/new.md"), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.NotEmpty(build.Formation.Findings);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory(DisplayName = "Primary sources cannot move into a registered outside boundary")]
    [InlineData(false), InlineData(true)]
    public async Task PrimaryToOutside(bool custom)
    {
        using var workspace = new RouteMoveOutsideWorkspace(custom);
        var before = workspace.Snapshot();
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(workspace.Request(source: workspace.MainRoot + "/notes/note.md"), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteMoveFindingCode.InvalidDestination, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }
}

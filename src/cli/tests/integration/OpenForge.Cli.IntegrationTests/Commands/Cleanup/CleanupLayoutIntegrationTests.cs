using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

[Trait("Feature", "cleanup-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class CleanupLayoutIntegrationTests
{
    private const string MainRoot = "knowledge/nested";
    private const string OtherRoot = "knowledge/replacement";
    private const string Loader = $"# Context\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n{MarkdownEntriesSectionReader.EmptyEntry}\n";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Cleanup retains recorded recovery bytes when settings change after planning")]
    public async Task ChangedLayoutPreservesFrozenRecovery()
    {
        using var files = TemporaryWorkspace.Create("cleanup-layout-revalidation");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(files);
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        files.WriteText($"{MainRoot}/{WorkspaceLayoutNames.LoaderFileName}", Loader);
        var prepared = await RecoveryBundleStore.PrepareAsync(
            RecoveryBundleStoreIntegrationTests.DeleteInput(files, workspace, Guid.NewGuid()), Token);
        var bundle = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var bytes = File.ReadAllBytes(bundle.BundlePath);
        var request = new CleanupRequest(workspace, CleanupMode.Apply);
        var observation = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(workspace, Token);
        var frozen = await RecoveryBundleCatalogue.ReadStoresAsync(workspace, Token);
        var stores = frozen.Select(store => new CleanupStoreApplication(store,
            new CleanupResultBuilder(CleanupPlanner.Create(request, store.Catalogue) with { LayoutRead = observation }))).ToArray();

        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot));
        await new CleanupApplicationOperation(WorkspaceLockManager.CreateForCurrentUser()).ApplyAsync(stores, Token);

        Assert.Equal(bytes, File.ReadAllBytes(bundle.BundlePath));
        Assert.Single(stores, store => !store.Result.Plan.DeletionEntries.IsEmpty);
        Assert.All(stores.Where(store => !store.Result.Plan.DeletionEntries.IsEmpty), store =>
        {
            Assert.Same(observation, store.Result.Plan.LayoutRead);
            Assert.Equal(CleanupCatalogueComparisonState.Blocked, store.Result.Revalidation.State);
            Assert.Contains(store.Result.Findings, finding => finding.Code == CleanupFindingCode.StoreValidationUnavailable);
        });
    }

    private static string Settings(string root) => $$"""
        {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
        """;
}

using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemovePersistenceIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Route Remove blocks invalid settings and ownership before subject effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    [InlineData("settings", "{ invalid settings json", "settings-unavailable")]
    [InlineData("ownership", "{ invalid ownership json", "ownership-unavailable")]
    public async Task InvalidPersistenceBlocksWithoutEffects(
        string record,
        string contents,
        string expectedFinding)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create($"route-remove-invalid-{record}");
        var path = record == "settings" ? ".agents/open-forge.json" : RouteRemoveIntegrationWorkspace.OwnershipPath;
        workspace.WriteText(path, contents);
        var before = workspace.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding =>
            RouteRemoveDefinitions.ReadMachineName(finding.Code) == $"route-remove.{expectedFinding}");
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove blocks an unreadable settings path before subject effects")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnavailableSettingsBlocksWithoutEffects()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-unavailable-settings");
        workspace.CreateDirectory(".agents/open-forge.json");
        var before = workspace.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteRemoveFindingCode.SettingsUnavailable);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove reuses an exact persisted absence as a no-op")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task RepeatedRemovalOfPersistentlyExcludedAbsenceIsANoOp()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-repeat-excluded");
        var operation = RouteRemoveOperationFactory.Create(workspace.LockStoreRoot);
        var request = new RouteRemoveRequest(
            workspace.Workspace,
            RouteRemoveIntegrationWorkspace.LeafId,
            RouteRemoveMode.Apply,
            automatic: true);

        var first = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        var afterFirst = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Settings.Outcome);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Ownership.Outcome);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Route Remove applies and repeats managed removal without creating an absent ownership lock"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    [InlineData(RouteRemoveIntegrationWorkspace.LeafId)]
    [InlineData(RouteRemoveIntegrationWorkspace.CategoryId)]
    public async Task MissingOwnershipLockAllowsManagedApplyAndRepeat(string sourceReference)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create(
            $"route-remove-absent-lock-{sourceReference.Replace('/', '-')}",
            seedOwnership: false);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.OwnershipPath)));
        var operation = RouteRemoveOperationFactory.Create(workspace.LockStoreRoot);
        var request = new RouteRemoveRequest(
            workspace.Workspace,
            sourceReference,
            RouteRemoveMode.Apply,
            automatic: true);

        var first = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(RouteRemovePersistenceOutcome.Applied, first.Persistence.Settings.Outcome);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, first.Persistence.Ownership.Outcome);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.OwnershipPath)));

        if (sourceReference == RouteRemoveIntegrationWorkspace.LeafId)
        {
            Assert.Contains(
                RouteRemoveIntegrationWorkspace.LeafPath,
                workspace.ReadSettings().RemovedFiles);
            Assert.Contains(
                RouteRemoveIntegrationWorkspace.LeafOverwritePath,
                workspace.ReadSettings().RemovedFiles);
        }
        else
        {
            Assert.Contains(
                ".agents/guidance/topics",
                workspace.ReadSettings().RemovedDirectories);
        }

        var afterFirst = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Settings.Outcome);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Ownership.Outcome);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.OwnershipPath)));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove blocks retry when a missing source still has an ownership claim"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingSourceWithPersistedIntentAndStaleOwnershipClaimBlocksRetry()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-stale-claim-retry");
        workspace.WriteBytes(
            ".agents/open-forge.json",
            SettingsWithRemovedLeaf());
        workspace.SeedFrameworkClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        workspace.DeleteFile(RouteRemoveIntegrationWorkspace.LeafPath);
        workspace.DeleteFile(RouteRemoveIntegrationWorkspace.LeafOverwritePath);
        workspace.WriteText(
            RouteRemoveIntegrationWorkspace.ParentPath,
            workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath)
                .Replace("- [Old guide](old%20guide.md) - #Guide\n", string.Empty, StringComparison.Ordinal));
        workspace.WriteText("README.md", "The selected source navigation was already removed.\n");
        var before = workspace.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteRemoveFindingCode.OwnershipClaimed);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        var ownership = workspace.ReadOwnership();
        Assert.NotNull(ownership.Framework);
        Assert.Contains(RouteRemoveIntegrationWorkspace.LeafPath, ownership.Framework.Paths);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove blocks retry when generated navigation still exposes a missing source"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingSourceWithPersistedIntentAndStaleNavigationBlocksRetry()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-stale-navigation-retry");
        workspace.WriteBytes(
            ".agents/open-forge.json",
            SettingsWithRemovedLeaf());
        workspace.DeleteFile(RouteRemoveIntegrationWorkspace.LeafPath);
        workspace.DeleteFile(RouteRemoveIntegrationWorkspace.LeafOverwritePath);
        var before = workspace.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteRemoveFindingCode.GeneratedRegionUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.ParentPath)));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies exclusions and publishes claims with different schema metadata")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DifferentSchemaMetadataDoesNotBlockSettingsOrOwnershipPublication()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-version-metadata");
        const int settingsVersion = 47;
        const int ownershipVersion = 53;
        const string authorNote = "keep this authored setting";
        var settingsText = $$"""
            {
              "schemaVersion": {{settingsVersion}},
              "authorNote": "{{authorNote}}",
              "allowInstallPaths": []
            }
            """;
        workspace.WriteText(".agents/open-forge.json", settingsText);
        workspace.SeedExtensionClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        var ownership = Assert.IsType<JsonObject>(JsonNode.Parse(
            workspace.ReadText(RouteRemoveIntegrationWorkspace.OwnershipPath)));
        ownership[WorkspaceOwnershipDefinitions.SchemaVersionProperty] = ownershipVersion;
        var versionedOwnershipText = ownership.ToJsonString();
        workspace.WriteText(RouteRemoveIntegrationWorkspace.OwnershipPath, versionedOwnershipText);
        var observedOwnershipBytes = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var request = new RouteRemoveRequest(
            workspace.Workspace,
            RouteRemoveIntegrationWorkspace.LeafId,
            RouteRemoveMode.Apply,
            automatic: true);

        var plan = await workspace.BuildApplicationPlanAsync();
        var ownershipSnapshot = plan.Projection.Ownership.Snapshot;
        Assert.NotNull(ownershipSnapshot);
        var ownershipChange = Assert.IsType<PlannedFileChange>(plan.Projection.OwnershipChange);
        Assert.Equal(observedOwnershipBytes, ownershipSnapshot.Bytes.ToArray());
        Assert.Equal(ownershipSnapshot.Expectation, ownershipChange.Expectation);
        var recoveryTarget = Assert.Single(
            plan.Projection.RecoveryTargets,
            target => target.Change.LogicalPath == workspace.Combine(RouteRemoveIntegrationWorkspace.OwnershipPath));
        Assert.Equal(observedOwnershipBytes, recoveryTarget.Before.Bytes.ToArray());

        var operation = RouteRemoveOperationFactory.Create(workspace.LockStoreRoot);
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteRemovePersistenceOutcome.Applied, result.Persistence.Settings.Outcome);
        Assert.Equal(RouteRemovePersistenceOutcome.Applied, result.Persistence.Ownership.Outcome);
        var settings = workspace.ReadSettings();
        Assert.Equal(settingsVersion, settings.SchemaVersion);
        Assert.Contains(RouteRemoveIntegrationWorkspace.LeafPath, settings.RemovedFiles);
        Assert.Contains(RouteRemoveIntegrationWorkspace.LeafOverwritePath, settings.RemovedFiles);
        var settingsAfter = Encoding.UTF8.GetString(workspace.ReadBytes(".agents/open-forge.json"));
        Assert.Contains($"\"authorNote\": \"{authorNote}\"", settingsAfter, StringComparison.Ordinal);
        Assert.Contains($"\"schemaVersion\": {settingsVersion}", settingsAfter, StringComparison.Ordinal);

        var publishedOwnership = workspace.ReadOwnership();
        Assert.Equal(ownershipVersion, publishedOwnership.SchemaVersion);
        var retainedExtension = Assert.Single(publishedOwnership.Extensions);
        Assert.Equal("toolkit", retainedExtension.Id);
        Assert.Empty(retainedExtension.Paths);

        var afterApply = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Settings.Outcome);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, repeated.Persistence.Ownership.Outcome);
        Assert.Equal(afterApply, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove recovery observes exact settings lock and edited shared source bytes")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task RecoveryTargetsContainExactPriorPersistenceAndEditedContent()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-persistence-recovery");
        const string settingsText = "{\n  \"authorNote\": \"preserve this key\",\n  \"allowInstallPaths\": []\n}\n";
        workspace.WriteText(".agents/open-forge.json", settingsText);
        workspace.SeedMultiplyOwnedExtensionClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        var editedLeafText = workspace.ReadText(RouteRemoveIntegrationWorkspace.LeafPath)
            + "\nUser edit before removal.\n";
        workspace.WriteText(RouteRemoveIntegrationWorkspace.LeafPath, editedLeafText);
        var priorSettings = workspace.ReadBytes(".agents/open-forge.json");
        var priorOwnership = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var priorLeaf = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.LeafPath);

        var plan = await workspace.BuildApplicationPlanAsync();

        Assert.NotNull(plan.Projection.SettingsChange);
        Assert.NotNull(plan.Projection.OwnershipChange);
        var settingsTarget = Assert.Single(plan.Projection.RecoveryTargets,
            target => target.Change.LogicalPath == workspace.Combine(".agents/open-forge.json"));
        var ownershipTarget = Assert.Single(plan.Projection.RecoveryTargets,
            target => target.Change.LogicalPath == workspace.Combine(RouteRemoveIntegrationWorkspace.OwnershipPath));
        var contentTarget = Assert.Single(plan.Projection.RecoveryTargets,
            target => target.Change.LogicalPath == workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath));

        Assert.Equal(priorSettings, settingsTarget.Before.Bytes.ToArray());
        Assert.Equal(priorOwnership, ownershipTarget.Before.Bytes.ToArray());
        Assert.Equal(priorLeaf, contentTarget.Before.Bytes.ToArray());
        Assert.Equal(2, plan.Projection.ClaimsToRelease.Length);
        Assert.All(plan.Projection.ClaimsToRelease, claim => Assert.Equal(RouteRemoveIntegrationWorkspace.LeafPath, claim.Path));
        Assert.Contains("preserve this key", System.Text.Encoding.UTF8.GetString(plan.Projection.SettingsChange!.IntendedBytes.AsSpan()), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove refuses a route that overlaps a registered Library source")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task RegisteredLibrarySourceBlocksSelectedRoute()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-library-source-guard");
        workspace.SeedLibrarySource(".agents/guidance");
        var before = workspace.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.LeafId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteRemoveFindingCode.ProtectedTarget);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Route Remove blocks category inventories containing nested Git metadata without effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NestedGitMetadataBlocksCategoryWithoutEffects(bool gitMetadataIsDirectory)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create(
            gitMetadataIsDirectory
                ? "route-remove-nested-git-directory"
                : "route-remove-nested-git-worktree-pointer");
        const string gitMetadataPath = ".agents/guidance/topics/vendor/.git";
        const string gitIgnorePath = ".agents/guidance/topics/vendor/.gitignore";
        const string settingsPath = ".agents/open-forge.json";
        const string authorSettings = "{\n  \"authorNote\": \"preserve during blocked removal\",\n  \"allowInstallPaths\": []\n}\n";

        workspace.WriteText(settingsPath, authorSettings);
        workspace.WriteText(gitIgnorePath, "ignored-local-file\n");
        if (gitMetadataIsDirectory)
        {
            workspace.CreateDirectory(gitMetadataPath);
            workspace.WriteText($"{gitMetadataPath}/HEAD", "ref: refs/heads/main\n");
            workspace.WriteText($"{gitMetadataPath}/config", "[core]\n\trepositoryformatversion = 0\n");
        }
        else
        {
            workspace.WriteText(gitMetadataPath, "gitdir: ../../../../.git/worktrees/vendor\n");
        }

        var before = workspace.SnapshotHashes();
        var settingsBefore = workspace.ReadBytes(settingsPath);
        var ownershipBefore = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var gitMetadataBefore = gitMetadataIsDirectory
            ? new Dictionary<string, byte[]>
            {
                [$"{gitMetadataPath}/HEAD"] = workspace.ReadBytes($"{gitMetadataPath}/HEAD"),
                [$"{gitMetadataPath}/config"] = workspace.ReadBytes($"{gitMetadataPath}/config"),
            }
            : new Dictionary<string, byte[]>
            {
                [gitMetadataPath] = workspace.ReadBytes(gitMetadataPath),
            };

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.CategoryId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        var finding = Assert.Single(result.Findings, item => item.Code == RouteRemoveFindingCode.ProtectedTarget);
        Assert.Equal(gitMetadataPath, finding.Target);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(settingsBefore, workspace.ReadBytes(settingsPath));
        Assert.Equal(ownershipBefore, workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath));
        Assert.Equal("ignored-local-file\n", workspace.ReadText(gitIgnorePath));
        if (gitMetadataIsDirectory)
        {
            Assert.True(Directory.Exists(workspace.Combine(gitMetadataPath)));
        }
        else
        {
            Assert.True(File.Exists(workspace.Combine(gitMetadataPath)));
        }

        foreach (var metadata in gitMetadataBefore)
        {
            Assert.Equal(metadata.Value, workspace.ReadBytes(metadata.Key));
        }
    }

    private static byte[] SettingsWithRemovedLeaf()
        => WorkspaceSettingsCodec.AddRemovals(
            "{}"u8.ToArray(),
            new WorkspaceRemovalSelection
            {
                Files = [
                    RouteRemoveIntegrationWorkspace.LeafPath,
                    RouteRemoveIntegrationWorkspace.LeafOverwritePath,
                ],
            })
            ?? throw new InvalidOperationException("The test settings fixture did not record its route removals.");

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove refuses an ancestor directory link without resolving its external source")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task AncestorDirectoryLinkIsNotFollowed()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-linked-source-ancestor");
        using var external = TemporaryWorkspace.Create("route-remove-linked-source-target");
        external.CreateFile(
            "leaf.md",
            OpenForgeDocumentSeed.Metadata("External leaf", ["Route"], "# External leaf\n"));
        if (!workspace.TryCreateDirectorySymbolicLink(".agents/guidance/linked-source", external.Path))
        {
            Assert.Skip("This operating system or filesystem does not permit directory symbolic links.");
        }
        var before = workspace.SnapshotHashes();
        var externalBefore = external.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                "guidance/linked-source/leaf",
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(externalBefore, external.SnapshotHashes());
        Assert.True(File.Exists(external.Combine("leaf.md")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove refuses an external directory link that is an ancestor of selected content")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ExternalDirectoryLinkInsideCategoryIsNotFollowed()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-linked-child");
        using var external = TemporaryWorkspace.Create("route-remove-external-child");
        external.CreateFile("outside.md", "External content.\n");
        if (!workspace.TryCreateDirectorySymbolicLink(".agents/guidance/topics/external", external.Path))
        {
            Assert.Skip("This operating system or filesystem does not permit directory symbolic links.");
        }
        var before = workspace.SnapshotHashes();
        var externalBefore = external.SnapshotHashes();

        var result = await RouteRemoveOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new RouteRemoveRequest(
                workspace.Workspace,
                RouteRemoveIntegrationWorkspace.CategoryId,
                RouteRemoveMode.Apply,
                automatic: true),
            TestContext.Current.CancellationToken);

        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(externalBefore, external.SnapshotHashes());
        Assert.True(File.Exists(external.Combine("outside.md")));
    }
}

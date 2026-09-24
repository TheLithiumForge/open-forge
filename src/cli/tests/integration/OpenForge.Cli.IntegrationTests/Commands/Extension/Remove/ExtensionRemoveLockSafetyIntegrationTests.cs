using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

[Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
public sealed class ExtensionRemoveLockSafetyIntegrationTests
{
    private const string LockPath = ".agents/open-forge.lock.json";
    private const string Target = ".agents/toolkit.txt";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Remove reports uninterpretable dependency receipts without deleting files")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UninterpretableDependenciesFailTowardMissedDeletion(bool duplicate)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-invalid-dependencies");
        using var source = ExtensionInstallCatalogue.Create("remove-invalid-dependencies-source");
        await InstallAsync(workspace, source);
        var ownership = JsonNode.Parse(workspace.ReadText(LockPath))!;
        var extensions = ownership["extensions"]!.AsArray();
        if (duplicate)
        {
            extensions.Add(extensions[0]!.DeepClone());
        }
        else
        {
            extensions[0]!["dependencies"] = new JsonArray(JsonValue.Create("toolkit"));
        }
        workspace.ReplaceText(LockPath, ownership.ToJsonString());
        var before = workspace.Snapshot();

        var run = await RemoveAsync(workspace);

        Assert.Equal(3, run.ExitCode);
        Assert.True(File.Exists(workspace.Combine(Target)));
        Assert.Contains("extension-remove.lifecycle-unavailable", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("extension-remove.ownership-observation", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove preserves all files when a stale receipt aliases another owner's path")]
    public async Task AliasedOwnerCannotBeDeletedAsFinalOwner()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-aliased-owner");
        using var source = ExtensionInstallCatalogue.Create("remove-aliased-owner-source");
        await InstallAsync(workspace, source);
        var ownership = JsonNode.Parse(workspace.ReadText(LockPath))!;
        var other = ownership["extensions"]![0]!.DeepClone();
        other["id"] = "other";
        other["paths"] = new JsonArray(JsonValue.Create(Target.Replace("toolkit", "TOOLKIT", StringComparison.Ordinal)));
        ownership["extensions"]!.AsArray().Add(other);
        workspace.ReplaceText(LockPath, ownership.ToJsonString());
        var before = workspace.Snapshot();

        var run = await RemoveAsync(workspace);

        Assert.Equal(3, run.ExitCode);
        Assert.True(File.Exists(workspace.Combine(Target)));
        Assert.Contains("extension-remove.lifecycle-unavailable", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("extension-remove.ownership-observation", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove records settings-only intent when ownership is known absent")]
    public async Task AbsentLockAllowsSettingsOnlyIntentAndLeavesLegacyFiles()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-unavailable-lock");
        using var source = ExtensionInstallCatalogue.Create("remove-unavailable-lock-source");
        await InstallAsync(workspace, source);
        File.Delete(workspace.Combine(LockPath));
        var legacyBytes = File.ReadAllBytes(workspace.Combine(Target));
        var sourceBefore = source.Snapshot();
        var before = workspace.Snapshot();

        var run = await RemoveAsync(workspace);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.True(File.Exists(workspace.Combine(Target)));
        Assert.Equal(legacyBytes, File.ReadAllBytes(workspace.Combine(Target)));
        Assert.False(File.Exists(workspace.Combine(LockPath)));
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        var result = document.RootElement.GetProperty("data");
        Assert.Contains(result.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == ".agents/open-forge.json"
            && effect.GetProperty("action").GetString() == "record-exclusion"
            && effect.GetProperty("outcome").GetString() == "verified");
        Assert.DoesNotContain(result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == LockPath);
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Equal(["toolkit"], settings.RootElement.GetProperty("removedExtensions")
            .EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            UnchangedWorkspace(before),
            UnchangedWorkspace(workspace.Snapshot()));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove blocks malformed ownership before settings or legacy file effects")]
    public async Task MalformedLockBlocksWithoutSettingsOrFileEffects()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-malformed-lock");
        using var source = ExtensionInstallCatalogue.Create("remove-malformed-lock-source");
        await InstallAsync(workspace, source);
        workspace.ReplaceText(LockPath, "{");
        var before = workspace.Snapshot();
        var legacyBytes = File.ReadAllBytes(workspace.Combine(Target));

        var run = await RemoveAsync(workspace);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(legacyBytes, File.ReadAllBytes(workspace.Combine(Target)));
        Assert.True(File.Exists(workspace.Combine(LockPath)));
        Assert.Contains("extension-remove.lifecycle-unavailable", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("extension-remove.ownership-observation", run.StandardOutput, StringComparison.Ordinal);

        var human = await workspace.RunAsync([
            "extension", "remove", "toolkit", "--automatic", "--detail", "minimal"]);

        Assert.Equal(3, human.ExitCode);
        Assert.Contains("The ownership record is unavailable:", human.StandardOutput, StringComparison.Ordinal);
        Assert.Empty(human.StandardError);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove refuses a stale claim outside the shared allow list without deleting anything")]
    public async Task UnallowedStaleClaimCannotDeleteAnOrdinaryFile()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-unallowed-claim");
        using var source = ExtensionInstallCatalogue.Create("remove-unallowed-claim-source");
        await InstallAsync(workspace, source);
        var ownership = JsonNode.Parse(workspace.ReadText(LockPath))!;
        ownership["extensions"]![0]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create("workspace-note.md"));
        workspace.ReplaceText(LockPath, ownership.ToJsonString());
        var before = workspace.Snapshot();

        var run = await RemoveAsync(workspace);

        Assert.Equal(5, run.ExitCode);
        Assert.Contains("extension-remove.permission-required", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove releases a stale missing path without planning or applying its deletion")]
    public async Task MissingClaimHasNoDeletionEffect()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-missing-claim");
        using var source = ExtensionInstallCatalogue.Create("remove-missing-claim-source");
        await InstallAsync(workspace, source);
        File.Delete(workspace.Combine(Target));
        var note = workspace.ReadText("workspace-note.md");

        var run = await RemoveAsync(workspace);

        Assert.Equal(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.DoesNotContain(result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == Target
                && effect.GetProperty("action").GetString() == "delete");
        var effects = result.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == Target
            && effect.GetProperty("action").GetString() == "release-ownership");
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents/open-forge.json"
            && effect.GetProperty("action").GetString() == "record-exclusion"
            && effect.GetProperty("outcome").GetString() == "verified");
        Assert.Equal(note, workspace.ReadText("workspace-note.md"));
        Assert.False(File.Exists(workspace.Combine(Target)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove deletion requires verified recovery and preparation preserves all bytes before any deletion")]
    public async Task RecoveryPrecedesDeletionAtTheApplicationBoundary()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove-recovery-order");
        using var source = ExtensionInstallCatalogue.Create("remove-recovery-order-source");
        await InstallAsync(workspace, source);
        const string edited = "uncommitted user bytes\r\n";
        workspace.ReplaceText(Target, edited);
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionRemovePlanner(
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionRemoveWording.Selection(),
            resolver);
        var build = await planner.BuildAsync(new ExtensionRemoveRequest(
            workspace.Workspace, ExtensionRemoveMode.Apply, ["toolkit"], automatic: true, allowInteraction: false),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionRemovePlan>(build.Plan);
        var execution = ExtensionRemoveExecutionPlanComposer.Compose(
            plan,
            new ExtensionPermissionStage(null, WorkspacePermissionResult.NotEvaluated, null, null, null));
        using var store = WorkspaceLockTestStore.Create("remove-recovery-order-locks");
        var acquired = await store.AcquireAsync(new WorkspaceLockRequest(
            workspace.Workspace, ExtensionRemoveDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var applier = new FileChangeApplier(revalidator, validator);
        var effect = Assert.Single(plan.Effects, effect => effect.Result.Path == Target);
        var change = Assert.IsType<Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChange>(effect.FileChange);
        var validation = await revalidator.ValidateAsync(lease, [change], TestContext.Current.CancellationToken);
        var check = Assert.Single(validation.Checks);

        var refused = await applier.ApplyAsync(lease, change, check, recoveryPreparation: null, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemEffectState.NotStarted, refused.EffectState);
        Assert.Equal(edited, workspace.ReadText(Target));

        var prepared = await ExtensionRemoveRecoveryApplication.PrepareAsync(execution, lease.Request.OperationId,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        using (var archive = ZipFile.OpenRead(preparation.BundlePath))
        {
            foreach (var entry in preparation.Entries.Where(entry => entry.PriorPayload is not null))
            {
                Assert.True(File.Exists(workspace.Combine(entry.TargetPath)));
                using var stream = archive.GetEntry(entry.PriorPayload!)!.Open();
                using var bytes = new MemoryStream();
                await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
                Assert.Equal(File.ReadAllBytes(workspace.Combine(entry.TargetPath)), bytes.ToArray());
            }
        }

        var deleted = await applier.ApplyAsync(lease, change, check, preparation, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemVerificationState.Verified, deleted.VerificationState);
        Assert.False(File.Exists(workspace.Combine(Target)));
        Assert.True(File.Exists(preparation.BundlePath));
    }

    private static Task<ExtensionInstallRun> RemoveAsync(ExtensionInstallIntegrationWorkspace workspace)
        => workspace.RunAsync(["extension", "remove", "toolkit", "--automatic", "--format", "json"]);

    private static KeyValuePair<string, string>[] UnchangedWorkspace(
        IReadOnlyDictionary<string, string> snapshot)
        => [.. snapshot.Where(item => item.Key != "file:.agents/open-forge.json"
            && !item.Key.StartsWith("recovery-", StringComparison.Ordinal))];

    private static async Task InstallAsync(ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (Target, "installed bytes\n"));
        var run = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, run.ExitCode);
    }
}

using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

[Trait("Feature", "extension-update"), Trait("Evidence", "IntegrationSafety")]
public sealed class ExtensionUpdateRecoveryOrderingIntegrationTests
{
    private const string Target = ".agents/toolkit.txt";
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update deletion requires verified recovery and preparation preserves all bytes before any deletion")]
    public async Task RecoveryPrecedesDeletionAtTheApplicationBoundary()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("update-recovery-order");
        using var source = ExtensionInstallCatalogue.Create("update-recovery-order-source");
        await InstallAsync(workspace, source);
        File.Delete(Path.Combine(source.PackagePath("toolkit"), "content", ".agents", "toolkit.txt"));
        const string edited = "uncommitted user bytes\r\n";
        workspace.ReplaceText(Target, edited);
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionUpdatePlanner(new OpenForge.Cli.Core.Framework.Extensions.ExtensionSourceReader(resolver),
            new FileExpectationValidator(resolver), resolver,
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionUpdateWording.Selection());
        var build = await planner.BuildAsync(new ExtensionUpdateRequest(
            workspace.Workspace, ExtensionUpdateMode.Apply, ["toolkit"], all: false, sourcePath: source.Path,
            force: false, prune: true, automatic: true, allowInteraction: false),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionUpdatePlan>(build.Plan);
        var execution = new ExtensionUpdateExecutionPlan(plan,
            new ExtensionPermissionStage(null, WorkspacePermissionResult.NotEvaluated, null, null, null));
        using var store = WorkspaceLockTestStore.Create("update-recovery-order-locks");
        var acquired = await store.AcquireAsync(new WorkspaceLockRequest(
            workspace.Workspace, ExtensionUpdateDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
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

        var prepared = await ExtensionUpdateRecoveryApplication.PrepareAsync(execution, lease.Request.OperationId,
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

    private static async Task InstallAsync(ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (Target, "installed bytes\n"));
        var run = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, run.ExitCode);
    }
}

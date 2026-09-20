using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "extension-update"), Trait("Evidence", "IntegrationSafety")]
public sealed class UpdateRecoveryOrderingIntegrationTests
{
    private const string Target = UpdateIntegrationWorkspace.HistoricalTargetPath;
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Update deletion requires verified recovery and preparation preserves all bytes before any deletion")]
    public async Task RecoveryPrecedesDeletionAtTheApplicationBoundary()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("root-update-recovery-order");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();
        const string edited = "uncommitted user bytes\r\n";
        workspace.ReplaceText(Target, edited);
        var resolver = new PhysicalPathResolver();
        var build = await UpdatePlanBuilder.Create().BuildExecutionAsync(workspace.Request(prune: true), TestContext.Current.CancellationToken);
        var execution = Assert.IsType<UpdatePlanExecution>(build.Execution);
        using var store = WorkspaceLockTestStore.Create("update-recovery-order-locks");
        var acquired = await store.AcquireAsync(new WorkspaceLockRequest(
            workspace.Workspace, UpdateDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var applier = new FileChangeApplier(revalidator, validator);
        var effect = Assert.Single(execution.Effects, effect => effect.ResultEffect.Path == Target);
        var change = Assert.IsType<Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChange>(effect.FileChange);
        var validation = await revalidator.ValidateAsync(lease, [change], TestContext.Current.CancellationToken);
        var check = Assert.Single(validation.Checks);

        var refused = await applier.ApplyAsync(lease, change, check, recoveryPreparation: null, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemEffectState.NotStarted, refused.EffectState);
        Assert.Equal(edited, workspace.ReadText(Target));

        var prepared = await UpdateRecoveryOperation.PrepareAsync(execution, lease.Request.OperationId,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        using (var archive = ZipFile.OpenRead(preparation.BundlePath))
        {
            foreach (var entry in preparation.Entries.Where(entry => entry.PriorPayload is not null))
            {
                Assert.True(File.Exists(Path.Combine(workspace.Workspace.LexicalRoot, entry.TargetPath)));
                using var stream = archive.GetEntry(entry.PriorPayload!)!.Open();
                using var bytes = new MemoryStream();
                await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
                Assert.Equal(File.ReadAllBytes(Path.Combine(workspace.Workspace.LexicalRoot, entry.TargetPath)), bytes.ToArray());
            }
        }

        var deleted = await applier.ApplyAsync(lease, change, check, preparation, TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemVerificationState.Verified, deleted.VerificationState);
        Assert.False(File.Exists(Path.Combine(workspace.Workspace.LexicalRoot, Target)));
        Assert.True(File.Exists(preparation.BundlePath));
    }

}

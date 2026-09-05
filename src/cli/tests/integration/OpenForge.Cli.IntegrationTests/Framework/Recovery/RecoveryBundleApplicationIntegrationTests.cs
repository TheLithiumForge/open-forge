using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Recovery;

public sealed class RecoveryBundleApplicationIntegrationTests
{
    [Fact(DisplayName = "Recovery preparation authorizes every existing-target effect and excludes create")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task OneRealPreparationAuthorizesItsExactMultiTargetApplication()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-application");
        using var lockStore = WorkspaceLockTestStore.Create("recovery-application-lock-store");
        var replacePath = temporary.CreateFile("replace.bin", []);
        var deletePath = temporary.CreateFile("delete.bin", new byte[] { 0, 255, 1, 128 });
        var generatedPath = temporary.CreateFile("generated.md", "before\n"u8.ToArray());
        var createPath = temporary.Combine("created.bin");
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var operationId = Guid.NewGuid();
        var command = "recovery application";
        var replace = PlannedFileChange.Replace(
            FileExpectation.File(replacePath, replacePath, FileExpectation.Hash([])),
            new byte[] { 9, 0, 8 });
        var delete = PlannedFileChange.Delete(
            FileExpectation.File(
                deletePath,
                deletePath,
                FileExpectation.Hash(new byte[] { 0, 255, 1, 128 })));
        var generated = PlannedFileChange.ReplaceGeneratedRegion(
            FileExpectation.File(
                generatedPath,
                generatedPath,
                FileExpectation.Hash("before\n"u8)),
            "after generated\n"u8);
        var create = PlannedFileChange.Create(
            FileExpectation.Missing(createPath),
            "created\n"u8);
        var input = RecoveryBundleInput.Create(
            workspace,
            command,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId,
            targets:
            [
                RecoveryBundleTarget.Create(
                    replace,
                    FileStateSnapshot.File(replacePath, replacePath, [])),
                RecoveryBundleTarget.Create(
                    delete,
                    FileStateSnapshot.File(
                        deletePath,
                        deletePath,
                        new byte[] { 0, 255, 1, 128 })),
                RecoveryBundleTarget.Create(
                    generated,
                    FileStateSnapshot.File(
                        generatedPath,
                        generatedPath,
                        "before\n"u8)),
                RecoveryBundleTarget.Create(create, FileStateSnapshot.Missing(createPath)),
            ]);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            await using var lease = await AcquireAsync(
                lockStore,
                workspace,
                command,
                operationId);
            var result = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
                input,
                TestContext.Current.CancellationToken);
            preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
            var resolver = new PhysicalPathResolver();
            var validator = new FileExpectationValidator(resolver);
            var applier = new FileChangeApplier(
                new MutationRevalidator(validator),
                validator);

            foreach (var change in new[] { replace, delete, generated })
            {
                var receipt = await applier.ApplyAsync(
                    lease,
                    change,
                    await CheckAsync(lease, change, validator, resolver),
                    preparation,
                    TestContext.Current.CancellationToken);
                Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
                Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
            }

            var createCheck = await CheckAsync(lease, create, validator, resolver);
            var rejectedCreate = await applier.ApplyAsync(
                lease,
                create,
                createCheck,
                preparation,
                TestContext.Current.CancellationToken);
            Assert.Equal(FilesystemEffectState.NotStarted, rejectedCreate.EffectState);
            Assert.Equal(FilesystemNotStartedReason.ContractRejected, rejectedCreate.NotStartedReason);
            Assert.False(File.Exists(createPath));

            var appliedCreate = await applier.ApplyAsync(
                lease,
                create,
                createCheck,
                recoveryPreparation: null,
                TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemVerificationState.Verified, appliedCreate.VerificationState);
            Assert.Equal(new byte[] { 9, 0, 8 }, await File.ReadAllBytesAsync(
                replacePath,
                TestContext.Current.CancellationToken));
            Assert.False(File.Exists(deletePath));
            Assert.Equal("after generated\n"u8.ToArray(), await File.ReadAllBytesAsync(
                generatedPath,
                TestContext.Current.CancellationToken));
            Assert.Equal("created\n"u8.ToArray(), await File.ReadAllBytesAsync(
                createPath,
                TestContext.Current.CancellationToken));
        }
        finally
        {
            if (File.Exists(createPath))
            {
                File.Delete(createPath);
            }

            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation?.BundlePath);
        }
    }

    [Fact(DisplayName = "File application rejects absent foreign mismatched corrupt and draft evidence")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ExistingTargetEffectsRequireTheirMatchingRealFinalPreparation()
    {
        using var temporary = TemporaryWorkspace.Create("recovery-application-blocked");
        using var lockStore = WorkspaceLockTestStore.Create("recovery-application-blocked-lock-store");
        var firstPath = temporary.CreateFile("first.md", "first"u8.ToArray());
        var secondPath = temporary.CreateFile("second.md", "second"u8.ToArray());
        var workspace = RecoveryBundleStoreIntegrationTests.Workspace(temporary);
        var operationId = Guid.NewGuid();
        var command = "recovery blocked";
        var first = PlannedFileChange.Replace(
            FileExpectation.File(firstPath, firstPath, FileExpectation.Hash("first"u8)),
            "first intended"u8);
        var second = PlannedFileChange.Replace(
            FileExpectation.File(secondPath, secondPath, FileExpectation.Hash("second"u8)),
            "second intended"u8);
        var input = Input(workspace, command, operationId, first, "first"u8);
        var foreignInput = Input(workspace, command, Guid.NewGuid(), first, "first"u8);
        RecoveryBundlePreparation? preparation = null;
        RecoveryBundlePreparation? foreignPreparation = null;
        string? draftPath = null;
        try
        {
            await using var lease = await AcquireAsync(
                lockStore,
                workspace,
                command,
                operationId);
            preparation = Assert.IsType<RecoveryBundlePreparation>(
                (await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
                    input,
                    TestContext.Current.CancellationToken)).Preparation);
            foreignPreparation = Assert.IsType<RecoveryBundlePreparation>(
                (await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
                    foreignInput,
                    TestContext.Current.CancellationToken)).Preparation);
            var resolver = new PhysicalPathResolver();
            var validator = new FileExpectationValidator(resolver);
            var applier = new FileChangeApplier(
                new MutationRevalidator(validator),
                validator);
            var firstCheck = await CheckAsync(lease, first, validator, resolver);
            var secondCheck = await CheckAsync(lease, second, validator, resolver);

            foreach (var attempted in new[]
            {
                await applier.ApplyAsync(
                    lease,
                    first,
                    firstCheck,
                    recoveryPreparation: null,
                    TestContext.Current.CancellationToken),
                await applier.ApplyAsync(
                    lease,
                    first,
                    firstCheck,
                    foreignPreparation,
                    TestContext.Current.CancellationToken),
                await applier.ApplyAsync(
                    lease,
                    second,
                    secondCheck,
                    preparation,
                    TestContext.Current.CancellationToken),
            })
            {
                Assert.Equal(FilesystemEffectState.NotStarted, attempted.EffectState);
                Assert.Equal(FilesystemNotStartedReason.ContractRejected, attempted.NotStartedReason);
            }

            using (var archive = ZipFile.Open(preparation.BundlePath, ZipArchiveMode.Update))
            {
                await using var payload = archive.Entries[1].Open();
                await payload.WriteAsync(
                    new byte[] { 42 },
                    TestContext.Current.CancellationToken);
            }

            var reader = new RecoveryBundleReader();
            var corruptRead = await reader.ReadExpectedFinalAsync(
                input,
                preparation.BundlePath,
                TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundleReadState.Malformed, corruptRead.Read.State);
            Assert.Null(corruptRead.Preparation);
            var corruptAttempt = await applier.ApplyAsync(
                lease,
                first,
                firstCheck,
                corruptRead.Preparation,
                TestContext.Current.CancellationToken);
            Assert.Equal(FilesystemEffectState.NotStarted, corruptAttempt.EffectState);
            Assert.Equal(FilesystemNotStartedReason.ContractRejected, corruptAttempt.NotStartedReason);

            draftPath = RecoveryBundlePathIdentity.DraftPath(
                RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
                    ?? throw new InvalidOperationException("LocalApplicationData must remain observable."),
                workspace.PhysicalRoot,
                operationId);
            File.Move(preparation.BundlePath, draftPath);
            var draftRead = await reader.ReadExpectedFinalAsync(
                input,
                draftPath,
                TestContext.Current.CancellationToken);
            Assert.Null(draftRead.Preparation);
            var catalogue = await new RecoveryBundleCatalogue(reader).ReadAsync(
                workspace,
                TestContext.Current.CancellationToken);
            Assert.Equal(
                RecoveryBundleIntegrity.Incomplete,
                Assert.Single(catalogue.Candidates, candidate => candidate.Path == draftPath).Integrity);
            var draftAttempt = await applier.ApplyAsync(
                lease,
                first,
                firstCheck,
                draftRead.Preparation,
                TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.NotStarted, draftAttempt.EffectState);
            Assert.Equal(FilesystemNotStartedReason.ContractRejected, draftAttempt.NotStartedReason);
            Assert.Equal("first", await File.ReadAllTextAsync(
                firstPath,
                TestContext.Current.CancellationToken));
            Assert.Equal("second", await File.ReadAllTextAsync(
                secondPath,
                TestContext.Current.CancellationToken));
        }
        finally
        {
            RecoveryBundleStoreIntegrationTests.DeleteOwned(preparation?.BundlePath);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(foreignPreparation?.BundlePath);
            RecoveryBundleStoreIntegrationTests.DeleteOwned(draftPath);
        }
    }

    private static RecoveryBundleInput Input(
        CliWorkspace workspace,
        string command,
        Guid operationId,
        PlannedFileChange change,
        ReadOnlySpan<byte> before)
        => RecoveryBundleInput.Create(
            workspace,
            command,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            operationId,
            [RecoveryBundleTarget.Create(
                change,
                FileStateSnapshot.File(change.LogicalPath, change.LogicalPath, before))]);

    private static async ValueTask<WorkspaceLockLease> AcquireAsync(
        WorkspaceLockTestStore lockStore,
        CliWorkspace workspace,
        string command,
        Guid operationId)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, command, operationId),
            TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    private static async ValueTask<FileExpectationValidationResult> CheckAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidator validator,
        PhysicalPathResolver resolver)
    {
        var result = await new MutationRevalidator(validator).ValidateAsync(
            lease,
            [change],
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, result.State);
        return Assert.Single(result.Checks);
    }
}

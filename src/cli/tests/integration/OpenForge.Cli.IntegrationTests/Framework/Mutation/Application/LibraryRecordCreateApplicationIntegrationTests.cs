using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Application;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Integration")]
public sealed class LibraryRecordCreateApplicationIntegrationTests
{
    [Theory(DisplayName = "Prior-missing Library record create accepts only its exact recovery preparation and unchanged ordinary missing leaf")]
    [InlineData("exact"), InlineData("foreign-operation"), InlineData("changed-file"), InlineData("changed-link")]
    public static async Task AppliesReversibleOrdinaryRecordCreate(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("library-record-create");
        using var locks = WorkspaceLockTestStore.Create("library-record-create-lock");
        temporary.CreateDirectory(".agents");
        var source = temporary.CreateFile("source.json", "source");
        var path = temporary.Combine(".agents/open-forge.libraries.json");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var before = FileStateSnapshot.Missing(path);
        var change = PlannedFileChange.Create(before.Expectation, "{\"schemaVersion\":1,\"libraries\":[]}"u8);
        var operationId = Guid.NewGuid();
        var input = RecoveryBundleInput.Create(workspace, command: "library attach",
            attribution: RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, workspace),
            operationId: scenario == "foreign-operation" ? Guid.NewGuid() : operationId,
            targets: [RecoveryBundleTarget.CreateReversible(change, before)]);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace, "library attach", operationId), TestContext.Current.CancellationToken)).Lease);
        RecoveryBundlePreparation? preparation = null;
        try
        {
            preparation = Assert.IsType<RecoveryBundlePreparation>((await RecoveryBundleStore.PrepareAsync(input, TestContext.Current.CancellationToken)).Preparation);
            if (scenario == "changed-file")
            {
                temporary.CreateFile(".agents/open-forge.libraries.json", "local");
            }
            else if (scenario == "changed-link")
            {
                temporary.CreateFileSymbolicLink(".agents/open-forge.libraries.json", "../source.json");
            }
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var applier = new FileChangeApplier(new MutationRevalidator(validator), validator);
            var check = FileExpectationValidationResult.Matched(before.Expectation, before, path);

            var receipt = await applier.ApplyAsync(lease, change, check, preparation, TestContext.Current.CancellationToken);

            if (scenario == "exact")
            {
                Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
                Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
                Assert.Null(new FileInfo(path).LinkTarget);
                Assert.Equal("{\"schemaVersion\":1,\"libraries\":[]}", File.ReadAllText(path));
            }
            else
            {
                Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
                if (scenario == "changed-file")
                {
                    Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
                    Assert.Equal("local", File.ReadAllText(path));
                }
                if (scenario == "changed-link")
                {
                    Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
                    Assert.Equal("../source.json", new FileInfo(path).LinkTarget);
                }
                if (scenario == "foreign-operation")
                {
                    Assert.Equal(FilesystemNotStartedReason.ContractRejected, receipt.NotStartedReason);
                    Assert.False(File.Exists(path));
                }
            }
            Assert.Equal("source", File.ReadAllText(source));
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            if (scenario == "exact" && File.Exists(path) && new FileInfo(path).LinkTarget is null)
            {
                File.Delete(path);
            }
            if (preparation is not null)
            {
                File.Delete(preparation.BundlePath);
            }
        }
    }
}

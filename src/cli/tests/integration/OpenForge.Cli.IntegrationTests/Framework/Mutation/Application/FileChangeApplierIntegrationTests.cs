using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Application;

public sealed class FileChangeApplierIntegrationTests : IDisposable
{
    private readonly List<string> recoveryBundlePaths = [];
    private readonly WorkspaceLockTestStore lockStore = WorkspaceLockTestStore.Create(
        "file-change-applier-lock-store");

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier creates exact bytes and leaves no stage")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task CreatesExactBytesWithVerifiedReceipt()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-create");
        var lookalike = temporary.CreateFile(".open-forge-stage-lookalike", "preserve me");
        var path = temporary.Combine("created.md");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(path),
            "created bytes\n"u8);
        await using var lease = await AcquireAsync(workspace);
        var check = await RevalidateAsync(lease, change, validator);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            recoveryPreparation: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        Assert.Equal("created bytes\n"u8.ToArray(), await File.ReadAllBytesAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Equal([lookalike], Stages(temporary));
        Assert.Equal("preserve me", await File.ReadAllTextAsync(
            lookalike,
            TestContext.Current.CancellationToken));
        File.Delete(path);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier replaces complete bytes for ordinary and generated changes")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ReplacesFullDocumentForBothReplacementKinds()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-replace");
        var path = temporary.CreateFile("document.md", "before\n");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var applier = new FileChangeApplier(
            new MutationRevalidator(validator),
            validator);
        await using var lease = await AcquireAsync(workspace);

        var replace = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before\n"u8)),
            "replace bytes\n"u8);
        var replaceCheck = await RevalidateAsync(lease, replace, validator);
        var replaced = await applier.ApplyAsync(
            lease,
            replace,
            replaceCheck,
            await PreparationAsync(lease, replace, replaceCheck),
            TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemVerificationState.Verified, replaced.VerificationState);
        Assert.Equal("replace bytes\n"u8.ToArray(), await File.ReadAllBytesAsync(
            path,
            TestContext.Current.CancellationToken));
        DeleteRecoveryBundle(recoveryBundlePaths[^1]);

        var generated = PlannedFileChange.ReplaceGeneratedRegion(
            FileExpectation.File(path, path, FileExpectation.Hash("replace bytes\n"u8)),
            "generated full document\n"u8);
        var generatedCheck = await RevalidateAsync(lease, generated, validator);
        var generatedReceipt = await applier.ApplyAsync(
            lease,
            generated,
            generatedCheck,
            await PreparationAsync(lease, generated, generatedCheck),
            TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemVerificationState.Verified, generatedReceipt.VerificationState);
        Assert.Equal("generated full document\n"u8.ToArray(), await File.ReadAllBytesAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier deletes only an ordinary file and verifies missing")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task DeletesOrdinaryFileButNeverDirectory()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-delete");
        var path = temporary.CreateFile("document.md", "remove me");
        var directory = temporary.CreateDirectory("retained");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Delete(
            FileExpectation.File(path, path, FileExpectation.Hash("remove me"u8)));
        await using var lease = await AcquireAsync(workspace);
        var check = await RevalidateAsync(lease, change, validator);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            await PreparationAsync(lease, change, check),
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        Assert.Equal(FileExpectationKind.Missing, receipt.After?.Kind);
        Assert.False(File.Exists(path));
        Assert.True(Directory.Exists(directory));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects stale replay before target effect")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task RejectsStaleReplayAndPreservesCurrentBytes()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-stale");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        var plannedCheck = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        temporary.ReplaceText("document.md", "changed after planning");
        await using var lease = await AcquireAsync(workspace);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            plannedCheck,
            await PreparationAsync(lease, change, plannedCheck),
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, receipt.VerificationState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.Equal("changed after planning", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier reports cancellation without a target effect")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task CancellationBeforeApplicationReturnsNotStarted()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-cancel");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        await using var lease = await AcquireAsync(workspace);
        var check = await RevalidateAsync(lease, change, validator);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            await PreparationAsync(lease, change, check),
            cancellation.Token);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, receipt.VerificationState);
        Assert.Equal(FilesystemNotStartedReason.Cancelled, receipt.NotStartedReason);
        Assert.Equal("before", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier blocks a disposed or foreign lease")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task RequiresLiveLeaseForTheSelectedWorkspace()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-lease");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        var check = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        await using var disposedLease = await AcquireAsync(workspace);
        var preparation = await PreparationAsync(disposedLease, change, check);
        await disposedLease.DisposeAsync();

        var applier = new FileChangeApplier(
            new MutationRevalidator(validator),
            validator);
        var disposed = await applier.ApplyAsync(
            disposedLease,
            change,
            check,
            preparation,
            TestContext.Current.CancellationToken);
        Assert.Equal(FilesystemEffectState.NotStarted, disposed.EffectState);
        Assert.Equal(FilesystemNotStartedReason.ContractRejected, disposed.NotStartedReason);

        using var foreign = TemporaryWorkspace.Create("mutation-apply-foreign");
        await using var foreignLease = await AcquireAsync(Workspace(foreign));
        var foreignResult = await applier.ApplyAsync(
            foreignLease,
            change,
            check,
            preparation,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, foreignResult.EffectState);
        Assert.Equal(FilesystemNotStartedReason.ContractRejected, foreignResult.NotStartedReason);
        Assert.Equal("before", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a create collision before staging")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task CreateCollisionReturnsNotStartedAndPreservesRacingFile()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-create-collision");
        var path = temporary.Combine("created.md");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(path),
            "intended"u8);
        var check = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        temporary.CreateFile("created.md", "racing file");
        await using var lease = await AcquireAsync(workspace);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            recoveryPreparation: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.Equal(change.Expectation, receipt.Before.Expectation);
        Assert.Equal("racing file", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a replacement target replaced by a directory")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ReplacementDirectoryRaceReturnsNotStartedAndPreservesDirectory()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-replacement-directory");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        var check = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        temporary.MoveFile("document.md", "document-before.md");
        temporary.CreateDirectory("document.md");
        await using var lease = await AcquireAsync(workspace);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            await PreparationAsync(lease, change, check),
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.True(Directory.Exists(path));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a delete target replaced after planning")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task DeleteReplacementRaceReturnsNotStartedAndPreservesCurrentFile()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-delete-race");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Delete(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)));
        var check = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        temporary.MoveFile("document.md", "document-before.md");
        temporary.CreateFile("document.md", "replacement");
        await using var lease = await AcquireAsync(workspace);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            await PreparationAsync(lease, change, check),
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.Equal("replacement", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a target whose resolved identity changes before staging")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ResolvedIdentityRaceReturnsNotStartedWithoutTouchingEitherTarget()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-identity-race");
        var first = temporary.CreateFile("first/document.md", "first");
        var second = temporary.CreateFile("second/document.md", "second");
        var alias = temporary.CreateDirectorySymbolicLink("alias", temporary.Combine("first"));
        var logicalPath = Path.Combine(alias, "document.md");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(logicalPath, first, FileExpectation.Hash("first"u8)),
            "intended"u8);
        var check = await validator.ValidateAsync(
            workspace,
            change.Expectation,
            TestContext.Current.CancellationToken);
        if (OperatingSystem.IsWindows())
        {
            Directory.Delete(alias);
        }
        else
        {
            File.Delete(alias);
        }

        Directory.CreateSymbolicLink(alias, temporary.Combine("second"));
        await using var lease = await AcquireAsync(workspace);

        var receipt = await new FileChangeApplier(
                new MutationRevalidator(validator),
                validator)
            .ApplyAsync(
            lease,
            change,
            check,
            await PreparationAsync(lease, change, check),
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.Equal("first", await File.ReadAllTextAsync(
            first,
            TestContext.Current.CancellationToken));
        Assert.Equal("second", await File.ReadAllTextAsync(
            second,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a check for a different planned target")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task MismatchedCheckAndChangeAreRejectedWithoutEffects()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-check-coherence");
        var first = temporary.CreateFile("first.md", "first");
        var second = temporary.CreateFile("second.md", "second");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var firstChange = PlannedFileChange.Replace(
            FileExpectation.File(first, first, FileExpectation.Hash("first"u8)),
            "first intended"u8);
        var secondChange = PlannedFileChange.Replace(
            FileExpectation.File(second, second, FileExpectation.Hash("second"u8)),
            "second intended"u8);
        var firstCheck = await validator.ValidateAsync(
            workspace,
            firstChange.Expectation,
            TestContext.Current.CancellationToken);
        await using var lease = await AcquireAsync(workspace);
        var applier = new FileChangeApplier(
            new MutationRevalidator(validator),
            validator);

        await Assert.ThrowsAsync<ArgumentException>(async () => await applier.ApplyAsync(
            lease,
            secondChange,
            firstCheck,
            recoveryPreparation: null,
            TestContext.Current.CancellationToken));
        Assert.Equal("first", await File.ReadAllTextAsync(
            first,
            TestContext.Current.CancellationToken));
        Assert.Equal("second", await File.ReadAllTextAsync(
            second,
            TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File change applier rejects a forged missing physical target before staging")]
    [Trait("Feature", "mutation-foundation")]
    [Trait("Evidence", "Integration")]
    public async Task ForgedMissingPhysicalTargetIsRejectedBeforeStaging()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-apply-forged-missing");
        var path = temporary.Combine("created.md");
        var otherDirectory = temporary.CreateDirectory("other");
        var forgedPhysicalPath = Path.Combine(otherDirectory, "forged.md");
        var unrelated = temporary.CreateFile("other/unrelated.md", "preserve me");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(path),
            "intended"u8);
        var forgedCheck = FileExpectationValidationResult.Matched(
            change.Expectation,
            FileStateSnapshot.Missing(path),
            forgedPhysicalPath);
        await using var lease = await AcquireAsync(workspace);
        var applier = new FileChangeApplier(
            new MutationRevalidator(validator),
            validator);

        var receipt = await applier.ApplyAsync(
            lease,
            change,
            forgedCheck,
            recoveryPreparation: null,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.False(File.Exists(path));
        Assert.False(File.Exists(forgedPhysicalPath));
        Assert.Equal("preserve me", await File.ReadAllTextAsync(
            unrelated,
            TestContext.Current.CancellationToken));
        Assert.Empty(Stages(temporary));
    }

    private async ValueTask<WorkspaceLockLease> AcquireAsync(CliWorkspace workspace)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "test apply", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    private static async ValueTask<FileExpectationValidationResult> RevalidateAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidator validator)
    {
        var result = await new MutationRevalidator(validator).ValidateAsync(
            lease,
            [change],
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, result.State);
        return Assert.Single(result.Checks);
    }

    public void Dispose()
    {
        foreach (var path in recoveryBundlePaths)
        {
            DeleteRecoveryBundle(path);
        }

        lockStore.Dispose();
    }

    private async ValueTask<RecoveryBundlePreparation> PreparationAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidationResult check)
    {
        var before = Assert.IsType<FileStateSnapshot>(check.Actual);
        var input = RecoveryBundleInput.Create(
            lease.Request.Workspace,
            lease.Request.Command,
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                lease.Request.Workspace),
            lease.Request.OperationId,
            [RecoveryBundleTarget.Create(change, before)]);
        var result = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
        recoveryBundlePaths.Add(preparation.BundlePath);
        return preparation;
    }

    private static void DeleteRecoveryBundle(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static string[] Stages(TemporaryWorkspace temporary)
        => [.. Directory.EnumerateFiles(
            temporary.Path,
            ".open-forge-stage-*",
            SearchOption.AllDirectories)];

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
}

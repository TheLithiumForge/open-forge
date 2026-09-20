using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Validation;

public sealed class MutationValidationIntegrationTests : IDisposable
{
    private readonly WorkspaceLockTestStore lockStore = WorkspaceLockTestStore.Create(
        "mutation-validation-lock-store");

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File expectation validation observes exact real filesystem states")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ValidatorObservesMissingFileDirectoryMismatchAndCancellation()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-validation-states");
        var directoryPath = temporary.CreateDirectory("directory");
        var filePath = temporary.CreateFile("document.md", "before");
        var missingPath = temporary.Combine("missing.md");
        var workspace = Workspace(temporary);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var bytes = Encoding.UTF8.GetBytes("before");

        var missing = await validator.ValidateAsync(
            workspace,
            FileExpectation.Missing(missingPath),
            TestContext.Current.CancellationToken);
        var file = await validator.ValidateAsync(
            workspace,
            FileExpectation.File(filePath, filePath, FileExpectation.Hash(bytes)),
            TestContext.Current.CancellationToken);
        var directory = await validator.ValidateAsync(
            workspace,
            FileExpectation.Directory(directoryPath, directoryPath),
            TestContext.Current.CancellationToken);
        var directoryAsFile = await validator.ValidateAsync(
            workspace,
            FileExpectation.File(
                directoryPath,
                directoryPath,
                FileExpectation.Hash("directory"u8)),
            TestContext.Current.CancellationToken);
        var mismatch = await validator.ValidateAsync(
            workspace,
            FileExpectation.File(filePath, filePath, FileExpectation.Hash("different"u8)),
            TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await validator.ValidateAsync(
            workspace,
            FileExpectation.Missing(missingPath),
            cancellation.Token);

        Assert.Equal(FileExpectationValidationState.Matched, missing.State);
        Assert.Equal(FileExpectationValidationState.Matched, file.State);
        Assert.Equal(bytes, file.Actual?.Bytes.ToArray());
        Assert.Equal(FileExpectationValidationState.Matched, directory.State);
        Assert.Equal(FileExpectationValidationState.Mismatched, directoryAsFile.State);
        Assert.Equal(FileExpectationValidationState.Mismatched, mismatch.State);
        Assert.Equal(FileExpectationValidationState.Cancelled, cancelled.State);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "File expectation validation blocks an escaping physical alias")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ValidatorBlocksExternalPhysicalAlias()
    {
        using var outside = TemporaryWorkspace.Create("mutation-validation-outside");
        var outsideFile = outside.CreateFile("outside.md", "outside");
        using var temporary = TemporaryWorkspace.Create("mutation-validation-alias");
        var alias = temporary.CreateFileSymbolicLink("alias.md", outsideFile);

        var result = await new FileExpectationValidator(new PhysicalPathResolver())
            .ValidateAsync(
                Workspace(temporary),
                FileExpectation.Missing(alias),
                TestContext.Current.CancellationToken);

        Assert.Equal(FileExpectationValidationState.Blocked, result.State);
        Assert.Null(result.Actual);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation preflight blocks aliased prospective create targets")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PreflightBlocksMissingTargetsWithOneProspectivePhysicalIdentity()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-validation-prospective-alias");
        var shared = temporary.CreateDirectory("shared");
        temporary.CreateDirectorySymbolicLink("alias-a", shared);
        temporary.CreateDirectorySymbolicLink("alias-b", shared);
        var workspace = Workspace(temporary);
        var changes = new[]
        {
            PlannedFileChange.Create(
                FileExpectation.Missing(temporary.Combine("alias-a/new.md")),
                "first"u8),
            PlannedFileChange.Create(
                FileExpectation.Missing(temporary.Combine("alias-b/new.md")),
                "second"u8),
        };

        var result = await new MutationPreflight(
                new FileExpectationValidator(new PhysicalPathResolver()))
            .ValidateAsync(
                workspace,
                changes,
                TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Blocked, result.State);
        Assert.Equal(2, result.Checks.Count);
        Assert.False(File.Exists(Path.Combine(shared, "new.md")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation preflight accepts nonempty plans and reports cancellation and real I/O failure"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PreflightReportsNonemptyCancellationAndExclusiveHandleFailure()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-validation-preflight-states");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "after"u8);
        var preflight = new MutationPreflight(validator);

        var valid = await preflight.ValidateAsync(
            workspace,
            [change],
            TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = await preflight.ValidateAsync(
            workspace,
            [change],
            cancellation.Token);
        using var handle = new FileStream(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.ReadWrite,
                Share = FileShare.None,
            });
        var failed = await preflight.ValidateAsync(
            workspace,
            [change],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, valid.State);
        Assert.Single(valid.Checks);
        Assert.Equal(MutationValidationState.Cancelled, cancelled.State);
        Assert.Equal(MutationValidationState.Failed, failed.State);
        Assert.Equal(FilesystemFailureKind.InputOutput, failed.Failure?.Kind);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation revalidation observes an actual directory-entry replacement"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task RevalidatorDetectsDirectoryEntryReplacement()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-revalidation-entry-replacement");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "route update", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

        temporary.MoveFile("document.md", "document-old.md");
        temporary.CreateFile("document.md", "changed-after-entry-replacement");
        var result = await new MutationRevalidator(validator).ValidateAsync(
            lease,
            [change],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Mismatched, result.State);
        Assert.Equal("changed-after-entry-replacement", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));
        await lease.DisposeAsync();
        Assert.True(File.Exists(lockStore.Track(workspace)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation revalidation holds external coordination independently of workspace .agents changes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task RevalidatorUsesExternalLeaseWhenAgentsDirectoryAppears()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-revalidation-agents-appearance");
        var documentPath = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var change = PlannedFileChange.Replace(
            FileExpectation.File(
                documentPath,
                documentPath,
                FileExpectation.Hash("before"u8)),
            "intended"u8);
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "route update", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        temporary.CreateDirectory(".agents");

        var result = await new MutationRevalidator(validator).ValidateAsync(
            lease,
            [change],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, result.State);
        Assert.True(lease.IsHeldFor(workspace));
        Assert.True(Directory.Exists(temporary.Combine(".agents")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation preflight no longer reserves the former workspace lock path"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PreflightTreatsFormerWorkspaceLockPathAsOrdinaryTarget()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-validation-former-lock-path");
        temporary.CreateDirectory(".agents");
        var formerLockPath = temporary.Combine(".agents/open-forge.lock");
        var workspace = Workspace(temporary);
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(formerLockPath),
            "ordinary-target"u8);

        var result = await new MutationPreflight(
                new FileExpectationValidator(new PhysicalPathResolver()))
            .ValidateAsync(
                workspace,
                [change],
                TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, result.State);
        Assert.Single(result.Checks);
        Assert.False(File.Exists(formerLockPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Mutation revalidation detects a stale expectation under the live lock")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task RevalidatorRequiresLiveLeaseAndDetectsAfterPlanChangeWithoutEffects()
    {
        using var temporary = TemporaryWorkspace.Create("mutation-revalidation-stale");
        var path = temporary.CreateFile("document.md", "before");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var change = PlannedFileChange.Replace(
            FileExpectation.File(path, path, FileExpectation.Hash("before"u8)),
            "intended"u8);
        var changes = new[] { change };
        var preflight = await new MutationPreflight(validator).ValidateAsync(
            workspace,
            changes,
            TestContext.Current.CancellationToken);
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "route update", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var revalidator = new MutationRevalidator(validator);
        var empty = await revalidator.ValidateAsync(
            lease,
            [],
            TestContext.Current.CancellationToken);
        temporary.ReplaceText("document.md", "changed-after-plan");

        var stale = await revalidator.ValidateAsync(
            lease,
            changes,
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, preflight.State);
        Assert.Equal(MutationValidationState.Blocked, empty.State);
        Assert.Equal(MutationValidationState.Mismatched, stale.State);
        Assert.Equal("changed-after-plan", await File.ReadAllTextAsync(
            path,
            TestContext.Current.CancellationToken));

        await lease.DisposeAsync();
        var disposed = await revalidator.ValidateAsync(
            lease,
            changes,
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Blocked, disposed.State);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    public void Dispose() => lockStore.Dispose();
}

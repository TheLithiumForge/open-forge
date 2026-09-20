using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveDirectoryDeletionIntegrationTests : IDisposable
{
    private readonly WorkspaceLockTestStore lockStore = WorkspaceLockTestStore.Create(
        "route-move-directory-delete-lock-store");

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move directory deletion removes one exact empty directory nonrecursively")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task EmptyDirectoryIsRemovedAndVerifiedWithoutRecoveryBytes()
    {
        using var temporary = TemporaryWorkspace.Create("route-move-delete-empty");
        var target = temporary.CreateDirectory("old/leaf");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(
            FileExpectation.Directory(target, target));
        var check = await MatchedCheckAsync(workspace, deletion, validator);
        await using var lease = await AcquireAsync(workspace);

        var receipt = await Applier(validator).ApplyAsync(
            lease,
            deletion,
            check,
            TestContext.Current.CancellationToken);

        Assert.Equal(DirectoryDeletionDisposition.Removed, receipt.State.Disposition);
        Assert.Equal(FilesystemEffectState.Applied, receipt.State.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, receipt.State.VerificationState);
        Assert.Equal(FileExpectationKind.Missing, receipt.After?.Kind);
        Assert.False(Directory.Exists(target));
        Assert.True(Directory.Exists(temporary.Combine("old")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move directory deletion retains a nonempty directory and every child byte")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task NonemptyDirectoryIsRetainedWithoutRecursiveDeletion()
    {
        using var temporary = TemporaryWorkspace.Create("route-move-delete-nonempty");
        var target = temporary.CreateDirectory("old/leaf");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(
            FileExpectation.Directory(target, target));
        var check = await MatchedCheckAsync(workspace, deletion, validator);
        temporary.CreateFile("old/leaf/concurrent.md", "preserve me\n");
        await using var lease = await AcquireAsync(workspace);

        var receipt = await Applier(validator).ApplyAsync(
            lease,
            deletion,
            check,
            TestContext.Current.CancellationToken);

        Assert.Equal(DirectoryDeletionDisposition.Retained, receipt.State.Disposition);
        Assert.Equal(FilesystemEffectState.NotStarted, receipt.State.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.State.NotStartedReason);
        Assert.Equal("preserve me\n", await File.ReadAllTextAsync(
            temporary.Combine("old/leaf/concurrent.md"),
            TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Move directory deletion observes cancellation before the target effect")]
    [Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task CancellationLeavesTheDirectoryUntouched()
    {
        using var temporary = TemporaryWorkspace.Create("route-move-delete-cancelled");
        var target = temporary.CreateDirectory("old/leaf");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var deletion = PlannedDirectoryDeletion.DeleteIfEmpty(
            FileExpectation.Directory(target, target));
        var check = await MatchedCheckAsync(workspace, deletion, validator);
        await using var lease = await AcquireAsync(workspace);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var receipt = await Applier(validator).ApplyAsync(
            lease,
            deletion,
            check,
            cancellation.Token);

        Assert.Equal(DirectoryDeletionDisposition.Retained, receipt.State.Disposition);
        Assert.Equal(FilesystemEffectState.NotStarted, receipt.State.EffectState);
        Assert.Equal(FilesystemNotStartedReason.Cancelled, receipt.State.NotStartedReason);
        Assert.True(Directory.Exists(target));
    }

    public void Dispose() => lockStore.Dispose();

    private static DirectoryDeletionApplier Applier(FileExpectationValidator validator)
        => new(new MutationRevalidator(validator), validator);

    private async ValueTask<WorkspaceLockLease> AcquireAsync(CliWorkspace workspace)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                workspace,
                "route move directory deletion",
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    private static async ValueTask<FileExpectationValidationResult> MatchedCheckAsync(
        CliWorkspace workspace,
        PlannedDirectoryDeletion deletion,
        FileExpectationValidator validator)
    {
        var check = await validator.ValidateAsync(
            workspace,
            deletion.Expectation,
            TestContext.Current.CancellationToken);
        Assert.Equal(FileExpectationValidationState.Matched, check.State);
        return check;
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}

using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Completion;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Shared.Completion;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionReceiptProjectionTests
{
    [Theory]
    [InlineData((int)FilesystemEffectState.NotStarted, (int)FilesystemVerificationState.NotStarted, (int)WorkspacePermissionOutcome.NotStarted)]
    [InlineData((int)FilesystemEffectState.Unknown, (int)FilesystemVerificationState.NotStarted, (int)WorkspacePermissionOutcome.CompletionUnknown)]
    [InlineData((int)FilesystemEffectState.Applied, (int)FilesystemVerificationState.Verified, (int)WorkspacePermissionOutcome.Verified)]
    [InlineData((int)FilesystemEffectState.Applied, (int)FilesystemVerificationState.Failed, (int)WorkspacePermissionOutcome.VerificationFailed)]
    public void MapsEverySupportedState(int effect, int verification, int expected)
        => Assert.Equal((WorkspacePermissionOutcome)expected,
            WorkspacePermissionReceiptProjection.ReadOutcome((FilesystemEffectState)effect, (FilesystemVerificationState)verification));

    [Fact]
    public void RejectsUndefinedStatesAndAppliedWithoutVerification()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionReceiptProjection.ReadOutcome(
            (FilesystemEffectState)99, FilesystemVerificationState.NotStarted));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionReceiptProjection.ReadOutcome(
            FilesystemEffectState.Applied, (FilesystemVerificationState)99));
        Assert.Throws<InvalidOperationException>(() => WorkspacePermissionReceiptProjection.ReadOutcome(
            FilesystemEffectState.Applied, FilesystemVerificationState.NotStarted));
    }

    [Fact]
    public void PreservesVerifiedFailedUnavailableAndUnknownReceiptMeaning()
    {
        var path = Path.GetFullPath(Path.Combine("permission-unit", ".agents", "open-forge.permissions.json"));
        var before = FileStateSnapshot.Missing(path);
        var intended = Encoding.UTF8.GetBytes("approved\n");
        var change = PlannedFileChange.Create(before.Expectation, intended);
        var after = FileStateSnapshot.File(path, path, intended);
        var different = FileStateSnapshot.File(path, path, "different"u8);

        Assert.Equal(WorkspacePermissionOutcome.Verified, WorkspacePermissionReceiptProjection.ReadOutcome(
            FileChangeReceipt.Verified(change, before, after)));
        Assert.Equal(WorkspacePermissionOutcome.NotStarted, WorkspacePermissionReceiptProjection.ReadOutcome(
            FileChangeReceipt.NotStarted(change, before, FilesystemNotStartedReason.Cancelled, "cancelled")));
        Assert.Equal(WorkspacePermissionOutcome.VerificationFailed, WorkspacePermissionReceiptProjection.ReadOutcome(
            FileChangeReceipt.VerificationFailed(change, before, different, "different after-state")));
        Assert.Equal(WorkspacePermissionOutcome.VerificationFailed, WorkspacePermissionReceiptProjection.ReadOutcome(
            FileChangeReceipt.VerificationUnavailable(change, before, "unavailable after-state")));
        Assert.Equal(WorkspacePermissionOutcome.CompletionUnknown, WorkspacePermissionReceiptProjection.ReadOutcome(
            FileChangeReceipt.CompletionUnknown(change, before, after: null, cause: "unknown completion")));
    }
}

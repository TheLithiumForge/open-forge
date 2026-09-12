using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Models.Filesystem;

public sealed class DirectoryMutationContractTests
{
    [Fact(DisplayName = "Planned directory creations require one missing absolute target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlannedDirectoryCreationsRequireMissingTargets()
    {
        var paths = Paths();
        var missing = FileExpectation.Missing(paths.Logical);
        var creation = PlannedDirectoryCreation.Create(missing);

        Assert.Same(missing, creation.Expectation);
        Assert.Equal(paths.Logical, creation.LogicalPath);
        Assert.Throws<ArgumentException>(() => PlannedDirectoryCreation.Create(
            FileExpectation.Directory(paths.Logical, paths.Physical)));
        Assert.Throws<ArgumentException>(() => PlannedDirectoryCreation.Create(
            FileExpectation.File(
                paths.Logical,
                paths.Physical,
                FileExpectation.Hash("content"u8))));
    }

    [Fact(DisplayName = "Directory creation receipts expose every state-valid mechanical outcome")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void DirectoryCreationReceiptsExposeFiniteStates()
    {
        var paths = Paths();
        var creation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(paths.Logical));
        var before = FileStateSnapshot.Missing(paths.Logical);
        var intended = FileStateSnapshot.Directory(paths.Logical, paths.Physical);
        var unexpected = FileStateSnapshot.File(paths.Logical, paths.Physical, "file"u8);
        var retargeted = FileStateSnapshot.Directory(paths.Logical, paths.OtherPhysical);

        var verified = DirectoryCreationReceipt.Verified(
            creation,
            before,
            paths.Physical,
            intended);
        var failed = DirectoryCreationReceipt.VerificationFailed(
            creation,
            before,
            paths.Physical,
            unexpected,
            "Verification differed.");
        var unavailable = DirectoryCreationReceipt.VerificationUnavailable(
            creation,
            before,
            paths.Physical,
            "Verification was unavailable.");
        var notStarted = DirectoryCreationReceipt.NotStarted(
            creation,
            before,
            paths.Physical,
            FilesystemNotStartedReason.TargetChanged,
            "The target changed.");
        var unknown = DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            paths.Physical,
            after: null,
            "Completion was unknown.");
        var unknownObserved = DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            paths.Physical,
            retargeted,
            "Completion was unknown.");

        Assert.Equal(FilesystemEffectState.Applied, verified.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, verified.VerificationState);
        Assert.Same(intended, verified.After);
        Assert.Equal(paths.Physical, verified.IntendedPhysicalPath);
        Assert.Equal(FilesystemEffectState.Applied, failed.EffectState);
        Assert.Equal(FilesystemVerificationState.Failed, failed.VerificationState);
        Assert.Same(unexpected, failed.After);
        Assert.Equal(FilesystemEffectState.Applied, unavailable.EffectState);
        Assert.Null(unavailable.After);
        Assert.Equal(FilesystemEffectState.NotStarted, notStarted.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, notStarted.NotStartedReason);
        Assert.Equal(FilesystemEffectState.Unknown, unknown.EffectState);
        Assert.Null(unknown.After);
        Assert.Equal(FilesystemEffectState.Unknown, unknownObserved.EffectState);
        Assert.Same(retargeted, unknownObserved.After);
    }

    [Fact(DisplayName = "Directory creation receipts reject contradictory states and undefined reasons")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void DirectoryCreationReceiptsRejectImpossibleStates()
    {
        var paths = Paths();
        var creation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(paths.Logical));
        var before = FileStateSnapshot.Missing(paths.Logical);
        var wrongBefore = FileStateSnapshot.Missing(paths.OtherLogical);
        var intended = FileStateSnapshot.Directory(paths.Logical, paths.Physical);
        var retargeted = FileStateSnapshot.Directory(paths.Logical, paths.OtherPhysical);

        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.Verified(
            creation,
            wrongBefore,
            paths.Physical,
            intended));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.Verified(
            creation,
            before,
            paths.Physical,
            retargeted));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.VerificationFailed(
            creation,
            before,
            paths.Physical,
            intended,
            "Verification differed."));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            paths.Physical,
            intended,
            "Completion was unknown."));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            paths.Physical,
            before,
            "Completion was unknown."));
        Assert.Throws<ArgumentOutOfRangeException>(() => DirectoryCreationReceipt.NotStarted(
            creation,
            before,
            paths.Physical,
            (FilesystemNotStartedReason)int.MaxValue,
            "The effect did not start."));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.NotStarted(
            creation,
            before,
            paths.Physical,
            FilesystemNotStartedReason.ApplicationFailed,
            ""));
    }

    [Fact(DisplayName = "Directory creation failure receipts reject after-states for another logical target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void DirectoryCreationFailureReceiptsRequireThePlannedLogicalTarget()
    {
        var paths = Paths();
        var creation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(paths.Logical));
        var before = FileStateSnapshot.Missing(paths.Logical);
        var wrongTarget = FileStateSnapshot.Directory(
            paths.OtherLogical,
            paths.OtherPhysical);

        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.VerificationFailed(
            creation,
            before,
            paths.Physical,
            wrongTarget,
            "Verification differed."));
        Assert.Throws<ArgumentException>(() => DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            paths.Physical,
            wrongTarget,
            "Completion was unknown."));
    }

    [Fact(DisplayName = "Filesystem receipt enums retain their exact shared members")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FilesystemReceiptEnumsRetainExactMembers()
    {
        Assert.Equal(
            [FilesystemEffectState.NotStarted, FilesystemEffectState.Applied, FilesystemEffectState.Unknown],
            Enum.GetValues<FilesystemEffectState>());
        Assert.Equal(
            [FilesystemVerificationState.NotStarted, FilesystemVerificationState.Verified, FilesystemVerificationState.Failed],
            Enum.GetValues<FilesystemVerificationState>());
        Assert.Equal(
            [
                FilesystemNotStartedReason.Cancelled,
                FilesystemNotStartedReason.TargetChanged,
                FilesystemNotStartedReason.ApplicationFailed,
                FilesystemNotStartedReason.ContractRejected,
            ],
            Enum.GetValues<FilesystemNotStartedReason>());
    }

    private static (
        string Logical,
        string Physical,
        string OtherLogical,
        string OtherPhysical) Paths()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-directory-contract-tests"));
        return (
            Path.Combine(root, "target"),
            Path.Combine(root, "target"),
            Path.Combine(root, "other"),
            Path.Combine(root, "other"));
    }
}

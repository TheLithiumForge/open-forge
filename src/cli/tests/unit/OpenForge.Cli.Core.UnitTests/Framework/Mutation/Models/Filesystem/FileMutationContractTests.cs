using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Models.Filesystem;

public sealed class FileMutationContractTests
{
    private static readonly byte[] BeforeBytes = Encoding.UTF8.GetBytes("before\n");
    private static readonly byte[] AfterBytes = Encoding.UTF8.GetBytes("after\n");
    private static readonly byte[] DifferentBytes = Encoding.UTF8.GetBytes("different\n");

    [Fact(DisplayName = "File expectations preserve only state-valid path identity and hash facts"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FileExpectationsPreserveValidStates()
    {
        var paths = Paths();

        var missing = FileExpectation.Missing(paths.Logical);
        var file = FileExpectation.File(
            logicalPath: paths.Logical,
            physicalPath: paths.Physical,
            contentHash: FileExpectation.Hash(BeforeBytes));
        var directory = FileExpectation.Directory(paths.Logical, paths.Physical);

        Assert.Equal(FileExpectationKind.Missing, missing.Kind);
        Assert.False(missing.Exists);
        Assert.Null(missing.PhysicalPath);
        Assert.Null(missing.ContentHash);
        Assert.Equal(FileExpectationKind.File, file.Kind);
        Assert.True(file.Exists);
        Assert.Equal(paths.Physical, file.PhysicalPath);
        Assert.Equal(FileExpectation.Hash(BeforeBytes), file.ContentHash);
        Assert.Equal(FileExpectationKind.Directory, directory.Kind);
        Assert.True(directory.Exists);
        Assert.Null(directory.ContentHash);
    }

    [Theory(DisplayName = "File expectations reject relative paths and noncanonical hashes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    [InlineData("relative-path", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("absolute-path", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("absolute-path", "abc")]
    public void FileExpectationsRejectInvalidIdentity(string pathScenario, string hash)
    {
        var paths = Paths();
        var logicalPath = pathScenario == "relative-path" ? "relative.md" : paths.Logical;

        Assert.Throws<ArgumentException>(() => FileExpectation.File(
            logicalPath: logicalPath,
            physicalPath: paths.Physical,
            contentHash: hash));
    }

    [Fact(DisplayName = "File snapshots own exact bytes and derive their exact-byte hash"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FileSnapshotsOwnExactBytes()
    {
        var paths = Paths();
        var source = BeforeBytes.ToArray();

        var snapshot = FileStateSnapshot.File(paths.Logical, paths.Physical, source);
        source[0] = (byte)'X';

        Assert.True(snapshot.HasBytes);
        Assert.Equal(BeforeBytes, snapshot.Bytes);
        Assert.Equal(FileExpectation.Hash(BeforeBytes), snapshot.ContentHash);
    }

    [Fact(DisplayName = "Planned file changes admit only their compatible expected states"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void PlannedChangesRequireCompatibleExpectations()
    {
        var paths = Paths();
        var missing = FileExpectation.Missing(paths.Logical);
        var file = FileExpectation.File(
            logicalPath: paths.Logical,
            physicalPath: paths.Physical,
            contentHash: FileExpectation.Hash(BeforeBytes));

        var create = PlannedFileChange.Create(missing, AfterBytes);
        var replace = PlannedFileChange.Replace(file, AfterBytes);
        var delete = PlannedFileChange.Delete(file);
        var generated = PlannedFileChange.ReplaceGeneratedRegion(file, AfterBytes);

        Assert.Equal(PlannedFileChangeKind.Create, create.Kind);
        Assert.Equal(AfterBytes, create.IntendedBytes);
        Assert.Equal(PlannedFileChangeKind.Replace, replace.Kind);
        Assert.Equal(PlannedFileChangeKind.Delete, delete.Kind);
        Assert.False(delete.HasIntendedBytes);
        Assert.Empty(delete.IntendedBytes);
        Assert.Equal(PlannedFileChangeKind.ReplaceGeneratedRegion, generated.Kind);
        Assert.Throws<ArgumentException>(() => PlannedFileChange.Create(file, AfterBytes));
        Assert.Throws<ArgumentException>(() => PlannedFileChange.Replace(missing, AfterBytes));
        Assert.Throws<ArgumentException>(() => PlannedFileChange.Delete(missing));
        Assert.Throws<ArgumentException>(() => PlannedFileChange.ReplaceGeneratedRegion(missing, AfterBytes));
        Assert.Throws<ArgumentException>(() => PlannedFileChange.Replace(file, BeforeBytes));
        Assert.Throws<ArgumentException>(() => PlannedFileChange.ReplaceGeneratedRegion(file, BeforeBytes));
    }

    [Fact(DisplayName = "File receipts distinguish verified mismatched unavailable not-started and unknown outcomes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FileReceiptsExposeFiniteStates()
    {
        var paths = Paths();
        var before = FileStateSnapshot.File(paths.Logical, paths.Physical, BeforeBytes);
        var after = FileStateSnapshot.File(paths.Logical, paths.Physical, AfterBytes);
        var different = FileStateSnapshot.File(paths.Logical, paths.Physical, DifferentBytes);
        var change = PlannedFileChange.Replace(before.Expectation, AfterBytes);

        var verified = FileChangeReceipt.Verified(change, before, after);
        var failed = FileChangeReceipt.VerificationFailed(change, before, before, "Verification differed.");
        var unavailable = FileChangeReceipt.VerificationUnavailable(change, before, "Verification was unavailable.");
        var notStarted = FileChangeReceipt.NotStarted(
            change,
            before,
            FilesystemNotStartedReason.TargetChanged,
            "The expectation changed.");
        var unknown = FileChangeReceipt.CompletionUnknown(change, before, after: null, "The process ended.");
        var unknownObserved = FileChangeReceipt.CompletionUnknown(
            change,
            before,
            different,
            "The process ended after an observed effect.");

        Assert.Equal(FilesystemEffectState.Applied, verified.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, verified.VerificationState);
        Assert.Null(verified.Cause);
        Assert.Equal(FilesystemEffectState.Applied, failed.EffectState);
        Assert.Equal(FilesystemVerificationState.Failed, failed.VerificationState);
        Assert.Same(before, failed.After);
        Assert.Null(failed.NotStartedReason);
        Assert.Equal(FilesystemEffectState.Applied, unavailable.EffectState);
        Assert.Equal(FilesystemVerificationState.Failed, unavailable.VerificationState);
        Assert.Null(unavailable.After);
        Assert.Null(unavailable.NotStartedReason);
        Assert.Equal(FilesystemEffectState.NotStarted, notStarted.EffectState);
        Assert.Null(notStarted.After);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, notStarted.NotStartedReason);
        Assert.Equal(FilesystemEffectState.Unknown, unknown.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, unknown.VerificationState);
        Assert.Null(unknown.NotStartedReason);
        Assert.Equal(FilesystemEffectState.Unknown, unknownObserved.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, unknownObserved.VerificationState);
        Assert.Same(different, unknownObserved.After);
    }

    [Fact(DisplayName = "Not-started file receipts retain every defined neutral reason"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void NotStartedReceiptsRequireDefinedReasons()
    {
        var paths = Paths();
        var before = FileStateSnapshot.File(paths.Logical, paths.Physical, BeforeBytes);
        var change = PlannedFileChange.Replace(before.Expectation, AfterBytes);
        FilesystemNotStartedReason[] reasons =
        [
            FilesystemNotStartedReason.Cancelled,
            FilesystemNotStartedReason.TargetChanged,
            FilesystemNotStartedReason.ApplicationFailed,
            FilesystemNotStartedReason.ContractRejected,
        ];

        foreach (var reason in reasons)
        {
            var receipt = FileChangeReceipt.NotStarted(change, before, reason, "The effect did not start.");

            Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
            Assert.Equal(FilesystemVerificationState.NotStarted, receipt.VerificationState);
            Assert.Equal(reason, receipt.NotStartedReason);
            Assert.Null(receipt.After);
        }
    }

    [Fact(DisplayName = "File receipts reject mismatched before state and unverified intended bytes"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void FileReceiptsRejectImpossibleStates()
    {
        var paths = Paths();
        var before = FileStateSnapshot.File(paths.Logical, paths.Physical, BeforeBytes);
        var mismatchedBefore = FileStateSnapshot.File(paths.Logical, paths.Physical, AfterBytes);
        var mismatchedAfter = FileStateSnapshot.File(paths.Logical, paths.Physical, BeforeBytes);
        var retargetedAfter = FileStateSnapshot.File(
            paths.Logical,
            Path.ChangeExtension(paths.Physical, ".retargeted.md"),
            AfterBytes);
        var change = PlannedFileChange.Replace(before.Expectation, AfterBytes);

        Assert.Throws<ArgumentException>(() => FileChangeReceipt.Verified(change, mismatchedBefore, mismatchedAfter));
        Assert.Throws<ArgumentException>(() => FileChangeReceipt.Verified(change, before, mismatchedAfter));
        Assert.Throws<ArgumentException>(() => FileChangeReceipt.Verified(change, before, retargetedAfter));
        Assert.Throws<ArgumentException>(() =>
            FileChangeReceipt.VerificationFailed(change, before, mismatchedBefore, "Verification differed."));
        Assert.Throws<ArgumentException>(() =>
            FileChangeReceipt.CompletionUnknown(change, before, mismatchedBefore, "Completion was unknown."));
        Assert.Throws<ArgumentException>(() =>
            FileChangeReceipt.CompletionUnknown(change, before, before, "Completion was unknown."));
        Assert.Throws<ArgumentException>(() => FileChangeReceipt.NotStarted(
            change,
            before,
            FilesystemNotStartedReason.ApplicationFailed,
            ""));
        Assert.Throws<ArgumentOutOfRangeException>(() => FileChangeReceipt.NotStarted(
            change,
            before,
            (FilesystemNotStartedReason)int.MaxValue,
            "The effect did not start."));
    }

    [Fact(DisplayName = "Verified deletion receipts require the target to be absent"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public void VerifiedDeletionRequiresMissingAfterState()
    {
        var paths = Paths();
        var before = FileStateSnapshot.File(paths.Logical, paths.Physical, BeforeBytes);
        var change = PlannedFileChange.Delete(before.Expectation);

        var receipt = FileChangeReceipt.Verified(
            change,
            before,
            FileStateSnapshot.Missing(paths.Logical));

        Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        Assert.Equal(FileExpectationKind.Missing, receipt.After?.Kind);
        Assert.Throws<ArgumentException>(() => FileChangeReceipt.Verified(change, before, before));
        var intended = FileStateSnapshot.Missing(paths.Logical);
        Assert.Throws<ArgumentException>(() =>
            FileChangeReceipt.VerificationFailed(change, before, intended, "Verification differed."));
        Assert.Throws<ArgumentException>(() =>
            FileChangeReceipt.CompletionUnknown(change, before, intended, "Completion was unknown."));
    }

    private static (string Logical, string Physical) Paths()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-contract-tests"));
        var path = Path.Combine(root, "target.md");
        return (path, path);
    }
}

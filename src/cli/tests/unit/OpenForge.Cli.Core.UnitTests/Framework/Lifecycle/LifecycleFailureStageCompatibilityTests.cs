using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class LifecycleFailureStageCompatibilityTests
{
    [Fact(DisplayName = "Lifecycle consumers preserve distinct mechanical failure-stage policies"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void LifecycleStoreAndExtensionReaderPreserveDistinctFailureStageMappings()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-lifecycle-failure-compatibility"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var lifecyclePath = Path.Combine(root, LifecycleSchema.RelativePath);
        var resolutionFailure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Resolution denied.");
        var reconfirmationFailure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Reconfirmation denied.");
        var containedAccessFailure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Contained access denied.");
        var snapshots = new[]
        {
            LifecycleDocumentSnapshot.Unavailable(
                workspace,
                resolutionFailure,
                LifecycleDocumentFailureStage.PhysicalResolution),
            LifecycleDocumentSnapshot.Unavailable(
                workspace,
                reconfirmationFailure,
                LifecycleDocumentFailureStage.PhysicalReconfirmation),
            LifecycleDocumentSnapshot.Unavailable(
                workspace,
                containedAccessFailure,
                LifecycleDocumentFailureStage.ContainedFileAccess),
            LifecycleDocumentSnapshot.Blocked(
                workspace,
                FileStateSnapshot.Directory(lifecyclePath, lifecyclePath),
                "The lifecycle document path is a directory."),
            LifecycleDocumentSnapshot.Blocked(
                workspace,
                file: null,
                "The lifecycle document physical boundary is unsafe."),
        };
        var store = new LifecycleStore(new PhysicalPathResolver());
        var extensionReader = new LifecycleDocumentReader(new PhysicalPathResolver());

        var storeResults = snapshots
            .Select(snapshot => store.Read(snapshot, LifecycleSection.Framework))
            .ToArray();
        var extensionResults = snapshots
            .Select(extensionReader.ReadExtensions)
            .ToArray();

        Assert.Equal(
            [
                LifecycleDocumentFailureStage.PhysicalResolution,
                LifecycleDocumentFailureStage.PhysicalReconfirmation,
                LifecycleDocumentFailureStage.ContainedFileAccess,
                null,
                null,
            ],
            snapshots.Select(snapshot => snapshot.FailureStage));
        Assert.Equal(
            [
                LifecycleStoreReadState.Unavailable,
                LifecycleStoreReadState.Unavailable,
                LifecycleStoreReadState.Unavailable,
                LifecycleStoreReadState.Invalid,
                LifecycleStoreReadState.Blocked,
            ],
            storeResults.Select(result => result.State));
        Assert.Same(resolutionFailure, storeResults[0].Failure);
        Assert.Same(reconfirmationFailure, storeResults[1].Failure);
        Assert.Same(containedAccessFailure, storeResults[2].Failure);
        Assert.Null(storeResults[3].Failure);
        Assert.Null(storeResults[4].Failure);

        var expectedExtensionResults = new (
            string Scenario,
            LifecycleReadState State,
            LifecycleExtensionTrust Trust,
            LifecycleCoverageState Coverage,
            LifecycleWorkspaceBinding WorkspaceBinding)[]
        {
            ("physical-resolution", LifecycleReadState.Invalid, LifecycleExtensionTrust.Blocked, LifecycleCoverageState.Blocked, LifecycleWorkspaceBinding.Unavailable),
            ("physical-reconfirmation", LifecycleReadState.Invalid, LifecycleExtensionTrust.Blocked, LifecycleCoverageState.Blocked, LifecycleWorkspaceBinding.Unavailable),
            ("contained-file-access", LifecycleReadState.Unavailable, LifecycleExtensionTrust.Incomplete, LifecycleCoverageState.Incomplete, LifecycleWorkspaceBinding.NotChecked),
            ("directory", LifecycleReadState.Unavailable, LifecycleExtensionTrust.Incomplete, LifecycleCoverageState.Incomplete, LifecycleWorkspaceBinding.NotChecked),
            ("unsafe-boundary", LifecycleReadState.Invalid, LifecycleExtensionTrust.Blocked, LifecycleCoverageState.Blocked, LifecycleWorkspaceBinding.Unavailable),
        };
        var actualExtensionResults = extensionResults
            .Select((result, index) => (
                expectedExtensionResults[index].Scenario,
                result.State,
                result.Trust,
                result.Coverage,
                result.WorkspaceBinding))
            .ToArray();
        Assert.All(
            expectedExtensionResults.Zip(actualExtensionResults),
            row => Assert.Equal(row.First, row.Second));
    }
}

using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Filesystem;

public sealed class FilesystemValueTests
{
    [Fact(DisplayName = "Physical containment respects path-root boundaries")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void PhysicalContainmentRespectsRootBoundaries()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "workspace"));

        Assert.True(PhysicalContainment.Contains(root, root));
        Assert.True(PhysicalContainment.Contains(root, Path.Combine(root, "child")));
        Assert.False(PhysicalContainment.Contains(root, root + "-other"));
    }

    [Fact(DisplayName = "Physical walker fails closed for unsupported components")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void PhysicalWalkerFailsClosedForUnsupportedComponents()
    {
        var failure = new FilesystemFailure(
            FilesystemFailureKind.Unsupported,
            "Unsupported reparse point.");
        var component = new PathComponent(
            PathComponentState.Unsupported,
            "component",
            null,
            FileAttributes.ReparsePoint,
            failure);

        var result = PathComponentWalker.ClassifyComponent("logical", component);

        Assert.Equal(PhysicalPathState.Unsupported, result?.State);
        Assert.Same(failure, result?.Failure);
    }

    [Fact(DisplayName = "Physical identity tracker distinguishes active cycles from sequential aliases")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void IdentityTrackerDistinguishesCyclesFromSequentialAliases()
    {
        var tracker = new PhysicalIdentityTracker();
        var target = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "target"));

        Assert.Null(tracker.Enter("first", target));
        Assert.Equal("first", tracker.Enter("cycle", target));
        tracker.Exit(target);
        Assert.Null(tracker.Enter("alias", target));
        tracker.Exit(target);
    }

    [Fact(DisplayName = "Typed read values preserve invalid syntax separately")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void TypedReadValuesPreserveInvalidSyntaxSeparately()
    {
        var failure = new FilesystemFailure(FilesystemFailureKind.InvalidSyntax, "Malformed document.");

        var result = FileReadResult<string>.Failed(FileReadState.InvalidSyntax, "document", failure);

        Assert.Equal(FileReadState.InvalidSyntax, result.State);
        Assert.Equal(FilesystemFailureKind.InvalidSyntax, result.Failure?.Kind);
        Assert.Equal("document", result.LogicalPath);
    }

    [Fact(DisplayName = "Filesystem result factories reject inconsistent states")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void FilesystemResultFactoriesRejectInconsistentStates()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            PhysicalPathResolution.Classified(PhysicalPathState.External, "logical"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PhysicalPathResolution.Classified((PhysicalPathState)int.MaxValue, "logical"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CliWorkspaceSelectionResult.Classified((CliWorkspaceSelectionState)int.MaxValue));

        var failure = new FilesystemFailure(FilesystemFailureKind.InputOutput, "I/O failure.");
        var selection = CliWorkspaceSelectionResult.Failed(
            CliWorkspaceSelectionState.InputOutputFailure,
            failure);
        Assert.Equal(CliWorkspaceSelectionState.InputOutputFailure, selection.State);
        Assert.Same(failure, selection.Failure);
        var mismatched = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Access denied.");
        Assert.Throws<ArgumentException>(() =>
            PhysicalPathResolution.Failed(PhysicalPathState.InputOutputFailure, "logical", mismatched));
        Assert.Throws<ArgumentException>(() =>
            CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.InputOutputFailure,
                mismatched));
        Assert.Throws<ArgumentException>(() =>
            FileReadResult<string>.Failed(FileReadState.InvalidSyntax, "document", mismatched));
    }

    [Fact(DisplayName = "Filesystem exception projection removes physical paths")]
    [Trait("Feature", "cli-filesystem"), Trait("Evidence", "Unit")]
    public void ExceptionProjectionRemovesPhysicalPaths()
    {
        const string sensitivePath = "C:\\private\\workspace\\secret.txt";
        var failure = FilesystemFailure.FromException(
            FilesystemFailureKind.InputOutput,
            new IOException($"Unable to read {sensitivePath}."));

        Assert.DoesNotContain(sensitivePath, failure.DirectCause, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IOException", failure.DirectCause, StringComparison.Ordinal);
    }
}

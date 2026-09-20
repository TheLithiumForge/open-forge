using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal sealed partial class FileExpectationValidator(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal PhysicalPathResolution ResolvePath(
        CliWorkspace workspace,
        string path)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Resolve(workspace, path);
    }

    internal async ValueTask<FileExpectationValidationResult> ValidateAsync(
        CliWorkspace workspace,
        FileExpectation expectation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(expectation);
        if (cancellationToken.IsCancellationRequested)
        {
            return FileExpectationValidationResult.Cancelled(expectation);
        }

        if (!PhysicalContainment.Contains(workspace.LexicalRoot, expectation.LogicalPath))
        {
            return FileExpectationValidationResult.Blocked(
                expectation,
                "The expected logical path is outside the selected workspace.");
        }

        var leaf = NoFollowLeafObserver.Observe(
            _physicalPathResolver,
            workspace,
            expectation.LogicalPath,
            cancellationToken);
        var leafBoundary = FromLeafObservation(expectation, leaf);
        if (leafBoundary is not null)
        {
            return leafBoundary;
        }

        var resolution = Resolve(workspace, expectation.LogicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return TryResolveProspectivePhysicalPath(
                workspace,
                expectation.LogicalPath,
                out var prospectivePhysicalPath)
                ? Compare(
                    expectation,
                    FileStateSnapshot.Missing(expectation.LogicalPath),
                    prospectivePhysicalPath)
                : FileExpectationValidationResult.Blocked(
                    expectation,
                    "The missing expected target has no safe contained physical destination.");
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(expectation, resolution);
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & FileAttributes.Directory) != 0)
            {
                return Compare(
                    expectation,
                    FileStateSnapshot.Directory(expectation.LogicalPath, physicalPath),
                    physicalPath);
            }

            if ((attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return FileExpectationValidationResult.Blocked(
                    expectation,
                    "The expected path is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return FileExpectationValidationResult.Cancelled(expectation);
            }

            var confirmedLeaf = NoFollowLeafObserver.Observe(
                _physicalPathResolver,
                workspace,
                expectation.LogicalPath,
                cancellationToken);
            if (confirmedLeaf.State == NoFollowLeafState.Missing)
            {
                return CompareMissing(workspace, expectation);
            }

            if (confirmedLeaf.State != NoFollowLeafState.OrdinaryFile)
            {
                return FromLeafObservation(expectation, confirmedLeaf)
                    ?? FileExpectationValidationResult.Blocked(
                        expectation,
                        "The expected ordinary file changed object kind during validation.");
            }

            var confirmed = Resolve(workspace, expectation.LogicalPath);
            if (confirmed.State != PhysicalPathState.Contained)
            {
                return confirmed.State == PhysicalPathState.Missing
                    ? CompareMissing(workspace, expectation)
                    : FromResolution(expectation, confirmed);
            }

            var confirmedPhysicalPath = confirmed.GetContainedPhysicalPath();
            if (!string.Equals(physicalPath, confirmedPhysicalPath, PathComparison()))
            {
                return FileExpectationValidationResult.Blocked(
                    expectation,
                    "The expected path changed its resolved physical path during validation.");
            }

            return Compare(
                expectation,
                FileStateSnapshot.File(
                    expectation.LogicalPath,
                    confirmedPhysicalPath,
                    bytes),
                confirmedPhysicalPath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileExpectationValidationResult.Cancelled(expectation);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return CompareMissing(workspace, expectation);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Failed(expectation, FilesystemFailureKind.AccessDenied, exception);
        }
        catch (IOException exception)
        {
            return Failed(expectation, FilesystemFailureKind.InputOutput, exception);
        }
    }

    private PhysicalPathResolution Resolve(
        CliWorkspace workspace,
        string path)
        => _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            path);

    private static FileExpectationValidationResult Compare(
        FileExpectation expectation,
        FileStateSnapshot actual,
        string physicalPath)
        => expectation == actual.Expectation
            ? FileExpectationValidationResult.Matched(expectation, actual, physicalPath)
            : FileExpectationValidationResult.Mismatched(
                expectation,
                actual,
                physicalPath,
                "The current file state no longer matches the planned expectation.");

    private FileExpectationValidationResult CompareMissing(
        CliWorkspace workspace,
        FileExpectation expectation)
        => TryResolveProspectivePhysicalPath(
            workspace,
            expectation.LogicalPath,
            out var prospectivePhysicalPath)
            ? Compare(
                expectation,
                FileStateSnapshot.Missing(expectation.LogicalPath),
                prospectivePhysicalPath)
            : FileExpectationValidationResult.Blocked(
                expectation,
                "The missing expected target has no safe contained physical destination.");

    private static FileExpectationValidationResult FromResolution(
        FileExpectation expectation,
        PhysicalPathResolution resolution)
        => resolution.Failure is not null
            ? FileExpectationValidationResult.Failed(expectation, resolution.Failure)
            : FileExpectationValidationResult.Blocked(
                expectation,
                "The expected path physical boundary is unsafe or unavailable.");

    private static FileExpectationValidationResult? FromLeafObservation(
        FileExpectation expectation,
        NoFollowLeafObservation leaf)
    {
        return leaf.State switch
        {
            NoFollowLeafState.Missing
                or NoFollowLeafState.OrdinaryFile
                or NoFollowLeafState.Directory => null,
            NoFollowLeafState.Inaccessible
                or NoFollowLeafState.Unknown
                when leaf.Failure is { } failure =>
                FileExpectationValidationResult.Failed(expectation, failure),
            NoFollowLeafState.RelativeFileLink
                or NoFollowLeafState.Link
                or NoFollowLeafState.ReparsePoint
                or NoFollowLeafState.Special
                or NoFollowLeafState.Inaccessible
                or NoFollowLeafState.Unknown =>
                FileExpectationValidationResult.Blocked(
                    expectation,
                    "The expected path final leaf is not an ordinary file, directory, or proven missing leaf."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(leaf),
                leaf.State,
                "The no-follow leaf state is not defined."),
        };
    }

    private static FileExpectationValidationResult Failed(
        FileExpectation expectation,
        FilesystemFailureKind kind,
        Exception exception)
        => FileExpectationValidationResult.Failed(
            expectation,
            FilesystemFailure.FromException(kind, exception));

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

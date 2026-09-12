using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Ownership;

internal sealed partial class LifecycleOwnershipReader(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<LifecycleOwnershipReadResult> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }

        var logicalPath = Path.Combine(
            workspace.LexicalRoot,
            LifecycleSchema.DirectoryName,
            LifecycleSchema.FileName);
        var resolution = Resolve(workspace, logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleMissing,
                "The lifecycle document is missing.");
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleUnavailable,
                resolution.Failure?.DirectCause
                    ?? "The lifecycle document physical boundary is unsafe or unavailable.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & FileAttributes.Directory) != 0
                || (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.LifecycleInvalid,
                    "The lifecycle document is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return Interrupted();
            }

            var confirmed = Resolve(workspace, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    physicalPath,
                    confirmed.GetContainedPhysicalPath()))
            {
                return Blocked(
                    LifecycleOwnershipFindingCode.LifecycleUnavailable,
                    "The lifecycle document changed physical identity during inspection.");
            }

            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            return Decode(workspace, snapshot);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleMissing,
                "The lifecycle document is missing.");
        }
        catch (UnauthorizedAccessException exception)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleUnavailable,
                FilesystemFailure.FromException(
                    FilesystemFailureKind.AccessDenied,
                    exception).DirectCause);
        }
        catch (IOException exception)
        {
            return Blocked(
                LifecycleOwnershipFindingCode.LifecycleUnavailable,
                FilesystemFailure.FromException(
                    FilesystemFailureKind.InputOutput,
                    exception).DirectCause);
        }
    }

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string logicalPath)
        => _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);

    private static LifecycleOwnershipReadResult Interrupted()
    {
        const string cause = "Lifecycle ownership inspection was interrupted.";
        return new LifecycleOwnershipReadResult(
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Framework,
                LifecycleOwnershipReadState.Interrupted,
                cause),
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Extensions,
                LifecycleOwnershipReadState.Interrupted,
                cause),
            [],
            lifecycleFileExpectation: null,
            [new LifecycleOwnershipFinding(
                LifecycleOwnershipFindingCode.Interrupted,
                section: null,
                path: null,
                cause)]);
    }

    private static LifecycleOwnershipReadResult Blocked(
        LifecycleOwnershipFindingCode code,
        string cause,
        LifecycleOwnershipSection? section = null)
        => new(
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Framework,
                LifecycleOwnershipReadState.Blocked,
                cause),
            new LifecycleOwnershipSectionResult(
                LifecycleOwnershipSection.Extensions,
                LifecycleOwnershipReadState.Blocked,
                cause),
            [],
            lifecycleFileExpectation: null,
            [new LifecycleOwnershipFinding(code, section, path: null, cause)]);
}

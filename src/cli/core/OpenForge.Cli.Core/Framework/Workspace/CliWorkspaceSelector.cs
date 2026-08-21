using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Workspace;

internal sealed class CliWorkspaceSelector
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal CliWorkspaceSelector(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal CliWorkspaceSelectionResult Select(CliWorkspaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var selectedBy = request.IsExplicit
                ? CliWorkspaceSelectionMethod.ExplicitWorkspace
                : CliWorkspaceSelectionMethod.CurrentDirectory;
            var source = request.ExplicitPath ?? request.CurrentDirectory;
            var lexicalRoot = Path.GetFullPath(source, request.CurrentDirectory);
            var physical = _physicalPathResolver.ResolveRoot(lexicalRoot);
            if (physical.State != PhysicalPathState.Contained)
            {
                return FromPhysicalFailure(physical);
            }

            var physicalRoot = physical.ResolvedPhysicalPath!;
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(physicalRoot);
            }
            catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
            {
                return CliWorkspaceSelectionResult.Classified(CliWorkspaceSelectionState.Missing);
            }
            catch (UnauthorizedAccessException exception)
            {
                return CliWorkspaceSelectionResult.Failed(
                    CliWorkspaceSelectionState.Inaccessible,
                    FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
            }
            catch (IOException exception)
            {
                return CliWorkspaceSelectionResult.Failed(
                    CliWorkspaceSelectionState.InputOutputFailure,
                    FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                return CliWorkspaceSelectionResult.Failed(
                    CliWorkspaceSelectionState.Invalid,
                    new FilesystemFailure(
                        FilesystemFailureKind.InvalidPath,
                        "The selected workspace root is not a directory."));
            }

            return CliWorkspaceSelectionResult.Selected(
                new CliWorkspace(lexicalRoot, physicalRoot, selectedBy));
        }
        catch (Exception exception) when (exception is PlatformNotSupportedException or NotSupportedException)
        {
            return CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.Unsupported,
                FilesystemFailure.FromException(FilesystemFailureKind.Unsupported, exception));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.Invalid,
                FilesystemFailure.FromException(FilesystemFailureKind.InvalidPath, exception));
        }
    }

    private static CliWorkspaceSelectionResult FromPhysicalFailure(PhysicalPathResolution physical)
    {
        return physical.State switch
        {
            PhysicalPathState.Missing or PhysicalPathState.Dangling =>
                CliWorkspaceSelectionResult.Classified(CliWorkspaceSelectionState.Missing),
            PhysicalPathState.External or PhysicalPathState.Cycle =>
                CliWorkspaceSelectionResult.Classified(CliWorkspaceSelectionState.Unsafe),
            PhysicalPathState.Inaccessible => CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.Inaccessible,
                physical.Failure!),
            PhysicalPathState.Unsupported => CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.Unsupported,
                physical.Failure!),
            PhysicalPathState.Invalid => CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.Invalid,
                physical.Failure!),
            PhysicalPathState.InputOutputFailure => CliWorkspaceSelectionResult.Failed(
                CliWorkspaceSelectionState.InputOutputFailure,
                physical.Failure!),
            _ => throw new ArgumentOutOfRangeException(nameof(physical), physical.State, "The physical path state is not defined."),
        };
    }
}

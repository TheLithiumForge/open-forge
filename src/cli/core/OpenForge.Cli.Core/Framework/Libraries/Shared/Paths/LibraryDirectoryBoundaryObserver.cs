using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

internal static class LibraryDirectoryBoundaryObserver
{
    internal static NoFollowLeafObservation Observe(
        CliWorkspace workspace,
        string logicalDirectory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalDirectory);
        var normalized = Path.GetFullPath(logicalDirectory);
        if (!PhysicalContainment.Contains(workspace.LexicalRoot, normalized))
        {
            return Unknown(normalized, "The Library directory is outside the selected workspace.");
        }

        var relative = Path.GetRelativePath(workspace.LexicalRoot, normalized);
        var current = workspace.PhysicalRoot;
        if (relative == ".")
        {
            return NoFollowLeafObservation.Directory(normalized);
        }

        foreach (var segment in relative.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = Path.Combine(current, segment);
            var component = LinkTargetReader.Read(current);
            switch (component.State)
            {
                case PathComponentState.Missing:
                    return NoFollowLeafObservation.Missing(normalized);
                case PathComponentState.Link:
                    return NoFollowLeafObservation.CreateLink(
                        normalized,
                        NoFollowLinkIdentity.Create(
                            NoFollowLinkKind.SymbolicLink,
                            component.LinkTarget,
                            ReadTargetForm(component.LinkTarget)));
                case PathComponentState.Inaccessible:
                    return NoFollowLeafObservation.Classified(
                        normalized,
                        NoFollowLeafState.Inaccessible,
                        component.Failure
                            ?? new FilesystemFailure(
                                FilesystemFailureKind.AccessDenied,
                                "The Library directory is inaccessible."));
                case PathComponentState.Unsupported:
                    return NoFollowLeafObservation.Classified(
                        normalized,
                        NoFollowLeafState.ReparsePoint,
                        component.Failure);
                case PathComponentState.InputOutputFailure:
                    return NoFollowLeafObservation.Classified(
                        normalized,
                        NoFollowLeafState.Unknown,
                        component.Failure
                            ?? new FilesystemFailure(
                                FilesystemFailureKind.InputOutput,
                                "The Library directory could not be observed."));
                case PathComponentState.Ordinary:
                    if (component.Attributes is not { } attributes
                        || (attributes & FileAttributes.Directory) == 0)
                    {
                        return NoFollowLeafObservation.OrdinaryFile(normalized);
                    }

                    if ((attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        return NoFollowLeafObservation.Classified(
                            normalized,
                            NoFollowLeafState.ReparsePoint);
                    }

                    if ((attributes & FileAttributes.Device) != 0)
                    {
                        return NoFollowLeafObservation.Classified(
                            normalized,
                            NoFollowLeafState.Special);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(logicalDirectory),
                        component.State,
                        "The path component state is not defined.");
            }
        }

        return NoFollowLeafObservation.Directory(normalized);
    }

    internal static string PhysicalPath(CliWorkspace workspace, string logicalPath)
    {
        var relative = Path.GetRelativePath(workspace.LexicalRoot, logicalPath);
        return Path.GetFullPath(Path.Combine(workspace.PhysicalRoot, relative));
    }

    private static NoFollowLinkTargetForm ReadTargetForm(string? target)
        => target is null
            ? NoFollowLinkTargetForm.Unavailable
            : Path.IsPathFullyQualified(target) || target.StartsWith('/')
                ? NoFollowLinkTargetForm.Absolute
                : target.Contains('\\')
                    ? NoFollowLinkTargetForm.Unsupported
                    : NoFollowLinkTargetForm.Relative;

    private static NoFollowLeafObservation Unknown(string path, string cause)
        => NoFollowLeafObservation.Classified(
            path,
            NoFollowLeafState.Unknown,
            new FilesystemFailure(FilesystemFailureKind.InvalidPath, cause));
}

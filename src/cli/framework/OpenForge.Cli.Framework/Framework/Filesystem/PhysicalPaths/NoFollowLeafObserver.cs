using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal static class NoFollowLeafObserver
{
    internal static NoFollowLeafObservation Observe(
        PhysicalPathResolver physicalPathResolver,
        CliWorkspace workspace,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = Path.GetFullPath(logicalPath);

        if (!PhysicalContainment.Contains(workspace.LexicalRoot, normalized))
        {
            return NoFollowLeafObservation.Classified(
                normalized,
                NoFollowLeafState.Unknown,
                new FilesystemFailure(
                    FilesystemFailureKind.InvalidPath,
                    "The logical leaf is outside the selected workspace."));
        }

        var logicalParent = Path.GetDirectoryName(normalized);
        if (logicalParent is null)
        {
            return NoFollowLeafObservation.Classified(
                normalized,
                NoFollowLeafState.Unknown,
                new FilesystemFailure(
                    FilesystemFailureKind.InvalidPath,
                    "The logical leaf has no parent directory."));
        }

        var parent = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalParent);
        cancellationToken.ThrowIfCancellationRequested();
        if (parent.State == PhysicalPathState.Missing)
        {
            return NoFollowLeafObservation.Missing(normalized);
        }

        if (parent.State != PhysicalPathState.Contained)
        {
            return FromParentResolution(normalized, parent);
        }

        var physicalParent = parent.GetContainedPhysicalPath();
        var parentComponent = LinkTargetReader.Read(physicalParent);
        if (parentComponent.State != PathComponentState.Ordinary
            || parentComponent.Attributes is not { } parentAttributes
            || (parentAttributes & FileAttributes.Directory) == 0
            || (parentAttributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
        {
            return NoFollowLeafObservation.Classified(
                normalized,
                parentComponent.State == PathComponentState.Inaccessible
                    ? NoFollowLeafState.Inaccessible
                    : NoFollowLeafState.Unknown,
                parentComponent.Failure
                    ?? new FilesystemFailure(
                        FilesystemFailureKind.Unsupported,
                        "The logical leaf parent is not an ordinary directory."));
        }

        var physicalLeaf = Path.Combine(
            physicalParent,
            Path.GetFileName(normalized));
        var component = LinkTargetReader.Read(physicalLeaf);
        cancellationToken.ThrowIfCancellationRequested();
        return component.State switch
        {
            PathComponentState.Missing => NoFollowLeafObservation.Missing(normalized),
            PathComponentState.Link => ObserveLink(normalized, component),
            PathComponentState.Ordinary => ObserveOrdinary(normalized, component),
            PathComponentState.Inaccessible => NoFollowLeafObservation.Classified(
                normalized,
                NoFollowLeafState.Inaccessible,
                ReadFailure(component)),
            PathComponentState.Unsupported => NoFollowLeafObservation.Classified(
                normalized,
                NoFollowLeafState.ReparsePoint,
                component.Failure),
            PathComponentState.InputOutputFailure => NoFollowLeafObservation.Classified(
                normalized,
                NoFollowLeafState.Unknown,
                ReadFailure(component)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(logicalPath),
                component.State,
                "The path component state is not defined."),
        };
    }

    private static NoFollowLeafObservation ObserveLink(
        string logicalPath,
        PathComponent component)
    {
        var rawTarget = component.LinkTarget;
        if (string.IsNullOrWhiteSpace(rawTarget))
        {
            return NoFollowLeafObservation.CreateLink(
                logicalPath,
                NoFollowLinkIdentity.Create(
                    NoFollowLinkKind.Other,
                    rawTarget: null,
                    NoFollowLinkTargetForm.Unavailable));
        }

        var targetForm = ReadTargetForm(rawTarget);
        if (targetForm == NoFollowLinkTargetForm.Relative)
        {
            return NoFollowLeafObservation.CreateRelativeFileLink(
                logicalPath,
                RelativeFileLinkIdentity.Create(
                    NoFollowLinkKind.SymbolicLink,
                    rawTarget));
        }

        return NoFollowLeafObservation.CreateLink(
            logicalPath,
            NoFollowLinkIdentity.Create(
                NoFollowLinkKind.SymbolicLink,
                rawTarget,
                targetForm));
    }

    private static NoFollowLinkTargetForm ReadTargetForm(string target)
    {
        if (Path.IsPathFullyQualified(target) || target.StartsWith('/'))
        {
            return NoFollowLinkTargetForm.Absolute;
        }

        if (target.Contains('\\'))
        {
            return NoFollowLinkTargetForm.Unsupported;
        }

        return NoFollowLinkTargetForm.Relative;
    }

    private static NoFollowLeafObservation ObserveOrdinary(
        string logicalPath,
        PathComponent component)
    {
        var attributes = component.Attributes
            ?? throw new InvalidOperationException("An ordinary path component requires attributes.");
        if ((attributes & FileAttributes.Directory) != 0)
        {
            return NoFollowLeafObservation.Directory(logicalPath);
        }

        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            return NoFollowLeafObservation.Classified(
                logicalPath,
                NoFollowLeafState.ReparsePoint);
        }

        if ((attributes & FileAttributes.Device) != 0)
        {
            return NoFollowLeafObservation.Classified(
                logicalPath,
                NoFollowLeafState.Special);
        }

        return NoFollowLeafObservation.OrdinaryFile(logicalPath);
    }

    private static NoFollowLeafObservation FromParentResolution(
        string logicalPath,
        PhysicalPathResolution resolution)
    {
        if (resolution.Failure is { } failure)
        {
            return NoFollowLeafObservation.Classified(
                logicalPath,
                resolution.State == PhysicalPathState.Inaccessible
                    ? NoFollowLeafState.Inaccessible
                    : NoFollowLeafState.Unknown,
                failure);
        }

        return NoFollowLeafObservation.Classified(
            logicalPath,
            NoFollowLeafState.Unknown,
            new FilesystemFailure(
                FilesystemFailureKind.Unsupported,
                "The logical leaf parent cannot be safely resolved inside the selected workspace."));
    }

    private static FilesystemFailure ReadFailure(PathComponent component)
        => component.Failure
            ?? throw new InvalidOperationException("A failed path component requires a direct failure.");
}

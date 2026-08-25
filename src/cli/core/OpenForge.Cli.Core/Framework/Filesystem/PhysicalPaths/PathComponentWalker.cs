namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal sealed class PathComponentWalker
{
    private readonly PhysicalIdentityTracker _identityTracker;

    internal PathComponentWalker(PhysicalIdentityTracker identityTracker)
    {
        _identityTracker = identityTracker;
    }

    internal PhysicalPathResolution ResolveUnboundedAbsolute(string logicalPath, string absolutePath)
    {
        var root = Path.GetPathRoot(absolutePath);
        if (string.IsNullOrEmpty(root))
        {
            return PhysicalPathResolution.Failed(
                PhysicalPathState.Invalid,
                logicalPath,
                new FilesystemFailure(FilesystemFailureKind.InvalidPath, "The path has no filesystem root."));
        }

        var relative = Path.GetRelativePath(root, absolutePath);
        return Walk(logicalPath, root, Split(relative), null);
    }

    internal PhysicalPathResolution ResolveContainedRelative(
        string logicalPath,
        string containmentRoot,
        string relativePath)
    {
        return Walk(logicalPath, containmentRoot, Split(relativePath), containmentRoot);
    }

    private PhysicalPathResolution ResolveContainedAbsolute(
        string logicalPath,
        string containmentRoot,
        string absolutePath)
    {
        if (!PhysicalContainment.Contains(containmentRoot, absolutePath))
        {
            return PhysicalPathResolution.Classified(PhysicalPathState.External, logicalPath, absolutePath);
        }

        var relative = Path.GetRelativePath(containmentRoot, absolutePath);
        return Walk(logicalPath, containmentRoot, Split(relative), containmentRoot);
    }

    private PhysicalPathResolution Walk(
        string logicalPath,
        string startPath,
        IReadOnlyList<string> components,
        string? containmentRoot)
    {
        var current = Path.GetFullPath(startPath);
        for (var index = 0; index < components.Count; index++)
        {
            var componentPath = Path.GetFullPath(Path.Combine(current, components[index]));
            if (containmentRoot is not null && !PhysicalContainment.Contains(containmentRoot, componentPath))
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.External, logicalPath, componentPath);
            }

            var component = LinkTargetReader.Read(componentPath);
            var classified = ClassifyComponent(logicalPath, component);
            if (classified is not null)
            {
                return classified;
            }

            if (component.State == PathComponentState.Ordinary)
            {
                if (index < components.Count - 1
                    && (component.Attributes is not { } attributes
                        || (attributes & FileAttributes.Directory) == 0))
                {
                    return PhysicalPathResolution.Classified(PhysicalPathState.Missing, logicalPath);
                }

                current = componentPath;
                continue;
            }

            var linkTarget = component.LinkTarget
                ?? throw new InvalidOperationException("A link path component requires its immediate target.");
            var target = ResolveTarget(componentPath, linkTarget);
            if (containmentRoot is not null && !PhysicalContainment.Contains(containmentRoot, target))
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.External, logicalPath, target);
            }

            if (_identityTracker.Enter(componentPath, target) is not null)
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.Cycle, logicalPath, target);
            }

            PhysicalPathResolution targetResult;
            try
            {
                targetResult = containmentRoot is null
                    ? ResolveUnboundedAbsolute(logicalPath, target)
                    : ResolveContainedAbsolute(logicalPath, containmentRoot, target);
            }
            finally
            {
                _identityTracker.Exit(target);
            }

            if (targetResult.State == PhysicalPathState.Missing)
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.Dangling, logicalPath, target);
            }

            if (targetResult.State != PhysicalPathState.Contained)
            {
                return targetResult;
            }

            current = targetResult.GetContainedPhysicalPath();
        }

        return PhysicalPathResolution.Contained(logicalPath, current);
    }

    internal static PhysicalPathResolution? ClassifyComponent(string logicalPath, PathComponent component)
    {
        return component.State switch
        {
            PathComponentState.Ordinary or PathComponentState.Link => null,
            PathComponentState.Missing => PhysicalPathResolution.Classified(PhysicalPathState.Missing, logicalPath),
            PathComponentState.Inaccessible => PhysicalPathResolution.Failed(
                PhysicalPathState.Inaccessible,
                logicalPath,
                ReadFailure(component)),
            PathComponentState.Unsupported => PhysicalPathResolution.Failed(
                PhysicalPathState.Unsupported,
                logicalPath,
                ReadFailure(component)),
            PathComponentState.InputOutputFailure => PhysicalPathResolution.Failed(
                PhysicalPathState.InputOutputFailure,
                logicalPath,
                ReadFailure(component)),
            _ => throw new ArgumentOutOfRangeException(nameof(component), component.State, "The component state is not defined."),
        };
    }

    private static FilesystemFailure ReadFailure(PathComponent component)
    {
        return component.Failure
            ?? throw new InvalidOperationException("A failed path component requires its direct failure.");
    }

    private static string ResolveTarget(string linkPath, string linkTarget)
    {
        if (Path.IsPathRooted(linkTarget))
        {
            return Path.GetFullPath(linkTarget);
        }

        var directory = Path.GetDirectoryName(linkPath);
        return Path.GetFullPath(directory is null
            ? linkTarget
            : Path.Combine(directory, linkTarget));
    }

    private static string[] Split(string relativePath)
    {
        if (relativePath == ".")
        {
            return [];
        }

        return relativePath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
    }
}

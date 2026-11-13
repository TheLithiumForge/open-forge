using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal sealed class ToolStoreArea
{
    private readonly string _anchor;
    private readonly PhysicalPathResolver _paths = new();
    internal CliWorkspace Scope { get; }
    internal string Root => Scope.PhysicalRoot;

    internal ToolStoreArea(string root)
    {
        if (!Path.IsPathFullyQualified(root)) throw new ArgumentException("The tool store root must be absolute.", nameof(root));
        root = Path.GetFullPath(root);
        if (new DirectoryInfo(root).LinkTarget is not null) throw new IOException("A tool store root cannot be a link.");
        var cursor = root;
        while (!Directory.Exists(cursor)) cursor = Path.GetDirectoryName(cursor) ?? throw new IOException("The tool store has no available parent.");
        _anchor = _paths.ResolveRoot(cursor).GetContainedPhysicalPath();
        var relative = Path.GetRelativePath(cursor, root);
        var physical = relative == "." ? _anchor : Path.Combine(_anchor, relative);
        Scope = new(physical, physical, CliWorkspaceSelectionMethod.CurrentDirectory);
        ValidateRoot();
    }

    internal string At(string relative)
    {
        if (!ToolIdentity.IsRelativePath(relative)) throw new ArgumentException("The tool store member must be a portable relative path.", nameof(relative));
        ValidateRoot();
        return Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));
    }

    internal void EnsureRoot(CancellationToken token)
    {
        ValidateRoot();
        var relative = Path.GetRelativePath(_anchor, Root);
        var current = _anchor;
        if (relative == ".") return;
        var anchor = new CliWorkspace(_anchor, _anchor, CliWorkspaceSelectionMethod.CurrentDirectory);
        foreach (var segment in relative.Split(Path.DirectorySeparatorChar))
        {
            token.ThrowIfCancellationRequested();
            current = Path.Combine(current, segment);
            var state = NoFollowLeafObserver.Observe(_paths, anchor, current, token).State;
            if (state == NoFollowLeafState.Missing) Directory.CreateDirectory(current);
            else if (state != NoFollowLeafState.Directory) throw new IOException("A tool store parent isn't an ordinary directory.");
        }
        ValidateRoot();
    }

    internal void EnsureDirectory(string relative, CancellationToken token)
    {
        EnsureRoot(token);
        var current = Root;
        if (!ToolIdentity.IsRelativePath(relative)) throw new ArgumentException("The tool store directory must be a portable path.", nameof(relative));
        foreach (var segment in relative.Split('/'))
        {
            token.ThrowIfCancellationRequested();
            current = Path.Combine(current, segment);
            var state = ToolFileReader.Observe(Scope, current, token);
            if (state == NoFollowLeafState.Missing) Directory.CreateDirectory(current);
            else if (state != NoFollowLeafState.Directory) throw new IOException("A tool store directory path isn't ordinary.");
        }
    }

    internal void ValidateRoot()
    {
        var anchor = _paths.ResolveRoot(_anchor);
        if (anchor.State != PhysicalPathState.Contained || !Same(anchor.GetContainedPhysicalPath(), _anchor))
            throw new IOException("The tool store parent identity changed.");
        var root = _paths.ResolveCandidate(_anchor, _anchor, Root);
        if (root.State is not PhysicalPathState.Contained and not PhysicalPathState.Missing
            || root.State == PhysicalPathState.Contained && !Same(root.GetContainedPhysicalPath(), Root))
            throw new IOException("The tool store root identity couldn't be verified.");
        if (root.State == PhysicalPathState.Contained && !Directory.Exists(Root)) throw new IOException("A tool store root must be a directory.");
        if (new DirectoryInfo(Root).LinkTarget is not null) throw new IOException("A tool store root can't be a link.");
    }

    private static bool Same(string left, string right)
        => string.Equals(left, right, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}

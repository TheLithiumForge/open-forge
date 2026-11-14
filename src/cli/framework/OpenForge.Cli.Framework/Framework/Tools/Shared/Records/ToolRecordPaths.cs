using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal sealed class ToolRecordPaths
{
    internal const string Extension = ".json";
    internal const string PathFileName = "path.json";
    internal string WorkspacePath { get; }
    internal string HomePath { get; }
    internal string StatePath { get; }
    internal string WorkspaceKey => WorkspaceIdentity.Key(WorkspacePath);
    internal string WorkspaceRecordPath => Path.Combine(StatePath, ToolRecordDefinitions.WorkspacesDirectoryName, WorkspaceKey + Extension);
    internal string PathRecordPath => Path.Combine(StatePath, PathFileName);
    internal string ToolsDirectory => Path.Combine(StatePath, ToolRecordDefinitions.ToolsDirectoryName);
    internal string RuntimesDirectory => Path.Combine(StatePath, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName);

    internal ToolRecordPaths(string workspacePath, string homePath)
    {
        WorkspacePath = ResolveRoot(workspacePath);
        HomePath = ResolveDirectory(homePath);
        StatePath = Path.Combine(HomePath, ToolRecordDefinitions.MachineStateDirectoryName);
    }

    internal string Tool(string id)
    {
        ToolPortableCodec.ValidateId(id);
        return Path.Combine(ToolsDirectory, id + Extension);
    }

    internal string Runtime(string version)
    {
        if (string.IsNullOrWhiteSpace(version) || version.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '-' and not '+'))
            throw new ArgumentException("The runtime version cannot contain path characters.");
        return Path.Combine(RuntimesDirectory, version + Extension);
    }

    internal static void Check(string root, string target)
    {
        if (!PhysicalIdentityTracker.PathComparer.Equals(ResolveDirectory(root), root))
            throw new IOException($"The record root changed its physical binding: {root}.");
        var resolution = new PhysicalPathResolver().ResolveCandidate(root, root, target);
        if (resolution.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing))
            throw new IOException($"The record path is unavailable or escapes its root: {target}.");
        var current = root;
        var segments = Path.GetRelativePath(root, target).Split(Path.DirectorySeparatorChar);
        for (var index = 0; index < segments.Length; index++)
        {
            current = Path.Combine(current, segments[index]);
            var component = LinkTargetReader.Read(current);
            if (component.State == PathComponentState.Missing) return;
            if (component.State != PathComponentState.Ordinary)
                throw new IOException($"The record path requires ordinary files and directories: {current}.");
            if (index < segments.Length - 1 && (component.Attributes & FileAttributes.Directory) == 0)
                throw new IOException($"The record parent is a file: {current}.");
        }
    }

    internal static string ResolveDirectory(string path)
    {
        if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("The record root must be absolute.");
        var current = Path.GetFullPath(path);
        var missing = new Stack<string>();
        while (LinkTargetReader.Read(current).State == PathComponentState.Missing)
        {
            missing.Push(Path.GetFileName(current));
            current = Path.GetDirectoryName(current) ?? throw new IOException("The record root has no existing ancestor.");
        }
        var resolved = ResolveRoot(current);
        foreach (var segment in missing) resolved = Path.Combine(resolved, segment);
        return WorkspaceIdentity.NormalizePhysicalPath(resolved);
    }

    internal static string ResolveRoot(string path)
    {
        var root = new PhysicalPathResolver().ResolveRoot(path);
        if (root.State != PhysicalPathState.Contained || !Directory.Exists(root.ResolvedPhysicalPath))
            throw new IOException($"The record root is unavailable: {path}.");
        return WorkspaceIdentity.NormalizePhysicalPath(root.GetContainedPhysicalPath());
    }
}

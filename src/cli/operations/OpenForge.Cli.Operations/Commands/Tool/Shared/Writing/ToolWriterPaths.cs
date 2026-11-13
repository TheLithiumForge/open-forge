using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal static class ToolWriterPaths
{
    internal static string Relative(CliWorkspace workspace, string input, bool allowRoot = false)
    {
        try
        {
            var path = Path.GetFullPath(input, workspace.LexicalRoot);
            var relative = Path.GetRelativePath(workspace.LexicalRoot, path).Replace(Path.DirectorySeparatorChar, '/');
            if (allowRoot && relative == ".") return relative;
            if (PortableWorkspacePath.TryNormalize(relative, out var normalized)) return normalized;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException) { }
        throw ToolWriterReader.Failure(ToolWriteIssueCode.InvalidInput, CliSemanticStatus.Invalid, input, "A contained portable workspace path is required.");
    }

    internal static string CodeRoot(CliWorkspace workspace, string relative, bool requireExisting, CancellationToken token)
    {
        if (relative == ".") return workspace.PhysicalRoot;
        var path = Path.GetFullPath(relative, workspace.LexicalRoot);
        var state = ToolFileReader.Observe(workspace, path, token);
        if (state != NoFollowLeafState.Directory && (requireExisting || state != NoFollowLeafState.Missing))
            throw ToolWriterReader.Failure(ToolWriteIssueCode.VerificationFailed, CliSemanticStatus.Blocked, relative, "The code root isn't an ordinary contained directory.");
        var resolved = new PhysicalPathResolver().ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, path);
        if (resolved.State == PhysicalPathState.Contained) return resolved.GetContainedPhysicalPath();
        if (!requireExisting && resolved.State == PhysicalPathState.Missing)
            return Path.GetFullPath(relative, workspace.PhysicalRoot);
        throw ToolWriterReader.Failure(ToolWriteIssueCode.VerificationFailed, CliSemanticStatus.Blocked, relative, "The code-root identity is unavailable.");
    }

    internal static bool Contains(string parent, string child)
        => string.Equals(child, parent, Comparison) || child.StartsWith(parent + "/", Comparison);

    private static StringComparison Comparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}

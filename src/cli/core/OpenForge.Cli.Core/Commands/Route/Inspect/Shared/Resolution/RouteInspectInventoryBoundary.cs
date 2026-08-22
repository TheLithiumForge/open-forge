using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectInventoryBoundary
{
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteInspectInventoryBoundary(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal PhysicalPathResolution ResolveCandidate(
        CliWorkspace workspace,
        string logicalPath)
    {
        var lexicalPath = logicalPath == RouteLogicalPath.AgentsRoot
            ? Path.Combine(workspace.LexicalRoot, RouteLogicalPath.AgentsRoot)
            : RouteLogicalPath.ToLexicalPath(workspace.LexicalRoot, logicalPath);
        return _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
    }

    internal string Combine(string parent, string name)
    {
        return parent == RouteLogicalPath.AgentsRoot
            ? $"{RouteLogicalPath.AgentsRoot}/{name}"
            : RouteLogicalPath.Combine(parent, name);
    }

    internal bool IsDirectory(string physicalPath)
    {
        try
        {
            return (File.GetAttributes(physicalPath) & FileAttributes.Directory) != 0;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return false;
        }
    }

    internal bool TryReadAttributes(string physicalPath, out FileAttributes attributes)
    {
        try
        {
            attributes = File.GetAttributes(physicalPath);
            return true;
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException
            or UnauthorizedAccessException
            or IOException)
        {
            attributes = default;
            return false;
        }
    }

    internal RouteInspectResolutionIssue CreateBoundaryIssue(
        string logicalPath,
        PhysicalPathResolution resolution)
    {
        var code = resolution.State is PhysicalPathState.External
            or PhysicalPathState.Dangling
            or PhysicalPathState.Cycle
            ? RouteInspectResolutionIssueCode.UnsafeSource
            : RouteInspectResolutionIssueCode.ReadUnavailable;
        return new RouteInspectResolutionIssue(
            code,
            logicalPath,
            "The .agents source boundary could not be proved or read.");
    }
}

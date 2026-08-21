using System.Text;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal sealed class RouteListTestWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly string _workspacePath;

    private RouteListTestWorkspace(TemporaryWorkspace workspace, string workspacePath)
    {
        _workspace = workspace;
        _workspacePath = workspacePath;
    }

    internal string Path => _workspacePath;

    internal static RouteListTestWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("route-list-integration");
        workspace.CreateDirectory(RouteListDefinitions.AgentsDirectoryName);
        return new RouteListTestWorkspace(workspace, workspace.Path);
    }

    internal static RouteListTestWorkspace CreateNested()
    {
        var workspace = TemporaryWorkspace.Create("route-list-nested-integration");
        var workspacePath = workspace.CreateDirectory("real-workspace");
        workspace.CreateDirectory("real-workspace/.agents");
        return new RouteListTestWorkspace(workspace, workspacePath);
    }

    internal void Write(string relativePath, string contents)
    {
        _workspace.WriteText(OwnedRelativePath(relativePath), contents);
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        _workspace.WriteBytes(OwnedRelativePath(relativePath), contents);
    }

    internal void WriteLoader(params string[] entries)
    {
        var body = new StringBuilder()
            .AppendLine("# Open Forge Loader")
            .AppendLine()
            .AppendLine("## Entries")
            .AppendLine()
            .AppendLine(RouteListDefinitions.LoaderStartMarker);
        foreach (var entry in entries)
        {
            body.AppendLine(entry);
        }

        body.AppendLine(RouteListDefinitions.LoaderEndMarker);
        Write(RouteListDefinitions.LoaderPath, body.ToString());
    }

    internal void WriteRoute(
        string relativePath,
        string description,
        string tags,
        string? body = null)
    {
        Write(
            relativePath,
            $"---\nopen-forge:\n  description: {description}\n  tags: [{tags}]\n---\n\n# {description}\n\n{body ?? "Authored route body."}\n");
    }

    internal void WriteSkill(string relativePath, string name, string description)
    {
        Write(
            relativePath,
            $"---\nname: {name}\ndescription: {description}\n---\n\n# {name}\n");
    }

    internal RouteListRequest Request(
        string? sourceReference = null,
        RouteListDepth? depth = null,
        CliWorkspaceSelection selection = CliWorkspaceSelection.ExplicitWorkspace)
    {
        return new RouteListRequest(
            new CliWorkspace(Path, selection),
            sourceReference is null ? null : RouteListSourceReference.Parse(sourceReference),
            depth ?? RouteListDepth.Default);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _workspace.SnapshotHashes();
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }

    private string OwnedRelativePath(string workspaceRelativePath)
    {
        var relativeWorkspace = System.IO.Path.GetRelativePath(_workspace.Path, Path);
        if (relativeWorkspace == ".")
        {
            return workspaceRelativePath;
        }

        return System.IO.Path.Combine(relativeWorkspace, workspaceRelativePath);
    }
}

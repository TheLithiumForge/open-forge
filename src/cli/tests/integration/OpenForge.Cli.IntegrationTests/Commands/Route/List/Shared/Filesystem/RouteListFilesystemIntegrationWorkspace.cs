using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListFilesystemIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private RouteListFilesystemIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static RouteListFilesystemIntegrationWorkspace Create()
    {
        return new RouteListFilesystemIntegrationWorkspace(
            TemporaryWorkspace.Create("route-list-filesystem-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _temporary.WriteBytes(relativePath, contents);
    }

    internal string CreateDirectory(string relativePath)
    {
        return _temporary.CreateDirectory(relativePath);
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
    }

    internal bool TryCreateDirectorySymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateDirectorySymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal bool TryCreateFileSymbolicLink(
        string relativeLinkPath,
        string targetPath,
        out string? linkPath)
    {
        return _temporary.TryCreateFileSymbolicLink(relativeLinkPath, targetPath, out linkPath);
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _temporary.SnapshotHashes();
    }

    internal async ValueTask<RouteListInventoryFacts> ReadAsync(CancellationToken cancellationToken = default)
    {
        return await new RouteListInventoryReader()
            .ReadAsync(new RouteListInventoryRequest(Workspace, cancellationToken));
    }

    internal static string OpenForgeMetadata(string description, params string[] tags)
    {
        return $"""
            ---
            open-forge:
              description: {description}
              tags: [{string.Join(", ", tags)}]
            ---
            """;
    }

    internal static string SkillMetadata(string name, string description)
    {
        return $"""
            ---
            name: {name}
            description: {description}
            ---
            """;
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}

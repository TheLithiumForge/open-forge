using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Resolution;

internal sealed class RouteInspectResolutionIntegrationWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _temporary;

    private RouteInspectResolutionIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        _temporary.CreateDirectory(".agents");
    }

    internal string Path => _temporary.Path;

    internal CliWorkspace Workspace => new(
        Path,
        Path,
        CliWorkspaceSelectionMethod.ExplicitWorkspace);

    internal static RouteInspectResolutionIntegrationWorkspace Create()
    {
        return new RouteInspectResolutionIntegrationWorkspace(
            TemporaryWorkspace.Create("route-inspect-resolution-integration"));
    }

    internal void Write(string relativePath, string contents)
    {
        _temporary.WriteText(relativePath, contents);
    }

    internal void Write(string relativePath, byte[] contents)
    {
        _temporary.WriteBytes(relativePath, contents);
    }

    internal string Absolute(string relativePath)
    {
        return _temporary.Combine(relativePath);
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

    internal ValueTask<RouteInspectResolution> ResolveAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        return new RouteInspectResolver().ResolveAsync(
            new RouteInspectRequest(
                Workspace,
                reference,
                allowInteractiveSourceSelection: false),
            cancellationToken);
    }

    internal void WriteLoader(string entries)
    {
        Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    internal static string OpenForgeMetadata(string description, params string[] tags)
    {
        return OpenForgeDocumentSeed.MetadataFrontmatter(
            description: description,
            tags: tags);
    }

    internal static string SkillMetadata(string name, string description)
    {
        return OpenForgeDocumentSeed.SkillFrontmatter(
            name: name,
            description: description);
    }

    public void Dispose()
    {
        _temporary.Dispose();
    }
}

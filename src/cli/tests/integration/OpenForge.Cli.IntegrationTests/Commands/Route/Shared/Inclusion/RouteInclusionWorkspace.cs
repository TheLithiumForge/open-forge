using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

internal sealed class RouteInclusionWorkspace : IDisposable
{
    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("route-inclusion");
    internal WorkspaceLockTestStore Locks { get; } = WorkspaceLockTestStore.Create("route-inclusion-locks");
    internal CliWorkspace Workspace { get; }

    internal RouteInclusionWorkspace(string entries = "- none - No entries - #Empty")
    {
        Workspace = new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Locks.Track(Workspace);
        Files.WriteText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
        Files.WriteText(".agents/open-forge.json", "{\"schemaVersion\":2,\"other\":true}\n");
        Files.WriteText("guide.md", "---\r\nforeign: true\r\n---\r\n# Guide\r\n\r\nKeep these bytes.\r\n");
    }

    internal string Read(string path) => File.ReadAllText(Files.Combine(path));
    internal byte[] Bytes(string path) => File.ReadAllBytes(Files.Combine(path));
    internal void SetRows(string rows) => Files.ReplaceText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(rows));
    internal void AddRoot() => Files.WriteText(".agents/patterns/_patterns.md",
        "---\nopen-forge:\n  description: Patterns\n  tags: [Pattern]\n---\n\n# Patterns\n\n## Entries\n\n- none - No entries - #Empty\n");
    public void Dispose() { Locks.Dispose(); Files.Dispose(); }
}

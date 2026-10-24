using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

internal sealed class RouteInclusionWorkspace : IDisposable
{
    private const string Category = "patterns";
    private const string CataloguePath = SourceLogicalPath.AgentsRoot + "/" + Category + "/_" + Category + SourceFormClassifier.MarkdownExtension;

    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("route-inclusion");
    internal WorkspaceLockTestStore Locks { get; } = WorkspaceLockTestStore.Create("route-inclusion-locks");
    internal CliWorkspace Workspace { get; }

    internal RouteInclusionWorkspace(string entries = MarkdownEntriesSectionReader.EmptyEntry)
    {
        Workspace = new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Locks.Track(Workspace);
        Files.WriteText(SourceLogicalPath.LoaderPath, GeneratedLoaderDocumentBuilder.Build(entries));
        Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{\"schemaVersion\":2,\"other\":true}\n");
        Files.WriteText("guide.md", "---\r\nforeign: true\r\n---\r\n# Guide\r\n\r\nKeep these bytes.\r\n");
    }

    internal string Read(string path) => File.ReadAllText(Files.Combine(path));
    internal byte[] Bytes(string path) => File.ReadAllBytes(Files.Combine(path));
    internal void SetRows(string rows) => Files.ReplaceText(SourceLogicalPath.LoaderPath, GeneratedLoaderDocumentBuilder.Build(rows));
    internal void AddRoot() => Files.WriteText(CataloguePath,
        "---\nopen-forge:\n  description: Patterns\n  tags: [Pattern]\n---\n\n# Patterns\n\n## Entries\n\n- none - No entries - #Empty\n");
    public void Dispose() { Locks.Dispose(); Files.Dispose(); }
}

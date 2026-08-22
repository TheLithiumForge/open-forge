using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteInspectWorkspace : IDisposable
{
    internal const string HostileSourcePath = ".agents/root/space value-東京-😀.md";

    private readonly TemporaryWorkspace _workspace;

    private PublishedRouteInspectWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal static PublishedRouteInspectWorkspace CreateComplete()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-inspect-complete");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #LoadNow");
            WriteEntrypoint(
                workspace,
                ".agents/root/_root.md",
                "Root",
                ["Root", "LoadNow"],
                "- [Child](child.md) - #Route\n"
                + "- [Unicode](東京-😀.md) - #Route");
            workspace.WriteText(".agents/root/_root.overwrite.md", "Root customization\n");
            WriteRoute(workspace, ".agents/root/child.md", "Child", "Child");
            WriteRoute(workspace, ".agents/root/東京-😀.md", "Unicode", "Unicode");
            return new PublishedRouteInspectWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteInspectWorkspace CreateAttention()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-inspect-attention");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
            WriteEntrypoint(workspace, ".agents/root/_root.md", "Root", ["Root"], "");
            WriteRoute(workspace, ".agents/root/collision.md", "Collision", "Route");
            WriteEntrypoint(
                workspace,
                ".agents/root/collision/_collision.md",
                "Collision entrypoint",
                ["Route"],
                "");
            return new PublishedRouteInspectWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteInspectWorkspace CreateIncomplete()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-inspect-incomplete");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #LoadNow");
            WriteEntrypoint(
                workspace,
                ".agents/root/_root.md",
                "Root",
                ["Root", "LoadNow"],
                "- [Malformed](malformed.md) - #LoadNow");
            workspace.WriteText(
                ".agents/root/malformed.md",
                "---\nopen-forge: [\n---\n\n# Malformed\n");
            return new PublishedRouteInspectWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteInspectWorkspace CreateHostileValue()
    {
        var workspace = CreateComplete();
        try
        {
            workspace._workspace.WriteText(
                HostileSourcePath,
                "---\nopen-forge:\n  description: Safe hostile value\n  tags: [Route]\n---\n# Safe hostile value\n");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
    {
        return _workspace.SnapshotHashes();
    }

    public void Dispose()
    {
        _workspace.Dispose();
    }

    private static void WriteLoader(TemporaryWorkspace workspace, string entries)
    {
        workspace.WriteText(
            ".agents/loader.md",
            $"""
            # Open Forge Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            {entries}
            <!-- open-forge:generated-index:end -->
            """);
    }

    private static void WriteEntrypoint(
        TemporaryWorkspace workspace,
        string path,
        string description,
        IEnumerable<string> tags,
        string entries)
    {
        var body = entries.Length == 0
            ? "- none - No entries - #Empty"
            : entries;
        workspace.WriteText(
            path,
            $"""
            ---
            open-forge:
              description: {description}
              tags: [{string.Join(", ", tags)}]
            ---
            # {description}

            ## Entries

            <!-- open-forge:generated-index:start -->
            {body}
            <!-- open-forge:generated-index:end -->
            """);
    }

    private static void WriteRoute(
        TemporaryWorkspace workspace,
        string path,
        string description,
        string tag)
    {
        workspace.WriteText(
            path,
            $"""
            ---
            open-forge:
              description: {description}
              tags: [{tag}]
            ---
            # {description}
            """);
    }
}

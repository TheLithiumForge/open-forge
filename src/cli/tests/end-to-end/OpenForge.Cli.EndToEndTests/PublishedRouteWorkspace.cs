using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;

    private PublishedRouteWorkspace(TemporaryWorkspace workspace)
    {
        _workspace = workspace;
    }

    internal string Path => _workspace.Path;

    internal static PublishedRouteWorkspace CreateComplete()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-list-complete");
        try
        {
            WriteLoader(
                workspace,
                "- [Root](root/_root.md) - #Root\n"
                + "- [Workspace](workspace-defined/_workspace-defined.md) - #Workspace");
            WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
            WriteRoute(workspace, ".agents/root/adjusted.md", "Adjusted route", "Adjusted");
            workspace.WriteText(".agents/root/adjusted.overwrite.md", "Workspace adjustment\n");
            WriteRoute(workspace, ".agents/root/child.md", "Child route", "Child");
            WriteSkill(workspace, ".agents/root/native/SKILL.md", "native", "Native route");
            WriteRoute(workspace, ".agents/root/nested/_nested.md", "Nested route", "Nested");
            WriteRoute(workspace, ".agents/root/nested/deep.md", "Deep route", "Deep");
            WriteRoute(
                workspace,
                ".agents/workspace-defined/_workspace-defined.md",
                "Workspace route",
                "Workspace");
            WriteRoute(workspace, ".agents/detached/_detached.md", "Detached route", "Detached");
            WriteRoute(workspace, ".agents/detached/leaf.md", "Detached leaf", "DetachedLeaf");
            return new PublishedRouteWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteWorkspace CreateIncomplete()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-list-incomplete");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
            WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
            workspace.WriteText(
                ".agents/root/malformed.md",
                "---\nopen-forge: [\n---\n\n# Malformed\n");
            return new PublishedRouteWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteWorkspace CreateAttention()
    {
        var workspace = TemporaryWorkspace.Create("e2e-route-list-attention");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
            WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
            WriteRoute(workspace, ".agents/root/compat/index.md", "Compatibility route", "Compatibility");
            return new PublishedRouteWorkspace(workspace);
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal static PublishedRouteWorkspace CreateCancellation(int childCount)
    {
        if (childCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(childCount), childCount, "At least one child is required.");
        }

        var workspace = TemporaryWorkspace.Create("e2e-route-list-cancel");
        try
        {
            WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
            WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
            for (var index = 0; index < childCount; index++)
            {
                var identity = index.ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
                WriteRoute(
                    workspace,
                    $".agents/root/child-{identity}.md",
                    $"Child route {identity}",
                    "Child");
            }

            return new PublishedRouteWorkspace(workspace);
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
        workspace.WriteText(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
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
            """);
    }

    private static void WriteSkill(
        TemporaryWorkspace workspace,
        string path,
        string name,
        string description)
    {
        workspace.WriteText(
            path,
            $"""
            ---
            name: {name}
            description: {description}
            ---
            """);
    }
}

using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Inventory;

public sealed class SourceCatalogueWorkTests
{
    [Fact(DisplayName = "A catalogue walk inspects each ancestor once and a later walk discovers changed files"),
        Trait("Feature", "source-inventory"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CatalogueDirectoryWorkIsBoundedPerRead()
    {
        using var temporary = TemporaryWorkspace.Create("catalogue-directory-work");
        temporary.WriteText(".agents/nested/one.md", "# One\n");
        temporary.WriteText(".agents/nested/two.md", "# Two\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var calls = new Dictionary<string, int>(StringComparer.Ordinal);
        var reader = new SourceCatalogueReader(new PhysicalPathResolver(path =>
        {
            calls[path] = calls.GetValueOrDefault(path) + 1;
            return LinkTargetReader.Read(path);
        }));
        var first = await reader.ReadAsync(new(workspace, [".agents"]), TestContext.Current.CancellationToken);
        Assert.Equal(2, first.Sources.Count);
        Assert.Equal(1, calls[temporary.Combine(".agents")]);
        Assert.Equal(1, calls[temporary.Combine(".agents/nested")]);

        File.Delete(temporary.Combine(".agents/nested/one.md"));
        var second = await reader.ReadAsync(new(workspace, [".agents"]), TestContext.Current.CancellationToken);
        Assert.Single(second.Sources);
        Assert.Equal(2, calls[temporary.Combine(".agents")]);
        Assert.Equal(2, calls[temporary.Combine(".agents/nested")]);
    }
}

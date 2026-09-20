using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution;

public sealed class EmbeddedFrameworkSourceProjectorTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Embedded Framework source projection maps canonical assets to contained sources and pairs adjacent overwrites"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ProjectMapsCanonicalAssetsToContainedSourcesAndPairsOverwrites()
    {
        var workspace = new OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-init-projector-red")),
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-init-projector-red")),
            OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var payload = FrameworkPayload.Create(
        [
            Asset("AGENTS.md", "root agents\n"),
            Asset("CLAUDE.md", "root claude\n"),
            Asset(".agents/loader.md", "loader\n"),
            Asset(".agents/guidance/_guidance.md", "guidance\n"),
            Asset(".agents/guidance/_guidance.overwrite.md", "overwrite\n"),
            Asset(".agents/memory/notes.md", "notes\n"),
        ]);

        var sources = new EmbeddedFrameworkSourceProjector().Project(workspace, payload);

        Assert.False(sources.IsDefault);
        Assert.Equal(
            [
                ".agents/guidance/_guidance.md",
                ".agents/loader.md",
                ".agents/memory/notes.md",
            ],
            sources.Select(source => source.Identity.CanonicalBasePath));

        var guidance = Assert.Single(
            sources.Where(source =>
                source.Identity.CanonicalBasePath == ".agents/guidance/_guidance.md"));
        Assert.Equal("guidance", guidance.Identity.AutomaticId);
        Assert.Equal(SourceDocumentForm.CanonicalEntrypoint, guidance.Base.Form);
        Assert.Equal(
            ".agents/guidance/_guidance.overwrite.md",
            guidance.Overwrite?.CanonicalPath);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(
                workspace.PhysicalRoot,
                ".agents",
                "guidance",
                "_guidance.md")),
            guidance.Base.PhysicalPath);

        var loader = Assert.Single(
            sources.Where(source => source.Identity.CanonicalBasePath == ".agents/loader.md"));
        Assert.Equal(SourceDocumentForm.Loader, loader.Base.Form);
        Assert.Equal("loader", loader.Identity.AutomaticId);
        Assert.DoesNotContain(
            sources,
            source => source.Identity.CanonicalBasePath is "AGENTS.md" or "CLAUDE.md");
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Embedded Framework source projection returns an immutable canonical projection for shuffled payload assets"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void ProjectReturnsImmutableCanonicalProjectionForShuffledAssets()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-init-projector-shuffled-red"));
        var workspace = new OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace(
            root,
            root,
            OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var agents = Asset("AGENTS.md", "agents");
        var claude = Asset("CLAUDE.md", "claude");
        var loader = Asset(".agents/loader.md", "loader");
        var source = Asset(".agents/memory/_memory.md", "memory");

        var first = new EmbeddedFrameworkSourceProjector().Project(
            workspace,
            FrameworkPayload.Create([agents, claude, loader, source]));
        var second = new EmbeddedFrameworkSourceProjector().Project(
            workspace,
            FrameworkPayload.Create([source, loader, claude, agents]));

        Assert.Equal(
            first.Select(item => item.Identity.CanonicalBasePath),
            second.Select(item => item.Identity.CanonicalBasePath));
        Assert.Equal(
            first.Select(item => item.Base.PhysicalPath),
            second.Select(item => item.Base.PhysicalPath));
        Assert.Equal(
            [".agents/loader.md", ".agents/memory/_memory.md"],
            first.Select(item => item.Identity.CanonicalBasePath));
    }

    private static FrameworkPayloadAsset Asset(string path, string content)
        => FrameworkPayloadAsset.Create(path, Encoding.UTF8.GetBytes(content));
}

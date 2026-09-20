using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusInitialTopologyTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status Initial uses canonical rooted topology"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void CanonicalTopologyProjectsRootedSources()
    {
        var context = Read(
            Loader("- [Root](root/_root.md) - #Root"),
            Asset(".agents/root/_root.md", Source("Root")),
            Asset(".agents/root/leaf.md", Source("Leaf")));

        Assert.True(context.IsComplete);
        Assert.All(
            context.Sources.Where(source => source.Path != FrameworkPayloadAsset.LoaderPath),
            source => Assert.Equal(SourceRouteState.Routed, source.RouteState));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status Initial keeps detached canonical topology unrouted"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void DetachedTopologyIsNotPromotedToRouted()
    {
        var context = Read(
            Loader("- [Root](root/_root.md) - #Root"),
            Asset(".agents/root/_root.md", Source("Root")),
            Asset(".agents/detached/_detached.md", Source("Detached")),
            Asset(".agents/detached/leaf.md", Source("Detached leaf")));

        var detached = context.Sources.Single(source =>
            source.Path == ".agents/detached/_detached.md");
        var leaf = context.Sources.Single(source =>
            source.Path == ".agents/detached/leaf.md");
        Assert.Equal(SourceRouteState.Unrouted, detached.RouteState);
        Assert.Equal(SourceRouteState.Unrouted, leaf.RouteState);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status Initial preserves ambiguous canonical topology as incomplete"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void AmbiguousTopologyRemainsAmbiguousAndIncomplete()
    {
        var context = Read(
            Loader("- [Root](root/_root.md) - #Root"),
            Asset(".agents/root/_root.md", Source("Root")),
            Asset(".agents/root/ambiguous/_ambiguous.md", Source("Canonical")),
            Asset(".agents/root/ambiguous/index.md", Source("Compatibility")),
            Asset(".agents/root/ambiguous/leaf.md", Source("Leaf")));

        var leaf = context.Sources.Single(source =>
            source.Path == ".agents/root/ambiguous/leaf.md");
        Assert.Equal(SourceRouteState.Ambiguous, leaf.RouteState);
        Assert.False(context.IsComplete);
    }

    private static RouteContextSet Read(
        params FrameworkPayloadAsset[] sourceAssets)
    {
        var payload = FrameworkPayload.Create(
        [
            Asset(FrameworkPayloadAsset.RootAgentPath, "# Workspace\n"),
            Asset(FrameworkPayloadAsset.RootClaudePath, "# Claude\n"),
            .. sourceAssets,
        ]);
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "status-initial-topology-unit"));
        var workspace = new CliWorkspace(
            root,
            root,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);

        return new RouteInitialContextReader().Read(
                workspace,
                FrameworkPayloadReadResult.Available(payload))
            ?? throw new InvalidOperationException("The embedded topology fixture is invalid.");
    }

    private static FrameworkPayloadAsset Loader(string entries)
        => Asset(
            FrameworkPayloadAsset.LoaderPath,
            OpenForgeDocumentSeed.GeneratedEntries(entries));

    private static string Source(string description)
        => OpenForgeDocumentSeed.Metadata(description, [], $"# {description}\n");

    private static FrameworkPayloadAsset Asset(string path, string text)
        => FrameworkPayloadAsset.Create(path, Encoding.UTF8.GetBytes(text));
}

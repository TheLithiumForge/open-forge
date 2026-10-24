using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

[Trait("Boundary", "Host"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class WorkspaceRuntimeAbsenceTests
{
    private const string DryRunScenario = "dry-run";
    private const string RefusalScenario = "refusal";
    private const string NoOpScenario = "no-op";
    private const string ObserverScenario = "observer";
    private const string RootDirectory = "root";
    private const string CatalogueFile = "_root.md";
    private const string ChildFile = "child.md";

    [Theory(DisplayName = "Dry run refusal verified no-op and observer leave runtime state absent")]
    [InlineData(DryRunScenario, 0)]
    [InlineData(RefusalScenario, 4)]
    [InlineData(NoOpScenario, 0)]
    [InlineData(ObserverScenario, 0)]
    public async Task ReadOnlyOutcomesCreateNoRuntimeState(string scenario, int expectedExit)
    {
        using var workspace = TemporaryWorkspace.Create("runtime-absence");
        const string catalogue = SourceLogicalPath.AgentsRoot + "/" + RootDirectory + "/" + CatalogueFile;
        workspace.CreateFile(catalogue, OpenForgeDocumentSeed.Metadata("Root", ["Docs"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = scenario == NoOpScenario ? $"- [Child]({ChildFile}) - #Docs" : "- stale",
            Prefix = "# Root",
        })));
        workspace.CreateFile(SourceLogicalPath.AgentsRoot + "/" + RootDirectory + "/" + ChildFile, OpenForgeDocumentSeed.Metadata("Child", ["Docs"], "# Child\n"));
        if (scenario == ObserverScenario)
        {
            workspace.CreateFile(SourceLogicalPath.LoaderPath, GeneratedLoaderDocumentBuilder.Build($"- [Root]({RootDirectory}/{CatalogueFile}) - #LoadNow #Docs"));
        }
        string[] arguments = scenario switch
        {
            DryRunScenario => [IndexDefinitions.CommandIdentity, catalogue, IndexDefinitions.DryRun.Name],
            RefusalScenario => [IndexDefinitions.CommandIdentity, "missing-source"],
            NoOpScenario => [IndexDefinitions.CommandIdentity, catalogue],
            ObserverScenario => [RouteDefinitions.RouteGroup.Name, RouteListDefinitions.ListCommand.Name],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var before = workspace.SnapshotHashes(includeRuntime: true);

        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes(includeRuntime: true));
        Assert.False(Directory.Exists(workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
    }
}

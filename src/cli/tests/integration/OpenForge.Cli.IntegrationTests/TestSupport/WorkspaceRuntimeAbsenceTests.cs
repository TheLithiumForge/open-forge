using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.TestSupport;

[Trait("Boundary", "Host"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
public sealed class WorkspaceRuntimeAbsenceTests
{
    [Theory(DisplayName = "Dry run refusal verified no-op and observer leave runtime state absent")]
    [InlineData("dry-run", 0)]
    [InlineData("refusal", 4)]
    [InlineData("no-op", 0)]
    [InlineData("observer", 0)]
    public async Task ReadOnlyOutcomesCreateNoRuntimeState(string scenario, int expectedExit)
    {
        using var workspace = TemporaryWorkspace.Create("runtime-absence");
        const string catalogue = ".agents/root/_root.md";
        workspace.CreateFile(catalogue, OpenForgeDocumentSeed.Metadata("Root", ["Docs"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = scenario == "no-op" ? "- [Child](child.md) - #Docs" : "- stale",
            Prefix = "# Root",
        })));
        workspace.CreateFile(".agents/root/child.md", OpenForgeDocumentSeed.Metadata("Child", ["Docs"], "# Child\n"));
        if (scenario == "observer")
        {
            workspace.CreateFile(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build("- [Root](root/_root.md) - #LoadNow #Docs"));
        }
        string[] arguments = scenario switch
        {
            "dry-run" => ["index", catalogue, "--dry-run"],
            "refusal" => ["index", "missing-source"],
            "no-op" => ["index", catalogue],
            "observer" => ["route", "list"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var before = workspace.SnapshotHashes(includeRuntime: true);

        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes(includeRuntime: true));
        Assert.False(Directory.Exists(workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
    }
}

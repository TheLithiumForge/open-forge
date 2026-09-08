using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class LibraryStatusBoundaryIntegrationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true, "current"), InlineData(false, "missing")]
    public static async Task StatusAccountsOnlyForRegisteredLinks(bool linked, string expectedState)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Source(".agents/directives/unregistered.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        if (linked)
        {
            workspace.Link();
        }

        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["status", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("result");
        var library = result.GetProperty("library");
        var registered = Assert.Single(library.GetProperty("records").EnumerateArray());
        Assert.Equal("team-knowledge", registered.GetProperty("id").GetString());
        var link = Assert.Single(registered.GetProperty("registeredLinks").GetProperty("links").EnumerateArray());
        Assert.Equal("directives/review", link.GetProperty("sourceId").GetString());
        Assert.Equal(expectedState, link.GetProperty("state").GetString());
        Assert.DoesNotContain("unregistered.md", library.GetRawText(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordDoesNotAdoptMatchingUnregisteredLink()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Link();
        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["status", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var library = document.RootElement.GetProperty("result").GetProperty("library");
        Assert.Equal("missing", library.GetProperty("record").GetProperty("state").GetString());
        Assert.Empty(library.GetProperty("records").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("ordinary", "trusted")]
    [InlineData("malformed", "blocked")]
    [InlineData("retargeted", "trusted")]
    public static async Task ConsumerRetainsLibrarySemanticBoundaryWithoutEffects(
        string boundary,
        string libraryState)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        LibraryMutationApplicationData.Lifecycle(workspace);
        workspace.RoutedSource();

        workspace.Record(LibraryMutationWorkspace.Leaf);
        if (boundary == "ordinary")
        {
            workspace.Write(LibraryMutationWorkspace.Leaf, "# Consumer-owned changed occupant\n");
        }
        else
        {
            workspace.Link(rawTarget: boundary == "retargeted" ? "../../shared/team-knowledge/.agents/other.md" : null);
        }

        if (boundary == "malformed")
        {
            workspace.Replace(LibraryMutationWorkspace.RecordPath, "{ malformed");
        }

        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["status", "--json"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected Status result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        Assert.Equal(libraryState, document.RootElement.GetProperty("result").GetProperty("library").GetProperty("state").GetString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task OtherwiseCompleteSafeLibraryDriftSelectsAttentionWithTrustedLibraryFacts()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-library-drift");
        workspace.WriteText("shared/team-knowledge/.agents/resources/data.json", "{\"value\":1}\n");
        workspace.WritePostInstallAgentText(".agents/open-forge.libraries.json", """
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","paths":[".agents/resources/data.json"]}]}
            """);
        var before = workspace.SnapshotHashes();

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--json");

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        Assert.Equal("attention", document.RootElement.GetProperty("status").GetString());
        var library = document.RootElement.GetProperty("result").GetProperty("library");
        Assert.Equal("trusted", library.GetProperty("state").GetString());
        Assert.Equal("missing", Assert.Single(Assert.Single(library.GetProperty("records").EnumerateArray())
            .GetProperty("registeredLinks").GetProperty("links").EnumerateArray()).GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}

using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class LibraryStatusBoundaryIntegrationTests
{
    [Trait("Boundary", "Host")]
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
        var run = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("data");
        var library = result.GetProperty("libraries");
        var registered = Assert.Single(library.EnumerateArray());
        Assert.Equal("team-knowledge", registered.GetProperty("id").GetString());
        var link = Assert.Single(registered.GetProperty("links").EnumerateArray());
        Assert.Equal(".agents/directives/review.md", link.GetProperty("path").GetString());
        Assert.Equal(expectedState, link.GetProperty("state").GetString());
        Assert.DoesNotContain("unregistered.md", library.GetRawText(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRecordDoesNotAdoptMatchingUnregisteredLink()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Link();
        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected consumer result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("data");
        Assert.Empty(result.GetProperty("libraries").EnumerateArray());
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.library-ownership-observation");
        Assert.Equal(before, workspace.Snapshot());
    }
    [Trait("Boundary", "Host")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("ordinary", "trusted")]
    [InlineData("malformed", "incomplete")]
    [InlineData("retargeted", "trusted")]
    public static async Task ConsumerRetainsLibrarySemanticBoundaryWithoutEffects(
        string boundary,
        string libraryState)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        LibraryMutationApplicationData.FrameworkFile(workspace);
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
            File.WriteAllText(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath), "{ malformed");
        }

        var before = workspace.Snapshot();
        var run = await CliHostCapture.RunAsync(["status", "--format", "json", "--detail", "full"], workspace.Path);
        Assert.True(run.Output.TrimStart().StartsWith('{'),
            $"Expected Status result JSON: exit {run.ExitCode}; stderr {run.Error}; stdout {run.Output}");
        using var document = JsonDocument.Parse(run.Output);
        var result = document.RootElement.GetProperty("data");
        if (libraryState == "trusted")
        {
            Assert.NotEmpty(result.GetProperty("libraries").EnumerateArray());
        }
        else
        {
            Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() == "status.library-record-malformed");
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "Host")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task OtherwiseCompleteSafeLibraryDriftSelectsAttentionWithTrustedLibraryFacts()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-library-drift");
        workspace.WriteText("shared/team-knowledge/.agents/resources/data.json", "{\"value\":1}\n");
        OpenForge.Cli.IntegrationTests.Framework.Ownership.OwnershipFixture.Libraries(workspace.Path,
            new("team-knowledge", "shared/team-knowledge", ".", [".agents/resources/data.json"]));
        var before = workspace.SnapshotHashes();

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format", "json",
            "--detail", "full");

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        Assert.Equal("completed-with-warnings", document.RootElement.GetProperty("status").GetString());
        var library = document.RootElement.GetProperty("data").GetProperty("libraries");
        Assert.Equal("missing", Assert.Single(Assert.Single(library.EnumerateArray())
            .GetProperty("links").EnumerateArray()).GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}

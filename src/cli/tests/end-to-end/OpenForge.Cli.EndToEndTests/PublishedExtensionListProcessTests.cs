using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionListProcessTests
{
    [Fact(DisplayName = "Published Extension group and List help expose exact grammar sections examples and streams"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpExposesCompleteContract()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var group = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension"]);
        var leaf = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--help"]);

        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Contains("list", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("open-forge extension list", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--installed", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--available", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Extension List reports trusted Installed and embedded Available facts without writes"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedDefaultReportsBothSections()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create(trustedInstalled: true);
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--format=json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("extension list", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var commandResult = document.RootElement.GetProperty("data");
        Assert.Equal("development-toolkit", Assert.Single(commandResult.GetProperty("installed").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal(ExtensionCatalogueSource.PackageIds,
            commandResult.GetProperty("available").EnumerateArray()
                .Select(package => package.GetProperty("id").GetString()));
    }



    [Fact(DisplayName = "Published Extension List reads one exact explicit package and preserves source bytes"), Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedExplicitPackageIsExactAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionListWorkspace.Create();
        var beforeSource = working.SnapshotSource();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["extension", "list", "--available", "--source", working.SourcePath, "--format=json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var commandResult = document.RootElement.GetProperty("data");
        Assert.Equal("package", commandResult.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal("local-toolkit", Assert.Single(commandResult.GetProperty("available").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal(beforeSource, working.SnapshotSource());
    }




}

using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionListProcessTests
{
    private const string CustomRoot = "docs/forge";

    [Theory(DisplayName = "Published Extension List observes installed custom-root content without writes")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "extension-list"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootInstalledList(string form)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create(CustomRoot, form);
        var framework = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath,
            [PublishedTokens.Install.Command, PublishedTokens.Install.Automatic], working.EnvironmentVariables);
        Assert.True(framework.ExitCode == 0, framework.StandardOutput + framework.StandardError);
        var installed = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath,
            [PublishedTokens.Extension.Command, PublishedTokens.ExtensionInstall.Command, PublishedExtensionInstallWorkspace.ToolkitId,
                PublishedTokens.ExtensionInstall.Source, working.CataloguePath, PublishedTokens.ExtensionInstall.Automatic], working.EnvironmentVariables);
        Assert.True(installed.ExitCode == 0, installed.StandardOutput + installed.StandardError);
        var before = Directory.GetFiles(working.WorkspacePath, "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
        var sourceBytes = working.SourcePayloadBytes();
        var listed = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath,
            [PublishedTokens.Extension.Command, PublishedTokens.ExtensionList.Command, PublishedTokens.ExtensionList.Installed,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], working.EnvironmentVariables);
        Assert.True(listed.ExitCode == 0, listed.StandardOutput + listed.StandardError);
        using var document = JsonDocument.Parse(listed.StandardOutput);
        var row = Assert.Single(document.RootElement.GetProperty(PublishedTokens.Json.Data)
            .GetProperty(PublishedTokens.ExtensionList.InstalledData).EnumerateArray());
        Assert.Equal(PublishedExtensionInstallWorkspace.ToolkitId, row.GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(working.WorkspacePath, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
        Assert.Equal(sourceBytes, working.SourcePayloadBytes());
    }

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
        Assert.Contains("Automation", leaf.StandardOutput, StringComparison.Ordinal);
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

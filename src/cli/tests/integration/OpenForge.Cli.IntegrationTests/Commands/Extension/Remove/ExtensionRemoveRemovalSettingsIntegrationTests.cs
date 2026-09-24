using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveRemovalSettingsIntegrationTests
{
    private const string RootPackagePath = ".agents/toolkit/_toolkit.md";
    private const string LoaderPath = ".agents/loader.md";
    private const string HandoffPath = ".agents/templates/observations-and-handoffs/handoff.md";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Remove deletes selected files and preserves excluded generated navigation bytes")]
    [InlineData("removedFiles", ".agents/loader.md", ".agents/loader.md", "toolkit/_toolkit.md", false)]
    [InlineData("removedDirectories", ".agents/templates", ".agents/templates/_templates.md", "observations-and-handoffs/_observations-and-handoffs.md", true)]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ExcludedGeneratedHostIsPreservedWhileRemovalCompletes(
        string exclusionKey,
        string exclusionPath,
        string excludedHostPath,
        string expectedEntry,
        bool useCorePackage)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-excluded-host-{exclusionKey}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-excluded-host-{exclusionKey}-source");
        string extensionId;
        string selectedPackagePath;
        if (useCorePackage)
        {
            extensionId = "observations-and-handoffs";
            selectedPackagePath = HandoffPath;
            var install = await workspace.RunAsync(
            [
                "extension", "install", extensionId,
                "--automatic", "--format", "json",
            ]);
            Assert.True(install.ExitCode == 0, install.StandardOutput);
            Assert.Equal(CliSemanticStatus.Complete, install.Status);
        }
        else
        {
            extensionId = "toolkit";
            selectedPackagePath = RootPackagePath;
            source.AddPackage(extensionId, [], (selectedPackagePath, Document("Toolkit")));
            await InstallAsync(workspace, source);
        }

        var loaderBefore = await File.ReadAllBytesAsync(workspace.Combine(excludedHostPath), TestContext.Current.CancellationToken);
        var hostText = System.Text.Encoding.UTF8.GetString(loaderBefore);
        Assert.Contains(expectedEntry, hostText, StringComparison.Ordinal);
        await WriteExclusionAsync(workspace, exclusionKey, exclusionPath);
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", extensionId,
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.True(run.ExitCode == 0, run.StandardOutput);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-remove.path-excluded", finding.GetProperty("code").GetString());
        Assert.Equal("info", finding.GetProperty("severity").GetString());
        Assert.Equal("informational", finding.GetProperty("resolution").GetString());
        Assert.Equal(excludedHostPath, finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.Contains("left unchanged", finding.GetProperty("message").GetString(), StringComparison.Ordinal);
        var action = finding.GetProperty("actions")[0].GetProperty("command").GetString();
        Assert.Contains("removedCategories", action, StringComparison.Ordinal);
        Assert.Contains("removedFiles", action, StringComparison.Ordinal);
        Assert.Contains("removedDirectories", action, StringComparison.Ordinal);
        Assert.Contains("open-forge index", action, StringComparison.Ordinal);
        Assert.Equal(loaderBefore, await File.ReadAllBytesAsync(workspace.Combine(excludedHostPath), TestContext.Current.CancellationToken));
        Assert.False(File.Exists(workspace.Combine(selectedPackagePath)));
        var effects = document.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == selectedPackagePath
            && effect.GetProperty("action").GetString() == "delete"
            && effect.GetProperty("outcome").GetString() == "verified");
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == excludedHostPath);
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Contains(extensionId, settings.RootElement.GetProperty("removedExtensions").EnumerateArray()
            .Select(value => value.GetString()));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove does not recreate an excluded missing generated host")]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ExcludedMissingGeneratedHostStaysMissing()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-excluded-missing-host");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-excluded-missing-host-source");
        source.AddPackage("toolkit", [], (RootPackagePath, Document("Toolkit")));
        await InstallAsync(workspace, source);
        await WriteExclusionAsync(workspace, "removedFiles", LoaderPath);
        File.Delete(workspace.Combine(LoaderPath));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.False(File.Exists(workspace.Combine(LoaderPath)));
        Assert.False(File.Exists(workspace.Combine(RootPackagePath)));
        using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
        Assert.Contains("toolkit", settings.RootElement.GetProperty("removedExtensions").EnumerateArray()
            .Select(value => value.GetString()));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    private static async Task WriteExclusionAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string key,
        string path)
        => await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            $$"""{"schemaVersion":1,"{{key}}":["{{path}}"]}""",
            TestContext.Current.CancellationToken);

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}

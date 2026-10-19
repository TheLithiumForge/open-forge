using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedDoctorInstallationProcessTests
{
    [Fact(DisplayName = "Install ignores a new archived note's broken outward link and reaches clean Doctor"), Trait("Feature", "doctor-link-scope"), Trait("Evidence", "EndToEnd")]
    public async Task ArchivedOutwardLinkJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        await InstallAsync(target, workspace, ["install", "--automatic", "--format=json"]);
        var path = Path.Combine(workspace.WorkspacePath, ".agents", "memory", "archived", "note.md");
        var note = OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata("Archived note", ["Archived"], "# Note\n\n[Missing](missing.md)\n");
        await File.WriteAllTextAsync(path, note, TestContext.Current.CancellationToken);
        await InstallAsync(target, workspace, ["index", "--format=json"]);
        var run = await DiagnoseAsync(target, workspace);
        Assert.True(run.ExitCode == 0, run.StandardOutput);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("severity").GetString() is "warning" or "error");
        Assert.Equal(note, await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
        var text = await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, ["doctor"], workspace.EnvironmentVariables);
        Assert.Contains("links ignored", text.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Doctor completely checks a fresh Framework installation without false errors or writes"), Trait("Feature", "doctor-installation"), Trait("Evidence", "EndToEnd")]
    public async Task FreshFrameworkHasCompleteCoverage()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        await InstallAsync(target, workspace, ["install", "--automatic", "--format=json"]);

        var run = await DiagnoseAsync(target, workspace);

        Assert.Equal(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var categories = document.RootElement.GetProperty("data").GetProperty("categories");
        Assert.Equal(6, categories.GetArrayLength());
        foreach (var domain in categories.EnumerateArray())
        {
            AssertCompleteDomain(domain);
        }
    }

    [Theory(DisplayName = "Doctor reads installed embedded packages from their recorded catalogue identity"),
        InlineData("development"), InlineData("development-toolkit"),
        Trait("Feature", "doctor-installation"), Trait("Evidence", "EndToEnd")]
    public async Task InstalledEmbeddedPackagesHaveCompleteExtensionCoverage(string id)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        await InstallAsync(target, workspace, ["install", "--automatic", "--format=json"]);
        await InstallAsync(target, workspace, ["extension", "install", id, "--automatic", "--format=json"]);

        var run = await DiagnoseAsync(target, workspace);

        using var document = JsonDocument.Parse(run.StandardOutput);
        var extension = Assert.Single(
            document.RootElement.GetProperty("data").GetProperty("categories").EnumerateArray(),
            category => category.GetProperty("name").GetString() == "Extensions");
        AssertCompleteDomain(extension);
    }

    private static async Task InstallAsync(
        PublishedExecutableTarget target,
        PublishedExtensionInstallWorkspace workspace,
        IReadOnlyList<string> arguments)
    {
        var run = await PublishedProcessTestSupport.RunAsync(
            target, workspace.WorkspacePath, arguments, workspace.EnvironmentVariables);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(0, run.ExitCode);
    }

    private static async Task<ProcessRunResult> DiagnoseAsync(
        PublishedExecutableTarget target,
        PublishedExtensionInstallWorkspace workspace)
    {
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, workspace.WorkspacePath, workspace.SnapshotWorkspace, ["doctor", "--format=json", "--detail=full"], workspace.EnvironmentVariables);
        Assert.Equal(string.Empty, run.StandardError);
        workspace.AssertPersistentLock();
        return run;
    }

    private static void AssertCompleteDomain(JsonElement domain)
    {
        Assert.Equal("complete", domain.GetProperty("coverage").GetString());
        Assert.Empty(domain.GetProperty("limitations").EnumerateArray());
    }
}

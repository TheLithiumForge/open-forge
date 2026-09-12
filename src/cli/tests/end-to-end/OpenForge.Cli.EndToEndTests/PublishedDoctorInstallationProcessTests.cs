using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedDoctorInstallationProcessTests
{
    [Fact(DisplayName = "Doctor completely checks a fresh Framework installation without false errors or writes"), Trait("Feature", "doctor-installation"), Trait("Evidence", "EndToEnd")]
    public async Task FreshFrameworkHasCompleteCoverage()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        await InstallAsync(target, workspace, ["install", "--automatic", "--json"]);

        var run = await DiagnoseAsync(target, workspace);

        Assert.Equal(0, run.ExitCode);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var domains = document.RootElement.GetProperty("result").GetProperty("domains");
        Assert.Equal(6, domains.GetArrayLength());
        foreach (var domain in domains.EnumerateArray())
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
        await InstallAsync(target, workspace, ["install", "--automatic", "--json"]);
        await InstallAsync(target, workspace, ["extension", "install", id, "--automatic", "--json"]);

        var run = await DiagnoseAsync(target, workspace);

        using var document = JsonDocument.Parse(run.StandardOutput);
        var extension = Assert.Single(
            document.RootElement.GetProperty("result").GetProperty("domains").EnumerateArray(),
            domain => domain.GetProperty("domain").GetString() == "extension-lifecycle");
        Assert.Equal("trusted", extension.GetProperty("lifecycle").GetString());
        Assert.Equal("available", extension.GetProperty("sourceAvailability").GetString());
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
            target, workspace.WorkspacePath, workspace.SnapshotWorkspace, ["doctor", "--json"], workspace.EnvironmentVariables);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.True(document.RootElement.GetProperty("result").GetProperty("readOnly").GetBoolean());
        Assert.False(document.RootElement.GetProperty("result").GetProperty("changesMade").GetBoolean());
        workspace.AssertPersistentLock();
        return run;
    }

    private static void AssertCompleteDomain(JsonElement domain)
    {
        Assert.Equal("complete", domain.GetProperty("coverage").GetString());
        Assert.Empty(domain.GetProperty("limitations").EnumerateArray());
        Assert.DoesNotContain(
            domain.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("severity").GetString() is "warning" or "error");
    }
}

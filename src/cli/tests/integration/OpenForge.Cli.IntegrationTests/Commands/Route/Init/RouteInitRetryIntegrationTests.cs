using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

public sealed class RouteInitRetryIntegrationTests
{
    [Fact(DisplayName = "Route Init write failure retains the target metadata workspace and presentation in its debug retry")]
    [Trait("Feature", "route-init"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task FailureRetryPreservesRequest()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-retry workspace's");
        var initial = await CliHostCapture.RunAsync(["route", "init", "documents", "--description", "Documents", "--tag", "Docs", "--workspace", workspace.Path], workspace.Path);
        Assert.Equal(0, initial.ExitCode);
        var parent = workspace.Absolute(".agents/documents/_documents.md");
        var before = File.ReadAllBytes(parent);
        using var held = File.Open(parent, FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await CliHostCapture.RunAsync(["route", "init", "documents/design", "--workspace", workspace.Path,
            "--description", "Team's design", "--responsibility", "", "--tag", "Design", "--apply-to", "src/**",
            "--format", "json", "--detail", "standard", "--detail-filter", "warning"], workspace.Path);
        Assert.Equal(1, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Equal($"open-forge route init documents/design --description {CliCommandText.Quote("Team's design")} --responsibility '' --tag Design --apply-to 'src/**' --format json --detail-filter warning --detail debug --workspace {CliCommandText.Quote(workspace.Path)}",
            json.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(before, File.ReadAllBytes(parent));
        Assert.True(Directory.Exists(workspace.Absolute(".agents/documents/design")));
        Assert.Equal("retained", json.RootElement.GetProperty("recovery").GetProperty("disposition").GetString());
    }
}

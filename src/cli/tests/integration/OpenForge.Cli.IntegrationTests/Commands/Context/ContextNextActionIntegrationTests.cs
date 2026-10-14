using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextNextActionIntegrationTests
{
    [Theory(DisplayName = "Context missing-loader and invalid-source next actions agree in text and JSON for the selected workspace")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "context"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task TextAndJsonPreserveWorkspace(bool missingLoader)
    {
        using var caller = TemporaryWorkspace.Create("context-next caller");
        using var selected = TemporaryWorkspace.Create("context-next workspace's");
        if (!missingLoader) selected.WriteText(".agents/loader.md", "# Loader\n\n## Entries\n");
        var before = selected.SnapshotHashes();
        string[] sources = missingLoader ? [] : ["unknown"];
        string[] arguments = ["context", .. sources, "--workspace", selected.Path];
        var text = await CliHostCapture.RunAsync(arguments, caller.Path);
        var json = await CliHostCapture.RunAsync([.. arguments, "--format", "json"], caller.Path);
        Assert.Equal(missingLoader ? 3 : 4, text.ExitCode);
        Assert.Equal(text.ExitCode, json.ExitCode);
        using var document = JsonDocument.Parse(json.Output);
        var command = document.RootElement.GetProperty("next").GetProperty("command").GetString();
        Assert.Equal(missingLoader
            ? $"open-forge install --workspace {CliCommandText.Quote(selected.Path)} --dry-run"
            : $"open-forge route list --depth all --workspace {CliCommandText.Quote(selected.Path)}", command);
        Assert.Contains($"Next: {command}", missingLoader ? text.Output : text.Error, StringComparison.Ordinal);
        Assert.Equal(before, selected.SnapshotHashes());
        if (missingLoader)
        {
            Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "context.layer-unavailable");
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("counts").GetProperty("sources").ValueKind);
        }
    }
}

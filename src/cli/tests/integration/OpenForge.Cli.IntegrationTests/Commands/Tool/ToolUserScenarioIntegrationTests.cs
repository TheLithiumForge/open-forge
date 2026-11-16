using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

// User scenarios through the composed Root with owned homes and synthetic hosts. No real download or PATH edit happens.
[Trait("Feature", "tool-scenarios"), Trait("Evidence", "Integration")]
public sealed class ToolUserScenarioIntegrationTests
{
    private const string OutOfRangeVersion = "9.9.9";

    [Fact(DisplayName = "A self-update outside support warns in Status and Update keeps that installation")]
    public async Task OutOfRangeInstallationIsKept()
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), (await fixture.RunAsync(Write(ToolWriteKind.Install, ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption))).ExitCode);
        fixture.DetectedVersion = OutOfRangeVersion;
        var installed = fixture.Snapshot();
        var events = fixture.Events.Count;

        var status = await fixture.RunAsync(Status());
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Attention), status.ExitCode);
        using (var json = status.Json())
        {
            var tool = Assert.Single(Tools(json));
            Assert.Equal(ToolReadProjection.Name(ToolObservationState.Unsupported), ToolCompositionFixture.Field(tool, nameof(ToolReadTool.State)).GetString());
            Assert.Equal(OutOfRangeVersion, ToolCompositionFixture.Field(tool, nameof(ToolReadTool.DetectedVersion)).GetString());
        }

        var update = await fixture.RunAsync(Write(ToolWriteKind.Update, ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption));

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Blocked), update.ExitCode);
        Assert.Equal(installed, fixture.Snapshot());
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events.Skip(events));
        Assert.DoesNotContain(ToolHostEvent.Uninstall, fixture.Events.Skip(events));
        Assert.Equal(0, fixture.DownloadCalls);
    }

    [Fact(DisplayName = "Update within support is a verified no-op that keeps the locked version and downloads nothing")]
    public async Task UpdateWithinSupportIsNoOp()
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), (await fixture.RunAsync(Write(ToolWriteKind.Install, ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption))).ExitCode);
        var installed = fixture.Snapshot();

        var preview = await fixture.RunAsync(Write(ToolWriteKind.Update, null, ToolWriteBinding.DryRunOption));
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), preview.ExitCode);
        Assert.Equal(installed, fixture.Snapshot());

        var update = await fixture.RunAsync(Write(ToolWriteKind.Update, ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption));

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), update.ExitCode);
        using var json = update.Json();
        var tool = Assert.Single(ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolWriteData.Tools)).EnumerateArray());
        Assert.Equal(ToolCompositionFixture.Version, ToolCompositionFixture.Field(tool, nameof(ToolWriteToolView.Version)).GetString());
        Assert.True(ToolCompositionFixture.Field(tool, nameof(ToolWriteToolView.ReuseExisting)).GetBoolean() || ToolCompositionFixture.Field(tool, nameof(ToolWriteToolView.NoOp)).GetBoolean());
        Assert.True(fixture.HasMember());
        Assert.Equal(0, fixture.DownloadCalls);
    }

    [Fact(DisplayName = "Install with --yes leaves a missing PATH entry unchanged for the user to add")]
    public async Task UnattendedInstallKeepsPath()
    {
        using var fixture = new ToolCompositionFixture { PathReadiness = ToolPathReadiness.Missing };

        var run = await fixture.RunAsync(Write(ToolWriteKind.Install, ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption));

        Assert.Contains(run.ExitCode, new[] { ToolCompositionFixture.Exit(CliSemanticStatus.Complete), ToolCompositionFixture.Exit(CliSemanticStatus.Attention) });
        Assert.True(fixture.HasMember());
        Assert.Equal(0, fixture.PathEffects);
        Assert.DoesNotContain(ToolHostEvent.PathAdd, fixture.Events);
        using var json = run.Json();
        var plan = ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolWriteData.Plan));
        Assert.NotEqual(ToolReadProjection.Name(ToolPathReadiness.Ready), ToolCompositionFixture.Field(plan, nameof(ToolWritePlanView.PathReadiness)).GetString());
    }

    private static string[] Status()
        => [ToolDefinitions.Group.Name, ToolStatusDefinitions.CommandName, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat];

    private static string[] Write(ToolWriteKind kind, string? id, string flag)
        => [ToolDefinitions.Group.Name, ToolWriteBinding.Name(kind), .. id is null ? Array.Empty<string>() : [id], flag,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat];

    private static JsonElement.ArrayEnumerator Tools(JsonDocument json)
        => ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolStatusResult.Data)), nameof(ToolReadPayload.Tools)).EnumerateArray();
}

using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-telemetry"), Trait("Evidence", "Integration")]
public sealed class ToolTelemetryReadTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Inspect reads saved telemetry without assuming off when settings are unavailable")]
    [InlineData("absent", false), InlineData("off", false), InlineData("on", true), InlineData("invalid", null)]
    public async Task ReadsSavedChoice(string situation, bool? expected)
    {
        using var workspace = TemporaryWorkspace.Create("tool-telemetry-read");
        workspace.WriteBytes(ToolFixtureSource.ManifestName, ToolFixtureSource.ManifestBytes());
        if (situation == "invalid") workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        else if (situation != "absent") workspace.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, Settings(situation == "on", active: false));
        var before = workspace.SnapshotHashes();
        var result = await new ToolInspectOperation(new()).ExecuteAsync(new(Selected(workspace), ToolFixtureSource.Id,
            workspace.Combine(ToolFixtureSource.ManifestName), null), Token);
        Assert.Equal(expected, result.Telemetry);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Active tool configuration distinguishes saved telemetry from the applied receipt")]
    [InlineData(false, false, (int)ToolEvidenceState.Matching)]
    [InlineData(true, true, (int)ToolEvidenceState.Matching)]
    [InlineData(false, true, (int)ToolEvidenceState.Drifted)]
    [InlineData(true, false, (int)ToolEvidenceState.Drifted)]
    public async Task ReportsConfigurationDrift(bool desired, bool applied, int state)
    {
        using var workspace = TemporaryWorkspace.Create("tool-telemetry-drift");
        var settingBytes = Settings(desired, active: true);
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(settingBytes).Document);
        var receipt = new ToolReceipt
        {
            Id = ToolFixtureSource.Id,
            Version = ToolFixtureSource.Version,
            Target = ToolNativeTarget.Current,
            ManifestSha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ManifestBytes()),
            ManifestBytes = ToolFixtureSource.ManifestBytes(),
            ManifestFormat = "json",
            AdapterRevision = 1,
            CodeRoot = ".",
            Hosts = [],
            Bindings = [],
            Files = [],
            RuntimeDirectories = [],
            NativeProjects = [],
            Telemetry = applied,
        };
        workspace.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, settingBytes);
        workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt) }));
        var before = workspace.SnapshotHashes();
        var row = new ToolInstalledRow(ToolFixtureSource.Id, ToolFixtureSource.Version, [ToolVocabulary.CodeNavigationRole], null, new(ToolEvidenceState.Matching));
        var result = await ToolPassiveStatusReader.ReadAsync(Selected(workspace), settings, row, receipt, Token);
        Assert.Equal((ToolEvidenceState)state, result.Configuration.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Inspect renders on, off and unavailable telemetry choices through reviewed output")]
    public async Task RendersChoice()
    {
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var situation in new[] { "off", "on", "invalid" })
        {
            using var workspace = TemporaryWorkspace.Create("tool-telemetry-output");
            workspace.WriteText(ToolFixtureSource.ManifestName, ToolFixtureSource.ManifestForAllTargets().ToJsonString());
            if (situation == "invalid") workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
            else workspace.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, Settings(situation == "on", active: false));
            var before = workspace.SnapshotHashes();
            var result = await CliHostCapture.RunAsync(["tool", "inspect", ToolFixtureSource.Id, "--source", workspace.Combine(ToolFixtureSource.ManifestName)], workspace.Path);
            captures[situation] = CommandOutputNormalization.Normalize(result.Error, workspace.Path, CliBuildVersion.InformationalVersion, normalizeTextPaths: true)
                .Replace($"{ToolFixtureSource.Id} {ToolFixtureSource.Version}  {ToolNativeTarget.Current}",
                    $"{ToolFixtureSource.Id} {ToolFixtureSource.Version}  <native-target>", StringComparison.Ordinal);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Empty(result.Output);
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    private static byte[] Settings(bool telemetry, bool active) => WorkspaceSettingsCodec.SetTools(ReadOnlyMemory<byte>.Empty,
        new Dictionary<string, WorkspaceToolSetting>
        {
            [ToolFixtureSource.Id] = new()
            {
                Version = ToolFixtureSource.Version,
                Roles = active ? [ToolVocabulary.CodeNavigationRole] : [],
                Hosts = [],
                Telemetry = telemetry,
            },
        }, ImmutableDictionary<string, ToolRunSummary>.Empty);

    private static CliWorkspace Selected(TemporaryWorkspace workspace) => new(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
}

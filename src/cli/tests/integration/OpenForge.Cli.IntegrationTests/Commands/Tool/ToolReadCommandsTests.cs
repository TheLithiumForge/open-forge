using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Inventory;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Provider;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
public sealed class ToolReadCommandsTests
{
    [Fact(DisplayName = "Tool group and leaf help stop before workspace resolution")]
    public async Task HelpStopsBeforeWorkspace()
    {
        using var workspace = TemporaryWorkspace.Create("tool-help");
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var leaf in new[] { "", "list", "inspect", "status" })
        {
            string[] command = leaf.Length == 0 ? ["tool"] : ["tool", leaf, "--help"];
            var result = await CliHostCapture.RunAsync([.. command, "--workspace", workspace.Combine("missing"), "--format", "json"], workspace.Path);
            Assert.Equal(0, result.ExitCode);
            Assert.Empty(result.Error);
            snapshots[leaf.Length == 0 ? "group.help" : leaf + ".help"] = OpenForge.Cli.TestSupport.Snapshots.CommandOutputNormalization.Normalize(
                result.Output, workspace.Path, "test");
        }
        Assert.Empty(workspace.SnapshotHashes());
        OpenForge.Cli.TestSupport.Snapshots.CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(HelpStopsBeforeWorkspace));
    }

    public static TheoryData<string[]> InvalidInputs => new()
    {
        new string[] { "list", "--dry-run" },
        new string[] { "list", "unexpected" },
        new string[] { "status", "--source", "unread" },
        new string[] { "inspect", ToolFixtureSource.Id, "--allow-download" },
        new string[] { "inspect" },
        new string[] { "inspect", "bad-id!" },
        new string[] { "inspect", "none", "--source", "unread" },
        new string[] { "inspect", ToolFixtureSource.Id, "--tool-version", "1" },
        new string[] { "list", "--role", "unknown" },
        new string[] { "list", "--role", ToolVocabulary.CodeNavigationRole, "--role", ToolVocabulary.CodeNavigationRole },
        new string[] { "inspect", ToolFixtureSource.Id, "--source", "a", "--source", "b" },
        new string[] { "inspect", ToolFixtureSource.Id, "--tool-version", ToolFixtureSource.Version, "--tool-version", ToolFixtureSource.Version },
    };

    [Theory(DisplayName = "Tool read commands reject writer flags, operands and repeated singletons before effects"), MemberData(nameof(InvalidInputs))]
    public async Task RejectsInvalidInput(string[] arguments)
    {
        using var workspace = TemporaryWorkspace.Create("tool-input");
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["tool", .. arguments], workspace.Path);
        Assert.Equal(4, result.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool List keeps installed settings when the selected catalogue is invalid")]
    public async Task KeepsIndependentInstalledRows()
    {
        using var workspace = TemporaryWorkspace.Create("tool-independent");
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([]));
        workspace.WriteText(ToolFixtureSource.ManifestName, "{}");
        var before = workspace.SnapshotHashes();
        var result = await new ToolListOperation(new()).ExecuteAsync(
            new(Selected(workspace), Path.Combine(workspace.Path, ToolFixtureSource.ManifestName), null), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(ToolFixtureSource.Id, Assert.Single(result.Installed).Id);
        Assert.Equal(ToolEvidenceState.Unavailable, result.Installed[0].Verification.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Cancelled Tool readers return cancellation before opening sources")]
    public async Task CancelsBeforeObservation()
    {
        using var workspace = TemporaryWorkspace.Create("tool-cancel");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var selected = Selected(workspace);
        var list = await new ToolListOperation(new()).ExecuteAsync(new(selected, null, null), cancellation.Token);
        var inspect = await new ToolInspectOperation(new()).ExecuteAsync(new(selected, ToolFixtureSource.Id, null, null), cancellation.Token);
        var status = await new ToolStatusOperation().ExecuteAsync(new(selected, null), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, list.Status);
        Assert.Equal(CliSemanticStatus.Interrupted, inspect.Status);
        Assert.Equal(CliSemanticStatus.Interrupted, status.Status);
        Assert.Empty(workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Tool Inspect explains reviewed candidates and blocks unreviewed tuples without losing definition facts")]
    [InlineData(true, (int)CliSemanticStatus.Complete)]
    [InlineData(false, (int)CliSemanticStatus.Blocked)]
    public async Task InspectKeepsCandidateFacts(bool reviewed, int status)
    {
        using var workspace = TemporaryWorkspace.Create("tool-inspect");
        var bytes = ToolFixtureSource.ManifestBytes();
        workspace.WriteBytes(ToolFixtureSource.ManifestName, bytes);
        var definition = new ToolCatalogueReader().Definition(bytes, ToolFixtureSource.ManifestName, false);
        var support = new ToolReleaseSupport(reviewed ? [new(ToolFixtureSource.Id, ToolFixtureSource.Version, definition.Sha256, definition.Manifest.Adapter.Revision)] : [],
            [new(definition.Manifest.Adapter.Id, definition.Manifest.Adapter.Revision)]);
        var before = workspace.SnapshotHashes();
        var result = await new ToolInspectOperation(new(support)).ExecuteAsync(
            new(Selected(workspace), ToolFixtureSource.Id, Path.Combine(workspace.Path, ToolFixtureSource.ManifestName), null), TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.NotNull(result.Definition);
        Assert.Equal(ToolNativeTarget.Current, result.Target);
        Assert.Null(result.Installed);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Tool Status reports missing, changed and present index evidence without touching the workspace")]
    [InlineData("current", false, (int)ToolEvidenceState.Verified, (int)ToolEvidenceState.NeedsIndex)]
    [InlineData("missing", false, (int)ToolEvidenceState.Missing, (int)ToolEvidenceState.NeedsIndex)]
    [InlineData("changed", false, (int)ToolEvidenceState.Changed, (int)ToolEvidenceState.NeedsIndex)]
    [InlineData("current", true, (int)ToolEvidenceState.Verified, (int)ToolEvidenceState.PresentUnverified)]
    public async Task PassiveReceiptChecks(string file, bool indexPresent, int expectedInstallation, int expectedIndex)
    {
        using var workspace = TemporaryWorkspace.Create("tool-status");
        var manifest = ToolFixtureSource.Manifest();
        var index = Assert.IsType<JsonObject>(manifest["index"]);
        index["location"] = "project-directory";
        index["path"] = "fixture-index";
        var bytes = Encoding.UTF8.GetBytes(manifest.ToJsonString());
        var receipt = Receipt(bytes) with { Files = [new(null, ToolFixtureSource.ArtifactName, ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes()), false)] };
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([]));
        workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt) }));
        if (file != "missing") workspace.WriteBytes(ToolFixtureSource.ArtifactName, file == "current" ? ToolFixtureSource.ArtifactBytes() : [1, 2]);
        if (indexPresent) workspace.WriteText("fixture-index", "native database fixture");
        var before = workspace.SnapshotHashes();
        var result = await new ToolStatusOperation().ExecuteAsync(new(Selected(workspace), null), TestContext.Current.CancellationToken);
        var row = Assert.Single(result.Tools);
        Assert.Equal((ToolEvidenceState)expectedInstallation, row.Installation.State);
        Assert.Equal((ToolEvidenceState)expectedIndex, row.Index.State);
        Assert.Equal(ToolEvidenceState.Inactive, row.Guidance.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool source paths resolve from the process directory independently of workspace selection")]
    public async Task SourceUsesProcessDirectory()
    {
        using var workspace = TemporaryWorkspace.Create("tool-selected-workspace");
        using var source = TemporaryWorkspace.Create("tool-process-directory");
        source.WriteBytes(ToolFixtureSource.ManifestName, ToolFixtureSource.ManifestBytes());
        var before = source.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            ["tool", "inspect", ToolFixtureSource.Id, "--source", ToolFixtureSource.ManifestName, "--workspace", workspace.Path, "--format", "json"], source.Path);
        Assert.Equal(5, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.NotEqual(JsonValueKind.Null, json.RootElement.GetProperty("data").GetProperty("definition").ValueKind);
        Assert.Equal(before, source.SnapshotHashes());
        Assert.Empty(workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool read output uses reviewed human and JSON results at every detail")]
    public async Task ReadOutput()
    {
        using var workspace = TemporaryWorkspace.Create("tool-read-output");
        var manifest = ToolFixtureSource.Manifest();
        var targets = Assert.IsType<JsonObject>(manifest["targets"]);
        var available = targets.First(pair => pair.Value?["state"]?.GetValue<string>() == "available").Value;
        Assert.NotNull(available);
        foreach (var target in targets.Select(pair => pair.Key).ToArray()) targets[target] = available.DeepClone();
        workspace.WriteText(ToolFixtureSource.ManifestName, manifest.ToJsonString());
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        var before = workspace.SnapshotHashes();
        foreach (var scenario in new[]
        {
            new ReadOutputScenario { Situation = "empty-list", Arguments = ["tool", "list"], ExitCode = 0 },
            new ReadOutputScenario { Situation = "local-list", Arguments = ["tool", "list", "--source", Path.Combine(workspace.Path, ToolFixtureSource.ManifestName)], ExitCode = 0 },
            new ReadOutputScenario { Situation = "empty-status", Arguments = ["tool", "status"], ExitCode = 0 },
            new ReadOutputScenario
            { Situation = "candidate", Arguments = ["tool", "inspect", ToolFixtureSource.Id, "--source", Path.Combine(workspace.Path, ToolFixtureSource.ManifestName)], ExitCode = 5 },
            new ReadOutputScenario { Situation = "unknown", Arguments = ["tool", "status", ToolFixtureSource.Id], ExitCode = 4 },
        })
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(scenario, snapshotCollector: snapshots);
        Assert.Equal(before, workspace.SnapshotHashes());
        // Only the actual runtime target varies. The synthetic definition declares the same candidate bytes for all six targets.
        foreach (var key in snapshots.Keys.ToArray())
            snapshots[key] = snapshots[key].Replace($"\"target\": \"{ToolNativeTarget.Current}\"", "\"target\": \"<native-target>\"", StringComparison.Ordinal)
                .Replace($"{ToolFixtureSource.Id} {ToolFixtureSource.Version}  {ToolNativeTarget.Current}",
                    $"{ToolFixtureSource.Id} {ToolFixtureSource.Version}  <native-target>", StringComparison.Ordinal);
        OpenForge.Cli.TestSupport.Snapshots.CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: nameof(ReadOutput));
    }

    private static CliWorkspace Selected(TemporaryWorkspace workspace) => new(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
    private static string Settings(string[] roles) => new JsonObject
    {
        [WorkspaceSettingsDefinitions.ToolsProperty] = new JsonObject
        { [ToolFixtureSource.Id] = new JsonObject { ["version"] = ToolFixtureSource.Version, ["roles"] = new JsonArray(roles.Select(role => JsonValue.Create(role)).ToArray()) } },
    }.ToJsonString();
    private static ToolReceipt Receipt(byte[] manifest) => new()
    {
        Id = ToolFixtureSource.Id,
        Version = ToolFixtureSource.Version,
        Target = "win-x64",
        ManifestSha256 = ToolCatalogueReader.Hash(manifest),
        ManifestBytes = manifest,
        ManifestFormat = "json",
        AdapterRevision = 1,
        CodeRoot = ".",
        Hosts = [],
        Bindings = [],
        Files = [],
        Members = [],
        RuntimeDirectories = [],
        NativeProjects = [],
    };
}

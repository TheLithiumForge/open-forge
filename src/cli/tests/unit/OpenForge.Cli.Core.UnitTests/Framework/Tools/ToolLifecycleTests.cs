using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools;

[Trait("Feature", "tool-lifecycle"), Trait("Evidence", "Unit")]
public sealed class ToolLifecycleTests
{
    [Fact(DisplayName = "Reviewed lifecycle vectors keep repeated argument values and exact command outputs")]
    public void ReadsDeclaredLifecycle()
    {
        var source = WithLifecycle();
        var definition = new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(source.ToJsonString()), ToolFixtureSource.ManifestName, true);
        var lifecycle = Assert.IsType<ToolLifecycle>(definition.Manifest.Lifecycle);
        Assert.Equal([ToolVocabulary.InstallAction, "repeated", "repeated", "${host}"], lifecycle.Install.Args);
        Assert.Equal(ToolVocabulary.HostScope, lifecycle.Install.Scope);
        Assert.Equal(ToolVocabulary.CodeRootPrefix + "fixture-output", Assert.Single(lifecycle.Install.WorkspaceOutputs));
        Assert.False(definition.Reviewed);
    }

    [Theory(DisplayName = "Lifecycle declarations reject unsafe output scope and invalid command shapes")]
    [InlineData("parent-output")]
    [InlineData("absolute-output")]
    [InlineData("variable-output")]
    [InlineData("duplicate-output")]
    [InlineData("null-command")]
    [InlineData("unknown-command")]
    [InlineData("zero-timeout")]
    [InlineData("large-timeout")]
    [InlineData("workspace-host")]
    [InlineData("host-cleanup")]
    [InlineData("unknown-variable")]
    [InlineData("bad-ignore")]
    [InlineData("missing-removal-command")]
    [InlineData("duplicate-removal-command")]
    [InlineData("unexpected-removal-command")]
    [InlineData("telemetry-timeout")]
    public void RejectsInvalidLifecycle(string situation)
    {
        var source = WithLifecycle();
        var lifecycle = Assert.IsType<JsonObject>(source["lifecycle"]);
        var install = Assert.IsType<JsonObject>(lifecycle["install"]);
        switch (situation)
        {
            case "parent-output": install["workspaceOutputs"] = new JsonArray("../outside"); break;
            case "absolute-output": install["workspaceOutputs"] = new JsonArray("/outside"); break;
            case "variable-output": install["workspaceOutputs"] = new JsonArray("${worktree}/output"); break;
            case "duplicate-output": install["workspaceOutputs"] = new JsonArray("output", "output"); break;
            case "null-command": lifecycle["install"] = null; break;
            case "unknown-command": install["executable"] = "unreviewed"; break;
            case "zero-timeout": install["timeoutSeconds"] = 0; break;
            case "large-timeout": install["timeoutSeconds"] = ToolVocabulary.MaximumCommandTimeoutSeconds + 1; break;
            case "workspace-host": install["scope"] = ToolVocabulary.WorkspaceScope; break;
            case "host-cleanup": Assert.IsType<JsonObject>(lifecycle["indexCleanup"])["scope"] = ToolVocabulary.HostScope; break;
            case "unknown-variable": install["args"] = new JsonArray("${unreviewed}"); break;
            case "bad-ignore": lifecycle["ignorePaths"] = new JsonArray("output//"); break;
            case "missing-removal-command": lifecycle["removalOrder"] = new JsonArray(ToolVocabulary.UninstallAction); break;
            case "duplicate-removal-command": lifecycle["removalOrder"] = new JsonArray(ToolVocabulary.UninstallAction, ToolVocabulary.UninstallAction); break;
            case "unexpected-removal-command": lifecycle["removalOrder"] = new JsonArray(ToolVocabulary.UninstallAction, ToolVocabulary.InstallAction); break;
            case "telemetry-timeout":
                var telemetry = Assert.IsType<JsonObject>(install.DeepClone());
                telemetry["timeoutSeconds"] = 0;
                lifecycle["telemetryOff"] = telemetry;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(situation));
        }
        Assert.Throws<JsonException>(() => new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(source.ToJsonString()), ToolFixtureSource.ManifestName, true));
    }

    [Theory(DisplayName = "A removal definition orders each required command exactly once")]
    [InlineData(false), InlineData(true)]
    public void KeepsDeclaredRemovalOrder(bool cleanupFirst)
    {
        var source = WithLifecycle();
        var lifecycle = Assert.IsType<JsonObject>(source["lifecycle"]);
        var order = cleanupFirst
            ? new[] { ToolVocabulary.IndexCleanupAction, ToolVocabulary.UninstallAction }
            : new[] { ToolVocabulary.UninstallAction, ToolVocabulary.IndexCleanupAction };
        lifecycle["removalOrder"] = new JsonArray(order.Select(value => JsonValue.Create(value)).ToArray());
        var definition = new ToolCatalogueReader().Definition(Encoding.UTF8.GetBytes(source.ToJsonString()), ToolFixtureSource.ManifestName, false);
        Assert.Equal(order, Assert.IsType<ToolLifecycle>(definition.Manifest.Lifecycle).RemovalOrder);
    }

    [Fact(DisplayName = "Failed first-install evidence survives settings and lock round trips without an installed registration")]
    public void FailedAttemptIsNotAnInstallation()
    {
        var summary = new ToolRunSummary(ToolVocabulary.InstallAction, ToolCommandState.Failed,
            [new(ToolVocabulary.InstallAction, null, ToolCommandState.Exited, 23)]);
        var runs = ImmutableDictionary<string, ToolRunSummary>.Empty.Add(ToolFixtureSource.Id, summary);
        var bytes = WorkspaceSettingsCodec.SetTools("{\"fixtureUnknown\":true}"u8.ToArray(), ImmutableDictionary<string, WorkspaceToolSetting>.Empty, runs);
        var settings = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Empty(settings.Tools);
        Assert.Equal(23, Assert.Single(settings.ToolRuns[ToolFixtureSource.Id].Commands).ExitCode);
        using var json = JsonDocument.Parse(bytes);
        Assert.True(json.RootElement.GetProperty("fixtureUnknown").GetBoolean());
        var command = new ToolCommandReceipt
        {
            Action = ToolVocabulary.InstallAction,
            Host = null,
            Entry = ToolFixtureSource.ArtifactName,
            EntrySha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes()),
            Args = [ToolVocabulary.InstallAction],
            Environment = [],
            State = ToolCommandState.Exited,
            ExitCode = 23,
            Cause = null,
        };
        var record = new ToolRunRecord
        {
            Version = ToolFixtureSource.Version,
            Target = ToolVocabulary.Targets.Order(StringComparer.Ordinal).First(),
            ManifestSha256 = ToolCatalogueReader.Hash(ToolFixtureSource.ManifestBytes()),
            Action = summary.Action,
            State = summary.State,
            Commands = [command],
            ManifestBytes = ToolFixtureSource.ManifestBytes(),
            ManifestFormat = "json",
            CodeRoot = ".",
            Hosts = [],
        };
        var document = WorkspaceOwnershipDocument.Empty with { ToolRuns = ImmutableDictionary<string, ToolRunRecord>.Empty.Add(ToolFixtureSource.Id, record) };
        var read = Assert.IsType<WorkspaceOwnershipDocument>(WorkspaceOwnershipCodec.Read(WorkspaceOwnershipCodec.Write(document)).Document);
        Assert.Empty(read.Tools);
        Assert.Equal(command.EntrySha256, Assert.Single(read.ToolRuns[ToolFixtureSource.Id].Commands).EntrySha256);
    }

    [Theory(DisplayName = "Tool run summaries keep typed command states and reject invented states")]
    [InlineData((int)ToolCommandState.Planned, null)]
    [InlineData((int)ToolCommandState.Started, null)]
    [InlineData((int)ToolCommandState.Exited, 0)]
    [InlineData((int)ToolCommandState.Failed, null)]
    [InlineData((int)ToolCommandState.Interrupted, null)]
    public void RoundTripsExecutionStates(int stateValue, int? exitCode)
    {
        var state = (ToolCommandState)stateValue;
        var summary = new ToolRunSummary(ToolVocabulary.InstallAction, state, [new(ToolVocabulary.InstallAction, null, state, exitCode)]);
        var runs = new Dictionary<string, ToolRunSummary> { [ToolFixtureSource.Id] = summary };
        var bytes = WorkspaceSettingsCodec.SetTools(ReadOnlyMemory<byte>.Empty, ImmutableDictionary<string, WorkspaceToolSetting>.Empty, runs);
        var read = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document);
        Assert.Equal(state, read.ToolRuns[ToolFixtureSource.Id].State);
        Assert.Equal(exitCode, Assert.Single(read.ToolRuns[ToolFixtureSource.Id].Commands).ExitCode);
        Assert.Throws<JsonException>(() => ToolLifecycleValidator.Validate(new Dictionary<string, ToolRunSummary>
        { [ToolFixtureSource.Id] = summary with { State = (ToolCommandState)int.MaxValue } }));
    }

    [Theory(DisplayName = "Malformed run records cannot turn unknown execution into success")]
    [InlineData("{\"action\":\"install\",\"state\":\"exited\",\"commands\":[{\"action\":\"install\",\"host\":null,\"state\":\"exited\",\"exitCode\":null}]}")]
    [InlineData("{\"action\":\"install\",\"state\":0,\"commands\":[]}")]
    [InlineData("{\"action\":\"arbitrary\",\"state\":\"failed\",\"commands\":[]}")]
    public void RejectsMalformedRun(string run)
    {
        var root = new JsonObject { [WorkspaceSettingsDefinitions.ToolRunsProperty] = new JsonObject { [ToolFixtureSource.Id] = JsonNode.Parse(run) } };
        Assert.Null(WorkspaceSettingsCodec.Read(Encoding.UTF8.GetBytes(root.ToJsonString())).Document);
    }

    private static JsonObject WithLifecycle()
    {
        var source = ToolFixtureSource.Manifest();
        JsonObject Command(string action, string scope) => new()
        {
            ["args"] = scope == ToolVocabulary.HostScope ? new JsonArray(action, "repeated", "repeated", "${host}") : new JsonArray(action),
            ["scope"] = scope,
            ["workspaceOutputs"] = new JsonArray(ToolVocabulary.CodeRootPrefix + "fixture-output"),
            ["timeoutSeconds"] = 30,
        };
        source["lifecycle"] = new JsonObject
        {
            ["install"] = Command(ToolVocabulary.InstallAction, ToolVocabulary.HostScope),
            ["uninstall"] = Command(ToolVocabulary.UninstallAction, ToolVocabulary.HostScope),
            ["indexCleanup"] = Command(ToolVocabulary.IndexCleanupAction, ToolVocabulary.WorkspaceScope),
            ["ignorePaths"] = new JsonArray("fixture-index/"),
        };
        return source;
    }
}

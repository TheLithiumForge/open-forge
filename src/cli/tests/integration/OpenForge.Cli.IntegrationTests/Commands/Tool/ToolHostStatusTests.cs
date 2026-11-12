using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
public sealed class ToolHostStatusTests
{
    [Theory(DisplayName = "Passive status reads persisted JSON and TOML member receipts and active guidance")]
    [InlineData(ToolVocabulary.JsonMembers, "matching", (int)ToolEvidenceState.Matching)]
    [InlineData(ToolVocabulary.TomlTable, "matching", (int)ToolEvidenceState.Matching)]
    [InlineData(ToolVocabulary.JsonMembers, "missing", (int)ToolEvidenceState.Missing)]
    [InlineData(ToolVocabulary.JsonMembers, "changed", (int)ToolEvidenceState.Changed)]
    [InlineData(ToolVocabulary.JsonMembers, "excluded", (int)ToolEvidenceState.Excluded)]
    [InlineData(ToolVocabulary.JsonMembers, "inactive", (int)ToolEvidenceState.Inactive)]
    public async Task ReadsPersistedHostMembers(string format, string guidanceState, int expectedGuidance)
    {
        using var workspace = TemporaryWorkspace.Create("tool-host-status");
        var bytes = ToolFixtureSource.ManifestBytes();
        var definition = new ToolCatalogueReader().Definition(bytes, ToolFixtureSource.ManifestName, false);
        var host = definition.Manifest.Mcp.Hosts[format == ToolVocabulary.JsonMembers ? "claude-code" : "codex"];
        const string guidance = "Fixture usage guidance\n";
        using var value = JsonDocument.Parse("{\"command\":\"fixture\"}");
        var receipt = new ToolReceipt
        {
            Id = ToolFixtureSource.Id, Version = ToolFixtureSource.Version, Target = "win-x64",
            ManifestSha256 = definition.Sha256, ManifestBytes = bytes, ManifestFormat = "json", AdapterRevision = definition.Manifest.Adapter.Revision,
            CodeRoot = ".", Hosts = [format == ToolVocabulary.JsonMembers ? "claude-code" : "codex"], Bindings = [],
            Files = [new(null, ToolPassiveStatusReader.GuidancePath, ToolCatalogueReader.Hash(Encoding.UTF8.GetBytes(guidance)), false)],
            Members = [new(host.Path, format, host.Selector, value.RootElement.Clone(), false)], RuntimeDirectories = [], NativeProjects = [],
        };
        workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        { Tools = ImmutableDictionary<string, ToolReceipt>.Empty.Add(ToolFixtureSource.Id, receipt) }));
        var settings = new JsonObject
        {
            [WorkspaceSettingsDefinitions.ToolsProperty] = new JsonObject
            {
                [ToolFixtureSource.Id] = new JsonObject
                { ["version"] = ToolFixtureSource.Version, ["roles"] = guidanceState == "inactive" ? new JsonArray() : new JsonArray(ToolVocabulary.CodeNavigationRole) },
            },
        };
        if (guidanceState == "excluded") settings[WorkspaceSettingsDefinitions.RemovedFilesProperty] = new JsonArray(ToolPassiveStatusReader.GuidancePath);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, settings.ToJsonString());
        if (guidanceState is not ("missing" or "inactive"))
            workspace.WriteText(ToolPassiveStatusReader.GuidancePath, guidanceState == "changed" ? "Changed fixture guidance\n" : guidance);
        workspace.WriteText(host.Path, format == ToolVocabulary.JsonMembers
            ? new JsonObject { ["unrelated"] = true, ["mcpServers"] = new JsonObject
                { ["open-forge-" + ToolFixtureSource.Id] = new JsonObject { ["command"] = "fixture" } } }.ToJsonString()
            : $"# unrelated comment\nunrelated = true\n[{host.Selector}]\ncommand = 'fixture'\n");
        var before = workspace.SnapshotHashes();

        var result = await new ToolStatusOperation().ExecuteAsync(
            new(new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory), null), TestContext.Current.CancellationToken);

        var row = Assert.Single(result.Tools);
        Assert.Equal(guidanceState == "inactive" ? ToolEvidenceState.Inactive : ToolEvidenceState.Matching, row.Configuration.State);
        Assert.Equal((ToolEvidenceState)expectedGuidance, row.Guidance.State);
        if (guidanceState == "inactive")
            Assert.DoesNotContain(result.Issues, issue => issue.Code is ToolReadIssueCode.UsageSkillMissing or ToolReadIssueCode.InstalledFileMissing);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}

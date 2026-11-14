using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation;

public sealed class ToolSetupDocumentationTests
{
    [Fact(DisplayName = "Tool setup Markdown links resolve across the planned contract and outline pack")]
    [Trait("Boundary", "Host"), Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public async Task PlannedDocumentsHaveValidLinks()
    {
        var repositoryRoot = RepositoryRoot();
        string[] sources =
        [
            ".agents/memory/working/cli-development/tasks/task90-tool-setup.md",
            ".agents/memory/working/cli-development/project-control.md",
            ".agents/memory/working/cli-development/plan.md",
            ".agents/memory/working/cli-development/tool-setup/_tool-setup.md",
            ".agents/memory/working/cli-development/tool-setup/packets/_packets.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-a.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-b.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-c.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-d.md",
            ".agents/memory/working/cli-development/tool-setup/packets/lane-e.md",
            ".agents/memory/crystallized/documents/cli/contracts/_contracts.md",
            ".agents/memory/crystallized/documents/cli/contracts/shared/_shared.md",
            ".agents/memory/crystallized/documents/cli/technical-designs/_technical-designs.md",
            ".agents/memory/crystallized/documents/maintenance/_maintenance.md",
            ".agents/skills/open-forge-cli/SKILL.md",
            "src/open-forge/.agents/skills/open-forge-cli/SKILL.md",
            ".agents/memory/crystallized/decisions/framework/workspace-state-files.md",
            ".agents/memory/crystallized/documents/cli/architecture.md",
            ".agents/memory/crystallized/documents/cli/command-contract-set.md",
            ".agents/memory/crystallized/documents/cli/contracts/doctor/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/doctor/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/repair/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/repair/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/shared/tool-records/_tool-records.md",
            ".agents/memory/crystallized/documents/cli/contracts/shared/tool-records/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/shared/tool-records/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/status/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/status/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/_tool.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/inspect/_inspect.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/inspect/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/inspect/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/install/_install.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/install/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/install/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/list/_list.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/list/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/list/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/remove/_remove.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/remove/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/remove/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/status/_status.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/status/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/status/interface.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/update/_update.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/update/behavior.md",
            ".agents/memory/crystallized/documents/cli/contracts/tool/update/interface.md",
            ".agents/memory/crystallized/documents/cli/shared-operation-contract.md",
            ".agents/memory/crystallized/documents/cli/technical-designs/tool-setup-lanes.md",
            ".agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md",
            ".agents/memory/crystallized/documents/maintenance/tools.md",
            ".agents/skills/open-forge-cli/references/tools.md",
            "docs/cli.md",
            "src/cli/output-text/OpenForge.Cli.OutputText/Help/tool-setup-planned.md",
            "src/open-forge/.agents/skills/open-forge-cli/references/tools.md",
        ];

        var diagnostics = await RepositoryMarkdownChecker.CheckAsync(
            repositoryRoot,
            sources,
            TestContext.Current.CancellationToken);

        Assert.True(diagnostics.Count == 0, string.Join(Environment.NewLine, diagnostics));
    }

    [Theory(DisplayName = "Tool record schemas keep the released schema fields")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Boundary", "Host"), Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public void NewSchemasKeepReleasedFields(bool ownership)
    {
        var repositoryRoot = RepositoryRoot();
        var releasedVersion = ownership ? ToolRecordDefinitions.ReleasedLockSchemaVersion : ToolRecordDefinitions.ReleasedSettingsSchemaVersion;
        var plannedVersion = ownership ? ToolRecordDefinitions.LockSchemaVersion : ToolRecordDefinitions.SettingsSchemaVersion;
        var schemaUrl = ownership ? WorkspaceOwnershipDefinitions.SchemaUrl : WorkspaceSettingsDefinitions.SchemaUrl;
        var fileName = Path.GetFileName(new Uri(schemaUrl).LocalPath);
        var released = ReadSchema(releasedVersion);
        var planned = ReadSchema(plannedVersion);
        var plannedProperties = Assert.IsType<JsonObject>(planned["properties"]);
        Assert.True(plannedProperties.Remove(ToolRecordDefinitions.ToolsProperty));
        planned["$id"] = released["$id"]?.DeepClone();
        if (planned["examples"] is JsonArray examples)
        {
            foreach (var example in examples)
            {
                if (example?["schemaVersion"]?.GetValue<int>() == plannedVersion)
                {
                    example["schemaVersion"] = releasedVersion;
                }
            }
        }

        Assert.True(JsonNode.DeepEquals(released, planned));

        JsonObject ReadSchema(int version)
            => Assert.IsType<JsonObject>(JsonNode.Parse(File.ReadAllText(Path.Combine(repositoryRoot, WorkspaceSettingsDefinitions.SchemaDirectoryName, $"v{version}", fileName))));
    }

    [Theory(DisplayName = "Tool record schemas accept stable versions and supported minor ranges")]
    [InlineData(true, "1.6.2", true)]
    [InlineData(true, "1.6.2+build.5", true)]
    [InlineData(true, "1.6.2-alpha", false)]
    [InlineData(true, "01.6.2", false)]
    [InlineData(true, "1.6.2+.", false)]
    [InlineData(false, "1.6.x", true)]
    [InlineData(false, "1.6.2", false)]
    [InlineData(false, "1.6.x-beta", false)]
    [Trait("Boundary", "Host"), Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public void SchemasValidateVersionForms(bool ownership, string version, bool expected)
    {
        var schemaVersion = ownership ? ToolRecordDefinitions.LockSchemaVersion : ToolRecordDefinitions.SettingsSchemaVersion;
        var schemaUrl = ownership ? WorkspaceOwnershipDefinitions.SchemaUrl : WorkspaceSettingsDefinitions.SchemaUrl;
        var fileName = Path.GetFileName(new Uri(schemaUrl).LocalPath);
        var schema = Assert.IsType<JsonObject>(JsonNode.Parse(File.ReadAllText(
            Path.Combine(RepositoryRoot(), WorkspaceSettingsDefinitions.SchemaDirectoryName, $"v{schemaVersion}", fileName))));
        var tools = Assert.IsType<JsonObject>(schema["properties"]?[ToolRecordDefinitions.ToolsProperty]);
        var pattern = tools["additionalProperties"]?["properties"]?[ToolRecordDefinitions.VersionProperty]?["pattern"]?.GetValue<string>();
        Assert.NotNull(pattern);

        Assert.Equal(expected, Regex.IsMatch(version, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)));
    }

    [Fact(DisplayName = "Tool lock schema records exact versions, channels and additive receipts")]
    [Trait("Boundary", "Host"), Trait("Feature", "tool-documentation"), Trait("Evidence", "Integration")]
    public void LockSchemaDefinesAdditiveRecords()
    {
        var fileName = Path.GetFileName(new Uri(WorkspaceOwnershipDefinitions.SchemaUrl).LocalPath);
        var schema = Assert.IsType<JsonObject>(JsonNode.Parse(File.ReadAllText(
            Path.Combine(RepositoryRoot(), WorkspaceSettingsDefinitions.SchemaDirectoryName,
                $"v{ToolRecordDefinitions.LockSchemaVersion}", fileName))));
        var entry = Assert.IsType<JsonObject>(schema["properties"]?[ToolRecordDefinitions.ToolsProperty]?["additionalProperties"]);
        var required = Assert.IsType<JsonArray>(entry["required"]).Select(value => value?.GetValue<string>()).ToArray();

        Assert.Equal(
            [ToolRecordDefinitions.VersionProperty, ToolRecordDefinitions.ChannelProperty, ToolRecordDefinitions.ChangesProperty],
            required);

        var cases = Assert.IsType<JsonArray>(
            entry["properties"]?[ToolRecordDefinitions.ChangesProperty]?["items"]?["oneOf"]);
        var actualKinds = cases.Select(value =>
            value?["properties"]?[ToolRecordDefinitions.KindProperty]?["const"]?.GetValue<string>())
            .Order(StringComparer.Ordinal).ToArray();
        var expectedKinds = new[]
        {
            ToolRecordDefinitions.JsonUnionKind,
            ToolRecordDefinitions.JsonValueKind,
            ToolRecordDefinitions.MarkedBlockKind,
            ToolRecordDefinitions.ManagedFileKind,
            ToolRecordDefinitions.NativeCommandKind,
        }.Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(expectedKinds, actualKinds);

        var native = Assert.Single(cases, value =>
            value?["properties"]?[ToolRecordDefinitions.KindProperty]?["const"]?.GetValue<string>()
                == ToolRecordDefinitions.NativeCommandKind);
        var nativeProperties = Assert.IsType<JsonObject>(native?["properties"]);
        Assert.Equal(
            new[]
            {
                ToolRecordDefinitions.KindProperty,
                ToolRecordDefinitions.IdProperty,
                ToolRecordDefinitions.AllowListIdProperty,
            }.Order(StringComparer.Ordinal),
            nativeProperties.Select(pair => pair.Key).Order(StringComparer.Ordinal));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}

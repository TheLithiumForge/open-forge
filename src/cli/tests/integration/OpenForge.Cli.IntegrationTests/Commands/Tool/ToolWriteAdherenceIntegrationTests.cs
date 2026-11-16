using OpenForge.Cli.Core.Shell.Definitions;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

[Trait("Feature", "tool-composition"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolWriteAdherenceIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    // Install and Update share the writer path. Unit ToolWritePermissionTests covers both kinds.
    [Theory(DisplayName = "One missing authored destination blocks the full composed writer batch before effects")]
    [InlineData("install")]
    public async Task MissingPermissionBlocksBatch(string leaf)
    {
        using var fixture = new ToolCompositionFixture(twoTools: true, grantDestinations: false);
        fixture.Declare();
        fixture.GrantDestinations(ToolCompositionFixture.ConfigurationDirectory + "/" + ToolCompositionFixture.FirstId + ".json");
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", leaf, ToolWriteBinding.YesOption, "--format", "json"]);

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Blocked), run.ExitCode);
        using var json = run.Json();
        var tools = Tools(json.RootElement).EnumerateArray().ToArray();
        Assert.Equal(2, tools.Length);
        var denied = tools.Single(tool => Field(tool, nameof(ToolWriteToolView.Id)).GetString() == ToolCompositionFixture.LastId);
        var permissions = Field(denied, nameof(ToolWriteToolView.Permissions));
        Assert.Equal(WorkspacePermissionDecision.Required.ToString(), Field(permissions, nameof(ToolWritePermissionView.Decision)).GetString());
        Assert.NotEmpty(Field(permissions, nameof(ToolWritePermissionView.Missing)).EnumerateArray());
        Assert.Equal(before, fixture.Snapshot());
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Empty(fixture.Backups());
    }

    [Theory(DisplayName = "Explicit invocation grants admit composed preview without saving settings")]
    [InlineData("update")]
    public async Task PreviewGrantIsReadOnly(string leaf)
    {
        using var fixture = new ToolCompositionFixture(grantDestinations: false);
        fixture.Declare();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", leaf, ToolCompositionFixture.FirstId, ToolWriteBinding.DryRunOption,
            "--allow-path", ToolCompositionFixture.ConfigurationDirectory, "--format", "json"]);

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        using var json = run.Json();
        var permission = Field(Assert.Single(Tools(json.RootElement).EnumerateArray()), nameof(ToolWriteToolView.Permissions));
        Assert.Equal(WorkspacePermissionDecision.Granted.ToString(), Field(permission, nameof(ToolWritePermissionView.Decision)).GetString());
        Assert.Empty(Field(permission, nameof(ToolWritePermissionView.Missing)).EnumerateArray());
        Assert.Equal(before, fixture.Snapshot());
        Assert.Empty(fixture.Backups());
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
    }

    [Fact(DisplayName = "Project removal enforces current destination permission before cleanup")]
    public async Task RemovalRequiresAdmission()
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), (await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        fixture.GrantDestinations();
        fixture.Events.Clear();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "remove", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Blocked), run.ExitCode);
        Assert.Equal(before, fixture.Snapshot());
        Assert.True(fixture.HasMember());
        Assert.DoesNotContain(ToolHostEvent.Uninstall, fixture.Events);
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "Custom-root archive exclusion is resolved once and preview matches the applied receipt")]
    public async Task ResolvedArchivePlanMatchesApplication()
    {
        using var fixture = new ToolCompositionFixture();
        const string mainRoot = "synthetic-$cash-context";
        var path = Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath);
        var settings = JsonNode.Parse(File.ReadAllBytes(path))?.AsObject() ?? throw new InvalidOperationException();
        settings[WorkspaceSettingsDefinitions.MainRootProperty] = mainRoot;
        File.WriteAllText(path, settings.ToJsonString());
        var entry = fixture.Catalogue.Entries[ToolCompositionFixture.FirstId];
        var original = Assert.IsType<ToolJsonValueChange>(Assert.Single(entry.Profiles[0].Changes));
        var template = ToolRecipeDefinitions.MainRootToken + "/memory/archived/**";
        using var value = JsonDocument.Parse(JsonSerializer.Serialize(new[] { template }, ToolCompositionSnapshotJsonContext.Default.StringArray));
        fixture.Catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry>
            {
                [entry.Identity.Id] = entry with { Profiles = [entry.Profiles[0] with { Changes = [original with { Value = value.RootElement.Clone() }] }] },
            },
        };
        var preview = await fixture.RunAsync(["tool", "install", entry.Identity.Id, ToolWriteBinding.DryRunOption, "--format", "json"]);
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), preview.ExitCode);
        using var json = preview.Json();
        var change = Assert.Single(Field(Assert.Single(Tools(json.RootElement).EnumerateArray()), nameof(ToolWriteToolView.Changes)).EnumerateArray());
        var planned = Assert.Single(Field(change, nameof(ToolWriteChangeView.Items)).EnumerateArray()).GetString();
        Assert.Equal(mainRoot + "/memory/archived/**", planned);
        Assert.Equal(original.PathTemplate, Field(change, nameof(ToolWriteChangeView.Path)).GetString());
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), (await fixture.RunAsync(["tool", "install", entry.Identity.Id, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        var records = await fixture.RecordService().ReadAsync(new(fixture.Workspace, fixture.Home), Token);
        var receipt = Assert.IsType<ToolJsonUnionAddition>(Assert.Single(records.Portable!.Lock[entry.Identity.Id].Changes));
        Assert.Equal(planned, Assert.Single(receipt.Items).GetString());
        using var applied = JsonDocument.Parse(File.ReadAllBytes(fixture.ConfigFile()));
        Assert.Equal(planned, Assert.Single(applied.RootElement.GetProperty(ToolCompositionFixture.MembersPointer[1..]).EnumerateArray()).GetString());
    }

    [Fact(DisplayName = "Confirmed shared deletion removes actual record identities and then offers safe final-tool PATH cleanup")]
    public async Task ExplicitDeletionEnablesFinalPathOffer()
    {
        using var fixture = new ToolCompositionFixture { Missing = true, AllowSyntheticDownload = true };
        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), (await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        var backups = new ToolBackupService(fixture);
        var records = fixture.RecordService(backups: backups);
        var locations = new Core.Framework.Tools.Models.Services.ToolRecordsRequest(fixture.Workspace, fixture.Home);
        var before = await records.ReadAsync(locations, Token);
        var selected = Runtime("22.1.0");
        var kept = Runtime("22.2.0");
        foreach (var runtime in new[] { selected, kept })
        {
            Directory.CreateDirectory(runtime.RuntimePath);
            File.WriteAllBytes(Path.Combine(runtime.RuntimePath, "synthetic-runtime-data"), [1]);
        }
        var machine = before.Machine[ToolCompositionFixture.FirstId] with { RuntimePath = selected.RuntimePath };
        var path = new ToolPathState
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            BinPath = ToolSupplyPaths.Bin(fixture.Home),
            Changes = [new()
            {
                Kind = ToolPathChangeKind.ShellBlock, Target = Path.Combine(fixture.Home, "synthetic-shell-profile"),
                StartMarker = "synthetic-start", EndMarker = "synthetic-end",
                Prior = new() { Present = false, Kind = ToolRecordedValueKind.Bytes },
                Applied = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = [1] },
            }],
        };
        // The reader observes absences only for catalogue runtimes. These seeded runtime records are absent before the write.
        var absent = new Dictionary<string, string?>(before.ExpectedFileSha256, StringComparer.Ordinal);
        foreach (var runtime in new[] { selected, kept }) absent.TryAdd(fixture.RecordPaths.Runtime(runtime.Version), null);
        var written = await records.WriteAsync(new()
        {
            Locations = locations,
            Expected = before with { ExpectedFileSha256 = absent },
            Portable = before.Portable!,
            Machine = [machine],
            Runtimes = [selected, kept],
            Path = path,
        }, Token);
        Assert.Null(written.Failure);
        foreach (var backup in written.PendingBackups) Assert.Null((await backups.CompleteAsync(backup, Token)).Failure);
        fixture.Events.Clear();
        var run = await fixture.RunAsync(["tool", "remove", ToolCompositionFixture.FirstId], "y\n1\n2\n2\n2\n");

        Assert.Equal(ToolCompositionFixture.Exit(CliSemanticStatus.Complete), run.ExitCode);
        Assert.Contains(ToolHostEvent.PathRemove, fixture.Events);
        Assert.Equal(1, fixture.PathEffects);
        var current = await records.ReadAsync(locations, Token);
        Assert.Empty(current.Machine);
        Assert.False(File.Exists(fixture.RecordPaths.Tool(machine.ToolId)));
        Assert.False(current.Runtimes.ContainsKey(selected.Version));
        Assert.True(current.Runtimes.ContainsKey(kept.Version));
        Assert.False(File.Exists(fixture.RecordPaths.Runtime(selected.Version)));
        Assert.False(Directory.Exists(selected.RuntimePath));
        Assert.True(Directory.Exists(kept.RuntimePath));
        Assert.Empty(current.Path!.Changes);
        Assert.Empty(fixture.Backups());

        ToolNodeRuntimeRecord Runtime(string version) => new()
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            Version = version,
            RuntimePath = ToolSupplyPaths.Runtime(fixture.Home, version),
            Integrity = new() { Kind = ToolVerificationKind.SignedNodeChecksums, Source = "https://example.invalid/synthetic-runtime", Sha256 = new string('d', 64) },
        };
    }

    private static JsonElement Field(JsonElement value, string name) => ToolCompositionFixture.Field(value, name);
    private static JsonElement Tools(JsonElement root) => Field(Field(root, nameof(ToolWriteResult.Data)), nameof(ToolWriteData.Tools));
}

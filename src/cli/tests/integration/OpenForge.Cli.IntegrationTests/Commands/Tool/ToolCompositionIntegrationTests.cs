using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Status;
using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolCompositionIntegrationTests
{
    [Fact(DisplayName = "Root registers one ordered Tool branch with exact leaf identities before any host activity"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public void Registration()
    {
        using var fixture = new ToolCompositionFixture { ThrowHosts = true, InvalidHome = true };
        var inputs = new CliCompositionInputs { StandardInput = TextReader.Null, PromptOutput = TextWriter.Null, StandardInputRedirected = true, PromptOutputRedirected = true, Tools = fixture.Inputs };
        var root = CliCoreApplicationAccess.Tree(CliCompositionRoot.Create(ToolCompositionFixture.Identity, inputs));
        var group = Assert.Single(root.Root.Subcommands, command => command.Name == ToolDefinitions.Group.Name);
        Assert.True(root.IsGroup(group));
        Assert.Equal([ToolListDefinitions.CommandName, ToolInspectDefinitions.CommandName, ToolStatusDefinitions.CommandName, ToolWriteBinding.Name(ToolWriteKind.Install), ToolWriteBinding.Name(ToolWriteKind.Update), ToolWriteBinding.Name(ToolWriteKind.Remove)], group.Subcommands.Select(command => command.Name));
        Assert.All(group.Subcommands, command => Assert.Same(command, root.FindBinding(command)?.Command));
        Assert.Equal(0, fixture.ResolveCalls);
        Assert.False(Directory.Exists(fixture.Home));
    }

    [Theory(DisplayName = "Composed read leaves share catalogue facts and never publish state"), InlineData("list"), InlineData("inspect"), InlineData("status"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task ReadLeaves(string leaf)
    {
        using var fixture = new ToolCompositionFixture();
        fixture.Declare();
        var before = fixture.Snapshot();
        string[] operand = leaf == "inspect" ? [ToolCompositionFixture.FirstId] : [];
        var run = await fixture.RunAsync([ToolDefinitions.Group.Name, leaf, .. operand, "--format", "json"]);
        using var json = run.Json();
        var tools = ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolReadPayload.Tools));
        Assert.Equal(ToolCompositionFixture.FirstId, ToolCompositionFixture.Field(Assert.Single(tools.EnumerateArray()), nameof(ToolReadTool.Id)).GetString());
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
        Assert.DoesNotContain(ToolHostEvent.Uninstall, fixture.Events);
        if (leaf == "list") Assert.Equal(0, fixture.ResolveCalls);
    }

    [Theory(DisplayName = "All three composed writer previews plan without native or record effects"), InlineData("install"), InlineData("update"), InlineData("remove"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task WriterPreviews(string leaf)
    {
        using var fixture = new ToolCompositionFixture();
        fixture.Declare();
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync([ToolDefinitions.Group.Name, leaf, ToolCompositionFixture.FirstId, ToolWriteBinding.DryRunOption, "--format", "json"]);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        using var json = run.Json();
        var data = ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data));
        Assert.True(ToolCompositionFixture.Field(data, nameof(ToolWriteData.Preview)).GetBoolean());
        Assert.Single(ToolCompositionFixture.Field(data, nameof(ToolWriteData.Tools)).EnumerateArray());
        Assert.Equal(before, fixture.Snapshot());
        Assert.Empty(fixture.Backups());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
        Assert.DoesNotContain(ToolHostEvent.Uninstall, fixture.Events);
    }

    [Fact(DisplayName = "The complete writer graph shares backups across explicit grants native config and record publication"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task CompleteWriterGraph()
    {
        using var fixture = new ToolCompositionFixture();
        fixture.Declare();
        var settingsPath = Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath);
        var document = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllBytes(settingsPath))?.AsObject();
        Assert.NotNull(document);
        const string mainRoot = "synthetic-context";
        document[WorkspaceSettingsDefinitions.MainRootProperty] = mainRoot;
        File.WriteAllText(settingsPath, document.ToJsonString());
        var entry = fixture.Catalogue.Entries[ToolCompositionFixture.FirstId];
        fixture.Catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry>
            {
                [entry.Identity.Id] = entry with
                {
                    Profiles = [entry.Profiles[0] with { Setup = entry.Profiles[0].Setup with
        { Arguments = [ToolCompositionFixture.Setup, Core.Framework.Tools.Models.Recipes.ToolRecipeDefinitions.MainRootToken, entry.Identity.Id] } }]
                }
            }
        };
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId + "@" + ToolCompositionFixture.Version,
            ToolWriteBinding.YesOption, "--allow-path", ToolCompositionFixture.ConfigurationDirectory, "--format", "json"]);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.True(fixture.HasMember());
        Assert.Contains(fixture.NativeRequests, request => request.Arguments.Contains(mainRoot, StringComparer.Ordinal));
        Assert.True(fixture.NativeSawBackup);
        Assert.Empty(fixture.Backups());
        var records = await fixture.RecordService().ReadAsync(new(fixture.Workspace, fixture.Home), TestContext.Current.CancellationToken);
        var locked = records.Portable?.Lock[ToolCompositionFixture.FirstId];
        Assert.NotNull(locked);
        Assert.Equal(ToolCompositionFixture.Version, locked.Version);
        Assert.Equal(ToolCompositionFixture.ChannelId, locked.Channel);
        Assert.Single(locked.Changes);
        var settings = WorkspaceSettingsCodec.Read(File.ReadAllBytes(Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath))).Document;
        Assert.NotNull(settings);
        Assert.Contains(ToolCompositionFixture.ConfigurationDirectory, settings.AllowInstallPaths);
        Assert.False(Directory.Exists(Path.Combine(fixture.Workspace, TemporaryWorkspace.RuntimeDirectoryName)));
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "A missing synthetic tool uses the supplied download host and publishes owned verified files"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task SuppliedDownloadGraph()
    {
        using var fixture = new ToolCompositionFixture { Missing = true, AllowSyntheticDownload = true };
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.Equal(1, fixture.DownloadCalls);
        Assert.True(fixture.HasMember());
        Assert.True(fixture.NativeSawBackup);
        Assert.Empty(fixture.Backups());
        var records = await fixture.RecordService().ReadAsync(new(fixture.Workspace, fixture.Home), TestContext.Current.CancellationToken);
        var machine = records.Machine[ToolCompositionFixture.FirstId];
        Assert.True(machine.PlacedByOpenForge);
        var launcher = Assert.Single(machine.Launchers);
        Core.Framework.Tools.Shared.Records.ToolRecordPaths.Check(fixture.Home, launcher.LauncherPath);
        Core.Framework.Tools.Shared.Records.ToolRecordPaths.Check(fixture.Home, launcher.EntryPath);
        Assert.Equal(ToolCompositionFixture.DownloadBytes, File.ReadAllBytes(launcher.LauncherPath));
        Assert.Equal(ToolCompositionFixture.DownloadBytes, File.ReadAllBytes(launcher.EntryPath));
        Assert.Contains(machine.Integrity, item => item.Kind == ToolVerificationKind.PinnedSha256);
        Assert.Equal(ToolCompositionFixture.Version, records.Portable?.Lock[ToolCompositionFixture.FirstId].Version);
        Assert.Equal(0, fixture.PathEffects);
    }

    [Fact(DisplayName = "Composed bare Install restores the exact lock and named Install can reuse latest support"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task ExactRestoreAndNamedLatest()
    {
        using var fixture = new ToolCompositionFixture();
        Assert.Equal(0, (await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"])).ExitCode);
        var preview = await fixture.RunAsync(["tool", "install", ToolWriteBinding.DryRunOption, "--format", "json"]);
        Assert.Equal(0, preview.ExitCode);
        using var json = preview.Json();
        var tool = Assert.Single(ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolWriteData.Tools)).EnumerateArray());
        Assert.Equal(ToolCompositionFixture.Version, ToolCompositionFixture.Field(tool, nameof(ToolWriteToolView.Version)).GetString());
        Assert.True(ToolCompositionFixture.Field(tool, nameof(ToolWriteToolView.ReuseExisting)).GetBoolean());
        Assert.Equal(0, fixture.DownloadCalls);
    }

    [Fact(DisplayName = "Authored grants survive later minimum-version blocking without Tool lock or home publication"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task GrantsSurviveBlockedTool()
    {
        using var fixture = new ToolCompositionFixture();
        var entry = fixture.Catalogue.Entries[ToolCompositionFixture.FirstId];
        fixture.Catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry>
            { [entry.Identity.Id] = entry with { Profiles = [entry.Profiles[0] with { MinimumOpenForgeVersion = "999.0.0" }] } }
        };
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--allow-path", ToolCompositionFixture.ConfigurationDirectory, "--format", "json"]);
        Assert.Equal(5, run.ExitCode);
        var settings = WorkspaceSettingsCodec.Read(File.ReadAllBytes(Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath))).Document;
        Assert.NotNull(settings);
        Assert.Contains(ToolCompositionFixture.ConfigurationDirectory, settings.AllowInstallPaths);
        Assert.False(File.Exists(Path.Combine(fixture.Workspace, WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.False(Directory.Exists(fixture.Home));
        Assert.False(fixture.HasMember());
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
    }

    [Fact(DisplayName = "A failed native effect keeps its real pending backup and leaves project additions unstarted"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task NativeFailureKeepsBackup()
    {
        using var fixture = new ToolCompositionFixture { FailSetup = true };
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.True(run.ExitCode == 1, run.Output + run.Error);
        Assert.True(fixture.NativeSawBackup);
        Assert.NotEmpty(fixture.Backups());
        Assert.False(fixture.HasMember());
        using var json = run.Json();
        var plan = ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolWriteData.Plan));
        Assert.NotEmpty(ToolCompositionFixture.Field(plan, nameof(ToolWritePlanView.Backups)).EnumerateArray());
    }
    [Fact(DisplayName = "An owned Git workspace uses the supplied process host for real backup coverage checks"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task OwnedGitBackupCoverage()
    {
        using var fixture = new ToolCompositionFixture();
        await fixture.EnableGitAsync();
        fixture.Declare();
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.True(fixture.GitCalls > 0);
        Assert.True(fixture.HasMember());
        Assert.Empty(fixture.Backups());
    }

    [Fact(DisplayName = "Independent client file bindings reach native planning as exact files"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task ExactClientFileBinding()
    {
        using var fixture = new ToolCompositionFixture();
        fixture.ClientBindings[Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryNativeOutputs.ConfigurationRootKey("synthetic")] = fixture.NativeFile();
        var entry = fixture.Catalogue.Entries[ToolCompositionFixture.FirstId];
        var profile = entry.Profiles[0];
        fixture.Catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry>
            {
                [entry.Identity.Id] = entry with
                {
                    Profiles = [profile with { Setup = profile.Setup with { PossibleOutputs = [profile.Setup.PossibleOutputs[0] with
        { PathTemplate = Core.Framework.Tools.Catalogue.Shared.CodebaseMemory.CodebaseMemoryNativeOutputs.Configuration("synthetic") }] } }]
                }
            }
        };
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.True(run.ExitCode == 0, run.Output + run.Error);
        Assert.True(fixture.NativeSawBackup);
        Assert.False(Directory.Exists(fixture.NativeFile()));
        Assert.Empty(fixture.Backups());
    }

    [Fact(DisplayName = "The composed native inventory parser blocks malformed inventory before effects"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task InvalidNativeInventory()
    {
        using var fixture = new ToolCompositionFixture();
        var entry = fixture.Catalogue.Entries[ToolCompositionFixture.FirstId];
        var profile = entry.Profiles[0];
        fixture.Catalogue = fixture.Catalogue with
        {
            Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry>
            {
                [entry.Identity.Id] = entry with
                {
                    Profiles = [profile with { Setup = profile.Setup with { NativePlan = new()
        { Arguments = [ToolCompositionFixture.Inventory, entry.Identity.Id], Parser = Core.Framework.Tools.Models.Catalogue.ToolNativePlanParser.CbmInstallPlanV1, Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 1024 } } }]
                }
            }
        };
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.Equal(5, run.ExitCode);
        Assert.Contains(ToolHostEvent.Inventory, fixture.Events);
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Empty(fixture.Backups());
    }

    [Theory(DisplayName = "Root async factory failure and cancellation keep typed exits and create no Tool state"), InlineData(false), InlineData(true), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task FactoryFailures(bool cancel)
    {
        using var fixture = new ToolCompositionFixture { CancelPathObservation = cancel };
        if (!cancel)
        {
            fixture.Declare();
            File.WriteAllText(Path.Combine(fixture.Workspace, WorkspaceSettingsDefinitions.RelativePath), "[]");
        }
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "install", ToolCompositionFixture.FirstId, ToolWriteBinding.YesOption, "--format", "json"]);
        Assert.Equal(cancel ? 130 : 1, run.ExitCode);
        using var json = run.Json();
        Assert.Equal(JsonValueKind.Object, ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)).ValueKind);
        Assert.Equal(before, fixture.Snapshot());
        Assert.Empty(fixture.Backups());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.DoesNotContain(ToolHostEvent.Setup, fixture.Events);
    }

    [Fact(DisplayName = "Project manager hints use ordinary owned lock files without reading their format"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public void ProjectManagerHints()
    {
        using var fixture = new ToolCompositionFixture();
        var workspace = new Core.Framework.Workspace.Models.CliWorkspace(fixture.Workspace, fixture.Workspace, Core.Framework.Workspace.Models.CliWorkspaceSelectionMethod.CurrentDirectory);
        var path = Path.Combine(fixture.Workspace, global::OpenForge.Cli.Composition.Shared.ToolComposition.ToolProjectManagerReader.NpmLockfile);
        Directory.CreateDirectory(path);
        Assert.Empty(global::OpenForge.Cli.Composition.Shared.ToolComposition.ToolProjectManagerReader.Read(workspace, TestContext.Current.CancellationToken));
        Directory.Delete(path);
        File.WriteAllText(path, "synthetic presence hint");
        Assert.Equal([Core.Framework.Tools.Models.Channels.ToolNpmClient.Npm], global::OpenForge.Cli.Composition.Shared.ToolComposition.ToolProjectManagerReader.Read(workspace, TestContext.Current.CancellationToken));
        Assert.False(Directory.Exists(fixture.Home));
    }

}

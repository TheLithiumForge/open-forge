using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

internal sealed partial class ToolCompositionFixture : IDisposable
{
    internal const string FirstId = "alpha-tool";
    internal const string LastId = "zeta-tool";
    internal const string Version = "3.2.1";
    internal const string ChannelId = "synthetic-release";
    internal const string Probe = "version";
    internal const string Setup = "synthetic-setup";
    internal const string Uninstall = "synthetic-uninstall";
    internal const string Inventory = "synthetic-inventory";
    internal const string Secret = "synthetic-private-environment-value";
    internal const string ConfigurationDirectory = "config";
    internal const string MembersPointer = "/items";
    private readonly TemporaryWorkspace _owned = TemporaryWorkspace.Create("tool-composition");
    internal string Workspace { get; }
    internal string NativeHome { get; }
    internal string Home => _owned.Combine(ToolRecordDefinitions.HomeDirectoryName);
    internal ToolCatalogue Catalogue { get; set; }
    internal List<ToolHostEvent> Events { get; } = [];
    internal int ResolveCalls { get; private set; }
    internal int GitCalls { get; private set; }
    internal Dictionary<string, string> ClientBindings { get; } = new(StringComparer.Ordinal);
    internal Dictionary<string, string> EnvironmentValues { get; } = new(StringComparer.Ordinal);
    internal int DownloadCalls { get; private set; }
    internal int PathObservations { get; private set; }
    internal int PathEffects { get; private set; }
    internal bool Missing { get; set; }
    internal bool AllowSyntheticDownload { get; set; }
    internal static byte[] DownloadBytes => "inert synthetic download"u8.ToArray();
    internal bool ThrowHosts { get; set; }
    internal bool InvalidHome { get; set; }
    internal List<Core.Framework.Tools.Models.Services.ToolProcessRequest> NativeRequests { get; } = [];
    internal bool FailSetup { get; set; }
    internal bool CancelPathObservation { get; set; }
    internal bool NativeSawProjectAddition { get; private set; }
    internal bool NativeSawBackup { get; private set; }
    internal string DetectedVersion { get; set; } = Version;
    internal Core.Framework.Tools.Models.Launchers.ToolPathReadiness PathReadiness { get; set; }
        = Core.Framework.Tools.Models.Launchers.ToolPathReadiness.Ready;

    internal ToolCompositionFixture(bool twoTools = false)
    {
        Workspace = _owned.CreateDirectory("workspace");
        NativeHome = _owned.CreateDirectory("native");
        foreach (var id in new[] { FirstId, LastId })
        {
            _owned.CreateFile("native-bin/" + id, "inert synthetic command");
            _owned.CreateFile("native/" + id + ".json", "prior native bytes");
            _owned.CreateFile("workspace/" + ConfigurationDirectory + "/" + id + ".json", "{\"items\":[],\"unrelated\":17}");
        }
        Catalogue = CreateCatalogue(twoTools);
    }

    internal CliToolHostInputs Inputs => new()
    {
        HomePath = InvalidHome ? "not-an-absolute-home" : Home,
        NativeHomePath = NativeHome,
        ClientRoots = ClientBindings,
        ProcessEnvironment = EnvironmentValues,
        Processes = this,
        Downloads = this,
        Path = this,
    };
    internal string Command(string id) => _owned.Combine("native-bin", id);
    internal string NativeFile(string id = FirstId) => Path.Combine(NativeHome, id + ".json");
    internal string ConfigFile(string id = FirstId) => Path.Combine(Workspace, ConfigurationDirectory, id + ".json");
    internal ToolRecordPaths RecordPaths => new(Workspace, Home);
    internal IReadOnlyDictionary<string, string> Snapshot() => _owned.SnapshotHashes(includeRuntime: true);
    internal string[] Backups() => Directory.GetFiles(_owned.Path, "*" + ToolBackupPaths.Suffix, SearchOption.AllDirectories);
    internal static JsonElement Field(JsonElement value, string member) => value.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(member));
    internal static CliProcessIdentity Identity => new(CliSyntaxDefinitions.ExecutableName, CliBuildVersion.InformationalVersion);
    internal ToolRecordService RecordService(ToolCatalogue? catalogue = null)
        => new(catalogue ?? Catalogue, ToolHostFactory.CreateVersions(), new ToolBackupService(this));

    internal CliToolComposition Compose(CliCompositionInputs inputs)
        => CliToolComposer.Compose(CliCompositionRoot.CreateInteraction(inputs), Catalogue, inputs.Tools);

    internal async Task<ToolCompositionRun> RunAsync(string[] arguments, string? answers = null, bool fullRoot = false)
    {
        using var input = new StringReader(answers ?? string.Empty);
        using var output = new StringWriter();
        using var error = new StringWriter();
        using var prompt = new StringWriter();
        var inputs = new CliCompositionInputs
        {
            StandardInput = input,
            PromptOutput = prompt,
            StandardInputRedirected = answers is null,
            PromptOutputRedirected = answers is null,
            Tools = Inputs,
        };
        CliCoreApplication application;
        if (fullRoot) application = CliCompositionRoot.Create(Identity, inputs);
        else
        {
            var tool = Compose(inputs);
            var tree = CliCommandTree.Create(CliHelpContent.Empty, [tool.Branch],
                [tool.ListBinding, tool.InspectBinding, tool.StatusBinding, tool.InstallBinding, tool.UpdateBinding, tool.RemoveBinding]);
            application = new(Identity, tree, new CliWorkspaceSelector(new PhysicalPathResolver()));
        }
        var completion = await application.RunAsync(arguments, new(Workspace), new(output, error), TestContext.Current.CancellationToken);
        return new(completion, output.ToString(), error.ToString(), prompt.ToString());
    }

    internal void Declare(ToolCatalogue? catalogue = null)
    {
        var selected = catalogue ?? Catalogue;
        var codec = new Core.Framework.Tools.Shared.Serialization.ToolPortableCodec(ToolHostFactory.CreateVersions());
        var settings = codec.WriteSettings(new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
        }, selected.Entries.ToDictionary(pair => pair.Key, pair => new ToolSetting(pair.Value.Profiles[0].Range)));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(Workspace, WorkspaceSettingsDefinitions.RelativePath)) ?? Workspace);
        File.WriteAllText(Path.Combine(Workspace, WorkspaceSettingsDefinitions.RelativePath), settings.ToJsonString());
    }

    internal void EnsureCommands()
    {
        foreach (var entry in Catalogue.Entries.Values)
            _owned.CreateFile("native-bin/" + entry.Detection.Command, "inert synthetic command");
    }

    internal bool HasMember(string id = FirstId)
    {
        using var json = JsonDocument.Parse(File.ReadAllBytes(ConfigFile(id)));
        return json.RootElement.GetProperty(MembersPointer[1..]).EnumerateArray().Any(item => item.GetString() == id);
    }

    internal string Normalize(string value) => value
        .Replace(JsonEncodedText.Encode(_owned.Path).ToString(), "<owned>", StringComparison.Ordinal)
        .Replace(_owned.Path, "<owned>", StringComparison.Ordinal)
        .Replace(RecordPaths.WorkspaceKey, "<workspace-key>", StringComparison.Ordinal)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\\\\", "/", StringComparison.Ordinal).Replace('\\', '/');

    public void Dispose()
    {
        foreach (var child in Directory.GetDirectories(_owned.Path)) Clear(child);
        _owned.Dispose();
        void Clear(string directory)
        {
            ToolRecordPaths.Check(_owned.Path, directory);
            foreach (var file in Directory.GetFiles(directory)) { ToolRecordPaths.Check(_owned.Path, file); File.Delete(file); }
            foreach (var child in Directory.GetDirectories(directory)) Clear(child);
            Directory.Delete(directory);
        }
    }
}

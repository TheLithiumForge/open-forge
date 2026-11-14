using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using System.Text.Json;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Composition.Shared.Tools.Path;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.WindowsJourneys;

internal static class ManualToolJourneyHost
{
    internal static Task<int> RunAsync(string[] args) => RunAsync(args, null, null);

    internal static async Task<int> RunAsync(string[] args, IToolProcessHost? injectedProcesses, string? answers)
    {
        if (args.Length < 3) throw new ArgumentException("Expected owned root, tool and CLI arguments.");
        var root = Path.GetFullPath(args[0]);
        if (!root.StartsWith("D:\\ww-temp\\task90-e-final\\", StringComparison.OrdinalIgnoreCase)
            && !root.StartsWith("D:\\of-task90-e-final\\", StringComparison.OrdinalIgnoreCase)) throw new IOException("Unowned journey root.");
        var tool = args[1];
        if (tool is not ("codegraph" or "codebase-memory")) throw new ArgumentException("Unknown journey tool.");
        var location = Path.Combine(root, tool);
        var workspace = Path.Combine(location, "project");
        var nativeHome = Path.Combine(location, "account");
        var home = Path.Combine(nativeHome, ToolRecordDefinitions.HomeDirectoryName);
        var bin = Path.Combine(home, ToolRecordDefinitions.BinDirectoryName);
        var environment = CreateEnvironment(location, nativeHome, bin);
        Directory.CreateDirectory(Path.Combine(location, "logs"));
        WriteEnvironment(Path.Combine(location, "environment.json"), environment);
        var catalogue = ToolCatalogueFactory.Create(ToolHostFactory.CreateNodeRuntimes());
        var process = injectedProcesses ?? new RecordingProcesses(ToolHostFactory.CreateProcesses(environment.ToDictionary(p => p.Key, p => (string?)p.Value), catalogue), location);
        var path = new ToolPathHost(new OwnedPathEnvironment(environment["PATH"]), nativeHome, new ToolBackupService(process), static () => false);
        var provider = new ToolHomeProvider(environment, folder => folder switch
        {
            Environment.SpecialFolder.UserProfile => nativeHome,
            Environment.SpecialFolder.ApplicationData => Path.Combine(nativeHome, "AppData", "Roaming"),
            Environment.SpecialFolder.LocalApplicationData => Path.Combine(nativeHome, "AppData", "Local"),
            _ => throw new IOException("Unexpected known-folder request."),
        }, ToolOperatingSystem.Windows);
        var roots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var profile in catalogue.Entries.Values.SelectMany(e => e.Profiles))
        {
            var clients = provider.Resolve(profile).Clients;
            foreach (var pair in clients.Bindings) roots[pair.Key] = pair.Value;
            foreach (var pair in clients.Unavailable) Console.Error.WriteLine($"Unavailable native root {pair.Key}: {pair.Value}");
            if (process is ManualAccountRemovalProcesses simulation) simulation.BindUnavailable(clients.Unavailable.Keys, roots);
        }
        if (process is ManualAccountRemovalProcesses accountSimulation) accountSimulation.Configure(new()
        {
            WorkspacePath = workspace,
            HomePath = home,
            BinPath = bin,
            NativeHomePath = nativeHome,
            NativeInstallDirectory = bin,
            MainRoot = WorkspaceLayout.DefaultMainRoot,
            ClientRoots = roots,
        });
        using var promptInput = new StringReader(answers ?? string.Empty);
        var inputs = new CliCompositionInputs
        {
            StandardInput = promptInput,
            PromptOutput = Console.Error,
            StandardInputRedirected = answers is null,
            PromptOutputRedirected = answers is null,
            Tools = new()
            {
                HomePath = home,
                NativeHomePath = nativeHome,
                ClientRoots = roots,
                ProcessEnvironment = environment,
                Processes = process,
                Downloads = process as IToolDownloadHost ?? ToolHostFactory.CreateDownloads(nativeHome, environment.ToDictionary(p => p.Key, p => (string?)p.Value)),
                Path = path,
            },
        };
        Console.Error.WriteLine($"Manual host uses actual Root version {CliBuildVersion.InformationalVersion}");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(8));
        return await CliHost.RunAsync(args[2..], workspace, new CliOutputWriters(Console.Out, Console.Error), inputs, timeout.Token);
    }

    private static Dictionary<string, string> CreateEnvironment(string location, string home, string bin)
    {
        var windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var system = Environment.SystemDirectory;
        return new(StringComparer.OrdinalIgnoreCase)
        {
            ["SystemRoot"] = windows,
            ["WINDIR"] = windows,
            ["COMSPEC"] = Path.Combine(system, "cmd.exe"),
            ["PATH"] = $"{bin};C:\\Program Files\\Git\\cmd;{system};{windows}",
            ["PATHEXT"] = ".COM;.EXE;.BAT;.CMD",
            ["HOME"] = home,
            ["USERPROFILE"] = home,
            ["HOMEDRIVE"] = (Path.GetPathRoot(home) ?? throw new IOException("Missing drive root.")).TrimEnd('\\'),
            ["HOMEPATH"] = home[2..],
            ["APPDATA"] = Path.Combine(home, "AppData", "Roaming"),
            ["LOCALAPPDATA"] = Path.Combine(home, "AppData", "Local"),
            ["XDG_CONFIG_HOME"] = Path.Combine(home, ".config"),
            ["XDG_CACHE_HOME"] = Path.Combine(home, ".cache"),
            ["XDG_DATA_HOME"] = Path.Combine(home, ".local", "share"),
            ["XDG_STATE_HOME"] = Path.Combine(home, ".local", "state"),
            ["TEMP"] = Path.Combine(location, "temp"),
            ["TMP"] = Path.Combine(location, "temp"),
            ["CLAUDE_CONFIG_DIR"] = Path.Combine(home, ".claude"),
            ["CODEX_HOME"] = Path.Combine(home, ".codex"),
            ["CBM_CACHE_DIR"] = Path.Combine(location, "cache"),
            ["CBM_RUNTIME_DIR"] = Path.Combine(location, "runtime"),
            ["GIT_CONFIG_GLOBAL"] = Path.Combine(home, "gitconfig"),
            ["GIT_CONFIG_NOSYSTEM"] = "1",
            ["NPM_CONFIG_USERCONFIG"] = Path.Combine(home, "npmrc"),
            ["NPM_CONFIG_GLOBALCONFIG"] = Path.Combine(home, "global-npmrc"),
        };
    }

    private static void WriteEnvironment(string file, IReadOnlyDictionary<string, string> values)
    {
        using var stream = File.Create(file);
        using var json = new Utf8JsonWriter(stream, new() { Indented = true });
        json.WriteStartObject();
        foreach (var pair in values) json.WriteString(pair.Key, pair.Value);
        json.WriteEndObject();
    }
}

internal sealed class OwnedPathEnvironment(string processPath) : ToolPathEnvironment
{
    internal override bool IsWindows => true;
    internal override string? ProcessPath => processPath;
    internal override string? UserPath { get => null; set => throw new IOException("Persistent PATH is forbidden in the real journey."); }
}

internal sealed class RecordingProcesses(IToolProcessHost real, string location) : IToolProcessHost
{
    private int _sequence;
    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
        => real.ResolveAsync(command, workingDirectory, cancellationToken);

    public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        var cbm = Path.GetFileNameWithoutExtension(request.ExecutablePath) == "codebase-memory-mcp";
        if (cbm && request.Arguments.FirstOrDefault() == "uninstall") throw new IOException("Real account-wide CBM uninstall is forbidden.");
        if (cbm && request.Arguments.FirstOrDefault() == "install" && !request.Arguments.Contains("--skip-binary"))
            throw new IOException("CBM installation requires the native configuration-only mode.");
        var number = Interlocked.Increment(ref _sequence);
        var result = await real.RunAsync(request, cancellationToken);
        var file = Path.Combine(location, "logs", $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{number:D3}.json");
        using var stream = File.Create(file);
        using var json = new Utf8JsonWriter(stream, new() { Indented = true });
        json.WriteStartObject();
        json.WriteString("executable", request.ExecutablePath); json.WriteString("cwd", request.WorkingDirectory);
        json.WriteStartArray("arguments"); foreach (var arg in request.Arguments) json.WriteStringValue(arg); json.WriteEndArray();
        json.WriteStartObject("overlay"); foreach (var pair in request.Environment) { if (pair.Value is null) json.WriteNull(pair.Key); else json.WriteString(pair.Key, pair.Value); }
        json.WriteEndObject();
        if (result.ExitCode is { } exit) json.WriteNumber("exitCode", exit); else json.WriteNull("exitCode");
        json.WriteBoolean("timedOut", result.TimedOut); json.WriteBoolean("cancelled", result.Cancelled);
        json.WriteString("failure", result.Failure); json.WriteString("stdout", result.StandardOutput); json.WriteString("stderr", result.StandardError);
        json.WriteEndObject();
        return result;
    }
}

using System.Collections;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolHomes;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Composition.Shared.ToolComposition;

// One graph per selected execution. No workspace lease, recovery bundle or application-wide service cache.
// Contract: .agents/memory/crystallized/documents/cli/technical-designs/tool-setup.md#root-hosts
internal sealed class ToolCompositionGraph
{
    internal ToolHomeProvider Homes { get; }
    internal IToolProcessHost Processes { get; }
    internal ToolBackupService Backups { get; }
    internal ToolRecordService Records { get; }
    internal IToolChannelService Channels { get; }
    internal ToolConfigService Config { get; }
    internal IToolPathHost PathHost { get; }
    internal ToolObservationReader Observations { get; }

    internal ToolCompositionGraph(ToolCatalogue catalogue, CliToolHostInputs? inputs, Func<bool> hasLiveConsent)
    {
        var environment = inputs?.ProcessEnvironment ?? Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
            .ToDictionary(entry => (string)entry.Key, entry => entry.Value?.ToString() ?? string.Empty,
                OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var folders = new Dictionary<Environment.SpecialFolder, string>();
        string KnownFolder(Environment.SpecialFolder folder)
        {
            if (!folders.TryGetValue(folder, out var path)) folders.Add(folder, path = Environment.GetFolderPath(folder));
            return path;
        }
        Homes = new(environment, KnownFolder, OperatingSystemFact(), inputs);
        var processEnvironment = environment.ToDictionary(pair => pair.Key, pair => (string?)pair.Value,
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        Processes = inputs?.Processes ?? ToolHostFactory.CreateProcesses(processEnvironment, catalogue);
        Backups = new(Processes);
        var versions = ToolHostFactory.CreateVersions();
        Records = new(catalogue, versions, Backups);
        var downloads = inputs?.Downloads ?? ToolHostFactory.CreateDownloads(
            Homes.Resolve(catalogue.Entries.Values.SelectMany(entry => entry.Profiles).First(profile => profile.NativeHomePolicy == ToolNativeHomePolicy.Node)).NativeHomePath,
            processEnvironment);
        PathHost = inputs?.Path ?? ToolHostFactory.CreatePath(KnownFolder(Environment.SpecialFolder.UserProfile), Backups, hasLiveConsent);
        Channels = ToolHostFactory.CreateChannels(Processes, downloads, Backups, Records, processEnvironment);
        Config = new(Processes, Backups, new ToolNativePlanReader());
        Observations = new(Channels, Config, versions, PathHost, RecipeContext);
    }

    private ToolRecipeContext RecipeContext(ToolObservationRequest request, ToolVersionProfile profile, ToolDetectionResult detection)
    {
        var home = Homes.Resolve(profile);
        var installDirectory = Path.GetDirectoryName(detection.CommandPath)
            ?? throw new IOException("The detected tool has no verified installation directory.");
        return new()
        {
            WorkspacePath = request.Locations.WorkspacePath,
            HomePath = home.HomePath,
            BinPath = home.BinPath,
            NativeHomePath = home.NativeHomePath,
            NativeInstallDirectory = installDirectory,
            MainRoot = request.MainRoot,
            ClientRoots = home.Clients.Bindings,
            NativeEnvironment = home.NativeEnvironment,
        };
    }

    private static ToolOperatingSystem OperatingSystemFact()
    {
        if (OperatingSystem.IsWindows()) return ToolOperatingSystem.Windows;
        if (OperatingSystem.IsMacOS()) return ToolOperatingSystem.MacOS;
        if (OperatingSystem.IsLinux()) return ToolOperatingSystem.Linux;
        throw new PlatformNotSupportedException("The tool operating system isn't supported.");
    }
}

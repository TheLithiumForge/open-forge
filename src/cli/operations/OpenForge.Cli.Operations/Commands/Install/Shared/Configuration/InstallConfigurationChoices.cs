using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallConfigurationChoices
{
    internal const string MemoryWorkingRouteId = WorkspaceCategoryNames.Memory + "/working";
    internal const string MemoryEmergingRouteId = WorkspaceCategoryNames.Memory + "/emerging";
    internal const string MemoryCrystallizedRouteId = WorkspaceCategoryNames.Memory + "/crystallized";
    internal const string MemoryArchivedRouteId = WorkspaceCategoryNames.Memory + "/archived";

    internal static readonly ImmutableArray<string> RouteIds =
        [WorkspaceCategoryNames.Directives, WorkspaceCategoryNames.Guidance, WorkspaceCategoryNames.Maps, WorkspaceCategoryNames.Patterns,
         WorkspaceLayoutNames.SkillsDirectoryName, WorkspaceCategoryNames.Templates,
         MemoryWorkingRouteId, MemoryEmergingRouteId, MemoryCrystallizedRouteId, MemoryArchivedRouteId];

    internal static string Name(InstallPreset preset) => preset switch
    {
        InstallPreset.Essentials => "essentials",
        InstallPreset.FullCore => "full-core",
        InstallPreset.Custom => "custom",
        InstallPreset.Minimal => "minimal",
        _ => throw new ArgumentOutOfRangeException(nameof(preset)),
    };

    internal static string Name(InstallRouteAction action) => action switch
    {
        InstallRouteAction.Add => "add",
        InstallRouteAction.Remove => "remove",
        InstallRouteAction.GitIgnore => "git-ignore",
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };

    internal static InstallPreset? ReadPreset(string? text) => text switch
    {
        "essentials" => InstallPreset.Essentials,
        "full-core" => InstallPreset.FullCore,
        "custom" => InstallPreset.Custom,
        "minimal" => InstallPreset.Minimal,
        _ => null,
    };

    internal static InstallRouteAction? ReadAction(string? text) => text switch
    {
        "add" => InstallRouteAction.Add,
        "remove" => InstallRouteAction.Remove,
        "git-ignore" => InstallRouteAction.GitIgnore,
        _ => null,
    };

    internal static ImmutableArray<InstallRouteSelection> Defaults(InstallPreset preset)
        => RouteIds.Select(id => new InstallRouteSelection(id, DefaultAction(id, preset))).ToImmutableArray();

    internal static string Directory(WorkspaceLayout layout, string id) => layout.SourcePath(id);
    internal static string Entrypoint(WorkspaceLayout layout, string id)
        => $"{Directory(layout, id)}/{SourceFormClassifier.CanonicalEntrypointFileName(id.Split('/')[^1])}";
    internal static string LoaderRoot(WorkspaceLayout layout, string id) => Entrypoint(layout, id.Split('/')[0]);
    internal static bool ContainsPackagePath(string id, string path)
        => path.StartsWith($"{WorkspaceLayout.DefaultMainRoot}/{id}/", StringComparison.Ordinal);
    private static InstallRouteAction DefaultAction(string id, InstallPreset preset)
    {
        return preset switch
        {
            InstallPreset.FullCore => InstallRouteAction.Add,
            InstallPreset.Minimal => id is WorkspaceCategoryNames.Directives or MemoryCrystallizedRouteId ? InstallRouteAction.Add : InstallRouteAction.Remove,
            InstallPreset.Essentials or InstallPreset.Custom => id switch
            {
                WorkspaceCategoryNames.Guidance or WorkspaceCategoryNames.Maps or WorkspaceCategoryNames.Templates or MemoryArchivedRouteId => InstallRouteAction.Remove,
                MemoryWorkingRouteId => InstallRouteAction.GitIgnore,
                _ => InstallRouteAction.Add,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "The Install preset is not defined."),
        };
    }
}

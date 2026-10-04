using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal static class InstallConfigurationChoices
{
    internal static readonly ImmutableArray<string> RouteIds =
        ["directives", "guidance", "maps", "patterns", "skills", "templates",
         "memory/working", "memory/emerging", "memory/crystallized", "memory/archived"];

    internal static string Name(InstallPreset preset) => preset switch
    {
        InstallPreset.Essentials => "essentials",
        InstallPreset.FullCore => "full-core",
        InstallPreset.Custom => "custom",
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

    internal static string Directory(string id) => $".agents/{id}";
    internal static string Entrypoint(string id) => $"{Directory(id)}/_{id.Split('/')[^1]}.md";
    internal static bool Contains(string id, string path) => path.StartsWith($"{Directory(id)}/", StringComparison.Ordinal);

    private static InstallRouteAction DefaultAction(string id, InstallPreset preset)
    {
        return preset switch
        {
            InstallPreset.FullCore => InstallRouteAction.Add,
            InstallPreset.Essentials or InstallPreset.Custom => id switch
            {
                "guidance" or "maps" or "templates" or "memory/archived" => InstallRouteAction.Remove,
                "memory/working" => InstallRouteAction.GitIgnore,
                _ => InstallRouteAction.Add,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "The Install preset is not defined."),
        };
    }
}

using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

internal static class InstallSetupRecapSelector
{
    internal static IReadOnlyList<string> Select(InstallResult result)
    {
        var recap = result.Input.SetupRecap;
        if (recap is null && result.Input.Configuration is { Configure: false } configuration)
            recap = new(false, configuration.Preset, configuration.Routes, []);
        if (recap is null) return [];

        var lines = new List<string>();
        if (!recap.Configure)
        {
            lines.Add(InstallSetupText.Setup(PresetLabel(recap.Preset)));
            lines.Add(InstallSetupText.AddRoutes(Routes(InstallRouteAction.Add)));
            lines.Add(InstallSetupText.IgnoredRoutes(Routes(InstallRouteAction.GitIgnore)));
            lines.Add(InstallSetupText.LeftOutRoutes(Routes(InstallRouteAction.Remove)));
            return lines;
        }

        var previous = recap.PreviousRoutes.ToDictionary(row => row.Id, row => row.Action, StringComparer.Ordinal);
        var leftOut = false;
        foreach (var row in recap.Routes)
        {
            if (!previous.TryGetValue(row.Id, out var before) || before == row.Action) continue;
            lines.Add(InstallSetupText.RouteChange(row.Id, ActionLabel(before), ActionLabel(row.Action)));
            leftOut |= row.Action == InstallRouteAction.Remove;
        }
        if (leftOut) lines.Add(InstallSetupText.ExistingFilesStay());
        return lines;

        string Routes(InstallRouteAction action)
        {
            var ids = recap.Routes.Where(row => row.Action == action).Select(row => row.Id).ToArray();
            return ids.Length > 0 ? string.Join(", ", ids) : InstallSetupText.None();
        }
    }

    private static string PresetLabel(InstallPreset preset) => preset switch
    {
        InstallPreset.Essentials => InstallSetupText.EssentialsLabel(),
        InstallPreset.FullCore => InstallSetupText.FullCoreLabel(),
        InstallPreset.Custom => InstallSetupText.CustomLabel(),
        _ => throw new ArgumentOutOfRangeException(nameof(preset)),
    };

    private static string ActionLabel(InstallRouteAction action) => action switch
    {
        InstallRouteAction.Add => InstallSetupText.AddLegend(),
        InstallRouteAction.GitIgnore => InstallSetupText.IgnoredAction(),
        InstallRouteAction.Remove => InstallSetupText.RemoveLegend(),
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
}

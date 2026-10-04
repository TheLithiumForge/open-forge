using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;

internal static class InstallSetupQuestions
{
    internal static CliSelectQuestion<InstallPreset> Preset(InstallPreset initial)
    {
        CliChoice<InstallPreset>[] choices =
        [
            new(InstallPreset.Essentials, "Essentials", InstallSetupText.Essentials()),
            new(InstallPreset.FullCore, "Full Core", InstallSetupText.FullCore()),
            new(InstallPreset.Custom, "Custom", InstallSetupText.Custom()),
        ];
        if (initial == InstallPreset.Custom) choices = [choices[2], choices[0], choices[1]];
        return new(InstallSetupText.PresetQuestion(), choices);
    }

    internal static CliSelectQuestion<string> Route(InstallRouteQuestion question)
        => new(InstallSetupText.RouteQuestion(),
            new[] { new CliChoice<string>(string.Empty, "Finish selection", InstallSetupText.Retention()) }
                .Concat(question.Routes.Select(row => new CliChoice<string>(row.Id,
                    $"{row.Id} — {Label(row.Action)}",
                    question.FixedIds.Contains(row.Id, StringComparer.Ordinal) ? InstallSetupText.ExplicitOverride() : InstallSetupText.Retention())))
                .ToArray());

    internal static CliSelectQuestion<InstallRouteAction> Action(InstallRouteSelection row)
    {
        CliChoice<InstallRouteAction>[] choices =
        [
            new(InstallRouteAction.Add, "Add", InstallSetupText.Add()),
            new(InstallRouteAction.Remove, "Remove", InstallSetupText.Retention()),
            new(InstallRouteAction.GitIgnore, "Add + Git-ignore", InstallSetupText.GitIgnore()),
        ];
        return new(InstallSetupText.ActionQuestion(row.Id), choices.OrderBy(choice => choice.Value != row.Action).ToArray());
    }

    private static string Label(InstallRouteAction action) => action switch
    {
        InstallRouteAction.Add => "Add",
        InstallRouteAction.Remove => "Remove",
        InstallRouteAction.GitIgnore => "Add + Git-ignore",
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
}

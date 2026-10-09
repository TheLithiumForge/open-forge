using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;

internal static class InstallSetupQuestions
{
    private static readonly InstallRouteAction[] MarkActions = [InstallRouteAction.Add, InstallRouteAction.GitIgnore, InstallRouteAction.Remove];
    private static readonly IReadOnlyDictionary<string, Func<string>> Summaries = new Dictionary<string, Func<string>>(StringComparer.Ordinal)
    {
        ["directives"] = InstallSetupText.DirectivesSummary,
        ["guidance"] = InstallSetupText.GuidanceSummary,
        ["maps"] = InstallSetupText.MapsSummary,
        ["patterns"] = InstallSetupText.PatternsSummary,
        ["skills"] = InstallSetupText.SkillsSummary,
        ["templates"] = InstallSetupText.TemplatesSummary,
        ["memory/working"] = InstallSetupText.WorkingSummary,
        ["memory/emerging"] = InstallSetupText.EmergingSummary,
        ["memory/crystallized"] = InstallSetupText.CrystallizedSummary,
        ["memory/archived"] = InstallSetupText.ArchivedSummary,
    };

    internal static CliSelectQuestion<string> Frontmatter(InstallFrontmatterQuestion question)
    {
        CliChoice<string>[] choices =
        [
            new(Value: InstallFrontmatter.Root, Label: InstallSetupText.RootLabel(), Summary: InstallSetupText.RootExample()),
            new(Value: InstallFrontmatter.Scoped, Label: InstallSetupText.ScopedLabel(), Summary: InstallSetupText.ScopedExample()),
        ];
        if (question.InitialForm == InstallFrontmatter.Scoped) choices = [choices[1], choices[0]];
        else if (question.InitialForm != InstallFrontmatter.Root) throw new ArgumentOutOfRangeException(nameof(question));
        return new(InstallSetupText.FrontmatterQuestion(), choices);
    }

    internal static CliSelectQuestion<InstallPreset> Preset(InstallPreset initial)
    {
        CliChoice<InstallPreset>[] choices =
        [
            new(InstallPreset.Essentials, Label: "Essentials", Description: InstallSetupText.Essentials(), Summary: InstallSetupText.EssentialsSummary()),
            new(InstallPreset.FullCore, Label: "Full Core", Description: InstallSetupText.FullCore(), Summary: InstallSetupText.FullCoreSummary()),
            new(InstallPreset.Custom, Label: "Custom", Description: InstallSetupText.Custom(), Summary: InstallSetupText.CustomSummary()),
        ];
        if (initial == InstallPreset.Custom) choices = [choices[2], choices[0], choices[1]];
        return new(InstallSetupText.PresetQuestion(), choices);
    }

    internal static CliMarkedListQuestion<string> Routes(InstallRouteQuestion question)
        => new(InstallSetupText.RouteQuestion(),
            [new(Symbol: "+", Legend: InstallSetupText.AddLegend()), new(Symbol: "~", Legend: InstallSetupText.GitIgnoreLegend()), new(Symbol: "-", Legend: InstallSetupText.RemoveLegend())],
            question.Routes.Select(row => new CliMarkedRow<string>(Value: row.Id, Label: row.Id, Summary: Summaries[row.Id](), Mark: Mark(row.Action),
                Lock: question.Locks.FirstOrDefault(fixedRow => fixedRow.Id == row.Id) is { } fixedRow ? InstallSetupText.ExplicitOverride(row.Id, fixedRow.Action) : null)).ToArray(),
            (_, mark) => Details(mark, question.Installed));

    internal static async ValueTask<CliPromptReply<ImmutableArray<InstallRouteSelection>>> RoutesAsync(
        CliPrompts prompts, InstallRouteQuestion question, CliPromptPolicy policy, CancellationToken token)
    {
        var reply = await prompts.MarkedListAsync(Routes(question), policy, token).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered => CliPromptReply<ImmutableArray<InstallRouteSelection>>.Answered(
                reply.Value.Rows.Select(row => new InstallRouteSelection(row.Value, MarkActions[row.Mark])).ToImmutableArray()),
            CliPromptState.Cancelled => CliPromptReply<ImmutableArray<InstallRouteSelection>>.Cancelled(),
            CliPromptState.Unavailable => CliPromptReply<ImmutableArray<InstallRouteSelection>>.Unavailable(),
            _ => throw new ArgumentOutOfRangeException(nameof(reply)),
        };
    }

    private static int Mark(InstallRouteAction action)
    {
        var index = Array.IndexOf(MarkActions, action);
        return index >= 0 ? index : throw new ArgumentOutOfRangeException(nameof(action));
    }

    private static string? Details(int mark, bool installed) => MarkActions[mark] switch
    {
        InstallRouteAction.Add => installed ? InstallSetupText.Add() : null,
        InstallRouteAction.GitIgnore => InstallSetupText.GitIgnore(),
        InstallRouteAction.Remove => installed ? InstallSetupText.Retention() : InstallSetupText.FreshRemove(),
        _ => throw new ArgumentOutOfRangeException(nameof(mark)),
    };
}

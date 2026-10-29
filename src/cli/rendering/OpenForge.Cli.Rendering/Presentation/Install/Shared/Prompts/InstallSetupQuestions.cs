using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.OutputText.Install;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;

internal static class InstallSetupQuestions
{
    internal static string MainRootQuestion => InstallSetupText.MainRootQuestion();

    internal static string OutsideRouteQuestion => InstallSetupText.OutsideRouteQuestion();

    internal static CliTextQuestion<string> MainRoot(InstallMainRootQuestion question)
        => new($"{MainRootQuestion} {InstallSetupText.MainRootExplanation(question.Initial, InstallMainRootQuestion.SettingsDirectory)}",
            InstallSetupText.MainRootRule(), Required: false, answer =>
            {
                var value = answer.Length == 0 ? question.Initial : answer;
                return question.IsValidAnswer(value)
                    ? new(true, value, null)
                    : new(false, null, InstallSetupText.MainRootRule());
            });

    internal static CliTextQuestion<string> OutsideRoute(InstallOutsideRouteQuestion question)
        => new($"{OutsideRouteQuestion} {InstallSetupText.OutsideRouteExplanation(InstallOutsideRouteQuestion.ControlsDirectory)}",
            InstallSetupText.OutsideRouteRule(), Required: false, answer => question.IsValidAnswer(answer)
                ? new(true, answer, null)
                : new(false, null, InstallSetupText.OutsideRouteRule()));

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
            new(Value: InstallFrontmatter.Root, Label: InstallSetupText.RootLabel(), Summary: InstallSetupText.RootExample(), Description: InstallSetupText.RootDetail()),
            new(Value: InstallFrontmatter.Scoped, Label: InstallSetupText.ScopedLabel(), Summary: InstallSetupText.ScopedExample(), Description: InstallSetupText.ScopedDetail()),
        ];
        if (question.InitialForm == InstallFrontmatter.Scoped) choices = [choices[1], choices[0]];
        else if (question.InitialForm != InstallFrontmatter.Root) throw new ArgumentOutOfRangeException(nameof(question));
        return new(InstallSetupText.FrontmatterQuestion(), choices)
        {
            Context = question.Installed ? [InstallSetupText.CurrentMetadata(question.InitialForm)] : [],
        };
    }

    internal static CliSelectQuestion<InstallPreset> Preset(InstallPreset initial)
    {
        CliChoice<InstallPreset>[] choices =
        [
            new(InstallPreset.Minimal, Label: InstallSetupText.MinimalLabel(), Description: InstallSetupText.Minimal(), Summary: InstallSetupText.MinimalSummary()),
            new(InstallPreset.Essentials, Label: InstallSetupText.EssentialsLabel(), Description: InstallSetupText.Essentials(), Summary: InstallSetupText.EssentialsSummary()),
            new(InstallPreset.FullCore, Label: InstallSetupText.FullCoreLabel(), Description: InstallSetupText.FullCore(), Summary: InstallSetupText.FullCoreSummary()),
            new(InstallPreset.Custom, Label: InstallSetupText.CustomLabel(), Description: InstallSetupText.Custom(), Summary: InstallSetupText.CustomSummary()),
        ];
        if (initial == InstallPreset.Custom) choices = [choices[3], choices[0], choices[1], choices[2]];
        return new(InstallSetupText.PresetQuestion(), choices);
    }

    internal static CliMarkedListQuestion<string> Routes(InstallRouteQuestion question)
        => new(InstallSetupText.RouteQuestion(),
            [new(Symbol: "+", Legend: InstallSetupText.AddLegend()), new(Symbol: "~", Legend: InstallSetupText.GitIgnoreLegend()), new(Symbol: "-", Legend: InstallSetupText.RemoveLegend())],
            question.Routes.Select(row => new CliMarkedRow<string>(Value: row.Id, Label: row.Id, Summary: Summaries[row.Id](), Mark: Mark(row.Action),
                Lock: question.Locks.FirstOrDefault(fixedRow => fixedRow.Id == row.Id) is { } fixedRow ? InstallSetupText.ExplicitOverride(row.Id, fixedRow.Action) : null)).ToArray(),
            (id, mark) => Details(id, mark, question.Installed))
        {
            LineExplanations = question.Installed
                ? [InstallSetupText.LineGitIgnore(), InstallSetupText.LineTracked(), InstallSetupText.LineLeaveOut(), InstallSetupText.LineStopManaging()]
                : [InstallSetupText.LineGitIgnore(), InstallSetupText.LineTracked(), InstallSetupText.LineLeaveOut()],
        };

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

    private static string? Details(string id, int mark, bool installed) => MarkActions[mark] switch
    {
        InstallRouteAction.Add => installed ? InstallSetupText.Add() : null,
        InstallRouteAction.GitIgnore => InstallSetupText.GitIgnore(),
        InstallRouteAction.Remove => installed ? InstallSetupText.Retention() : InstallSetupText.FreshRemove(id),
        _ => throw new ArgumentOutOfRangeException(nameof(mark)),
    };
}

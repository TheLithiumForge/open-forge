using System.Globalization;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliPromptWording
{
    internal static string Confirm() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesYN();

    internal static string ConfirmDeleting(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesIncludingDeletingYN(count);

    internal static string ConfirmReplacing(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesIncludingReplacingYN(count);

    internal static string ConfirmReplacingAndDeleting(int replacementCount, int deletionCount)
        => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesIncludingReplacingAndDeletingYN(replacementCount, deletionCount);

    internal static string ConfirmChanges(int replacementCount, int deletionCount)
    {
        if (replacementCount > 0 && deletionCount > 0)
            return ConfirmReplacingAndDeleting(replacementCount, deletionCount);
        if (replacementCount > 0)
            return ConfirmReplacing(replacementCount);
        if (deletionCount > 0)
            return ConfirmDeleting(deletionCount);
        return Confirm();
    }

    internal static string ConfirmRemoving(int count)
        => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesIncludingRemovingYN(count);
    internal static string SelectKeys() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpDownMoveEnterChooseEscCancel());
    internal static string SelectLine(int count) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.SelectLine(count);
    internal static string MultiKeys() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSpaceToggleAAllNNoneUpDownMoveEnterContinueEscCancel());
    internal static string MultiLine() => global::OpenForge.Cli.OutputText.Shared.SharedText.HeadingChooseNumbersSeparatedBySpacesAllOrPressEnterToCancel();
    internal static string EmptySelection() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageChooseAtLeastOnePackageOrPressEscToCancel();
    internal static string Required(string label, string owners) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Required(label, owners);
    internal static string Removing(string dependency, string dependents) => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatRemovingAlsoRemoves(dependency, dependents);
    internal static string AlsoIncluded(string label, string owners) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.AlsoIncluded(label, owners);
    internal static string ConfirmationLineRule() => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.ConfirmationLineRule();
    internal static string PermissionLineRule() => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.PermissionLineRule();
    internal static string Needs(string labels) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Needs(labels);
    internal static string NeededBy(string labels) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.NeededBy(labels);
    internal static string RequiredBy(string labels) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.RequiredBy(labels);
    internal static string Installed() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInstalled();
    internal static string Permission(string owner) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Permission(owner);
    internal static string Directory(string path) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Directory(path);
    internal static string Always() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleAllowAlways();
    internal static string AlwaysReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSaveThesePathsToAgentsOpenForgeJson();
    internal static string Once() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleAllowOnce();
    internal static string OnceReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelThisRunOnly();
    internal static string Cancel() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleCancel();
    internal static string CancelReason() => global::OpenForge.Cli.OutputText.Shared.SharedText.PermissionCancelDescription();
    internal static string PermissionLine() => global::OpenForge.Cli.OutputText.Shared.SharedText.HeadingAlwaysOnceCancel();
    internal static string Position(int focus, int count) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Position(focus, count);
    internal static string MultiPosition(int focus, int count, int chosen, int required)
        => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MultiPosition(focus, count, chosen, required);
    internal static string SelectControls(int count) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.SelectControls(count);
    internal static string Legend(CliSelectionAction action, bool dependencies) => action switch
    {
        CliSelectionAction.Install => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.InstallLegend(dependencies),
        CliSelectionAction.Update => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.UpdateLegend(dependencies),
        CliSelectionAction.Remove => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.RemoveLegend(dependencies),
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };
    internal static string MarkedControls(IReadOnlyList<CliMark> marks)
        => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MarkedControls(string.Join(" ", marks.Select(mark => mark.Symbol)));
    internal static string MarkedLine(IReadOnlyList<CliMark> marks, int count)
        => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MarkedLine(
            string.Create(CultureInfo.InvariantCulture, $"{Math.Min(2, count)}{marks[0].Symbol}"),
            string.Create(CultureInfo.InvariantCulture, $"{Math.Min(7, count)}{marks[Math.Min(1, marks.Count - 1)].Symbol}"));
    internal static string MarkedRule(IReadOnlyList<CliMark> marks, int count)
    {
        var symbols = marks[0].Symbol;
        if (marks.Count > 1) symbols = global::OpenForge.Cli.OutputText.Shared.CliPromptWording.LastSymbol(
            string.Join(", ", marks.Take(marks.Count - 1).Select(mark => mark.Symbol)), marks[^1].Symbol);
        return global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MarkedRule(count, symbols,
            string.Create(CultureInfo.InvariantCulture, $"{Math.Min(2, count)}{marks[0].Symbol}"));
    }
    internal static string InstalledSummary() => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.InstalledSummary();
    internal static string MultiControls() => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.MultiControls();
    internal static string PathPage(int page, int count) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.PathPage(page, count);
    internal static string PermissionHeading() => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.PermissionHeading();
}

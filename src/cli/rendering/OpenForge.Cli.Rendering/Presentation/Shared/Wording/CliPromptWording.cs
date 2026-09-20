using System.Globalization;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliPromptWording
{
    internal static string Confirm() => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesYN();
    internal static string SelectKeys() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelUpDownMoveEnterChooseEscCancel());
    internal static string SelectLine(int count) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.SelectLine(count);
    internal static string MultiKeys() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSpaceToggleAAllNNoneUpDownMoveEnterContinueEscCancel());
    internal static string MultiLine() => global::OpenForge.Cli.OutputText.Shared.SharedText.HeadingChooseNumbersSeparatedBySpacesAllOrPressEnterToCancel();
    internal static string Legend() => ("  " + global::OpenForge.Cli.OutputText.Shared.SharedText.LabelXChosenRequiredByAChosenPackageNotChosen());
    internal static string EmptySelection() => global::OpenForge.Cli.OutputText.Shared.SharedText.MessageChooseAtLeastOnePackageOrPressEscToCancel();
    internal static string Required(string label, string owners) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.Required(label, owners);
    internal static string RemoveBoth(string label, string dependents) => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatIsNeededByRemoveBoth($"{label}", $"{dependents}");
    internal static string AlsoInstalling(string label, string owners) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.AlsoInstalling(label, owners);
    internal static string AlsoRemoving(string label, string owners) => global::OpenForge.Cli.OutputText.Shared.CliPromptWording.AlsoRemoving(label, owners);
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
    internal static string PermissionLine() => global::OpenForge.Cli.OutputText.Shared.SharedText.HeadingAlwaysOnceCancel();
}

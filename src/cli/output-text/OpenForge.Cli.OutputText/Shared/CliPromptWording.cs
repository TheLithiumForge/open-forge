using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class CliPromptWording
{
    // @OpenForgeText shared.wording.choose-a-number-1-or-press-enter-to-cancel
    internal static string SelectLine(int count) => string.Create(CultureInfo.InvariantCulture, $"Choose a number (1-{count}), or press Enter to cancel:");

    // @OpenForgeText shared.wording.is-required-by-unchoose-that-first
    internal static string Required(string label, string owners) => $"To leave {label} out, unchoose {owners} first.";

    // @OpenForgeText shared.wording.also-installing-required-by
    internal static string AlsoIncluded(string label, string owners) => $"Also included: {label}, required by {owners}.";

    // @OpenForgeText shared.wording.confirmation-line-rule
    internal static string ConfirmationLineRule() => "Type y or n.";

    // @OpenForgeText shared.wording.permission-line-rule
    internal static string PermissionLineRule() => "Type always, once or cancel.";

    // @OpenForgeText shared.wording.needs
    internal static string Needs(string labels) => $"needs: {labels}";

    // @OpenForgeText shared.wording.needed-by
    internal static string NeededBy(string labels) => $"needed by: {labels}";

    // @OpenForgeText shared.wording.required-by
    internal static string RequiredBy(string labels) => $"required by {labels}";

    // @OpenForgeText shared.wording.writes-outside-agents
    internal static string Permission(string owner) => $"Allow {owner} to change these paths outside .agents?";

    // @OpenForgeText shared.wording.directory-everything-under-it
    internal static string Directory(string path) => $"{path} (folder and everything below it)";

    // @OpenForgeText shared.wording.selection-position
    internal static string Position(int focus, int count) => string.Create(CultureInfo.InvariantCulture, $"Choice {focus + 1}/{count}");

    // @OpenForgeText shared.wording.multi-selection-position
    internal static string MultiPosition(int focus, int count, int chosen, int required)
        => string.Create(CultureInfo.InvariantCulture, $"Choice {focus + 1}/{count}  Chosen {chosen}  Required {required}");

    // @OpenForgeText shared.wording.selection-controls
    internal static string SelectControls(int count) => count <= 9
        ? string.Create(CultureInfo.InvariantCulture, $"up/down move   1-{count} choose   enter choose   esc cancel")
        : "up/down move   enter choose   esc cancel";

    // @OpenForgeText shared.wording.install-selection-legend
    internal static string InstallLegend(bool dependencies) => dependencies ? "+ install   * also installed" : "+ install";

    // @OpenForgeText shared.wording.update-selection-legend
    internal static string UpdateLegend(bool dependencies) => dependencies ? "+ update   * also updated" : "+ update";

    // @OpenForgeText shared.wording.remove-selection-legend
    internal static string RemoveLegend(bool dependencies) => dependencies ? "- remove   * also removed" : "- remove";

    // @OpenForgeText shared.wording.marked-list-controls
    internal static string MarkedControls(string symbols) => $"up/down move   {symbols} set   space next   enter done   esc cancel";

    // @OpenForgeText shared.wording.marked-list-line-instruction
    internal static string MarkedLine(string firstExample, string secondExample)
        => $"Type a row number and a mark, such as {firstExample} or {secondExample}. Press Enter on an empty line to continue.";

    // @OpenForgeText shared.wording.marked-list-invalid-edit
    internal static string MarkedRule(int count, string symbols, string example)
        => string.Create(CultureInfo.InvariantCulture, $"Use a row number from 1 to {count} followed by {symbols}, such as {example}.");

    // @OpenForgeText shared.wording.marked-list-last-symbol
    internal static string LastSymbol(string preceding, string last) => $"{preceding} or {last}";

    // @OpenForgeText shared.wording.selection-installed-summary
    internal static string InstalledSummary() => "Installed";

    // @OpenForgeText shared.wording.multi-selection-controls
    internal static string MultiControls() => "up/down move   space toggle   a all   n none   enter next   esc cancel";

    // @OpenForgeText shared.wording.permission-path-page
    internal static string PathPage(int page, int count)
        => string.Create(CultureInfo.InvariantCulture, $"Paths {page + 1}/{count}  pgup/pgdn review");

    // @OpenForgeText shared.wording.permission-heading
    internal static string PermissionHeading() => "Allow changes outside .agents?";
}

using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class ShellText
{
    // @OpenForgeText shared.input.workspace-unavailable
    internal static string WorkspaceUnavailable(string state)
        => $"The selected workspace is {state}.";

    // @OpenForgeText shared.input.option-requires-value
    internal static string OptionRequiresValue(string optionName)
        => $"{optionName} requires a value.";

    // @OpenForgeText shared.input.option-requires-nonempty-value
    internal static string OptionRequiresNonEmptyValue(string optionName)
        => $"{optionName} requires a non-empty value.";

    // @OpenForgeText shared.input.option-cannot-repeat
    internal static string OptionCannotRepeat(string optionName)
        => $"{optionName} accepts one value and cannot be repeated.";
}

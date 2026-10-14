namespace OpenForge.Cli.OutputText.Shared;

internal static class CliInputWording
{
    // @OpenForgeText shared.wording.option-needs-value
    internal static string MissingValue(string option) => $"Option {option} needs a value.";

    // @OpenForgeText shared.wording.option-value-example
    internal static string Example(string command, string option, string value) => $"For example: {command} {option} {value}";

    // @OpenForgeText shared.wording.command-help-next
    internal static string HelpNext(string command) => $"Next: {command} --help";

    // @OpenForgeText shared.wording.source-discovery-next
    internal static string SourceDiscoveryNext() => "Choose an ID or exact path from the route list.";
}

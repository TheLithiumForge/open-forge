using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Shared;

namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class CliResultHelp
{
    internal static string ResultsAndStreams()
    {
        var definitions = Enum.GetValues<CliSemanticStatus>().Select(CliStatusDefinitions.Read)
            .OrderBy(definition => definition.Disposition.ExitCode).ToArray();
        var mappings = definitions.Select(definition => SharedText.HelpAutomationMapping(
            definition.Disposition.ExitCode.ToString(CultureInfo.InvariantCulture),
            definition.MachineName.Replace('-', ' ')));
        var stdoutExits = definitions.Where(definition => definition.Disposition.HumanOutputTarget == CliOutputTarget.StandardOutput)
            .Select(definition => definition.Disposition.ExitCode.ToString(CultureInfo.InvariantCulture));
        return string.Join(Environment.NewLine,
            SharedText.HelpAutomationJson(),
            SharedText.HelpAutomationExitCodes(string.Join(", ", mappings)),
            SharedText.HelpAutomationStreams(string.Join(", ", stdoutExits)));
    }
}

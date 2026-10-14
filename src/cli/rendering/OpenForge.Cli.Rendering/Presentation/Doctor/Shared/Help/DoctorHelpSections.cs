using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Help;

internal static class DoctorHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Checks", global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpChecksDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Doctor.DoctorText.HelpRelatedCommands(),
        });
}

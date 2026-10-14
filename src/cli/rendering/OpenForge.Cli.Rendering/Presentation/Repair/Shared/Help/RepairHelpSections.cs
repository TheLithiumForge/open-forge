using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Help;

internal static class RepairHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Repair.RepairText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Repair.RepairText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Repair.RepairText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Relink coordinates", global::OpenForge.Cli.OutputText.Repair.RepairText.HelpRelinkCoordinatesDetails()),
                new CliHelpSection("Worked relink example", global::OpenForge.Cli.OutputText.Repair.RepairText.HelpWorkedRelinkExampleDetails(), preserveLines: true),
                new CliHelpSection("Other repairs", global::OpenForge.Cli.OutputText.Repair.RepairText.HelpOtherRepairsDetails()),
                new CliHelpSection("Safety", global::OpenForge.Cli.OutputText.Repair.RepairText.HelpSafetyDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Repair.RepairText.HelpRelatedCommands(),
        });
}

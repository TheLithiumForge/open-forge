using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Help;

internal static class InstallHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Install.InstallText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Install.InstallText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Install.InstallText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Force boundary", global::OpenForge.Cli.OutputText.Install.InstallText.HelpForceBoundaryDetails()),
                new CliHelpSection("Custom routes", global::OpenForge.Cli.OutputText.Install.InstallText.HelpCustomRoutesDetails()),
                new CliHelpSection("Conversion and boundaries", global::OpenForge.Cli.OutputText.Install.InstallText.HelpConversionAndBoundariesDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Install.InstallText.HelpRelatedCommands(),
        });
}

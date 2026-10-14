using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Help;

internal static class ExtensionListHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Filters and sources", global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpFiltersAndSourcesDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Extension.List.ExtensionListText.HelpRelatedCommands(),
        });
}

using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Help;

internal static class LibraryInspectHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpBehavior(),
            Details =
            [

            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.HelpRelatedCommands(),
        });
}

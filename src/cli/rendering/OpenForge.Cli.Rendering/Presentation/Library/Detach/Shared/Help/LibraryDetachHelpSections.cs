using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Help;

internal static class LibraryDetachHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Permission", global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpPermissionDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Library.Detach.LibraryDetachText.HelpRelatedCommands(),
        });
}

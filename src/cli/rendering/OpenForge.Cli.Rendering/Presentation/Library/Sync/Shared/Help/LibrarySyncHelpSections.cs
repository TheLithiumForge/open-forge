using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Help;

internal static class LibrarySyncHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Permission", global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpPermissionDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncText.HelpRelatedCommands(),
        });
}

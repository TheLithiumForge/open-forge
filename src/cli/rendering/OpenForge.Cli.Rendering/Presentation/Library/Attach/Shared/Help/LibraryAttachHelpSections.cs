using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Help;

internal static class LibraryAttachHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Links and permission", global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpLinksAndPermissionDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Library.Attach.LibraryAttachText.HelpRelatedCommands(),
        });
}

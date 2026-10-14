using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Remove;

namespace OpenForge.Cli.Core.Presentation.Remove.Shared.Help;

internal static class RemoveHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = RemoveText.HelpSyntax(),
            Examples = RemoveText.HelpExamples(),
            Behavior = RemoveText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Kinds", global::OpenForge.Cli.OutputText.Remove.RemoveText.HelpKindsDetails()),
            ],
            RelatedCommands = RemoveText.HelpRelatedCommands(),
        });
}

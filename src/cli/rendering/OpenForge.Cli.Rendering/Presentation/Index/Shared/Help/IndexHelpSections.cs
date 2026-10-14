using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Index.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.Index.Shared.Help;

internal static class IndexHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.Index.IndexText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.Index.IndexText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.Index.IndexText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Scope", global::OpenForge.Cli.OutputText.Index.IndexText.HelpScopeDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.Index.IndexText.HelpRelatedCommands(),
        });
}

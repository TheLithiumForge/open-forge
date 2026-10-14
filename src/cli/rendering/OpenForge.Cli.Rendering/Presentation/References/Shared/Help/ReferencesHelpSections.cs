using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
namespace OpenForge.Cli.Core.Presentation.References.Shared.Help;

internal static class ReferencesHelpSections
{
    internal static CliHelpContent Create()
        => CliCommandHelp.Create(new CliCommandHelpDefinition
        {
            Usage = global::OpenForge.Cli.OutputText.References.ReferencesText.HelpSyntax(),
            Examples = global::OpenForge.Cli.OutputText.References.ReferencesText.HelpExamples(),
            Behavior = global::OpenForge.Cli.OutputText.References.ReferencesText.HelpBehavior(),
            Details =
            [
                new CliHelpSection("Direction and filters", global::OpenForge.Cli.OutputText.References.ReferencesText.HelpDirectionAndFiltersDetails()),
                new CliHelpSection("Boundary", global::OpenForge.Cli.OutputText.References.ReferencesText.HelpBoundaryDetails()),
            ],
            RelatedCommands = global::OpenForge.Cli.OutputText.References.ReferencesText.HelpRelatedCommands(),
        });
}

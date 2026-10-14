using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Shared;

namespace OpenForge.Cli.Core.Presentation.Shared.Help;

// The shared global-flags help contract owns the leaf template and automation footer.
internal static class CliCommandHelp
{
    internal static CliHelpContent Create(CliCommandHelpDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return new CliHelpContent(
        [
            new(SharedText.HelpHeadingDetails(), definition.Details),
            new(SharedText.HelpHeadingAutomation(), CliResultHelp.ResultsAndStreams()),
            new(SharedText.HelpHeadingRelatedCommands(), definition.RelatedCommands),
        ])
        {
            Usage = definition.Usage,
            SummarizeGlobalOptions = true,
            Introduction =
            [
                new(SharedText.HelpHeadingExamples(), definition.Examples, preserveLines: true),
                new(SharedText.HelpHeadingBehavior(), definition.Behavior),
            ],
        };
    }
}

using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool;

internal static class ToolHelp
{
    internal static CliHelpContent Group() => Create(ToolText.GroupSyntax(), ToolText.GroupExamples(), ToolText.GroupBehavior(), leaf: null);

    internal static CliHelpContent List() => Create(ToolText.ListSyntax(), ToolText.ListExamples(), ToolText.ListBehavior(), ToolCommandDefinitions.ListName);

    internal static CliHelpContent Install()
        => Create(ToolText.InstallSyntax(), ToolText.InstallExamples(), ToolText.InstallBehavior(), ToolCommandDefinitions.InstallName);

    internal static CliHelpContent Remove()
        => Create(ToolText.RemoveSyntax(), ToolText.RemoveExamples(), ToolText.RemoveBehavior(), ToolCommandDefinitions.RemoveName);

    private static CliHelpContent Create(string usage, string examples, string behavior, string? leaf) => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = usage,
        Examples = examples,
        Behavior = behavior,
        Details = [],
        // Each Tool page lists the other leaves with their help purposes. A leaf page leaves itself out.
        RelatedCommands =
        [
            .. new (string Name, string Purpose)[]
            {
                (ToolCommandDefinitions.ListName, ToolCommandDefinitions.ListPurpose),
                (ToolCommandDefinitions.InstallName, ToolCommandDefinitions.InstallPurpose),
                (ToolCommandDefinitions.RemoveName, ToolCommandDefinitions.RemovePurpose),
            }
            .Where(item => !string.Equals(item.Name, leaf, StringComparison.Ordinal))
            .Select(item => (ToolText.RelatedLeaf(item.Name), (string?)item.Purpose)),
        ],
    });
}

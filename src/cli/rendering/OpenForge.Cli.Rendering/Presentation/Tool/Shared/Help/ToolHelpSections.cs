using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Help;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.OutputText.Tool.Shared.Help;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Help;

internal static class ToolHelpSections
{
    internal static CliHelpContent CreateGroup() => Create(ToolHelpText.GroupSyntax(), ToolHelpText.GroupExamples(), ToolHelpText.GroupBehavior(), leaf: null);
    internal static CliHelpContent CreateList() => Create(ToolHelpText.ListSyntax(), ToolHelpText.ListExamples(), ToolHelpText.ListBehavior(), ToolLeafDefinitions.ListName);
    internal static CliHelpContent CreateInspect() => Create(ToolHelpText.InspectSyntax(), ToolHelpText.InspectExamples(), ToolHelpText.InspectBehavior(), ToolLeafDefinitions.InspectName);
    internal static CliHelpContent CreateStatus() => Create(ToolHelpText.StatusSyntax(), ToolHelpText.StatusExamples(), ToolHelpText.StatusBehavior(), ToolLeafDefinitions.StatusName);

    private static CliHelpContent Create(string usage, string examples, string behavior, string? leaf) => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = usage,
        Examples = examples,
        Behavior = behavior,
        Details = [],
        RelatedCommands = Related(leaf),
    });

    // Tool pages list the six leaves with their help purposes, like the Extension and Library groups. A leaf page leaves itself out.
    internal static IReadOnlyList<(string Command, string? Description)> Related(string? leaf) =>
    [
        .. new (string Name, bool Help, string Purpose)[]
        {
            (ToolLeafDefinitions.ListName, false, ToolLeafDefinitions.ListPurpose),
            (ToolLeafDefinitions.InspectName, true, ToolLeafDefinitions.InspectPurpose),
            (ToolLeafDefinitions.StatusName, false, ToolLeafDefinitions.StatusPurpose),
            (ToolLeafDefinitions.WriteName(ToolWriteKind.Install), true, ToolLeafDefinitions.WritePurpose(ToolWriteKind.Install)),
            (ToolLeafDefinitions.WriteName(ToolWriteKind.Update), true, ToolLeafDefinitions.WritePurpose(ToolWriteKind.Update)),
            (ToolLeafDefinitions.WriteName(ToolWriteKind.Remove), true, ToolLeafDefinitions.WritePurpose(ToolWriteKind.Remove)),
        }
        .Where(item => !string.Equals(item.Name, leaf, StringComparison.Ordinal))
        .Select(item => (ToolHelpText.RelatedLeaf(item.Name, item.Help), (string?)item.Purpose)),
    ];
}

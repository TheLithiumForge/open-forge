using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool.Shared.Help;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Help;

internal static class ToolHelpSections
{
    internal static CliHelpContent CreateGroup() => Create(ToolHelpText.GroupSyntax(), ToolHelpText.GroupExamples(), ToolHelpText.GroupBehavior());
    internal static CliHelpContent CreateList() => Create(ToolHelpText.ListSyntax(), ToolHelpText.ListExamples(), ToolHelpText.ListBehavior());
    internal static CliHelpContent CreateInspect() => Create(ToolHelpText.InspectSyntax(), ToolHelpText.InspectExamples(), ToolHelpText.InspectBehavior());
    internal static CliHelpContent CreateStatus() => Create(ToolHelpText.StatusSyntax(), ToolHelpText.StatusExamples(), ToolHelpText.StatusBehavior());

    private static CliHelpContent Create(string usage, string examples, string behavior) => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = usage,
        Examples = examples,
        Behavior = behavior,
        Details = [],
        RelatedCommands = [("tool list", null), ("tool inspect <id>", null), ("tool status [id]", null), ("tool install", null), ("tool update", null), ("tool remove <id>", null)],
    });
}

using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.OutputText.Tool.Shared.Help;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Help;

internal static class ToolHelpSections
{
    internal static CliHelpContent CreateGroup() => Create(ToolHelpText.GroupSyntax(), ToolHelpText.GroupExamples(), ToolHelpText.GroupBehavior(), leaf: null);
    internal static CliHelpContent CreateList() => Create(ToolHelpText.ListSyntax(), ToolHelpText.ListExamples(), ToolHelpText.ListBehavior(), ToolListDefinitions.CommandName);
    internal static CliHelpContent CreateInspect() => Create(ToolHelpText.InspectSyntax(), ToolHelpText.InspectExamples(), ToolHelpText.InspectBehavior(), ToolInspectDefinitions.CommandName);
    internal static CliHelpContent CreateStatus() => Create(ToolHelpText.StatusSyntax(), ToolHelpText.StatusExamples(), ToolHelpText.StatusBehavior(), ToolStatusDefinitions.CommandName);

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
            (ToolListDefinitions.CommandName, false, ToolListDefinitions.Purpose),
            (ToolInspectDefinitions.CommandName, true, ToolInspectDefinitions.Purpose),
            (ToolStatusDefinitions.CommandName, false, ToolStatusDefinitions.Purpose),
            (ToolWriteBinding.Name(ToolWriteKind.Install), true, ToolWriteBinding.Description(ToolWriteKind.Install)),
            (ToolWriteBinding.Name(ToolWriteKind.Update), true, ToolWriteBinding.Description(ToolWriteKind.Update)),
            (ToolWriteBinding.Name(ToolWriteKind.Remove), true, ToolWriteBinding.Description(ToolWriteKind.Remove)),
        }
        .Where(item => !string.Equals(item.Name, leaf, StringComparison.Ordinal))
        .Select(item => (ToolHelpText.RelatedLeaf(item.Name, item.Help), (string?)item.Purpose)),
    ];
}

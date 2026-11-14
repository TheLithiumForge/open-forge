using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared;

internal static class ToolHelp
{
    internal static CliHelpContent Group() => new([new(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCommandHelp(), "  " + ToolText.CommandHelp())])
    { Usage = ToolText.GroupUsage() };
    internal static CliHelpContent List() => Read(ToolText.ListUsage(), ToolText.ListExamples());
    internal static CliHelpContent Inspect() => Read(ToolText.InspectUsage(), ToolText.InspectExamples());
    internal static CliHelpContent Status() => Read(ToolText.StatusUsage(), ToolText.StatusExamples());
    private static CliHelpContent Read(string usage, string[] examples) => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = usage,
        Examples = string.Join("\n", examples),
        Behavior = string.Join("\n", ToolText.ReadBehavior()),
        Details = [new(ToolText.SupplyHeading(), ToolText.SupplyDetails()), new(ToolText.StoreHeading(), ToolText.StoreDetails()),
            new(ToolText.WriterHeading(), ToolText.WriterDetails()), new(ToolText.ExecHeading(), ToolText.ExecDetails())],
        RelatedCommands = [("open-forge tool --help", null)],
    });
}

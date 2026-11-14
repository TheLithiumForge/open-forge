using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.OutputText.Tool.Install;

namespace OpenForge.Cli.Core.Presentation.Tool.Install;

internal static class ToolInstallPresentation
{
    internal static CliReportRendering<ToolWriteResult, ToolWriteData> Rendering => ToolWritePresentation.Rendering;
    internal static CliHelpContent CreateHelp() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = ToolInstallText.Syntax(),
        Examples = ToolInstallText.Examples(),
        Behavior = ToolInstallText.Behavior(),
        Details = [],
        RelatedCommands = [("open-forge tool inspect <id>", null), ("open-forge tool status", null)],
    });
}

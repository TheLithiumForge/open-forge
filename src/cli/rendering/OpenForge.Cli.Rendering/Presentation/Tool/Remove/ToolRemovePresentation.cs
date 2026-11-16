using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Help;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Presentation.Shared.Help;
using OpenForge.Cli.Core.Presentation.Shared.Help.Models;
using OpenForge.Cli.OutputText.Tool.Remove;

namespace OpenForge.Cli.Core.Presentation.Tool.Remove;

internal static class ToolRemovePresentation
{
    internal static CliReportRendering<ToolWriteResult, ToolWriteData> Rendering => ToolWritePresentation.Rendering;
    internal static CliHelpContent CreateHelp() => CliCommandHelp.Create(new CliCommandHelpDefinition
    {
        Usage = ToolRemoveText.Syntax(),
        Examples = ToolRemoveText.Examples(),
        Behavior = ToolRemoveText.Behavior(),
        Details = [],
        RelatedCommands = ToolHelpSections.Related(ToolWriteBinding.Name(ToolWriteKind.Remove)),
    });
}

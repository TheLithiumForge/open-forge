using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Presentation.Tool.List;
using OpenForge.Cli.Core.Presentation.Tool.Inspect;
using OpenForge.Cli.Core.Presentation.Tool.Status;
using OpenForge.Cli.Core.Presentation.Tool.Shared;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Composition;

namespace OpenForge.Cli.Composition;

internal static class CliToolComposer
{
    internal static CliToolComposition Compose(ToolReleaseSupport? support)
    {
        var group = ToolBinding.CreateGroup();
        var list = ToolListBinding.CreateSymbols(group);
        var inspect = ToolInspectBinding.CreateSymbols(group);
        var status = ToolStatusBinding.CreateSymbols(group);
        var reader = new ToolCatalogueReader(support);
        return new(new CliRootBranch(group, ToolHelp.Group()),
            CliReportBinding.Close(ToolListBinding.Create(list, ToolHelp.List(), new(reader)), ToolListPresentation.Rendering),
            CliReportBinding.Close(ToolInspectBinding.Create(inspect, ToolHelp.Inspect(), new(reader)), ToolInspectPresentation.Rendering),
            CliReportBinding.Close(ToolStatusBinding.Create(status, ToolHelp.Status(), new()), ToolStatusPresentation.Rendering));
    }
}

using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.ToolComposition;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Commands.Tool.Install;
using OpenForge.Cli.Core.Commands.Tool.Update;
using OpenForge.Cli.Core.Commands.Tool.Remove;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Presentation.Tool.List;
using OpenForge.Cli.Core.Presentation.Tool.Inspect;
using OpenForge.Cli.Core.Presentation.Tool.Status;
using OpenForge.Cli.Core.Presentation.Tool.Install;
using OpenForge.Cli.Core.Presentation.Tool.Update;
using OpenForge.Cli.Core.Presentation.Tool.Remove;
using OpenForge.Cli.Core.Presentation.Tool.Shared.Help;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Composition;

internal static class CliToolComposer
{
    internal static CliToolComposition Compose(CliInteractionComposition interaction, ToolCatalogue catalogue, CliToolHostInputs? inputs = null)
    {
        var factory = new ToolCompositionFactory(catalogue, interaction, inputs);
        var group = ToolBinding.CreateGroup();
        var list = ToolListBinding.CreateSymbols(group);
        var inspect = ToolInspectBinding.CreateSymbols(group);
        var status = ToolStatusBinding.CreateSymbols(group);
        var install = ToolInstallBinding.CreateSymbols(group);
        var update = ToolUpdateBinding.CreateSymbols(group);
        var remove = ToolRemoveBinding.CreateSymbols(group);
        return new()
        {
            Branch = new CliRootBranch(group, ToolHelpSections.CreateGroup()),
            ListBinding = CliReportBinding.Close(ToolListBinding.CreateRequestBinding(list,
                () => new(factory.CreateReadObserver()), ToolListPresentation.CreateHelp(), catalogue), ToolListPresentation.Rendering),
            InspectBinding = CliReportBinding.Close(ToolInspectBinding.CreateRequestBinding(inspect,
                () => new(factory.CreateReadObserver()), ToolInspectPresentation.CreateHelp(), catalogue), ToolInspectPresentation.Rendering),
            StatusBinding = CliReportBinding.Close(ToolStatusBinding.CreateRequestBinding(status,
                () => new(factory.CreateReadObserver()), ToolStatusPresentation.CreateHelp(), catalogue), ToolStatusPresentation.Rendering),
            InstallBinding = CliReportBinding.Close(ToolInstallBinding.CreateRequestBinding(install,
                factory.CreateInstallAsync, catalogue, factory.Versions, ToolInstallPresentation.CreateHelp()), ToolInstallPresentation.Rendering),
            UpdateBinding = CliReportBinding.Close(ToolUpdateBinding.CreateRequestBinding(update,
                factory.CreateUpdateAsync, catalogue, factory.Versions, ToolUpdatePresentation.CreateHelp()), ToolUpdatePresentation.Rendering),
            RemoveBinding = CliReportBinding.Close(ToolRemoveBinding.CreateRequestBinding(remove,
                factory.CreateRemoveAsync, catalogue, factory.Versions, ToolRemovePresentation.CreateHelp()), ToolRemovePresentation.Rendering),
            CreateWorkspaceReader = factory.CreateWorkspaceReader,
        };
    }
}

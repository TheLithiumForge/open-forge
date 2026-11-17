using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Install;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Remove;
using OpenForge.Cli.Core.Commands.Tool.Shared.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Presentation.Tool;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;

namespace OpenForge.Cli.Composition;

internal static class CliToolComposer
{
    internal static CliToolComposition Compose(CliInteractionComposition interaction, CliCompositionInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        ArgumentNullException.ThrowIfNull(inputs);
        var catalogue = ToolCatalogue.Create(inputs.ToolPlatform ?? ToolDefinitions.CurrentPlatform);
        var processes = inputs.ToolProcesses ?? new ToolProcessHost();
        var observer = new ToolObserver(catalogue, processes);
        var runner = new ToolSteps(processes);
        var files = new ToolFileApplication(inputs.LockStoreRoot);
        var group = ToolBinding.CreateGroup();
        var list = ToolBinding.CreateListSymbols(group);
        var install = ToolBinding.CreateWriteSymbols(group, ToolCommandKind.Install);
        var remove = ToolBinding.CreateWriteSymbols(group, ToolCommandKind.Remove);
        return new()
        {
            Branch = new CliRootBranch(group, ToolHelp.Group()),
            ListBinding = CliReportBinding.Close(ToolBinding.CreateListBinding(list, ToolHelp.List(),
                (request, token) => new ToolListOperation(observer).ExecuteAsync(request, token)), ToolListPresentation.Rendering),
            InstallBinding = CliReportBinding.Close(ToolBinding.CreateWriteBinding(install, catalogue, ToolHelp.Install(),
                (request, token) => new ToolInstallOperation(observer, runner, files, ToolInteractionPresentation.Create(interaction.Prompts))
                    .ExecuteAsync(request, token)), ToolWritePresentation.Rendering),
            RemoveBinding = CliReportBinding.Close(ToolBinding.CreateWriteBinding(remove, catalogue, ToolHelp.Remove(),
                (request, token) => new ToolRemoveOperation(observer, runner, files, ToolInteractionPresentation.Create(interaction.Prompts))
                    .ExecuteAsync(request, token)), ToolWritePresentation.Rendering),
        };
    }
}

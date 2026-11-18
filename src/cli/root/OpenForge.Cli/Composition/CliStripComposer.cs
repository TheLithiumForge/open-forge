using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Composition.Shared.WorkspaceFiles;
using OpenForge.Cli.Core.Commands.Export;
using OpenForge.Cli.Core.Commands.Export.Models.Binding;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Presentation.Export;
using OpenForge.Cli.Core.Presentation.Stripping;
using OpenForge.Cli.Core.Presentation.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Uninstall;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Composition;

// Composes uninstall and export, the two outputs of the shared plain-docs plan.
internal static class CliStripComposer
{
    internal static CliStripComposition Compose(CliInteractionComposition interaction, WorkspaceLockStoreRoot? lockStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var confirmation = interaction.Prompts.PlanConfirmation<StripResult, StripData, CliConfirmQuestion>(
            StripPresentation.Rendering, static question => question);
        var runner = new StripOperationRunner(lockStoreRoot, confirmation, Question);
        return new CliStripComposition
        {
            UninstallBinding = CliReportBinding.Close(UninstallBinding.CreateRequestBinding(UninstallBinding.CreateSymbols(), new UninstallBindingComponents
            {
                Help = UninstallHelp.Create(),
                Operation = new UninstallOperation(runner),
            }), StripPresentation.Rendering),
            ExportBinding = CliReportBinding.Close(ExportBinding.CreateRequestBinding(ExportBinding.CreateSymbols(), new ExportBindingComponents
            {
                Help = ExportHelp.Create(),
                Operation = new ExportOperation(WorkspaceFileList.ReadGitAsync),
            }), StripPresentation.Rendering),
        };
    }

    private static CliConfirmQuestion Question(StripPlan plan)
    {
        var deletions = plan.Entries.Count(entry => entry.Action is StripPlanAction.Delete or StripPlanAction.Move);
        return new CliConfirmQuestion(deletions > 0
            ? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesIncludingDeletingYN(deletions)
            : global::OpenForge.Cli.OutputText.Shared.SharedText.TitleApplyTheseChangesYN());
    }
}

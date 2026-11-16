using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Shared.Stripping;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Strip;
using OpenForge.Cli.Core.Commands.Strip.Models.Binding;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Commands.Uninstall.Models.Binding;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Presentation.Shared.Stripping;
using OpenForge.Cli.Core.Presentation.Shared.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Strip;
using OpenForge.Cli.Core.Presentation.Uninstall;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Composition;

internal static class CliStripComposer
{
    internal static CliStripComposition Compose(CliInteractionComposition interaction, WorkspaceLockStoreRoot? lockStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var confirmation = interaction.Prompts.PlanConfirmation<StripResult, StripData, CliConfirmQuestion>(
            StripPresentation.Rendering, static question => question);
        var runner = new StripOperationRunner(lockStoreRoot, confirmation, Question);
        var stripSymbols = StripBinding.CreateSymbols();
        var uninstallSymbols = UninstallBinding.CreateSymbols();
        return new CliStripComposition
        {
            StripBinding = CliReportBinding.Close(StripBinding.CreateRequestBinding(stripSymbols, new StripBindingComponents
            {
                Help = StripHelp.Create(),
                Runner = runner,
            }), StripPresentation.Rendering),
            UninstallBinding = CliReportBinding.Close(UninstallBinding.CreateRequestBinding(uninstallSymbols, new UninstallBindingComponents
            {
                Help = UninstallHelp.Create(),
                Operation = new UninstallOperation(runner),
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

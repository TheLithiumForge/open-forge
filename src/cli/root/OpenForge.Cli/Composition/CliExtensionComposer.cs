using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Help;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Commands.Extension.List;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Help;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Legacy.Extension.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Update;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.CommandTree;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Composition;

internal static class CliExtensionComposer
{
    internal static CliExtensionComposition Compose(
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var group = ExtensionBinding.CreateGroup();
        var listSymbols = ExtensionListBinding.CreateSymbols(group);
        var inspectSymbols = ExtensionInspectBinding.CreateSymbols(group);
        var createSymbols = ExtensionCreateBinding.CreateSymbols(group);
        var installSymbols = ExtensionInstallBinding.CreateSymbols(group);
        var updateSymbols = ExtensionUpdateBinding.CreateSymbols(group);
        var removeSymbols = ExtensionRemoveBinding.CreateSymbols(group);
        return new CliExtensionComposition
        {
            Branch = new CliRootBranch(group, ExtensionHelpSections.CreateGroup()),
            ListBinding = BuildList(listSymbols),
            InspectBinding = BuildInspect(inspectSymbols),
            CreateBinding = BuildCreate(createSymbols, interaction),
            InstallBinding = BuildInstall(
                installSymbols,
                interaction,
                lockStoreRoot),
            UpdateBinding = BuildUpdate(
                updateSymbols,
                interaction,
                lockStoreRoot),
            RemoveBinding = BuildRemove(
                removeSymbols,
                interaction,
                lockStoreRoot),
        };
    }

    private static ICliCommandBinding BuildList(ExtensionListSymbols symbols)
        => CliReportBinding.Close(ExtensionListBinding.CreateRequestBinding(
            symbols,
            new ExtensionListBindingComponents
            {
                Help = ExtensionListHelpSections.Create(),
                Operation = ExtensionListOperationFactory.Create(),
            }), ExtensionListPresentation.Rendering);

    private static ICliCommandBinding BuildInspect(ExtensionInspectSymbols symbols)
        => CliReportBinding.Close(ExtensionInspectBinding.CreateRequestBinding(
            symbols,
            new ExtensionInspectBindingComponents
            {
                Help = ExtensionInspectHelpSections.Create(),
                Operation = ExtensionInspectOperationFactory.Create(),
            }), ExtensionInspectPresentation.Rendering);

    private static ICliCommandBinding BuildCreate(
        ExtensionCreateSymbols symbols,
        CliInteractionComposition interaction)
    {
        var confirmation = interaction.Prompts.PlanConfirmation<
            ExtensionCreateResult,
            ExtensionCreateData,
            CliConfirmQuestion>(
            ExtensionCreatePresentation.Rendering,
            static question => question);
        return CliReportBinding.Close(ExtensionCreateBinding.CreateRequestBinding(
            symbols,
            new ExtensionCreateBindingComponents
            {
                Help = ExtensionCreateHelpSections.Create(),
                Operation = ExtensionCreateOperationFactory.Create(
                    new ExtensionCreateInteraction
                    {
                        Text = interaction.Prompts.TextAsync<string>,
                        ExtensionIdLabel = ExtensionCreateWording.ExtensionId(),
                        ExtensionIdRule = ExtensionCreateWording.ExtensionIdRule(),
                        InvalidExtensionId = ExtensionCreateWording.InvalidExtensionId,
                        PackageFolderLabel = ExtensionCreateWording.PackageFolder(),
                        PackageFolderRule = ExtensionCreateWording.PackageFolderRule(),
                        ConfirmationQuestion = new CliConfirmQuestion(
                            ExtensionCreateWording.ConfirmCreate()),
                        Confirm = confirmation,
                    }),
            }), ExtensionCreatePresentation.Rendering);
    }

    private static ICliCommandBinding BuildInstall(
        ExtensionInstallSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var confirmation = interaction.Prompts.PlanConfirmation<
            ExtensionInstallResult,
            ExtensionInstallData,
            CliConfirmQuestion>(
            ExtensionInstallPresentation.Rendering,
            static question => question);
        return CliReportBinding.Close(ExtensionInstallBinding.CreateRequestBinding(
            symbols,
            ExtensionInstallPresentation.CreateHelp(),
            ExtensionInstallOperationFactory.Create(
                new ExtensionInstallInteraction
                {
                    SelectPackages = interaction.Prompts.MultiSelectAsync<string>,
                    SelectionQuestion = ExtensionInstallWording.Selection(),
                    Permission = interaction.Prompts.PermissionAsync,
                    Force = confirmation,
                    ForceQuestion = static paths => new CliConfirmQuestion(
                        ExtensionInstallWording.ReplaceExisting(paths)),
                    ApplyQuestion = new CliConfirmQuestion(ExtensionInstallWording.Apply()),
                    Apply = confirmation,
                },
                lockStoreRoot)), ExtensionInstallPresentation.Rendering);
    }

    private static ICliCommandBinding BuildUpdate(
        ExtensionUpdateSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var confirmation = interaction.Prompts.PlanConfirmation<
            ExtensionUpdateResult,
            ExtensionUpdateData,
            CliConfirmQuestion>(
            ExtensionUpdatePresentation.Rendering,
            static question => question);
        return CliReportBinding.Close(ExtensionUpdateBinding.CreateRequestBinding(
            symbols,
            ExtensionUpdatePresentation.CreateHelp(),
            ExtensionUpdateOperationFactory.Create(
                new ExtensionUpdateInteraction
                {
                    SelectPackages = interaction.Prompts.MultiSelectAsync<string>,
                    SelectionQuestion = ExtensionUpdateWording.Selection(),
                    Permission = interaction.Prompts.PermissionAsync,
                    ApplyQuestion = new CliConfirmQuestion(ExtensionUpdateWording.Apply()),
                    PruneQuestion = static count => new CliConfirmQuestion(
                        ExtensionUpdateWording.Prune(count)),
                    Apply = confirmation,
                },
                lockStoreRoot)), ExtensionUpdatePresentation.Rendering);
    }

    private static ICliCommandBinding BuildRemove(
        ExtensionRemoveSymbols symbols,
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        var confirmation = interaction.Prompts.PlanConfirmation<
            ExtensionRemoveResult,
            ExtensionRemoveData,
            CliConfirmQuestion>(
            ExtensionRemovePresentation.Rendering,
            static question => question);
        return CliReportBinding.Close(ExtensionRemoveBinding.CreateRequestBinding(
            symbols,
            ExtensionRemovePresentation.CreateHelp(),
            ExtensionRemoveOperationFactory.Create(
                new ExtensionRemoveInteraction
                {
                    SelectPackages = interaction.Prompts.MultiSelectAsync<string>,
                    SelectionQuestion = ExtensionRemoveWording.Selection(),
                    Permission = interaction.Prompts.PermissionAsync,
                    DeleteQuestion = static count => new CliConfirmQuestion(
                        ExtensionRemoveWording.Delete(count)),
                    Apply = confirmation,
                },
                lockStoreRoot)), ExtensionRemovePresentation.Rendering);
    }
}

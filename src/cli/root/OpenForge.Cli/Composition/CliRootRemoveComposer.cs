using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Remove;
using OpenForge.Cli.Core.Presentation.Remove.Models;
using OpenForge.Cli.Core.Presentation.Route.Remove;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Composition.Models;

namespace OpenForge.Cli.Composition;

internal static class CliRootRemoveComposer
{
    internal static ICliCommandBinding Compose(
        CliInteractionComposition interaction,
        WorkspaceLockStoreRoot? lockStoreRoot)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var pathConfirmation = interaction.Prompts.PlanConfirmation<
            RemoveResult,
            RemoveData,
            RemoveConfirmationQuestion>(
            RemovePresentation.Rendering,
            CreatePathConfirmation);
        var pathOperation = RemovePathOperationFactory.Create(lockStoreRoot, pathConfirmation);
        var routeOperation = RouteRemoveOperationFactory.Create(
            lockStoreRoot,
            RouteRemoveSourceSelectionPrompt.Create(interaction.Prompts),
            RouteRemoveConfirmationPrompt.Create(interaction.Prompts));
        var extensionConfirmation = interaction.Prompts.PlanConfirmation<
            ExtensionRemoveResult,
            ExtensionRemoveData,
            CliConfirmQuestion>(
            ExtensionRemovePresentation.Rendering,
            static question => question);
        var extensionOperation = ExtensionRemoveOperationFactory.Create(
            new ExtensionRemoveInteraction
            {
                SelectPackages = interaction.Prompts.MultiSelectAsync<string>,
                SelectionQuestion = ExtensionRemoveWording.Selection(),
                Permission = interaction.Prompts.PermissionAsync,
                DeleteQuestion = static count => new CliConfirmQuestion(ExtensionRemoveWording.Delete(count)),
                Apply = extensionConfirmation,
            },
            lockStoreRoot);
        var libraryOperation = new LibraryDetachOperation(
            new LibraryPermissionOperation(interaction.Prompts.PermissionAsync),
            interaction.Prompts.PlanConfirmation<
                LibraryDetachResult,
                LibraryDetachData,
                LibraryDetachPlan>(
                LibraryDetachPresentation.Rendering,
                static plan => LibraryDetachInteractionPresentation.CreateConfirmationQuestion(plan)));
        return new CliRootRemoveCommandBinding(
            pathOperation,
            routeOperation,
            extensionOperation,
            libraryOperation,
            RemovePresentation.CreateHelp());
    }

    private static CliConfirmQuestion CreatePathConfirmation(RemoveConfirmationQuestion question)
    {
        if (question.IsMissing)
        {
            return new(global::OpenForge.Cli.OutputText.Remove.RemoveText.ConfirmMissing(question.Target));
        }
        if (question.FileCount == 1 && question.DirectoryCount == 0)
        {
            return new(global::OpenForge.Cli.OutputText.Remove.RemoveText.ConfirmFile());
        }
        return new(global::OpenForge.Cli.OutputText.Remove.RemoveText.ConfirmTree(question.FileCount, question.DirectoryCount));
    }
}

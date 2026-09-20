using OpenForge.Cli.Core.Commands.Extension.Create.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Create;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;

internal static class ExtensionInteractionTestFactory
{
    internal static ValueTask<CliPromptReply<CliMultiSelection<string>>> UnavailableSelection(
        CliMultiSelectQuestion<string> question,
        CliPromptPolicy policy,
        CancellationToken token)
        => ValueTask.FromResult(CliPromptReply<CliMultiSelection<string>>.Unavailable());

    internal static ValueTask<CliPromptReply<bool>> UnavailableInstallConfirmation(
        ExtensionInstallResult preview,
        CliConfirmQuestion question,
        CliPromptPolicy policy,
        CancellationToken token)
        => ValueTask.FromResult(CliPromptReply<bool>.Unavailable());

    internal static ExtensionCreateInteraction ForCreate(CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return new ExtensionCreateInteraction
        {
            Text = prompts.TextAsync<string>,
            ExtensionIdLabel = ExtensionCreateWording.ExtensionId(),
            ExtensionIdRule = ExtensionCreateWording.ExtensionIdRule(),
            InvalidExtensionId = ExtensionCreateWording.InvalidExtensionId,
            PackageFolderLabel = ExtensionCreateWording.PackageFolder(),
            PackageFolderRule = ExtensionCreateWording.PackageFolderRule(),
            ConfirmationQuestion = new CliConfirmQuestion(ExtensionCreateWording.ConfirmCreate()),
            Confirm = prompts.PlanConfirmation<ExtensionCreateResult, ExtensionCreateData, CliConfirmQuestion>(
                ExtensionCreatePresentation.Rendering,
                static question => question),
        };
    }

    internal static ExtensionInstallInteraction ForInstall(CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        var confirmation = prompts.PlanConfirmation<ExtensionInstallResult, ExtensionInstallData, CliConfirmQuestion>(
            ExtensionInstallPresentation.Rendering,
            static question => question);
        return new ExtensionInstallInteraction
        {
            SelectPackages = prompts.MultiSelectAsync<string>,
            SelectionQuestion = ExtensionInstallWording.Selection(),
            Permission = prompts.PermissionAsync,
            Force = confirmation,
            ForceQuestion = static paths => new CliConfirmQuestion(ExtensionInstallWording.ReplaceExisting(paths)),
            ApplyQuestion = new CliConfirmQuestion(ExtensionInstallWording.Apply()),
            Apply = confirmation,
        };
    }

    internal static ExtensionUpdateInteraction ForUpdate(CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return new ExtensionUpdateInteraction
        {
            SelectPackages = prompts.MultiSelectAsync<string>,
            SelectionQuestion = ExtensionUpdateWording.Selection(),
            Permission = prompts.PermissionAsync,
            ApplyQuestion = new CliConfirmQuestion(ExtensionUpdateWording.Apply()),
            PruneQuestion = static count => new CliConfirmQuestion(ExtensionUpdateWording.Prune(count)),
            Apply = prompts.PlanConfirmation<ExtensionUpdateResult, ExtensionUpdateData, CliConfirmQuestion>(
                ExtensionUpdatePresentation.Rendering,
                static question => question),
        };
    }

    internal static ExtensionRemoveInteraction ForRemove(CliPrompts prompts)
    {
        ArgumentNullException.ThrowIfNull(prompts);
        return new ExtensionRemoveInteraction
        {
            SelectPackages = prompts.MultiSelectAsync<string>,
            SelectionQuestion = ExtensionRemoveWording.Selection(),
            Permission = prompts.PermissionAsync,
            DeleteQuestion = static count => new CliConfirmQuestion(ExtensionRemoveWording.Delete(count)),
            Apply = prompts.PlanConfirmation<ExtensionRemoveResult, ExtensionRemoveData, CliConfirmQuestion>(
                ExtensionRemovePresentation.Rendering,
                static question => question),
        };
    }
}

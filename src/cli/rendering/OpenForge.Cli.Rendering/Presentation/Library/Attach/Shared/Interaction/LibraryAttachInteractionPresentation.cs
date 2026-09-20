using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Interaction;

internal static class LibraryAttachInteractionPresentation
{
    internal static CliConfirmQuestion CreateConfirmationQuestion(LibraryAttachPlan _)
        => new(CliPromptWording.Confirm());
}

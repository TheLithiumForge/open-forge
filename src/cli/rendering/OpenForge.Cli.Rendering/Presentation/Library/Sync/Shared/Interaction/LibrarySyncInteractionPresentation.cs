using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Sync.Shared.Interaction;

internal static class LibrarySyncInteractionPresentation
{
    internal static CliConfirmQuestion CreateConfirmationQuestion(LibrarySyncPlan _)
        => new(CliPromptWording.Confirm());
}

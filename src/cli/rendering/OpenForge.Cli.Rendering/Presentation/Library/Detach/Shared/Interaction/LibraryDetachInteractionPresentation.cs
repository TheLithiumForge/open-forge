using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Interaction;

internal static class LibraryDetachInteractionPresentation
{
    internal static CliConfirmQuestion CreateConfirmationQuestion(LibraryDetachPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new(LibraryDetachWording.Confirm(plan.LinkCount));
    }
}

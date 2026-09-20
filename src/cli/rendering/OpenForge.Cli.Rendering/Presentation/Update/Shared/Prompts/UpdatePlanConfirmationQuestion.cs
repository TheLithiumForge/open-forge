using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Presentation.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Prompts;

internal static class UpdatePlanConfirmationQuestion
{
    internal static CliConfirmQuestion Create(UpdateConfirmationFacts facts)
        => new(UpdateWording.Confirmation(facts));
}

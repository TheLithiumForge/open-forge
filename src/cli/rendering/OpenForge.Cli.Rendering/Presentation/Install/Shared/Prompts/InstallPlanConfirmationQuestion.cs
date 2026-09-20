using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;

internal static class InstallPlanConfirmationQuestion
{
    internal static CliConfirmQuestion Create(InstallConfirmationFacts facts)
        => new(InstallWording.Confirmation(facts));
}

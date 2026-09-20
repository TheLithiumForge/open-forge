using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

internal static class InstallInteractionTestSupport
{
    internal static CliPlanConfirmation<InstallResult, InstallConfirmationFacts> Unavailable()
        => (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Unavailable());

    internal static CliPlanConfirmation<InstallResult, InstallConfirmationFacts> Confirmation(
        bool accepted = true,
        Action<InstallResult, InstallConfirmationFacts>? observe = null)
        => (preview, facts, policy, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!policy.Allowed)
            {
                return ValueTask.FromResult(CliPromptReply<bool>.Unavailable());
            }

            observe?.Invoke(preview, facts);
            return ValueTask.FromResult(
                accepted
                    ? CliPromptReply<bool>.Answered(true)
                    : CliPromptReply<bool>.Cancelled());
        };
}

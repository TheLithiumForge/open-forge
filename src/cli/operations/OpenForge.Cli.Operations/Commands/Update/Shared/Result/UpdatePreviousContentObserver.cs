using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Result;

internal static class UpdatePreviousContentObserver
{
    internal static bool Read(UpdateRecovery recovery)
        => recovery.State == UpdateRecoveryState.Retained && recovery.ResidualPath is not null;
}

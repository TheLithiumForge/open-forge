using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Request;

internal sealed record CleanupRequest
{
    internal CleanupRequest(
        CliWorkspace workspace,
        CleanupMode mode)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (mode is not (CleanupMode.Apply or CleanupMode.DryRun))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Cleanup mode is not defined.");
        }

        Workspace = workspace;
        Mode = mode;
    }

    internal CliWorkspace Workspace { get; }

    internal CleanupMode Mode { get; }

    internal bool IsDryRun => Mode == CleanupMode.DryRun;
}

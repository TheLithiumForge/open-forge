using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Install.Models.Request;

internal enum InstallMode
{
    Apply,
    DryRun,
}

internal sealed record InstallRequest
{
    internal InstallRequest(
        CliWorkspace workspace,
        InstallMode mode,
        bool force,
        bool automatic,
        bool allowsInteractiveConfirmation)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Install mode is not defined.");
        }

        if (allowsInteractiveConfirmation
            && (mode == InstallMode.DryRun || automatic))
        {
            throw new ArgumentException(
                "Interactive confirmation is unavailable for dry-run or automatic Install requests.",
                nameof(allowsInteractiveConfirmation));
        }

        Workspace = workspace;
        Mode = mode;
        Force = force;
        Automatic = automatic;
        AllowsInteractiveConfirmation = allowsInteractiveConfirmation;
    }

    internal CliWorkspace Workspace { get; }

    internal InstallMode Mode { get; }

    internal bool Force { get; }

    internal bool Automatic { get; }

    internal bool AllowsInteractiveConfirmation { get; }

    internal bool IsDryRun => Mode == InstallMode.DryRun;
}

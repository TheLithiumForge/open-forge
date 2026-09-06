using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Update.Models.Request;

internal enum UpdateMode
{
    Apply,
    DryRun,
}

internal sealed record UpdateRequest
{
    internal UpdateRequest(
        CliWorkspace workspace,
        UpdateMode mode,
        bool force,
        bool prune,
        bool automatic,
        bool allowsInteractiveConfirmation)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Update mode is not defined.");
        }

        if (allowsInteractiveConfirmation && (mode == UpdateMode.DryRun || automatic))
        {
            throw new ArgumentException(
                "Interactive confirmation is unavailable for dry-run or automatic Update requests.",
                nameof(allowsInteractiveConfirmation));
        }

        Workspace = workspace;
        Mode = mode;
        Force = force;
        Prune = prune;
        Automatic = automatic;
        AllowsInteractiveConfirmation = allowsInteractiveConfirmation;
    }

    internal CliWorkspace Workspace { get; }

    internal UpdateMode Mode { get; }

    internal bool Force { get; }

    internal bool Prune { get; }

    internal bool Automatic { get; }

    internal bool AllowsInteractiveConfirmation { get; }

    internal bool IsDryRun => Mode == UpdateMode.DryRun;
}

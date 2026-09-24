using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Request;

internal sealed record RemovePathRequest
{
    internal RemovePathRequest(
        CliWorkspace workspace,
        string path,
        RemoveMode mode,
        bool automatic,
        bool allowInteractiveConfirmation = false,
        ImmutableArray<string> allowPaths = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Remove mode is not defined.");
        }

        Workspace = workspace;
        Path = path;
        Mode = mode;
        Automatic = automatic;
        AllowInteractiveConfirmation = allowInteractiveConfirmation;
        AllowPaths = allowPaths.IsDefault ? [] : allowPaths;
    }

    internal CliWorkspace Workspace { get; }

    internal string Path { get; }

    internal RemoveMode Mode { get; }

    internal bool Automatic { get; }

    internal bool AllowInteractiveConfirmation { get; }

    internal ImmutableArray<string> AllowPaths { get; }

    internal bool IsDryRun => Mode == RemoveMode.DryRun;
}

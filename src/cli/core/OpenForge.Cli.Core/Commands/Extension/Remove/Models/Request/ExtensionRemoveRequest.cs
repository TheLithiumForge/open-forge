using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;

internal enum ExtensionRemoveMode
{
    Apply,
    DryRun,
}

internal sealed record ExtensionRemoveRequest
{
    internal ExtensionRemoveRequest(
        CliWorkspace workspace,
        ExtensionRemoveMode mode,
        IEnumerable<string> requestedIds,
        bool prune,
        bool automatic,
        bool allowInteraction)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(requestedIds);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Extension Remove mode is not defined.");
        }

        var ids = requestedIds
            .Select(value => value ?? throw new ArgumentException(
                "Requested Extension IDs cannot contain null members.",
                nameof(requestedIds)))
            .ToArray();

        Workspace = workspace;
        Mode = mode;
        RequestedIds = new ReadOnlyCollection<string>(ids);
        Prune = prune;
        Automatic = automatic;
        AllowInteraction = allowInteraction;
    }

    internal CliWorkspace Workspace { get; }

    internal ExtensionRemoveMode Mode { get; }

    internal IReadOnlyList<string> RequestedIds { get; }

    internal bool Prune { get; }

    internal bool Automatic { get; }

    internal bool AllowInteraction { get; }

    internal bool IsDryRun => Mode == ExtensionRemoveMode.DryRun;
}

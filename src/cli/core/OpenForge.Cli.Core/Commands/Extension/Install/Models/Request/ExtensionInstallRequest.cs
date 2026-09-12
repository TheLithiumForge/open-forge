using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;

internal enum ExtensionInstallMode
{
    Apply,
    DryRun,
}

internal sealed record ExtensionInstallRequest
{
    internal ExtensionInstallRequest(
        CliWorkspace workspace,
        ExtensionInstallMode mode,
        IEnumerable<string> requestedIds,
        bool all,
        string? sourcePath,
        bool force,
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
                "The Extension Install mode is not defined.");
        }

        Workspace = workspace;
        Mode = mode;
        RequestedIds = new ReadOnlyCollection<string>(requestedIds
            .Select(value => value ?? throw new ArgumentException(
                "Requested Extension IDs cannot contain null members.",
                nameof(requestedIds)))
            .ToArray());
        All = all;
        SourcePath = sourcePath;
        Force = force;
        Automatic = automatic;
        AllowInteraction = allowInteraction;
    }

    internal CliWorkspace Workspace { get; }

    internal ExtensionInstallMode Mode { get; }

    internal IReadOnlyList<string> RequestedIds { get; }

    internal bool All { get; }

    internal string? SourcePath { get; }

    internal bool Force { get; }

    internal bool Automatic { get; }

    internal bool AllowInteraction { get; }
}

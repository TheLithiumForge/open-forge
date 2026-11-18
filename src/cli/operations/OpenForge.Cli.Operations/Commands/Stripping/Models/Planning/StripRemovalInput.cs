using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

/// <summary>The current facts the removal selection reads before the parts plan their edits.</summary>
internal sealed record StripRemovalInput
{
    public required StripRequest Request { get; init; }

    public required WorkspaceLayoutRead LayoutRead { get; init; }

    public required WorkspaceLayout Layout { get; init; }

    public required ImmutableArray<StripDocument> Documents { get; init; }

    // Reads one canonical workspace path as an exact snapshot, or null when it cannot be read safely.
    public required Func<string, CancellationToken, ValueTask<FileStateSnapshot?>> Read { get; init; }
}

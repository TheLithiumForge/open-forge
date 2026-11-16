using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

/// <summary>The current facts a removal source reads before strip plans its edits.</summary>
internal sealed record StripRemovalInput
{
    public required StripRequest Request { get; init; }

    public required WorkspaceLayoutRead LayoutRead { get; init; }

    public required WorkspaceLayout Layout { get; init; }

    public required ImmutableArray<StripDocument> Documents { get; init; }

    // Reads one canonical workspace path as an exact snapshot, or null when it cannot be read safely.
    public required Func<string, CancellationToken, ValueTask<FileStateSnapshot?>> Read { get; init; }
}

/// <summary>Adds whole-file removals to a strip plan. Uninstall supplies the only source.</summary>
internal interface IStripRemovalSource
{
    ValueTask<StripRemovalSet> ReadAsync(StripRemovalInput input, CancellationToken cancellationToken);
}

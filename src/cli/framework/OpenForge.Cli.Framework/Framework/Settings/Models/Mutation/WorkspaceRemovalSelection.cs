using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Mutation;

internal sealed record WorkspaceRemovalSelection
{
    internal ImmutableArray<string> Categories { get; init; } = [];

    internal ImmutableArray<string> Files { get; init; } = [];

    internal ImmutableArray<string> Directories { get; init; } = [];

    internal ImmutableArray<string> Extensions { get; init; } = [];

    internal ImmutableArray<string> Libraries { get; init; } = [];
}

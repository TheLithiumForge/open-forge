using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal sealed record LibraryMutationEffects
{
    public required ImmutableArray<PlannedDirectoryCreation> Directories { get; init; }
    public required ImmutableArray<RelativeFileLinkEffect> Links { get; init; }
    public required ImmutableArray<PlannedFileChange> GeneratedRegions { get; init; }
    public required PlannedFileChange? RecordChange { get; init; }
}

using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal sealed record LibraryConsumerBoundaryEvaluation(
    LibraryPlanState State,
    ImmutableArray<PlannedDirectoryCreation> Directories,
    string? Path,
    string? Cause);

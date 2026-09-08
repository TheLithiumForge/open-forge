using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Application;

internal sealed record LibrarySyncApplicationOutcome
{
    public required LibraryMutationApplication Application { get; init; }
    public required LibraryExecutionEvidence Execution { get; init; }
    public ImmutableArray<RelativeFileLinkReceipt> Links => Execution.Links;
    public ImmutableArray<FileChangeReceipt> GeneratedRegions => Execution.GeneratedRegions;
    public FileChangeReceipt? Record => Execution.Record;
    public ImmutableArray<DirectoryCreationReceipt> Directories => Execution.Directories;
}

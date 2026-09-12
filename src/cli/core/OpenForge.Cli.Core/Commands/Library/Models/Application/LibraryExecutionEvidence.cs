using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryExecutionEvidence
{
    public required LibraryPermissionApplication? Permission { get; init; }
    public required ImmutableArray<DirectoryCreationReceipt> Directories { get; init; }
    public required ImmutableArray<RelativeFileLinkReceipt> Links { get; init; }
    public required ImmutableArray<FileChangeReceipt> GeneratedRegions { get; init; }
    public required FileChangeReceipt? Record { get; init; }
    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
    public RecoveryBundlePreparationResult? RecoveryPreparationOutcome { get; init; }
    public required RecoveryBundleDeletionResult? RecoveryCleanup { get; init; }
    public required LibraryCancellationFact? Cancellation { get; init; }
    public required LibraryUnexpectedFailureFact? UnexpectedFailure { get; init; }
    public required LibrarySourceEffectScopeFacts? SourceEffectScope { get; init; }
    public required LibraryRecordPublicationOrder RecordPublicationOrder { get; init; }

    internal void Validate()
    {
        if (Directories.IsDefault || Directories.Any(receipt => receipt is null))
        {
            throw new ArgumentException("Execution evidence requires initialized directory receipts.", nameof(Directories));
        }

        if (Links.IsDefault || Links.Any(receipt => receipt is null))
        {
            throw new ArgumentException("Execution evidence requires initialized link receipts.", nameof(Links));
        }

        if (GeneratedRegions.IsDefault || GeneratedRegions.Any(receipt => receipt is null))
        {
            throw new ArgumentException("Execution evidence requires initialized generated-region receipts.", nameof(GeneratedRegions));
        }

        if (!Enum.IsDefined(RecordPublicationOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(RecordPublicationOrder), RecordPublicationOrder, "The Library record-publication order is not defined.");
        }

        SourceEffectScope?.Validate();
    }
}

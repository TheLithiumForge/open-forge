using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;

internal sealed class ExtensionUpdateEffectApplier(
    DirectoryCreationApplier directoryApplier,
    FileChangeApplier fileApplier)
{
    private readonly DirectoryCreationApplier _directoryApplier = directoryApplier;
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal ValueTask<DirectoryCreationReceipt> ApplyDirectoryAsync(
        WorkspaceLockLease lease,
        PlannedDirectoryCreation directory,
        FileExpectationValidationResult check,
        CancellationToken cancellationToken)
        => _directoryApplier.ApplyAsync(lease, directory, check, cancellationToken);

    internal ValueTask<FileChangeReceipt> ApplyFileAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidationResult check,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
        => _fileApplier.ApplyAsync(lease, change, check, preparation, cancellationToken);
}

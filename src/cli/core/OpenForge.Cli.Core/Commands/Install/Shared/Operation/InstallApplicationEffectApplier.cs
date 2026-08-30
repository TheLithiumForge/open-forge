using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Operation;

internal sealed class InstallApplicationEffectApplier(
    DirectoryCreationApplier directoryCreationApplier,
    FileChangeApplier fileChangeApplier)
{
    private readonly DirectoryCreationApplier _directoryCreationApplier = directoryCreationApplier;
    private readonly FileChangeApplier _fileChangeApplier = fileChangeApplier;

    internal ValueTask<DirectoryCreationReceipt> ApplyDirectoryAsync(
        WorkspaceLockLease lease,
        PlannedDirectoryCreation creation,
        FileExpectationValidationResult check,
        CancellationToken cancellationToken)
        => _directoryCreationApplier.ApplyAsync(
            lease,
            creation,
            check,
            cancellationToken);

    internal ValueTask<FileChangeReceipt> ApplyFileAsync(
        WorkspaceLockLease lease,
        InstallFileEffect effect,
        FileExpectationValidationResult check,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
        => _fileChangeApplier.ApplyAsync(
            lease,
            effect.Change,
            check,
            effect.Change.Kind == PlannedFileChangeKind.Create
                ? null
                : preparation,
            cancellationToken);
}

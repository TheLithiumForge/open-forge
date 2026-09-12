using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal sealed class MutationPreflight(FileExpectationValidator validator)
{
    private readonly FileExpectationValidator _validator = validator;

    internal ValueTask<MutationValidationResult> ValidateAsync(
        CliWorkspace workspace,
        IReadOnlyList<PlannedFileChange> changes,
        CancellationToken cancellationToken)
        => MutationValidationRunner.ValidateAsync(
            _validator,
            workspace,
            changes,
            cancellationToken);

    internal ValueTask<MutationValidationResult> ValidateAsync(
        CliWorkspace workspace,
        IReadOnlyList<PlannedDirectoryCreation> directoryCreations,
        IReadOnlyList<PlannedFileChange> fileChanges,
        CancellationToken cancellationToken)
        => MutationValidationRunner.ValidateAsync(
            _validator,
            workspace,
            directoryCreations,
            fileChanges,
            cancellationToken);
}

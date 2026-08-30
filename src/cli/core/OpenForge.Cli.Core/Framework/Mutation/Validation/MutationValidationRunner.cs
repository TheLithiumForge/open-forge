using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal static partial class MutationValidationRunner
{
    internal static async ValueTask<MutationValidationResult> ValidateAsync(
        FileExpectationValidator validator,
        CliWorkspace workspace,
        IReadOnlyList<PlannedFileChange> changes,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(changes);
        if (cancellationToken.IsCancellationRequested)
        {
            return MutationValidationResult.Cancelled();
        }

        if (changes.Count == 0)
        {
            return MutationValidationResult.Valid();
        }

        var pathComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var logicalPaths = new HashSet<string>(pathComparer);
        var expectedPhysicalPaths = new HashSet<string>(pathComparer);
        foreach (var change in changes)
        {
            if (change is null)
            {
                return MutationValidationResult.Blocked(
                    "Mutation changes cannot contain null members.");
            }

            if (!logicalPaths.Add(change.LogicalPath))
            {
                return MutationValidationResult.Blocked(
                    "Mutation changes cannot repeat a logical target.");
            }

            if (change.Expectation.PhysicalPath is { } physicalPath
                && !expectedPhysicalPaths.Add(physicalPath))
            {
                return MutationValidationResult.Blocked(
                    "Mutation changes cannot target one expected resolved physical path more than once.");
            }
        }

        var checks = new List<FileExpectationValidationResult>(changes.Count);
        var actualPhysicalPaths = new HashSet<string>(pathComparer);
        foreach (var change in changes)
        {
            var check = await validator
                .ValidateAsync(workspace, change.Expectation, cancellationToken)
                .ConfigureAwait(false);
            checks.Add(check);
            if (check.State != FileExpectationValidationState.Matched)
            {
                break;
            }

            if (check.PhysicalPath is { } physicalPath
                && !actualPhysicalPaths.Add(physicalPath))
            {
                return MutationValidationResult.Blocked(
                    "Mutation changes cannot resolve to one physical target more than once.",
                    checks);
            }
        }

        return MutationValidationResult.FromChecks(checks);
    }
}

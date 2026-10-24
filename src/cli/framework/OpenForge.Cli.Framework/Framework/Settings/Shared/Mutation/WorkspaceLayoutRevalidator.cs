using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

internal sealed class WorkspaceLayoutRevalidator(PhysicalPathResolver paths)
{
    private readonly FileExpectationValidator _validator = new(paths);

    internal async ValueTask<MutationValidationResult> ValidateAsync(
        WorkspaceLockLease lease,
        WorkspaceLayoutRead observation,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return MutationValidationResult.Cancelled();
        if (observation.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled)
            || observation.Layout is not { } layout)
            return MutationValidationResult.Blocked("The planned workspace layout is not ready for mutation.");
        if (!lease.IsHeldFor(layout.Workspace))
            return MutationValidationResult.Blocked("Layout revalidation requires the held physical-workspace lease.");
        if (observation.Expectations.IsDefaultOrEmpty)
            return MutationValidationResult.Blocked("The layout observation has no file expectations.");

        var checks = new List<FileExpectationValidationResult>();
        foreach (var expectation in observation.Expectations)
        {
            var check = await _validator.ValidateAsync(layout.Workspace, expectation, cancellationToken).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Matched && expectation.Kind == FileExpectationKind.Directory
                && expectation.PhysicalPath is { } directory
                && observation.OutsideRoutes.Any(route => PhysicalContainment.Contains(expectation.LogicalPath,
                    Path.Combine(layout.Workspace.LexicalRoot, route.Descriptor.DirectoryPath.Replace('/', Path.DirectorySeparatorChar)))))
            {
                try { _ = Directory.EnumerateFileSystemEntries(directory).Take(1).Any(); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    check = FileExpectationValidationResult.Failed(expectation,
                        FilesystemFailure.FromException(FilesystemFailure.ClassifyException(exception), exception));
                }
            }
            checks.Add(check);
            if (check.State != FileExpectationValidationState.Matched) break;
        }
        if (!lease.IsHeldFor(layout.Workspace))
            return MutationValidationResult.Blocked("The workspace lease was released during layout revalidation.", checks);
        return MutationValidationResult.FromChecks(checks);
    }
}

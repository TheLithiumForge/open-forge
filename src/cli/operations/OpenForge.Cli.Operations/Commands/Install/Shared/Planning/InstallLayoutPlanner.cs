using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallLayoutPlanner(PhysicalPathResolver paths)
{
    internal async ValueTask<InstallLayoutSelection> ReadAsync(InstallRequest request, CancellationToken token)
    {
        var observed = await new WorkspaceLayoutReader(paths).ReadAsync(request.Workspace, token).ConfigureAwait(false);
        if (request.RequestedMainRoot is { } requested && !WorkspaceMainRootPath.IsCanonical(requested))
            return Stop(InstallFindingCode.InvalidInput, "The requested main root must be a canonical workspace content directory.", requested);
        if (observed.Settings.State is WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete
            && observed.Ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return Stop(InstallFindingCode.LifecycleBlocked, observed.Ownership.Cause ?? "Install requires trustworthy route-sharing facts.");
        if (observed.Layout is not { } current)
            return Stop(observed.State == WorkspaceLayoutReadState.Invalid ? InstallFindingCode.InvalidInput : InstallFindingCode.LifecycleUnavailable,
                observed.Issues.FirstOrDefault()?.Cause ?? "The workspace layout is unavailable.");
        var intended = WorkspaceLayout.Create(request.Workspace, request.RequestedMainRoot ?? current.MainRoot);
        if (observed.State != WorkspaceLayoutReadState.Uninstalled && intended.MainRoot != current.MainRoot)
            return Stop(InstallFindingCode.InvalidInput, "An established installation cannot change its main root in this delivery.", intended.MainRoot);
        if (observed.State is not (WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled))
            return Stop(InstallFindingCode.LifecycleBlocked, observed.Issues.FirstOrDefault()?.Cause ?? "The workspace layout is inconsistent.");
        try
        {
            intended = intended.WithOutsideRoutes(current.OutsideRoutes.Select(route => route.DirectoryPath));
        }
        catch (ArgumentException exception)
        {
            return Stop(InstallFindingCode.InvalidInput, exception.Message, intended.MainRoot);
        }
        if (intended.IsDefault || observed.State == WorkspaceLayoutReadState.Ready)
            return new(observed, intended, null);

        var expectations = observed.Expectations.ToBuilder();
        var directory = string.Empty;
        try
        {
            foreach (var segment in intended.MainRoot.Split('/'))
            {
                token.ThrowIfCancellationRequested();
                directory = directory.Length == 0 ? segment : $"{directory}/{segment}";
                var logical = Path.Combine(request.Workspace.LexicalRoot, directory.Replace('/', Path.DirectorySeparatorChar));
                var leaf = NoFollowLeafObserver.Observe(paths, request.Workspace, logical, token);
                if (leaf.State == NoFollowLeafState.Missing)
                {
                    expectations.Add(FileExpectation.Missing(logical));
                    continue;
                }
                if (leaf.State != NoFollowLeafState.Directory)
                    return Stop(InstallFindingCode.TargetUnsafe, "A custom main root requires an ordinary directory with safe ancestry.", directory);
                var resolution = paths.ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, logical);
                if (resolution.State != PhysicalPathState.Contained)
                    return Stop(InstallFindingCode.TargetUnsafe, "The custom main root ancestry is unsafe.", directory);
                var physical = resolution.GetContainedPhysicalPath();
                // An existing folder becomes the main root by receiving the loader. Its other contents stay as they are,
                // and an occupant at a Framework path follows the same initial-occupant rules as the default root.
                expectations.Add(FileExpectation.Directory(logical, physical));
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return Stop(InstallFindingCode.LifecycleUnavailable, exception.Message, directory);
        }
        var captured = new WorkspaceLayoutRead(observed.State, observed.Settings, observed.Ownership, observed.Layout,
            observed.Issues, expectations.Distinct().ToImmutableArray(), observed.OutsideRoutes);
        return new(captured, intended, null);

        InstallLayoutSelection Stop(InstallFindingCode code, string cause, string? subject = null)
            => new(observed, null, new(code == InstallFindingCode.LifecycleUnavailable ? InstallManagementState.Incomplete : InstallManagementState.Blocked,
                code, cause, subject));
    }

}

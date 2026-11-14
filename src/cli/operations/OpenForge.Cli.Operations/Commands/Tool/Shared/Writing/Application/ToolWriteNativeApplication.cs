using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteNativeApplication(IToolProcessHost process, IToolBackupService backups, IToolSetupService setup)
{
    internal async ValueTask<ToolWriteToolPlan> RefreshAsync(ToolWriteToolPlan tool, ToolCommandTarget invocation, CancellationToken cancellationToken)
        => tool.Native is null ? tool : tool with
        {
            Native = await setup.PlanAsync((tool.DeferredNativeRemoval ?? tool.Native.Request) with
            {
                Invocation = invocation,
                InstalledInvocation = invocation,
            }, cancellationToken).ConfigureAwait(false),
            Config = tool.Config is null ? null : tool.Config with { Request = tool.Config.Request with { Invocation = invocation } },
        };

    internal async ValueTask<bool> ApplyAsync(
        ToolWriteToolPlan tool, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        if (tool.Native is not { } native) return true;
        var id = tool.Entry.Identity.Id;
        foreach (var expected in native.ExpectedFileSha256)
        {
            if (!SafePath(native.Request.Context, expected.Key)
                || !string.Equals(await HashAsync(expected.Key, cancellationToken).ConfigureAwait(false), expected.Value, StringComparison.OrdinalIgnoreCase))
                return Fail(ToolWriteFindingCode.ExpectedStateChanged, expected.Key, null);
        }
        foreach (var path in native.ExpectedFileSha256.Keys)
        {
            var backup = await backups.PrepareAsync(new(path), cancellationToken).ConfigureAwait(false);
            state.Backups.Add(new(backup, native.ExpectedFileSha256[path], false));
            if (backup.Failure is not null) return Fail(ToolWriteFindingCode.BackupFailed, path, backup.Failure);
        }
        foreach (var expected in native.ExpectedFileSha256)
        {
            if (!SafePath(native.Request.Context, expected.Key)
                || !string.Equals(await HashAsync(expected.Key, cancellationToken).ConfigureAwait(false), expected.Value, StringComparison.OrdinalIgnoreCase))
                return Fail(ToolWriteFindingCode.ExpectedStateChanged, expected.Key, null);
        }
        var result = await process.RunAsync(native.Command, cancellationToken).ConfigureAwait(false);
        state.Outcomes.Add(new(id, ToolWriteStages.Native, Success(result) ? ToolWriteStages.Completed : ToolWriteStages.Failed,
            Success(result) ? [.. native.ExpectedFileSha256.Keys] : [], Success(result) ? [] : [.. native.ExpectedFileSha256.Keys])
        {
            Output = new(result.ExitCode, result.TimedOut, result.Cancelled, result.StandardOutput, result.StandardError),
        });
        if (result.Cancelled || cancellationToken.IsCancellationRequested) throw new OperationCanceledException(cancellationToken);
        if (!Success(result)) return Fail(ToolWriteFindingCode.NativeFailed, null, result.Failure ?? result.StandardError);
        foreach (var path in native.ExpectedFileSha256.Keys)
        {
            if (!SafePath(native.Request.Context, path)) return Fail(ToolWriteFindingCode.VerificationFailed, path, null);
            var hash = await HashAsync(path, cancellationToken).ConfigureAwait(false);
            var original = state.Backups.Last(value => PhysicalIdentityTracker.PathComparer.Equals(value.Backup.TargetPath, path));
            state.Backups.Add(original with { ExpectedAppliedSha256 = hash, Verified = true });
        }
        return true;

        bool Fail(ToolWriteFindingCode code, string? path, string? cause)
        {
            state.Findings.Add(new(code, CliSemanticStatus.Failed, id, path, cause));
            state.Stopped = true;
            return false;
        }
    }

    internal async ValueTask CompleteBackupsAsync(ToolWriteExecution state, CancellationToken cancellationToken)
    {
        foreach (var group in state.Backups.GroupBy(value => value.Backup.TargetPath, PhysicalIdentityTracker.PathComparer).ToArray())
        {
            var latest = group.Last();
            var completion = new ToolBackupCompletionRequest(group.First().Backup, latest.ExpectedAppliedSha256, latest.Verified);
            var result = await backups.CompleteAsync(completion, cancellationToken).ConfigureAwait(false);
            if (result.Failure is not null || result.KeptPath is not null)
                state.Findings.Add(new(ToolWriteFindingCode.BackupFailed, CliSemanticStatus.Incomplete, Path: result.KeptPath ?? group.Key, Cause: result.Failure));
            else
                state.Backups.RemoveAll(backup => PhysicalIdentityTracker.PathComparer.Equals(backup.Backup.TargetPath, group.Key));
        }
        if (state.Findings.All(finding => finding.Code != ToolWriteFindingCode.BackupFailed)) state.Backups.Clear();
    }

    private static bool Success(ToolProcessResult result)
        => result.ExitCode == 0 && !result.Cancelled && !result.TimedOut && result.Failure is null;

    private static async Task<string?> HashAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path)) return null;
        await using var stream = File.OpenRead(path);
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
    }

    private static bool SafePath(ToolRecipeContext context, string path)
    {
        var resolver = new PhysicalPathResolver();
        foreach (var root in new[] { context.WorkspacePath, context.HomePath, context.NativeHomePath }.Concat(context.ClientRoots.Values))
        {
            if (!PhysicalContainment.Contains(root, path)) continue;
            var resolvedRoot = resolver.ResolveRoot(root);
            if (resolvedRoot.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing)) return false;
            var physical = resolvedRoot.ResolvedPhysicalPath ?? Path.GetFullPath(root);
            var resolved = resolver.ResolveCandidate(root, physical, path);
            if (resolved.State is not (PhysicalPathState.Contained or PhysicalPathState.Missing)) return false;
            if (File.Exists(path) && (File.GetAttributes(path) & (FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0) return false;
            return true;
        }
        return false;
    }
}

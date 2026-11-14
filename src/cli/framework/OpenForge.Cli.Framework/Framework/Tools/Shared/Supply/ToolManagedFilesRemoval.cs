using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal sealed class ToolManagedFilesRemoval(IToolRecordService records)
{
    internal async Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken token)
    {
        var removed = new List<string>();
        var kept = new List<string>();
        try
        {
            var fresh = await records.ReadAsync(request.Locations, token).ConfigureAwait(false);
            if (fresh.Problems.Count > 0 || fresh.MachineState != ToolRecordReadState.Available || !SameExpectations(request.Records, fresh)
                || !fresh.Machine.TryGetValue(request.Expected.ToolId, out var state)
                || state.Version != request.Expected.Version || state.ChannelId != request.Expected.ChannelId
                || !ToolSupplyPaths.Comparer.Equals(state.CommandPath, request.Expected.CommandPath))
                return new([], request.ApprovedToolPaths, "Machine records changed before shared-file removal.");
            var home = request.Locations.HomePath;
            var allowed = state.Launchers.Select(launcher => launcher.LauncherPath).ToHashSet(ToolSupplyPaths.Comparer);
            if (state.ToolPath is not null) allowed.Add(state.ToolPath);
            foreach (var path in request.ApprovedToolPaths.Distinct(ToolSupplyPaths.Comparer).OrderBy(path => Directory.Exists(path)))
            {
                token.ThrowIfCancellationRequested();
                if (!state.PlacedByOpenForge || !allowed.Contains(path) || !PhysicalContainment.Contains(home, path)
                    || ToolSupplyPaths.Comparer.Equals(home, path)) { kept.Add(path); continue; }
                ToolSupplyPaths.CheckOrdinary(path);
                if (Directory.Exists(path) && (fresh.Machine.Values.Any(tool => tool.ToolId != state.ToolId && Uses(tool, path))
                    || state.Launchers.Any(launcher => File.Exists(launcher.LauncherPath) && Uses(launcher, path))))
                { kept.Add(path); continue; }
                if (Directory.Exists(path)) DeleteTree(path, token);
                else if (File.Exists(path)) File.Delete(path);
                removed.Add(path);
            }
            foreach (var runtime in request.ApprovedRuntimes)
            {
                token.ThrowIfCancellationRequested();
                var expected = ToolSupplyPaths.Runtime(home, runtime.Version);
                if (!ToolSupplyPaths.Comparer.Equals(runtime.RuntimePath, expected)
                    || fresh.RuntimeRecordsState != ToolRecordReadState.Available
                    || !fresh.Runtimes.TryGetValue(runtime.Version, out var recorded) || recorded.Integrity.Sha256 != runtime.Integrity.Sha256
                    || recorded.Version != runtime.Version || recorded.Integrity.Kind != ToolVerificationKind.SignedNodeChecksums
                    || recorded.Integrity.Source != runtime.Integrity.Source || recorded.Integrity.VerifiedIdentity != runtime.Integrity.VerifiedIdentity
                    || !ToolSupplyPaths.Comparer.Equals(recorded.RuntimePath, expected)
                    || fresh.Machine.Values.Any(tool => tool.ToolId != state.ToolId && Uses(tool, expected))
                    || state.Launchers.Any(launcher => File.Exists(launcher.LauncherPath) && Uses(launcher, expected))
                    || File.Exists(state.CommandPath) && state.RuntimePath is { } active && PhysicalContainment.Contains(expected, active))
                { kept.Add(runtime.RuntimePath); continue; }
                ToolSupplyPaths.CheckOrdinary(expected);
                if (Directory.Exists(expected)) DeleteTree(expected, token);
                removed.Add(expected);
            }
            return new(removed, kept, null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
        {
            kept.AddRange(request.ApprovedToolPaths.Concat(request.ApprovedRuntimes.Select(runtime => runtime.RuntimePath)).Except(removed, ToolSupplyPaths.Comparer).Except(kept, ToolSupplyPaths.Comparer));
            return new(removed, kept, "Shared-file removal stopped. Check the kept paths before retrying.");
        }
    }

    private static bool SameExpectations(ToolRecordsResult expected, ToolRecordsResult actual)
        => expected.ExpectedFileSha256.Count == actual.ExpectedFileSha256.Count
            && expected.ExpectedFileSha256.All(pair => actual.ExpectedFileSha256.TryGetValue(pair.Key, out var hash) && hash == pair.Value);

    private static bool Uses(ToolMachineState tool, string runtime)
        => tool.RuntimePath is { } path && References(runtime, path) || References(runtime, tool.Invocation.ExecutablePath)
            || tool.Invocation.PrefixArguments.Any(argument => References(runtime, argument)) || tool.Launchers.Any(launcher => Uses(launcher, runtime));
    private static bool Uses(ToolLauncherSpec launcher, string runtime)
        => launcher.RuntimePath is { } path && References(runtime, path)
            || launcher.TargetKind != ToolLauncherTargetKind.NativeBinary && References(runtime, launcher.EntryPath);
    private static bool References(string root, string path) => System.IO.Path.IsPathFullyQualified(path) && PhysicalContainment.Contains(root, path);

    private static void DeleteTree(string path, CancellationToken token)
    {
        foreach (var entry in new DirectoryInfo(path).EnumerateFileSystemInfos())
        {
            token.ThrowIfCancellationRequested();
            if (entry is DirectoryInfo directory)
            {
                if (entry.LinkTarget is null && !entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) DeleteTree(directory.FullName, token);
                else directory.Delete();
            }
            else entry.Delete();
        }
        Directory.Delete(path);
    }
}

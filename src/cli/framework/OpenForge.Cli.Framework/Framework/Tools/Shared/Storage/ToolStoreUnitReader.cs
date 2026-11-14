using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal static class ToolStoreUnitReader
{
    internal static async ValueTask<ToolStoreUnitRead> ReadAsync(ToolStoreArea area, ToolStoredUnit unit, CancellationToken token)
    {
        var members = new List<ToolStoreMemberRead>();
        foreach (var file in unit.Files)
        {
            var path = unit.Path + "/" + file.Path;
            var state = ToolFileReader.Observe(area.Scope, area.At(path), token);
            if (state == NoFollowLeafState.Missing) members.Add(new(file.Path, false, ToolStoreMemberState.Missing, null));
            else if (state != NoFollowLeafState.OrdinaryFile) members.Add(new(file.Path, false, ToolStoreMemberState.Unavailable, "The store member isn't an ordinary file."));
            else
            {
                try
                {
                    var hash = await ToolArtifactIntegrity.FileHashAsync(area.At(path), token).ConfigureAwait(false);
                    var after = ToolFileReader.Observe(area.Scope, area.At(path), token);
                    var observed = ToolStoreMemberState.Unavailable;
                    if (after == NoFollowLeafState.OrdinaryFile) observed = hash == file.Sha256 ? ToolStoreMemberState.Matching : ToolStoreMemberState.Changed;
                    if (!OperatingSystem.IsWindows() && observed == ToolStoreMemberState.Matching)
                    {
                        var executable = (File.GetUnixFileMode(area.At(path)) & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
                        if (executable != file.Executable) observed = ToolStoreMemberState.Changed;
                    }
                    members.Add(new(file.Path, false, observed, null));
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                { members.Add(new(file.Path, false, ToolStoreMemberState.Unavailable, "The store member couldn't be read.")); }
            }
        }
        foreach (var directory in unit.Directories)
        {
            var state = ToolFileReader.Observe(area.Scope, area.At(unit.Path + "/" + directory), token);
            var observation = state switch
            {
                NoFollowLeafState.Directory => ToolStoreMemberState.Matching,
                NoFollowLeafState.Missing => ToolStoreMemberState.Missing,
                _ => ToolStoreMemberState.Unavailable,
            };
            members.Add(new(directory, true, observation, null));
        }
        return new(unit.Verified, [.. members]);
    }

    internal static string[] Members(ToolStoreArea area, string root, CancellationToken token)
    {
        var result = new List<string>();
        var pending = new Queue<string>();
        var leaf = ToolFileReader.Observe(area.Scope, area.At(root), token);
        if (leaf == NoFollowLeafState.Missing) return [];
        if (leaf != NoFollowLeafState.Directory) throw new IOException("The store unit isn't an ordinary directory.");
        pending.Enqueue(root);
        while (pending.TryDequeue(out var directory))
        {
            foreach (var path in Directory.EnumerateFileSystemEntries(area.At(directory)))
            {
                token.ThrowIfCancellationRequested();
                var relative = Path.GetRelativePath(area.Root, path).Replace(Path.DirectorySeparatorChar, '/');
                var state = ToolFileReader.Observe(area.Scope, path, token);
                if (state is not NoFollowLeafState.Directory and not NoFollowLeafState.OrdinaryFile)
                    throw new IOException("A store unit contains a link or unavailable member.");
                result.Add(relative[(root.Length + 1)..]);
                if (state == NoFollowLeafState.Directory) pending.Enqueue(relative);
            }
        }
        return [.. result.Order(StringComparer.Ordinal)];
    }
}

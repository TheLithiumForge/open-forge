using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Shared.Resolution;

internal static class ToolRunIntegrity
{
    internal static bool Matches(ToolLaunchBinding launch, ToolStoreRegistry store, ToolStoreRegistryDocument registry, CancellationToken token)
    {
        var units = registry.Units.Values.ToDictionary(unit => unit.Binding.Id, StringComparer.Ordinal);
        foreach (var stamp in launch.Files)
        {
            token.ThrowIfCancellationRequested();
            if (!units.TryGetValue(stamp.StoreId, out var unit)) return false;
            var area = store.Area(unit);
            var path = area.At(unit.Path + "/" + stamp.Path);
            if (ToolFileReader.Observe(area.Scope, path, token) != NoFollowLeafState.OrdinaryFile) return false;
            var file = new FileInfo(path);
            if (!file.Exists || file.Length != stamp.Bytes || file.LastWriteTimeUtc.Ticks != stamp.LastWriteTimeUtcTicks) return false;
            if (!OperatingSystem.IsWindows())
            {
                var executable = (File.GetUnixFileMode(path) & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
                if (executable != unit.Files.Single(member => member.Path == stamp.Path).Executable) return false;
            }
            if (ToolFileReader.Observe(area.Scope, path, token) != NoFollowLeafState.OrdinaryFile) return false;
        }
        return true;
    }
}

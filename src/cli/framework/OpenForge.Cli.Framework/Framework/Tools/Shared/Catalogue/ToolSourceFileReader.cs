using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Catalogue;

internal static class ToolSourceFileReader
{
    internal static async ValueTask<byte[]> ReadAsync(string path, string root, CancellationToken cancellationToken)
    {
        var physical = new PhysicalPathResolver().ResolveRoot(root);
        if (physical.State != PhysicalPathState.Contained) throw new IOException(physical.Failure?.DirectCause ?? "The tool source directory is unavailable.");
        var relative = Path.GetRelativePath(root, path);
        var current = physical.GetContainedPhysicalPath();
        foreach (var segment in relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            current = Path.Combine(current, segment);
            var component = LinkTargetReader.Read(current);
            if (component.State == PathComponentState.Link) throw new JsonException("Catalogue members cannot traverse links.");
            if (component.State != PathComponentState.Ordinary) throw new IOException(component.Failure?.DirectCause ?? "Required tool source bytes are unavailable.");
        }
        var attributes = File.GetAttributes(current);
        if ((attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            throw new JsonException("A tool source must be an ordinary file.");
        return await File.ReadAllBytesAsync(current, cancellationToken).ConfigureAwait(false);
    }
}

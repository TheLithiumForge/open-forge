using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

namespace OpenForge.Cli.Composition.Shared.Git;

internal static class GitRenameBlobBatchReader
{
    internal static ImmutableArray<RenameGitBlob> Read(byte[] output, ImmutableArray<RenameGitTrackedPath> paths)
    {
        var objects = paths.Select(path => path.Blob).Distinct(StringComparer.Ordinal).ToArray();
        var blobs = new Dictionary<string, ImmutableArray<byte>>(StringComparer.Ordinal);
        var position = 0;
        foreach (var expected in objects)
        {
            var end = Array.IndexOf(output, (byte)'\n', position);
            if (end < 0) throw new ArgumentException("Git batch output requires a complete header.", nameof(output));
            var header = Encoding.ASCII.GetString(output, position, end - position).Split(' ');
            if (header.Length != 3 || header[0] != expected || header[1] != "blob"
                || !int.TryParse(header[2], NumberStyles.None, CultureInfo.InvariantCulture, out var size)
                || size < 0 || size > output.Length - end - 2)
            {
                throw new ArgumentException("Git batch output requires the requested blob and its exact size.", nameof(output));
            }

            var start = end + 1;
            if (output[start + size] != '\n') throw new ArgumentException("Git batch output requires an exact blob boundary.", nameof(output));
            blobs.Add(expected, ImmutableArray.Create(output.AsSpan(start, size)));
            position = start + size + 1;
        }

        if (position != output.Length) throw new ArgumentException("Git batch output contains unexpected records.", nameof(output));
        return paths.Select(path => new RenameGitBlob(Path: path.Path, Blob: path.Blob, Bytes: blobs[path.Blob])).ToImmutableArray();
    }
}

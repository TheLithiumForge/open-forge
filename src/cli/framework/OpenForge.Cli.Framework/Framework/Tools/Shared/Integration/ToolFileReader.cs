using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Integration;

/// <summary>A workspace file's observed bytes and expectation, or the plain reason it can't be edited safely.</summary>
internal sealed record ToolFileRead(string RelativePath, FileStateSnapshot? Snapshot, string? Cause)
{
    internal byte[]? Bytes => Snapshot is { HasBytes: true } snapshot ? [.. snapshot.Bytes] : null;
}

internal static class ToolFileReader
{
    internal static async ValueTask<ToolFileRead> ReadAsync(CliWorkspace workspace, string relativePath, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var native = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var logical = Path.Combine(workspace.LexicalRoot, native);
        var resolver = new PhysicalPathResolver();
        var leaf = NoFollowLeafObserver.Observe(resolver, workspace, logical, cancellationToken);
        if (leaf.State == NoFollowLeafState.Missing) return new(relativePath, FileStateSnapshot.Missing(logical), null);
        if (leaf.State != NoFollowLeafState.OrdinaryFile) return new(relativePath, null, $"{relativePath} must be an ordinary file.");
        try
        {
            var physical = Path.Combine(workspace.PhysicalRoot, native);
            var bytes = await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false);
            return new(relativePath, FileStateSnapshot.File(logical, physical, bytes), null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(relativePath, null, $"{relativePath} couldn't be read.");
        }
    }

    /// <summary>The planned change from the observed state to the intended bytes, where null deletes the file.</summary>
    internal static PlannedFileChange? Plan(FileStateSnapshot before, byte[]? intended)
    {
        ArgumentNullException.ThrowIfNull(before);
        if (intended is null)
            return before.Kind == FileExpectationKind.Missing ? null : PlannedFileChange.Delete(before.Expectation);
        if (before.Kind == FileExpectationKind.Missing) return PlannedFileChange.Create(before.Expectation, intended);
        return before.Bytes.AsSpan().SequenceEqual(intended) ? null : PlannedFileChange.Replace(before.Expectation, intended);
    }
}

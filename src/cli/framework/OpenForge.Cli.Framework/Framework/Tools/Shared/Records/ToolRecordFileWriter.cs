using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal static class ToolRecordFileWriter
{
    internal static async Task<byte[]?> CheckExpectedAsync(string root, string path, ToolRecordsResult expected, CancellationToken token)
    {
        ToolRecordPaths.Check(root, path);
        if (!expected.ExpectedFileSha256.TryGetValue(path, out var hash))
            throw new IOException($"The record has no expected file or absence observation: {path}.");
        byte[]? bytes = null;
        if (File.Exists(path)) bytes = await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
        else if (Directory.Exists(path)) throw new IOException($"The record destination is a directory: {path}.");
        if (!string.Equals(hash, bytes is null ? null : Hash(bytes), StringComparison.OrdinalIgnoreCase))
            throw new IOException($"The record changed before publication: {path}.");
        return bytes;
    }

    internal static async Task ReplaceAsync(ToolRecordWrite write, ToolRecordsResult expected, CancellationToken token)
    {
        ToolRecordPaths.Check(write.Root, write.Path);
        var parent = Path.GetDirectoryName(write.Path) ?? throw new IOException("The record requires a parent directory.");
        Directory.CreateDirectory(parent);
        ToolRecordPaths.Check(write.Root, write.Path);
        var temporary = Path.Combine(parent, Path.GetRandomFileName());
        var created = false;
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                created = true;
                await stream.WriteAsync(write.Bytes, token).ConfigureAwait(false);
                await stream.FlushAsync(token).ConfigureAwait(false);
            }
            await CheckExpectedAsync(write.Root, write.Path, expected, token).ConfigureAwait(false);
            if (File.Exists(write.Path))
            {
                if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(temporary, File.GetUnixFileMode(write.Path));
                File.Replace(temporary, write.Path, destinationBackupFileName: null);
            }
            else File.Move(temporary, write.Path);
        }
        finally { if (created && File.Exists(temporary)) File.Delete(temporary); }
    }

    internal static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}

using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

// Tool config edits and sibling backups share expected-state atomic replacement.
internal static class ToolFileReplacement
{
    internal static async ValueTask WriteAsync(CliWorkspace scope, FileExpectation expected, byte[]? bytes, CancellationToken token)
    {
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        async ValueTask<FileExpectationValidationResult> CheckAsync()
        {
            var check = await validator.ValidateAsync(scope, expected, token).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
            if (check.State != FileExpectationValidationState.Matched) throw new IOException(check.Cause ?? "The file changed before replacement.");
            return check;
        }

        _ = ToolFileReader.Observe(scope, expected.LogicalPath, token);
        var initial = await CheckAsync().ConfigureAwait(false);
        var target = initial.PhysicalPath ?? throw new IOException("The replacement target has no physical path.");
        if (bytes is null)
        {
            _ = await CheckAsync().ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            File.Delete(target);
            return;
        }
        var parent = Path.GetDirectoryName(target) ?? throw new IOException("The target has no parent.");
        Directory.CreateDirectory(parent);
        _ = ToolFileReader.Observe(scope, expected.LogicalPath, token);
        var stage = Path.Combine(Path.GetDirectoryName(target) ?? throw new IOException("The replacement target has no parent."), Path.GetRandomFileName());
        try
        {
            await using (var stream = new FileStream(stage, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await stream.WriteAsync(bytes, token).ConfigureAwait(false);
            if (!(await File.ReadAllBytesAsync(stage, token).ConfigureAwait(false)).AsSpan().SequenceEqual(bytes.AsSpan()))
                throw new IOException("The staged bytes differ from the replacement bytes.");
            var final = await CheckAsync().ConfigureAwait(false);
            if (!string.Equals(final.PhysicalPath, target, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new IOException("The replacement target's physical identity changed.");
            if (!OperatingSystem.IsWindows())
            {
                UnixFileMode? mode = null;
                if (mode is null && expected.Exists) mode = File.GetUnixFileMode(target);
                if (mode is { } permissions) File.SetUnixFileMode(stage, permissions);
            }
            token.ThrowIfCancellationRequested();
            if (expected.Exists) File.Replace(stage, target, destinationBackupFileName: null);
            else File.Move(stage, target, overwrite: false);
        }
        finally { if (File.Exists(stage)) File.Delete(stage); }
    }
}

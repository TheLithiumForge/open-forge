using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

// Tool backup restoration and account records share this byte-only replacement mechanism.
internal static class ToolFileReplacement
{
    internal static async ValueTask WriteAsync(CliWorkspace scope, ToolFileWrite write, Action requireLease, CancellationToken token)
    {
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        async ValueTask<FileExpectationValidationResult> CheckAsync()
        {
            requireLease();
            var check = await validator.ValidateAsync(scope, write.Expectation, token).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
            if (check.State != FileExpectationValidationState.Matched) throw new IOException(check.Cause ?? "The file changed before replacement.");
            return check;
        }

        var initial = await CheckAsync().ConfigureAwait(false);
        var target = initial.PhysicalPath ?? throw new IOException("The replacement target has no physical path.");
        var stage = Path.Combine(Path.GetDirectoryName(target) ?? throw new IOException("The replacement target has no parent."), Path.GetRandomFileName());
        try
        {
            await using (var stream = new FileStream(stage, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await stream.WriteAsync(write.Bytes, token).ConfigureAwait(false);
            if (!(await File.ReadAllBytesAsync(stage, token).ConfigureAwait(false)).AsSpan().SequenceEqual(write.Bytes.Span))
                throw new IOException("The staged bytes differ from the replacement bytes.");
            var final = await CheckAsync().ConfigureAwait(false);
            if (!string.Equals(final.PhysicalPath, target, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                throw new IOException("The replacement target's physical identity changed.");
            if (!OperatingSystem.IsWindows())
            {
                var mode = write.Mode;
                if (mode is null && write.Expectation.Exists) mode = File.GetUnixFileMode(target);
                if (mode is { } permissions) File.SetUnixFileMode(stage, permissions);
            }
            requireLease();
            token.ThrowIfCancellationRequested();
            File.Move(stage, target, overwrite: write.Expectation.Exists);
        }
        finally { if (File.Exists(stage)) File.Delete(stage); }
    }
}

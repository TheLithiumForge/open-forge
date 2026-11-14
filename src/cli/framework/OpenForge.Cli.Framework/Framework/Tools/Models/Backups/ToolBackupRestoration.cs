using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Backups;

internal sealed record ToolBackupRestoreFile(FileStateSnapshot Target, FileStateSnapshot Backup);

internal sealed record ToolBackupRestoreReceipt(string Target, string Backup, bool Written, bool Verified, bool BackupRemoved, string? Cause)
{
    internal bool CleanupUnknown { get; init; }
}

internal sealed record ToolBackupRestoration(ToolBackupRestoreReceipt[] Files, bool Interrupted, string? Cause);

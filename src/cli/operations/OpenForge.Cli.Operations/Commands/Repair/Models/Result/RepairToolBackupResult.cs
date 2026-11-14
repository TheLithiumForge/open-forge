namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairToolBackupFile(string Target, string Backup, string? Before, string After, bool Written, bool Verified, bool BackupRemoved, string? Cause);
internal sealed record RepairToolBackupResult(RepairToolBackupFile[] Files, bool Applied);

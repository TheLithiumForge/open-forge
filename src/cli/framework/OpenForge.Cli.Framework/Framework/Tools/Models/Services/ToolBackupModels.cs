namespace OpenForge.Cli.Core.Framework.Tools.Models.Services;

internal enum ToolBackupKind { Git, Sibling, PriorAbsence }

internal sealed record ToolBackupRequest(string TargetPath);
internal sealed record ToolBackupResult
{
    public required string TargetPath { get; init; }
    public required ToolBackupKind Kind { get; init; }
    public string? BackupPath { get; init; }
    public string? GitObjectId { get; init; }
    public string? Failure { get; init; }
}

internal sealed record ToolBackupRestoreRequest(ToolBackupResult Backup, string ExpectedAppliedSha256);
internal sealed record ToolBackupRestoreResult(bool Restored, bool Conflict, string? Failure);

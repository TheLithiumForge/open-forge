namespace OpenForge.Cli.Core.Framework.Tools.Models.Backups;

internal enum ToolBackupMode { Git, Sibling }

internal sealed record ToolBackupResidual(string Target, string? Backup, string Cause);

internal sealed record ToolBackupCandidate(string Target, string Backup, bool SafePaths, string? Cause);

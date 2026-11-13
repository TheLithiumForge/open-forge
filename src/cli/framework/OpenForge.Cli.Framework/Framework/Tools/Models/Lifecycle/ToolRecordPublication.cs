using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

internal sealed record ToolRecordWrite(FileStateSnapshot Settings, FileStateSnapshot Lock, byte[] SettingsBytes, byte[] LockBytes);
internal sealed record ToolRecordPublication(FileStateSnapshot Settings, FileStateSnapshot Lock, ToolBackupMode BackupMode,
    ToolBackupResidual[] Residuals, string[] CreatedDirectories);

internal sealed class ToolRecordWriteException(string message, Exception inner, ToolBackupMode? mode, ToolBackupResidual[] residuals) : IOException(message, inner)
{
    internal ToolBackupMode? BackupMode { get; } = mode;
    internal ToolBackupResidual[] Residuals { get; } = residuals;
    internal bool Interrupted => InnerException is OperationCanceledException;
}

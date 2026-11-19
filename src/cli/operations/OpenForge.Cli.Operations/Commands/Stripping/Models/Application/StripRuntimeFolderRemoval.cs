namespace OpenForge.Cli.Core.Commands.Stripping.Models.Application;

internal enum StripRuntimeFolderState
{
    Absent,
    Removable,
    HoldsRecoveryData,
    Link,
    Failed,
}

internal sealed record StripRuntimeFolderRemoval(StripRuntimeFolderState State, string? Cause = null);

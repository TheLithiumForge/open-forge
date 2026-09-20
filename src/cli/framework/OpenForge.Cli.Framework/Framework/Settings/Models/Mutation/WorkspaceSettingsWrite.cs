using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Mutation;

internal enum WorkspaceSettingsWriteState
{
    /// <summary>Every path was already admitted; the file was not touched.</summary>
    AlreadyAdmitted,

    /// <summary>The file was written.</summary>
    Written,

    /// <summary>The file exists and cannot be written without destroying it.</summary>
    Refused,
}

internal sealed record WorkspaceSettingsWrite(
    WorkspaceSettingsWriteState State,
    ImmutableArray<string> Added,
    string LogicalPath,
    string? Cause);

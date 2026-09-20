using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Observation;

internal enum WorkspaceSettingsReadState
{
    /// <summary>No settings file. The defaults apply and nothing is wrong.</summary>
    Absent,

    /// <summary>The file was read and understood.</summary>
    Complete,

    /// <summary>The file exists but could not be understood. <see cref="WorkspaceSettingsRead.Cause"/> says why.</summary>
    Invalid,

    /// <summary>The file exists but could not be read at all.</summary>
    Unavailable,
}

/// <summary>
/// One reading of the authored settings. <see cref="Document"/> is never null:
/// an absent, invalid, or unreadable file yields the defaults, so a consumer
/// always has settings to act on and reports the state separately. Settings
/// trouble is a finding, never a reason to refuse a command.
/// </summary>
internal sealed record WorkspaceSettingsRead(
    WorkspaceSettingsReadState State,
    WorkspaceSettingsDocument Document,
    string LogicalPath,
    string? Cause)
{
    internal FileStateSnapshot? Snapshot { get; init; }

    internal bool MatchesObservation(WorkspaceSettingsRead current)
        => State == current.State
            && Document.AllowInstallPaths.SequenceEqual(current.Document.AllowInstallPaths, StringComparer.Ordinal)
            && MatchesSnapshot(current);

    private bool MatchesSnapshot(WorkspaceSettingsRead current)
    {
        if (Snapshot is not { } expected)
        {
            return current.Snapshot is null;
        }
        return current.Snapshot is { } actual
            && expected.Expectation == actual.Expectation
            && expected.Bytes.AsSpan().SequenceEqual(actual.Bytes.AsSpan());
    }

    internal static WorkspaceSettingsRead Absent(string logicalPath)
        => new(WorkspaceSettingsReadState.Absent, WorkspaceSettingsDocument.Empty, logicalPath, Cause: null)
        {
            Snapshot = FileStateSnapshot.Missing(logicalPath),
        };
}

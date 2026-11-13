using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal sealed record SourceLinkHistoryRequest(CliWorkspace Workspace, IReadOnlyList<LocalReferenceObservation> References);

internal sealed record SourceLinkHistoryEntry(
    string SourcePath,
    string Destination,
    string Commit,
    string? MovedPath,
    string? PreviousText)
{
    internal IReadOnlyList<SourceLinkDiffHunk> TargetChanges { get; init; } = [];
}

internal sealed record SourceLinkDiffHunk(int PreviousLine, int PreviousCount, int CurrentLine, int CurrentCount);

internal sealed record SourceLinkHistory(IReadOnlyList<SourceLinkHistoryEntry> Entries, bool TimedOut = false);

internal delegate ValueTask<SourceLinkHistory> SourceLinkHistoryRead(SourceLinkHistoryRequest request, CancellationToken token);

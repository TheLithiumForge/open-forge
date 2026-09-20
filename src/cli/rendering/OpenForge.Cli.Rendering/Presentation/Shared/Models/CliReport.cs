using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Models;

internal sealed record CliReport<TData> where TData : class
{
    public required string Command { get; init; }
    public required CliSemanticStatus Status { get; init; }
    public required CliHeadline Headline { get; init; }
    public string? HeadlineFindingCode { get; init; }
    internal bool ShowMinimalTextWarnings { get; init; }
    public CliWorkspaceEcho? Workspace { get; init; }
    public IReadOnlyList<CliFinding> Findings { get; init; } = [];
    public IReadOnlyList<CliEffect> Effects { get; init; } = [];
    public IReadOnlyList<CliCount> Counts { get; init; } = [];
    public IReadOnlyList<CliLimitation> Limitations { get; init; } = [];
    public required TData Data { get; init; }
    public CliRecovery? Recovery { get; init; }
    public CliNextAction? Next { get; init; }
    public IReadOnlyList<string> Diagnostics { get; init; } = [];
}

internal sealed record CliHeadline(string Sentence, CliHeadlineKind Kind);
internal enum CliHeadlineKind { Done, NothingToDo, Preview, Warnings, Incomplete, CannotStart, Blocked, Failed, Cancelled }
internal sealed record CliWorkspaceEcho(string Path, bool Explicit);
internal sealed record CliCount(string Name, string Label, decimal? Value, string? UnavailableReason = null);
internal sealed record CliLimitation(string What, string Why, CliSubject? Subject = null);
internal sealed record CliRecovery(string? Path, CliRecoveryDisposition Disposition);
internal enum CliRecoveryDisposition { NotRequired, Removed, Retained, Unknown, Failed }

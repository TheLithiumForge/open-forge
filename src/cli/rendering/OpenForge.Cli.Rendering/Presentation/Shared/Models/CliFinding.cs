using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Models;

internal sealed record CliFinding
{
    public required CliSeverity Severity { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required CliSubject Subject { get; init; }
    public string? Category { get; init; }
    public CliResolution? Resolution { get; init; }
    public IReadOnlyList<CliNextAction> Actions { get; init; } = [];
    public IReadOnlyList<CliCandidate> Candidates { get; init; } = [];
    public IReadOnlyList<CliEvidence> Evidence { get; init; } = [];
    public CliProvenance? Provenance { get; init; }
}

internal sealed record CliSubject(CliSubjectKind Kind, string? Path = null, string? Id = null, CliSourceLocation? Location = null);
internal enum CliSubjectKind { File, Directory, Source, Workspace, Identifier }
internal sealed record CliSourceLocation(int Line, int Column);
internal sealed record CliCandidate(CliSubject Subject, IReadOnlyList<string> Reasons);
internal sealed record CliEvidence(string Label, string Value);
internal sealed record CliProvenance(string Source, string? Path = null, CliSourceLocation? Location = null);
internal enum CliResolution { SafeExact, GuidedChoice, TargetedOperation, ManualDecision, BlockedRepair, Informational }

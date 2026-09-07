namespace OpenForge.Cli.Core.Commands.Repair.Models.Presentation;

internal sealed class RepairJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RepairJsonWorkspace? Workspace { get; init; }

    public required RepairJsonResult Result { get; init; }

    public required RepairJsonNext? Next { get; init; }
}

internal sealed class RepairJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class RepairJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}

internal sealed class RepairJsonResult
{
    public required string Mode { get; init; }

    public required bool Automatic { get; init; }

    public required string SelectionMode { get; init; }

    public required RepairJsonRelink[] Relinks { get; init; }

    public required RepairJsonCoverage Diagnosis { get; init; }

    public required RepairJsonSelection? Selection { get; init; }

    public required RepairJsonPlan? Plan { get; init; }

    public required string[] AffectedPaths { get; init; }

    public required RepairJsonCounts Counts { get; init; }

    public required RepairJsonPreflight Preflight { get; init; }

    public required RepairJsonApplication Application { get; init; }

    public required RepairJsonVerification Verification { get; init; }

    public required RepairJsonRecovery Recovery { get; init; }

    public required RepairJsonPostDiagnosis PostDiagnosis { get; init; }

    public required RepairJsonFinding[] Findings { get; init; }
}

internal sealed class RepairJsonRelink
{
    public required string SourcePath { get; init; }

    public required int Line { get; init; }

    public required int Column { get; init; }

    public required string ExpectedDestination { get; init; }

    public required RepairJsonTarget Target { get; init; }
}

internal sealed class RepairJsonLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}

internal sealed class RepairJsonTarget
{
    public required string Path { get; init; }

    public required string? Fragment { get; init; }
}

internal sealed class RepairJsonCoverage
{
    public required string WorkspaceAndPath { get; init; }

    public required string RouteAndHeading { get; init; }

    public required string LocalReferences { get; init; }

    public required string SelectedScope { get; init; }
}

internal sealed class RepairJsonSelection
{
    public required string Mode { get; init; }

    public required RepairJsonSelectedProposal[] Selected { get; init; }

    public required RepairJsonProposal[] Unselected { get; init; }
}

internal sealed class RepairJsonProposal
{
    public required string Member { get; init; }

    public required string SourcePath { get; init; }

    public required RepairJsonLocation Occurrence { get; init; }

    public required string ExpectedDestination { get; init; }

    public required string? IntendedDestination { get; init; }

    public required RepairJsonTarget? Target { get; init; }

    public required RepairJsonCandidateSet? Candidates { get; init; }
}

internal sealed class RepairJsonSelectedProposal
{
    public required RepairJsonProposal Proposal { get; init; }

    public required RepairJsonResolution Resolution { get; init; }

    public required string[] Origins { get; init; }
}

internal sealed class RepairJsonResolution
{
    public required string IntendedDestination { get; init; }

    public required RepairJsonTarget Target { get; init; }
}

internal sealed class RepairJsonCandidateSet
{
    public required string Cardinality { get; init; }

    public required RepairJsonCandidate[] Items { get; init; }
}

internal sealed class RepairJsonCandidate
{
    public required RepairJsonTarget Target { get; init; }

    public required RepairJsonCandidateEvidence[] Evidence { get; init; }

    public required bool RecommendedForReview { get; init; }
}

internal sealed class RepairJsonCandidateEvidence
{
    public required string Kind { get; init; }

    public required string Value { get; init; }

    public required RepairJsonLocation? Location { get; init; }
}

internal sealed class RepairJsonPlan
{
    public required bool Blocked { get; init; }

    public required bool NoOp { get; init; }

    public required RepairJsonStep[] Steps { get; init; }

    public required RepairJsonEffect[] Effects { get; init; }

    public required RepairJsonNoOp[] NoOps { get; init; }

    public required RepairJsonConflict[] Conflicts { get; init; }
}

internal sealed class RepairJsonStep
{
    public required int Ordinal { get; init; }

    public required RepairJsonProposal Proposal { get; init; }

    public required RepairJsonResolution Resolution { get; init; }

    public required string[] Origins { get; init; }

    public required string[] Dependencies { get; init; }

    public required string[] Verification { get; init; }

    public required string RecoveryRequirement { get; init; }

    public required RepairJsonEffect? Effect { get; init; }

    public required RepairJsonNoOp? NoOp { get; init; }

    public required string Outcome { get; init; }
}

internal sealed class RepairJsonEffect
{
    public required string SourcePath { get; init; }

    public required RepairJsonState ExpectedState { get; init; }

    public required RepairJsonState IntendedState { get; init; }

    public required RepairJsonChange[] Changes { get; init; }

    public required RepairJsonAttribution Recovery { get; init; }

    public required string FileChangeKind { get; init; }
}

internal sealed class RepairJsonChange
{
    public required RepairJsonLocation Occurrence { get; init; }

    public required string ExpectedDestination { get; init; }

    public required string IntendedDestination { get; init; }

    public required string CatalogueMember { get; init; }

    public required RepairJsonTarget Target { get; init; }

    public required string[] Origins { get; init; }
}

internal sealed class RepairJsonNoOp
{
    public required string SourcePath { get; init; }

    public required RepairJsonLocation Occurrence { get; init; }

    public required string Destination { get; init; }

    public required string CatalogueMember { get; init; }

    public required RepairJsonTarget Target { get; init; }

    public required RepairJsonState CurrentState { get; init; }

    public required string[] Origins { get; init; }
}

internal sealed class RepairJsonConflict
{
    public required string Kind { get; init; }

    public required string? SourcePath { get; init; }

    public required RepairJsonLocation? Occurrence { get; init; }

    public required string Cause { get; init; }
}

internal sealed class RepairJsonState
{
    public required string Kind { get; init; }

    public required string LogicalPath { get; init; }

    public required string? PhysicalPath { get; init; }

    public required string? ContentHash { get; init; }

    public required int ByteLength { get; init; }

    public required bool HasBytes { get; init; }
}

internal sealed class RepairJsonAttribution
{
    public required string Producer { get; init; }

    public required string Operation { get; init; }

    public required string SubjectKind { get; init; }

    public required string SubjectIdentity { get; init; }
}

internal sealed class RepairJsonCounts
{
    public required int SelectedFindings { get; init; }

    public required int UnselectedFindings { get; init; }

    public required int Repaired { get; init; }

    public required int Remaining { get; init; }

    public required int NewFindings { get; init; }

    public required int Manual { get; init; }

    public required int Guided { get; init; }

    public required int Blocked { get; init; }

    public required int SelectedEffects { get; init; }

    public required int AppliedEffects { get; init; }

    public required int VerifiedEffects { get; init; }

    public required int NoOps { get; init; }

    public required int Conflicts { get; init; }
}

internal sealed class RepairJsonPreflight
{
    public required string State { get; init; }

    public required string? Cause { get; init; }

    public required string Recovery { get; init; }
}

internal sealed class RepairJsonApplication
{
    public required string State { get; init; }

    public required int AppliedEffects { get; init; }

    public required string? Cause { get; init; }
}

internal sealed class RepairJsonVerification
{
    public required string Targets { get; init; }

    public required string ResultingBytes { get; init; }

    public required string PostConditions { get; init; }
}

internal sealed class RepairJsonRecovery
{
    public required string State { get; init; }

    public required string Residual { get; init; }

    public required string? ResidualPath { get; init; }

    public required RepairJsonAttribution? Attribution { get; init; }
}

internal sealed class RepairJsonPostDiagnosis
{
    public required string State { get; init; }

    public required RepairJsonCoverage Coverage { get; init; }

    public required RepairJsonFinding[] Findings { get; init; }
}

internal sealed class RepairJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string Cause { get; init; }

    public required string? SourcePath { get; init; }

    public required RepairJsonLocation? Occurrence { get; init; }

    public required RepairJsonTarget? Target { get; init; }
}

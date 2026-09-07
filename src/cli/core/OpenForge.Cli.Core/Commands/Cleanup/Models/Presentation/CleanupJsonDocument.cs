using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;

internal sealed record CleanupJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required CleanupJsonWorkspace? Workspace { get; init; }

    public required CleanupJsonResult Result { get; init; }

    public required CleanupJsonNext? Next { get; init; }
}

internal sealed record CleanupJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record CleanupJsonResult
{
    public required string Mode { get; init; }

    public required CleanupJsonCatalogue Catalogue { get; init; }

    public required CleanupJsonPlan Plan { get; init; }

    public required CleanupJsonPreflight Preflight { get; init; }

    public required CleanupJsonLease Lease { get; init; }

    public required CleanupJsonCatalogueComparison Revalidation { get; init; }

    public required CleanupJsonEffect[] Effects { get; init; }

    public required CleanupJsonResidual[] Residuals { get; init; }

    public required CleanupJsonVerification Verification { get; init; }

    public required CleanupJsonFinding[] Findings { get; init; }
}

internal sealed record CleanupJsonCatalogue
{
    public required string Coverage { get; init; }

    public required CleanupJsonCandidate[] Candidates { get; init; }
}

internal sealed record CleanupJsonCandidate
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Integrity { get; init; }

    public required string FileKind { get; init; }

    public required CleanupJsonWorkspaceAssociation WorkspaceAssociation { get; init; }

    public required CleanupJsonLeaseBoundary LeaseBoundary { get; init; }

    public required CleanupJsonRecoveryProvenance? Provenance { get; init; }

    public required CleanupJsonVerificationCondition Verification { get; init; }

    public required string Eligibility { get; init; }

    public required string Action { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonPlan
{
    public required string Safety { get; init; }

    public required CleanupJsonPlanEntry[] Entries { get; init; }
}

internal sealed record CleanupJsonPlanEntry
{
    public required int Ordinal { get; init; }

    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Integrity { get; init; }

    public required string FileKind { get; init; }

    public required CleanupJsonWorkspaceAssociation WorkspaceAssociation { get; init; }

    public required CleanupJsonLeaseBoundary LeaseBoundary { get; init; }

    public required CleanupJsonRecoveryProvenance? Provenance { get; init; }

    public required CleanupJsonVerificationCondition Verification { get; init; }

    public required string Eligibility { get; init; }

    public required string Action { get; init; }

    public required CleanupJsonEffectCondition ResultEffect { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonPreflight
{
    public required string State { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonLease
{
    public required string State { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonCatalogueComparison
{
    public required string State { get; init; }

    public required CleanupJsonCatalogue? Planned { get; init; }

    public required CleanupJsonCatalogue? Observed { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Integrity { get; init; }

    public required string FileKind { get; init; }

    public required CleanupJsonWorkspaceAssociation WorkspaceAssociation { get; init; }

    public required CleanupJsonLeaseBoundary LeaseBoundary { get; init; }

    public required CleanupJsonRecoveryProvenance? Provenance { get; init; }

    public required CleanupJsonVerificationCondition Verification { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonResidual
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Integrity { get; init; }

    public required string FileKind { get; init; }

    public required CleanupJsonWorkspaceAssociation WorkspaceAssociation { get; init; }

    public required CleanupJsonLeaseBoundary LeaseBoundary { get; init; }

    public required CleanupJsonRecoveryProvenance? Provenance { get; init; }

    public required CleanupJsonVerificationCondition Verification { get; init; }

    public required string Action { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonWorkspaceAssociation
{
    public required string State { get; init; }

    public required string? SelectedPhysicalPath { get; init; }

    public required string? CandidatePhysicalPath { get; init; }

    public required string? SelectedWorkspaceKey { get; init; }

    public required string? CandidateWorkspaceKey { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonLeaseBoundary
{
    public required string State { get; init; }

    public required string? WorkspaceKey { get; init; }

    public required string? Command { get; init; }

    public required string? OperationId { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonRecoveryProvenance
{
    public required string Producer { get; init; }

    public required string Operation { get; init; }

    public required CleanupJsonRecoverySubject Subject { get; init; }

    public required string Command { get; init; }

    public required string WorkspacePhysicalPath { get; init; }

    public required string WorkspaceKey { get; init; }

    public required string OperationId { get; init; }
}

internal sealed record CleanupJsonRecoverySubject
{
    public required string Kind { get; init; }

    public required string Identity { get; init; }
}

internal sealed record CleanupJsonVerificationCondition
{
    public required string State { get; init; }

    public required string? ExpectedPath { get; init; }

    public required string ExpectedFileKind { get; init; }

    public required string? ExpectedIntegrity { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonEffectCondition
{
    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record CleanupJsonVerification
{
    public required string State { get; init; }

    public required string? Cause { get; init; }
}

internal sealed record CleanupJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }
}

internal sealed record CleanupJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(CleanupJsonDocument))]
internal sealed partial class CleanupJsonContext : JsonSerializerContext;

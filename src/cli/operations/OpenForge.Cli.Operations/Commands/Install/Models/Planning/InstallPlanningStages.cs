using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal sealed record InstallPlanningBoundary(
    InstallManagementState State,
    InstallFindingCode Code,
    string Cause,
    string? Subject = null,
    FrameworkPayload? Payload = null,
    InstallIntendedState? IntendedState = null);

internal sealed record InstallInspectionFacts(
    InstallRequest Request,
    FrameworkPayload Payload,
    InstallIntendedState IntendedState,
    WorkspaceOwnershipRead Ownership,
    IReadOnlyDictionary<string, InstallTargetRead> CurrentTargets,
    FrameworkOwnership? CurrentFramework);

internal abstract record InstallInspectionResult;

internal sealed record InstallInspectionCompleted(InstallInspectionFacts Facts)
    : InstallInspectionResult;

internal sealed record InstallInspectionStopped(InstallPlanningBoundary Boundary)
    : InstallInspectionResult;

internal sealed record InstallPlanningBasis(
    InstallPlanContext Context,
    WorkspaceOwnershipRead Ownership,
    IReadOnlyDictionary<string, InstallTargetRead> CurrentTargets,
    FrameworkOwnership? CurrentFramework);

internal abstract record InstallPlanningBasisResult;

internal sealed record InstallPlanningBasisCompleted(InstallPlanningBasis Basis)
    : InstallPlanningBasisResult;

internal sealed record InstallPlanningBasisStopped(InstallPlanningBoundary Boundary)
    : InstallPlanningBasisResult;

internal abstract record InstallManagedStateResult;

internal sealed record InstallManagedStateTrustedExact(InstallPlanContext Context)
    : InstallManagedStateResult;

internal sealed record InstallManagedStateEstablishment(InstallEstablishmentPlanInput Input)
    : InstallManagedStateResult;

internal sealed record InstallManagedStateStopped(InstallPlanningBoundary Boundary)
    : InstallManagedStateResult;

internal abstract record InstallPlanningDecision;

internal sealed record InstallPlanningCompleted(
    InstallPlanContext Context,
    InstallManagementState State,
    InstallPlanEffects Effects)
    : InstallPlanningDecision;

internal sealed record InstallPlanningWithFindings(
    InstallPlanContext Context,
    InstallManagementState State,
    IReadOnlyList<InstallFinding> Findings)
    : InstallPlanningDecision;

using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal enum UpdateTargetReadState
{
    Available,
    Missing,
    Unavailable,
    Blocked,
    Cancelled,
}

internal sealed record UpdateTargetRead(
    string RelativePath,
    UpdateTargetReadState State,
    FileStateSnapshot? Snapshot,
    string? Cause);

internal sealed record UpdateOwnedTarget(string Path, string? Region, string? SourceAssetPath);

internal sealed record UpdateComparisonObservation(
    UpdateComparison Comparison,
    FileStateSnapshot Snapshot,
    byte[]? IntendedDocumentBytes);

internal sealed record UpdatePlannedEffect(
    UpdatePhysicalEffect ResultEffect,
    PlannedFileChange FileChange);

internal sealed record UpdatePlanExecution(
    UpdateRequest Request,
    UpdatePlanBuild Build,
    FrameworkPayload Payload,
    WorkspaceOwnershipRead OwnershipRead,
    IReadOnlyList<UpdateComparisonObservation> Observations,
    IReadOnlyList<FileStateSnapshot> ProjectionInputs,
    IReadOnlyList<UpdatePlannedEffect> Effects,
    PlannedFileChange? OwnershipChange);

internal sealed record UpdatePlanResolution(
    UpdatePlanBuild Build,
    UpdatePlanExecution? Execution);

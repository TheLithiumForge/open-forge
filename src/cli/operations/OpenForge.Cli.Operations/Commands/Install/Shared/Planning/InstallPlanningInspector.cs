using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPlanningInspector
{
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly InstallTargetReader _targetReader;
    private readonly InstallIntendedStateBuilder _intendedStateBuilder;

    internal InstallPlanningInspector(
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
        _targetReader = new InstallTargetReader(physicalPathResolver);
        _intendedStateBuilder = new InstallIntendedStateBuilder(physicalPathResolver);
    }

    internal async ValueTask<InstallInspectionResult> InspectAsync(
        InstallRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (cancellationToken.IsCancellationRequested)
        {
            return Stopped(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "Install planning was interrupted before source inspection.");
        }

        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        if (payloadRead.State != FrameworkPayloadReadState.Available
            || payloadRead.Payload is not { } payload)
        {
            var code = payloadRead.State == FrameworkPayloadReadState.Unavailable
                ? InstallFindingCode.PayloadUnavailable
                : InstallFindingCode.PayloadInvalid;
            var state = payloadRead.State == FrameworkPayloadReadState.Unavailable
                ? InstallManagementState.Incomplete
                : InstallManagementState.Blocked;
            return Stopped(
                state,
                code,
                payloadRead.Cause ?? "The embedded Framework payload is unavailable.");
        }

        var intendedBuild = await _intendedStateBuilder.BuildAsync(
                request,
                payload,
                cancellationToken)
            .ConfigureAwait(false);
        if (intendedBuild.State != InstallIntendedStateBuildState.Complete
            || intendedBuild.IntendedState is not { } intendedState)
        {
            return new InstallInspectionStopped(ReadIntendedBoundary(intendedBuild, payload));
        }

        var ownership = await WorkspaceOwnershipReader.ReadAsync(
                _physicalPathResolver,
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);

        var recovery = await RecoveryBundleCatalogue.ReadAsync(
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        if (ReadRecoveryBoundary(recovery, payload, intendedState) is { } recoveryBoundary)
        {
            return new InstallInspectionStopped(recoveryBoundary);
        }

        var currentTargets = await ReadTargetsAsync(
                request,
                intendedState,
                cancellationToken)
            .ConfigureAwait(false);
        if (ReadTargetBoundary(currentTargets.Values, payload, intendedState) is { } targetBoundary)
        {
            return new InstallInspectionStopped(targetBoundary);
        }

        return new InstallInspectionCompleted(new InstallInspectionFacts(
            request,
            payload,
            intendedState,
            ownership,
            currentTargets,
            ownership.Document.Framework));
    }

    private async ValueTask<IReadOnlyDictionary<string, InstallTargetRead>> ReadTargetsAsync(
        InstallRequest request,
        InstallIntendedState intended,
        CancellationToken cancellationToken)
    {
        var paths = intended.TargetBytes.Keys
            .Concat(intended.ManagedBlockBytes.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);
        var reads = new Dictionary<string, InstallTargetRead>(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            reads.Add(
                path,
                await _targetReader.ReadAsync(
                        request.Workspace,
                        path,
                        cancellationToken)
                    .ConfigureAwait(false));
        }

        return reads;
    }

    private static InstallPlanningBoundary? ReadRecoveryBoundary(
        RecoveryBundleCatalogueResult recovery,
        FrameworkPayload payload,
        InstallIntendedState intendedState)
    {
        return recovery.State switch
        {
            RecoveryBundleCatalogueState.Available when recovery.Candidates.Length == 0 => null,
            RecoveryBundleCatalogueState.Available => Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.RecoveryConflict,
                "Recognized Framework recovery residuals prevent management establishment or verification.",
                payload: payload,
                intendedState: intendedState),
            RecoveryBundleCatalogueState.Unavailable => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.RecoveryUnavailable,
                recovery.Cause ?? "Framework recovery residual facts are unavailable.",
                payload: payload,
                intendedState: intendedState),
            RecoveryBundleCatalogueState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "Framework recovery residual inspection was interrupted.",
                payload: payload,
                intendedState: intendedState),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The recovery catalogue state is not defined."),
        };
    }

    private static InstallPlanningBoundary? ReadTargetBoundary(
        IEnumerable<InstallTargetRead> targets,
        FrameworkPayload payload,
        InstallIntendedState intendedState)
    {
        foreach (var target in targets)
        {
            var boundary = target.State switch
            {
                InstallTargetReadState.Missing
                    or InstallTargetReadState.File => null,
                InstallTargetReadState.Unavailable => Boundary(
                    InstallManagementState.Incomplete,
                    InstallFindingCode.LifecycleUnavailable,
                    target.Cause ?? "A recognized Install target is unavailable.",
                    subject: target.RelativePath,
                    payload: payload,
                    intendedState: intendedState),
                InstallTargetReadState.Blocked => Boundary(
                    InstallManagementState.Blocked,
                    InstallFindingCode.TargetUnsafe,
                    target.Cause ?? "A recognized Install target is unsafe.",
                    subject: target.RelativePath,
                    payload: payload,
                    intendedState: intendedState),
                InstallTargetReadState.Cancelled => Boundary(
                    InstallManagementState.Interrupted,
                    InstallFindingCode.Interrupted,
                    "Recognized Install target inspection was interrupted.",
                    subject: target.RelativePath,
                    payload: payload,
                    intendedState: intendedState),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(targets),
                    target.State,
                    "The Install target read state is not defined."),
            };
            if (boundary is not null)
            {
                return boundary;
            }
        }

        return null;
    }

    private static InstallPlanningBoundary ReadIntendedBoundary(
        InstallIntendedStateBuild result,
        FrameworkPayload payload)
    {
        return result.State switch
        {
            InstallIntendedStateBuildState.Incomplete => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.ProjectionUnavailable,
                result.Cause ?? "The intended Framework projection is unavailable.",
                payload: payload),
            InstallIntendedStateBuildState.Blocked => Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.GeneratedRegionUnsafe,
                result.Cause ?? "The intended Framework projection is unsafe.",
                payload: payload),
            InstallIntendedStateBuildState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "The intended Framework projection was interrupted.",
                payload: payload),
            InstallIntendedStateBuildState.Complete => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "A complete intended state requires its value."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The intended state build state is not defined."),
        };
    }

    private static InstallInspectionStopped Stopped(
        InstallManagementState state,
        InstallFindingCode code,
        string cause)
        => new(Boundary(state, code, cause));

    private static InstallPlanningBoundary Boundary(
        InstallManagementState state,
        InstallFindingCode code,
        string cause,
        string? subject = null,
        FrameworkPayload? payload = null,
        InstallIntendedState? intendedState = null)
        => new(state, code, cause, subject, payload, intendedState);
}

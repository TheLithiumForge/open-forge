using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPlanBuilder(
    PhysicalPathResolver physicalPathResolver,
    LifecycleStore lifecycleStore,
    RecoveryBundleCatalogue recoveryCatalogue)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly RecoveryBundleCatalogue _recoveryCatalogue = recoveryCatalogue;
    private readonly InstallTargetReader _targetReader = new(physicalPathResolver);
    private readonly InstallIntendedStateBuilder _intendedStateBuilder = new(physicalPathResolver);
    private readonly InstallContentIdentity _contentIdentity = new();
    private readonly InstallPreservationVerifier _preservationVerifier = new(
        physicalPathResolver);

    internal async ValueTask<InstallPlanBuild> BuildAsync(
        InstallRequest request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Boundary(
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
            return Boundary(
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
            return ReadIntendedBoundary(intendedBuild, Evidence(payload));
        }

        var evidence = Evidence(payload, intendedState);

        var lifecycle = await _lifecycleStore.ReadAsync(
                request.Workspace,
                LifecycleSection.Framework,
                cancellationToken)
            .ConfigureAwait(false);
        var lifecycleBoundary = ReadLifecycleBoundary(lifecycle, evidence);
        if (lifecycleBoundary is not null)
        {
            return lifecycleBoundary;
        }

        var recovery = await _recoveryCatalogue.ReadAsync(
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        var recoveryBoundary = ReadRecoveryBoundary(recovery, evidence);
        if (recoveryBoundary is not null)
        {
            return recoveryBoundary;
        }

        var currentTargets = await ReadTargetsAsync(
                request,
                intendedState,
                cancellationToken)
            .ConfigureAwait(false);
        var targetBoundary = ReadTargetBoundary(currentTargets.Values, evidence);
        if (targetBoundary is not null)
        {
            return targetBoundary;
        }

        var currentFramework = lifecycle.State == LifecycleStoreReadState.Available
            ? lifecycle.Framework
                ?? throw new InvalidOperationException(
                    "An available Framework lifecycle read requires its typed state.")
            : null;
        FrameworkLifecycleState intendedLifecycle;
        try
        {
            intendedLifecycle = _contentIdentity.CreateLifecycle(
                payload,
                intendedState,
                currentFramework);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.LifecycleBlocked,
                $"The intended Framework lifecycle state is invalid: {exception.Message}",
                evidence: evidence);
        }

        FileExpectation agentsDirectoryExpectation;
        try
        {
            agentsDirectoryExpectation = ReadAgentsDirectoryExpectation(request);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.WorkspaceUnavailable,
                "The .agents directory state is unavailable for exact planning.",
                subject: SourceLogicalPath.AgentsRoot,
                evidence: evidence);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or NotSupportedException)
        {
            return Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.TargetUnsafe,
                $"The .agents directory state is unsafe for exact planning: {exception.Message}",
                subject: SourceLogicalPath.AgentsRoot,
                evidence: evidence);
        }

        var context = new InstallPlanContext
        {
            Request = request,
            Payload = payload,
            IntendedState = intendedState,
            IntendedLifecycle = intendedLifecycle,
            AgentsDirectoryExpectation = agentsDirectoryExpectation,
        };

        if (currentFramework is not null)
        {
            bool exact;
            try
            {
                exact = _contentIdentity.IsCurrentBaseExact(
                    currentFramework,
                    intendedLifecycle,
                    currentTargets,
                    intendedState);
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Boundary(
                    InstallManagementState.Blocked,
                    InstallFindingCode.LifecycleBlocked,
                    $"The managed Framework state cannot be verified safely: {exception.Message}",
                    evidence: evidence);
            }

            if (!exact)
            {
                return Boundary(
                    InstallManagementState.ManagedDivergence,
                    InstallFindingCode.ManagedDivergence,
                    "The trusted managed Framework state differs from its embedded current baseline.",
                    evidence: evidence);
            }

            var preservation = await _preservationVerifier.VerifyAsync(
                    currentFramework,
                    intendedState,
                    request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
            var preservationBoundary = ReadPreservationBoundary(preservation, evidence);
            if (preservationBoundary is not null)
            {
                return preservationBoundary;
            }

            return CompletePlan(
                context,
                InstallManagementState.TrustedExact,
                new InstallPlanEffects
                {
                    DirectoryCreations = [],
                    TargetEffects = [],
                    LifecycleEffect = null,
                });
        }

        return BuildEstablishmentPlan(new InstallEstablishmentPlanInput
        {
            Context = context,
            Lifecycle = lifecycle,
            CurrentTargets = currentTargets,
        });
    }

    private InstallPlanBuild BuildEstablishmentPlan(
        InstallEstablishmentPlanInput input)
    {
        var context = input.Context;
        var request = context.Request;
        var intendedState = context.IntendedState;
        var findings = new List<InstallFinding>();
        var effects = new List<InstallFileEffect>();
        var hasEligibleOccupant = false;
        foreach (var target in intendedState.TargetBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            var read = input.CurrentTargets[target.Key];
            var isGeneratedRegion = intendedState.GeneratedRegionPaths.Contains(target.Key);
            if (read.State == InstallTargetReadState.File)
            {
                if (isGeneratedRegion
                    && !HasSafeGeneratedRegion(read, out var generatedCause))
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.GeneratedRegionUnsafe,
                        cause: generatedCause,
                        subject: target.Key));
                    continue;
                }

                hasEligibleOccupant = true;
                if (!request.Force)
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.TargetOccupied,
                        cause: "An exact current Framework destination is occupied before management establishment.",
                        subject: target.Key));
                    continue;
                }
            }

            if (CreateEffect(new InstallFileEffectInput
            {
                Read = read,
                IntendedBytes = target.Value,
                Kind = isGeneratedRegion && read.State == InstallTargetReadState.File
                        ? InstallEffectKind.GeneratedRegion
                        : InstallEffectKind.File,
                Action = read.State == InstallTargetReadState.Missing
                        ? InstallEffectAction.Create
                        : InstallEffectAction.Replace,
                SourceAssetPath = isGeneratedRegion
                        && read.State == InstallTargetReadState.File
                            ? null
                            : target.Key,
            }) is { } effect)
            {
                effects.Add(effect);
            }
        }

        foreach (var block in intendedState.ManagedBlockBytes.OrderBy(
                     value => value.Key,
                     StringComparer.Ordinal))
        {
            var read = input.CurrentTargets[block.Key];
            if (read.Snapshot is not { } snapshot)
            {
                throw new InvalidOperationException(
                    "A readable managed host requires its exact snapshot.");
            }

            if (read.State == InstallTargetReadState.Missing)
            {
                effects.Add(CreateEffect(new InstallFileEffectInput
                {
                    Read = read,
                    IntendedBytes = block.Value,
                    Kind = InstallEffectKind.File,
                    Action = InstallEffectAction.Create,
                    SourceAssetPath = block.Key,
                })
                    ?? throw new InvalidOperationException(
                        "A missing managed host requires one create effect."));
                continue;
            }

            var blockResolution = _contentIdentity.ResolveManagedBlock(
                snapshot.Bytes.AsSpan(),
                block.Value);
            if (blockResolution.State == ManagedBlockState.Blocked)
            {
                findings.Add(new InstallFinding(
                    code: InstallFindingCode.TargetUnsafe,
                    cause: blockResolution.Cause
                        ?? "The managed host marker boundary is unsafe.",
                    subject: block.Key));
                continue;
            }

            if (blockResolution.State == ManagedBlockState.Present)
            {
                hasEligibleOccupant = true;
                if (!request.Force)
                {
                    findings.Add(new InstallFinding(
                        code: InstallFindingCode.TargetOccupied,
                        cause: "An Open Forge managed block is occupied before management establishment.",
                        subject: block.Key));
                    continue;
                }
            }

            var intendedDocument = blockResolution.IntendedDocumentBytes
                ?? throw new InvalidOperationException(
                    "A safe managed host resolution requires intended document bytes.");
            if (CreateEffect(new InstallFileEffectInput
            {
                Read = read,
                IntendedBytes = intendedDocument,
                Kind = InstallEffectKind.ManagedRegion,
                Action = blockResolution.State == ManagedBlockState.Absent
                        ? InstallEffectAction.Append
                        : InstallEffectAction.Replace,
                SourceAssetPath = block.Key,
            }) is { } effect)
            {
                effects.Add(effect);
            }
        }

        if (findings.Count > 0)
        {
            var state = findings.Any(finding =>
                    finding.Code is InstallFindingCode.TargetUnsafe
                        or InstallFindingCode.GeneratedRegionUnsafe)
                ? InstallManagementState.Blocked
                : InstallManagementState.EligibleInitialOccupant;
            return PlanWithFindings(
                context,
                state,
                findings);
        }

        var lifecyclePlan = _lifecycleStore.PlanFrameworkUpdate(
            input.Lifecycle,
            context.IntendedLifecycle);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return PlanWithFindings(
                context,
                InstallManagementState.Blocked,
                [new InstallFinding(
                    code: InstallFindingCode.LifecycleBlocked,
                    cause: lifecyclePlan.Cause
                        ?? "The Framework lifecycle publication plan is blocked.",
                    subject: LifecycleSchema.RelativePath)]);
        }

        var lifecycleEffect = lifecyclePlan.Change is { } lifecycleChange
            ? new InstallFileEffect
            {
                Identity = new InstallEffectIdentity
                {
                    Path = LifecycleSchema.RelativePath,
                    Kind = InstallEffectKind.File,
                    Action = lifecycleChange.Kind == PlannedFileChangeKind.Create
                        ? InstallEffectAction.Create
                        : InstallEffectAction.Replace,
                    SourceAssetPath = null,
                },
                Change = lifecycleChange,
                RecoveryTarget = RecoveryBundleTarget.Create(
                    lifecycleChange,
                    input.Lifecycle.File
                        ?? throw new InvalidOperationException(
                            "A lifecycle write plan requires exact prior file facts.")),
            }
            : null;
        IReadOnlyList<PlannedDirectoryCreation> directories;
        try
        {
            directories = CreateDirectoryPlan(
                request,
                context.AgentsDirectoryExpectation,
                effects.Select(effect => effect.Change)
                    .Concat(lifecycleEffect is null
                        ? []
                        : [lifecycleEffect.Change]));
        }
        catch (InvalidDataException exception)
        {
            return PlanWithFindings(
                context,
                InstallManagementState.Blocked,
                [new InstallFinding(
                    InstallFindingCode.TargetUnsafe,
                    exception.Message)]);
        }

        return CompletePlan(
            context,
            hasEligibleOccupant
                ? InstallManagementState.EligibleInitialOccupant
                : InstallManagementState.SafelyAbsent,
            new InstallPlanEffects
            {
                DirectoryCreations = directories,
                TargetEffects = effects,
                LifecycleEffect = lifecycleEffect,
            });
    }

    private IReadOnlyList<PlannedDirectoryCreation> CreateDirectoryPlan(
        InstallRequest request,
        FileExpectation agentsDirectoryExpectation,
        IEnumerable<PlannedFileChange> changes)
    {
        var agentsPath = Path.Combine(
            request.Workspace.LexicalRoot,
            SourceLogicalPath.AgentsRoot);
        var missing = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        if (agentsDirectoryExpectation.Kind == FileExpectationKind.Missing)
        {
            missing.Add(agentsPath);
        }

        foreach (var change in changes.Where(change =>
                     change.LogicalPath.StartsWith(
                         agentsPath + Path.DirectorySeparatorChar,
                         PathComparison())))
        {
            var parent = Path.GetDirectoryName(change.LogicalPath);
            while (parent is not null
                && !PhysicalIdentityTracker.PathComparer.Equals(
                    parent,
                    request.Workspace.LexicalRoot))
            {
                if (PhysicalIdentityTracker.PathComparer.Equals(parent, agentsPath))
                {
                    if (agentsDirectoryExpectation.Kind == FileExpectationKind.Missing)
                    {
                        missing.Add(parent);
                    }

                    break;
                }

                var resolution = _physicalPathResolver.ResolveCandidate(
                    request.Workspace.LexicalRoot,
                    request.Workspace.PhysicalRoot,
                    parent);
                if (resolution.State == PhysicalPathState.Missing)
                {
                    missing.Add(parent);
                    parent = Path.GetDirectoryName(parent);
                    continue;
                }

                if (resolution.State != PhysicalPathState.Contained
                    || !IsOrdinaryDirectory(resolution.GetContainedPhysicalPath()))
                {
                    throw new InvalidDataException(
                        "A planned Install directory parent is unsafe or unavailable.");
                }

                break;
            }
        }

        return missing
            .OrderBy(path => path.Count(character =>
                character == Path.DirectorySeparatorChar))
            .ThenBy(path => path, PhysicalIdentityTracker.PathComparer)
            .Select(path => PlannedDirectoryCreation.Create(
                FileExpectation.Missing(path)))
            .ToArray();
    }

    private static InstallFileEffect? CreateEffect(InstallFileEffectInput input)
    {
        var read = input.Read;
        var before = read.Snapshot
            ?? throw new InvalidOperationException(
                "A readable Install target requires one exact prior snapshot.");
        PlannedFileChange? change = read.State switch
        {
            InstallTargetReadState.Missing => PlannedFileChange.Create(
                before.Expectation,
                input.IntendedBytes),
            InstallTargetReadState.File when before.Bytes.AsSpan().SequenceEqual(input.IntendedBytes) => null,
            InstallTargetReadState.File when input.Kind == InstallEffectKind.GeneratedRegion
                => PlannedFileChange.ReplaceGeneratedRegion(
                    before.Expectation,
                    input.IntendedBytes),
            InstallTargetReadState.File => PlannedFileChange.Replace(
                before.Expectation,
                input.IntendedBytes),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "Only readable Install target states can form an effect."),
        };
        return change is null
            ? null
            : new InstallFileEffect
            {
                Identity = new InstallEffectIdentity
                {
                    Path = read.RelativePath,
                    Kind = input.Kind,
                    Action = input.Action,
                    SourceAssetPath = input.SourceAssetPath,
                },
                Change = change,
                RecoveryTarget = RecoveryBundleTarget.Create(change, before),
            };
    }

    private bool HasSafeGeneratedRegion(
        InstallTargetRead read,
        out string cause)
    {
        try
        {
            var snapshot = read.Snapshot
                ?? throw new InvalidDataException(
                    "An occupied generated target requires exact current bytes.");
            _ = _contentIdentity.ReadGeneratedFingerprint(snapshot.Bytes.AsSpan());
            cause = string.Empty;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            cause = $"The occupied generated target has an unsafe marker boundary: {exception.Message}";
            return false;
        }
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

    private FileExpectation ReadAgentsDirectoryExpectation(InstallRequest request)
    {
        var path = Path.Combine(
            request.Workspace.LexicalRoot,
            SourceLogicalPath.AgentsRoot);
        var resolution = _physicalPathResolver.ResolveCandidate(
                request.Workspace.LexicalRoot,
                request.Workspace.PhysicalRoot,
                path);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return FileExpectation.Missing(path);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            throw new InvalidDataException(
                resolution.Failure?.DirectCause
                    ?? "The .agents directory is not a contained ordinary directory.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        var attributes = File.GetAttributes(physicalPath);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidDataException("The .agents path is not an ordinary directory.");
        }

        return FileExpectation.Directory(path, physicalPath);
    }

    private static InstallPlanBuild? ReadLifecycleBoundary(
        LifecycleStoreReadResult lifecycle,
        InstallPlanningEvidence evidence)
    {
        return lifecycle.State switch
        {
            LifecycleStoreReadState.Available
                or LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing => null,
            LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => Boundary(
                    InstallManagementState.Blocked,
                    InstallFindingCode.LifecycleBlocked,
                    lifecycle.Cause ?? "The Framework lifecycle state is invalid or unsafe.",
                    evidence: evidence),
            LifecycleStoreReadState.Unavailable => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.LifecycleUnavailable,
                lifecycle.Cause ?? "The Framework lifecycle state is unavailable.",
                evidence: evidence),
            LifecycleStoreReadState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "Framework lifecycle inspection was interrupted.",
                evidence: evidence),
            _ => throw new ArgumentOutOfRangeException(
                nameof(lifecycle),
                lifecycle.State,
                "The lifecycle read state is not defined."),
        };
    }

    private static InstallPlanBuild? ReadRecoveryBoundary(
        RecoveryBundleCatalogueResult recovery,
        InstallPlanningEvidence evidence)
    {
        return recovery.State switch
        {
            RecoveryBundleCatalogueState.Available when recovery.Candidates.Length == 0 => null,
            RecoveryBundleCatalogueState.Available => Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.RecoveryConflict,
                "Recognized Framework recovery residuals prevent management establishment or verification.",
                evidence: evidence),
            RecoveryBundleCatalogueState.Unavailable => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.RecoveryUnavailable,
                recovery.Cause ?? "Framework recovery residual facts are unavailable.",
                evidence: evidence),
            RecoveryBundleCatalogueState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "Framework recovery residual inspection was interrupted.",
                evidence: evidence),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The recovery catalogue state is not defined."),
        };
    }

    private static InstallPlanBuild? ReadPreservationBoundary(
        InstallPreservationVerification preservation,
        InstallPlanningEvidence evidence)
    {
        return preservation.State switch
        {
            InstallPreservationVerificationState.Verified => null,
            InstallPreservationVerificationState.Changed
                or InstallPreservationVerificationState.Blocked => Boundary(
                    InstallManagementState.Blocked,
                    InstallFindingCode.LifecycleBlocked,
                    preservation.Cause
                        ?? "A preserved scoped Framework target is changed or unsafe.",
                    subject: preservation.Subject,
                    evidence: evidence),
            InstallPreservationVerificationState.Incomplete => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.LifecycleUnavailable,
                    preservation.Cause
                        ?? "A preserved scoped Framework target is unavailable.",
                    subject: preservation.Subject,
                    evidence: evidence),
            InstallPreservationVerificationState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                    preservation.Cause
                        ?? "Scoped Framework preservation verification was interrupted.",
                    subject: preservation.Subject,
                    evidence: evidence),
            _ => throw new ArgumentOutOfRangeException(
                nameof(preservation),
                preservation.State,
                "The scoped Framework preservation state is not defined."),
        };
    }

    private static InstallPlanBuild? ReadTargetBoundary(
        IEnumerable<InstallTargetRead> targets,
        InstallPlanningEvidence evidence)
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
                    evidence: evidence),
                InstallTargetReadState.Blocked => Boundary(
                    InstallManagementState.Blocked,
                    InstallFindingCode.TargetUnsafe,
                    target.Cause ?? "A recognized Install target is unsafe.",
                    subject: target.RelativePath,
                    evidence: evidence),
                InstallTargetReadState.Cancelled => Boundary(
                    InstallManagementState.Interrupted,
                    InstallFindingCode.Interrupted,
                    "Recognized Install target inspection was interrupted.",
                    subject: target.RelativePath,
                    evidence: evidence),
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

    private static InstallPlanBuild ReadIntendedBoundary(
        InstallIntendedStateBuild result,
        InstallPlanningEvidence evidence)
    {
        return result.State switch
        {
            InstallIntendedStateBuildState.Incomplete => Boundary(
                InstallManagementState.Incomplete,
                InstallFindingCode.ProjectionUnavailable,
                result.Cause ?? "The intended Framework projection is unavailable.",
                evidence: evidence),
            InstallIntendedStateBuildState.Blocked => Boundary(
                InstallManagementState.Blocked,
                InstallFindingCode.GeneratedRegionUnsafe,
                result.Cause ?? "The intended Framework projection is unsafe.",
                evidence: evidence),
            InstallIntendedStateBuildState.Cancelled => Boundary(
                InstallManagementState.Interrupted,
                InstallFindingCode.Interrupted,
                "The intended Framework projection was interrupted.",
                evidence: evidence),
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

    private static InstallPlanBuild CompletePlan(
        InstallPlanContext context,
        InstallManagementState managementState,
        InstallPlanEffects effects)
        => new()
        {
            ManagementState = managementState,
            Findings = [],
            Evidence = Evidence(context.Payload, context.IntendedState),
            Plan = new InstallPlan
            {
                Request = context.Request,
                Payload = context.Payload,
                IntendedState = context.IntendedState,
                ManagementState = managementState,
                IntendedLifecycle = context.IntendedLifecycle,
                DirectoryCreations = effects.DirectoryCreations,
                TargetEffects = effects.TargetEffects,
                LifecycleEffect = effects.LifecycleEffect,
                Effects = CreateEffectIdentities(context, effects),
                Findings = [],
            },
        };

    private static InstallPlanBuild PlanWithFindings(
        InstallPlanContext context,
        InstallManagementState managementState,
        IReadOnlyList<InstallFinding> findings)
        => new()
        {
            ManagementState = managementState,
            Findings = findings,
            Evidence = Evidence(context.Payload, context.IntendedState),
            Plan = new InstallPlan
            {
                Request = context.Request,
                Payload = context.Payload,
                IntendedState = context.IntendedState,
                ManagementState = managementState,
                IntendedLifecycle = context.IntendedLifecycle,
                DirectoryCreations = [],
                TargetEffects = [],
                LifecycleEffect = null,
                Effects = [],
                Findings = findings,
            },
        };

    private static InstallPlanBuild Boundary(
        InstallManagementState state,
        InstallFindingCode code,
        string cause,
        string? subject = null,
        InstallPlanningEvidence? evidence = null)
        => new()
        {
            ManagementState = state,
            Plan = null,
            Findings = [new InstallFinding(code: code, cause: cause, subject: subject)],
            Evidence = evidence ?? Evidence(),
        };

    private static InstallPlanningEvidence Evidence(
        FrameworkPayload? payload = null,
        InstallIntendedState? intendedState = null)
        => new()
        {
            Payload = payload,
            IntendedState = intendedState,
        };

    private static IReadOnlyList<InstallEffectIdentity> CreateEffectIdentities(
        InstallPlanContext context,
        InstallPlanEffects effects)
    {
        var identities = new List<InstallEffectIdentity>();
        identities.AddRange(effects.DirectoryCreations.Select(creation =>
            new InstallEffectIdentity
            {
                Path = CanonicalRelativePath(
                    context.Request.Workspace.LexicalRoot,
                    creation.LogicalPath),
                Kind = InstallEffectKind.Directory,
                Action = InstallEffectAction.Create,
                SourceAssetPath = null,
            }));
        identities.AddRange(effects.TargetEffects.Select(effect => effect.Identity));
        if (effects.LifecycleEffect is { } lifecycle)
        {
            identities.Add(lifecycle.Identity);
        }

        return identities;
    }

    private static string CanonicalRelativePath(string root, string path)
        => Path.GetRelativePath(root, path)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static bool IsOrdinaryDirectory(string physicalPath)
    {
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            return (attributes & FileAttributes.Directory) != 0
                && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException
            or UnauthorizedAccessException
            or IOException)
        {
            return false;
        }
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}

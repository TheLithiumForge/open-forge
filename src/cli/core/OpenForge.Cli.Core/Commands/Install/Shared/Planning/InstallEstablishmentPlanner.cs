using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallEstablishmentPlanner
{
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly LifecycleStore _lifecycleStore;
    private readonly InstallContentIdentity _contentIdentity = new();

    internal InstallEstablishmentPlanner(
        PhysicalPathResolver physicalPathResolver,
        LifecycleStore lifecycleStore)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lifecycleStore);
        _physicalPathResolver = physicalPathResolver;
        _lifecycleStore = lifecycleStore;
    }

    internal InstallPlanningDecision Build(InstallEstablishmentPlanInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

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
            return new InstallPlanningWithFindings(context, state, findings);
        }

        var lifecyclePlan = _lifecycleStore.PlanFrameworkUpdate(
            input.Lifecycle,
            context.IntendedLifecycle);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return new InstallPlanningWithFindings(
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
            return new InstallPlanningWithFindings(
                context,
                InstallManagementState.Blocked,
                [new InstallFinding(
                    InstallFindingCode.TargetUnsafe,
                    exception.Message)]);
        }

        return new InstallPlanningCompleted(
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
        Commands.Install.Models.Request.InstallRequest request,
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

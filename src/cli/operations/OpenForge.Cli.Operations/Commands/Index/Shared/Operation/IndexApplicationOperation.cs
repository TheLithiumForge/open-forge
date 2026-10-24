using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Operation;

internal sealed class IndexApplicationOperation(
    WorkspaceLockManager lockManager,
    MutationRevalidator revalidator,
    FileChangeApplier fileChangeApplier,
    IndexProjectionReader projectionReader)
{
    private readonly WorkspaceLayoutRevalidator _layoutRevalidator = new(new PhysicalPathResolver());
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly FileChangeApplier _fileChangeApplier = fileChangeApplier;
    private readonly IndexProjectionReader _projectionReader = projectionReader;

    internal async ValueTask<IndexOperationOutcome> ExecuteAsync(
        IndexPlan plan,
        CancellationToken cancellationToken)
    {
        var application = new IndexApplicationContext(
            plan: plan,
            operationId: Guid.NewGuid());
        var lockRequest = new WorkspaceLockRequest(
            workspace: application.Request.Workspace,
            command: IndexDefinitions.CommandIdentity,
            operationId: application.OperationId);
        WorkspaceLockResult lockResult;
        try
        {
            lockResult = await _lockManager.AcquireAsync(
                    lockRequest,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FinishBeforePreparation(application, IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return FinishBeforePreparation(application, IndexFindingCode.OperationFailed);
        }

        var lockFinding = IndexMutationMapper.ReadLockFindingCode(lockResult);
        if (lockFinding is { } findingCode)
        {
            return FinishBeforePreparation(application, findingCode, lockFailure: WorkspaceLockFailureProjection.From(lockResult.LockFailure ?? throw new InvalidOperationException("A failed workspace lock requires its typed failure.")));
        }

        var lease = lockResult.Lease
            ?? throw new InvalidOperationException("An acquired Index lock requires its held lease.");
        await using (lease.ConfigureAwait(false))
        {
            return await ExecuteUnderLeaseAsync(application, lease, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask<IndexOperationOutcome> ExecuteUnderLeaseAsync(
        IndexApplicationContext application,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        MutationValidationResult validation;
        try
        {
            var layoutValidation = await _layoutRevalidator.ValidateAsync(lease, application.Plan.LayoutRead, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (layoutValidation.State != MutationValidationState.Valid)
                return FinishBeforePreparation(application, IndexFindingCode.TargetChanged, cause: layoutValidation.Cause);
            if (!await _projectionReader.VerifyMaintenanceAsync(application.Plan, cancellationToken).ConfigureAwait(false))
                return FinishBeforePreparation(application, IndexFindingCode.TargetChanged,
                    cause: "Workspace inputs or selected rename evidence changed after Index planning.");
            validation = await _revalidator.ValidateAsync(
                    lease,
                    application.Plan.Updates,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FinishBeforePreparation(application, IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return FinishBeforePreparation(application, IndexFindingCode.OperationFailed);
        }

        var validationMapping = IndexMutationMapper.ReadValidation(new IndexValidationMappingInput
        {
            Plan = application.Plan,
            Validation = validation,
        });
        if (validationMapping.FindingCode is { } findingCode)
        {
            return FinishBeforePreparation(
                application: application,
                findingCode: findingCode,
                cause: validationMapping.Cause,
                source: validationMapping.Source);
        }

        if (application.Plan.RecoveryTargets.Count == 0)
            return await ApplyPreparedAsync(new(application, lease, validation, preparation: null), cancellationToken).ConfigureAwait(false);
        var preparation = await IndexRecoveryLifecycle.PrepareAsync(application, cancellationToken)
            .ConfigureAwait(false);
        if (!preparation.CanApply)
        {
            return Finish(
                application: application,
                regions: application.Plan.Regions,
                recovery: preparation.Recovery,
                finding: IndexMutationMapper.CreateFinding(preparation.FindingCode
                    ?? throw new InvalidOperationException(
                        "A rejected Index preparation requires its finding code.")));
        }

        var prepared = new IndexPreparedApplication(
            application: application,
            lease: lease,
            validation: validation,
            preparation: preparation.Preparation
                ?? throw new InvalidOperationException(
                    "An applicable Index preparation requires its verified final."));
        return await ApplyPreparedAsync(prepared, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<IndexOperationOutcome> ApplyPreparedAsync(
        IndexPreparedApplication prepared,
        CancellationToken cancellationToken)
    {
        var regions = prepared.Application.Plan.Regions.ToArray();
        for (var index = 0; index < prepared.Application.Plan.Updates.Count; index++)
        {
            var change = prepared.Application.Plan.Updates[index];
            var regionIndex = Array.FindIndex(regions, region => SourceLogicalPath.ToLexicalPath(prepared.Application.Request.Workspace.LexicalRoot, region.Source.Path) == change.LogicalPath);
            var path = Path.GetRelativePath(prepared.Application.Request.Workspace.LexicalRoot, change.LogicalPath).Replace('\\', '/');
            var source = regionIndex >= 0 ? regions[regionIndex].Source : null;
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileChangeApplier.ApplyAsync(
                        lease: prepared.Lease,
                        change: prepared.Application.Plan.Updates[index],
                        check: prepared.Validation.Checks[index],
                        recoveryPreparation: change.Kind == PlannedFileChangeKind.Create ? null : prepared.Preparation,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                if (regionIndex >= 0 && regions[regionIndex].Action == IndexRegionAction.Update) regions[regionIndex] = regions[regionIndex].WithOutcome(IndexRegionOutcome.Unknown);
                prepared.Application.EffectOutcomes[path] = IndexRegionOutcome.Unknown;
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: IndexFindingCode.Interrupted,
                    source: source);
            }
            catch (Exception)
            {
                if (regionIndex >= 0 && regions[regionIndex].Action == IndexRegionAction.Update) regions[regionIndex] = regions[regionIndex].WithOutcome(IndexRegionOutcome.Unknown);
                prepared.Application.EffectOutcomes[path] = IndexRegionOutcome.Unknown;
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: IndexFindingCode.OperationFailed,
                    source: source);
            }

            var mapping = IndexMutationMapper.ReadReceipt(receipt);
            if (regionIndex >= 0 && regions[regionIndex].Action == IndexRegionAction.Update) regions[regionIndex] = regions[regionIndex].WithOutcome(mapping.Outcome);
            prepared.Application.EffectOutcomes[path] = mapping.Outcome;
            if (!mapping.ShouldContinue)
            {
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: mapping.FindingCode
                        ?? throw new InvalidOperationException(
                            "A stopped Index receipt requires its finding code."),
                    source: source);
            }
        }

        return await VerifyAndDeleteAsync(prepared, regions, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<IndexOperationOutcome> VerifyAndDeleteAsync(
        IndexPreparedApplication prepared,
        IReadOnlyList<IndexRegion> regions,
        CancellationToken cancellationToken)
    {
        IndexProjectionReadResult fresh;
        try
        {
            fresh = await _projectionReader.ReadAsync(
                    prepared.Application.Request,
                    cancellationToken,
                    prepared.Application.Plan.Input.Projection.Selection.Targets.Select(source => source.Identity.CanonicalBasePath).ToArray())
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FinishPrepared(
                prepared,
                regions,
                IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return FinishPrepared(
                prepared,
                regions,
                IndexFindingCode.VerificationFailed);
        }

        if (fresh.Projection is { } freshProjection
            && freshProjection.LayoutRead?.Layout?.MainRoot != prepared.Application.Plan.LayoutRead.Layout?.MainRoot)
            return FinishPrepared(prepared, regions, IndexFindingCode.VerificationFailed);

        var verification = IndexMutationMapper.ReadFreshVerification(
            new IndexFreshVerificationInput
            {
                Original = prepared.Application.Plan,
                Fresh = fresh,
            });
        if (verification.FindingCode is { } findingCode)
        {
            return FinishPrepared(
                prepared: prepared,
                regions: regions,
                findingCode: findingCode,
                cause: verification.Cause,
                source: verification.Source);
        }

        if (prepared.Preparation is null)
            return Finish(prepared.Application, regions, IndexRecovery.NotRequired, finding: null);
        var deletion = await IndexRecoveryLifecycle.DeleteAsync(prepared, cancellationToken)
            .ConfigureAwait(false);
        return Finish(
            application: prepared.Application,
            regions: regions,
            recovery: deletion.Recovery,
            finding: deletion.FindingCode is { } deletionFindingCode
                ? IndexMutationMapper.CreateFinding(deletionFindingCode)
                : null);
    }

    private static IndexOperationOutcome FinishBeforePreparation(
        IndexApplicationContext application,
        IndexFindingCode findingCode,
        string? cause = null,
        IndexLogicalSource? source = null,
        WorkspaceLockFailureInfo? lockFailure = null)
        => Finish(
            application: application,
            regions: application.Plan.Regions,
            recovery: new IndexRecovery(IndexRecoveryState.NotCreated, residualPath: null),
            finding: IndexMutationMapper.CreateFinding(
                code: findingCode,
                cause: cause,
                source: source) with
            { LockFailure = lockFailure });

    private static IndexOperationOutcome FinishPrepared(
        IndexPreparedApplication prepared,
        IReadOnlyList<IndexRegion> regions,
        IndexFindingCode findingCode,
        string? cause = null,
        IndexLogicalSource? source = null)
        => Finish(
            application: prepared.Application,
            regions: regions,
            recovery: prepared.Preparation is { } preparation
                ? new IndexRecovery(IndexRecoveryState.Retained, preparation.BundlePath)
                : new IndexRecovery(IndexRecoveryState.NotCreated, residualPath: null),
            finding: IndexMutationMapper.CreateFinding(
                code: findingCode,
                cause: cause,
                source: source));

    private static IndexOperationOutcome Finish(
        IndexApplicationContext application,
        IReadOnlyList<IndexRegion> regions,
        IndexRecovery recovery,
        IndexFinding? finding)
    {
        var findings = application.Plan.Input.Projection.Findings.ToList();
        if (finding is not null)
        {
            findings.Add(finding);
        }

        return new()
        {
            Request = application.Request,
            Selection = application.Plan.Input.Projection.Selection.Selection,
            Regions = regions,
            Recovery = recovery,
            Findings = findings,
            Metadata = IndexCompletionProjector.Metadata(application.Plan, application.EffectOutcomes),
            Entrypoints = IndexCompletionProjector.Entrypoints(application.Plan, application.EffectOutcomes),
            Deletions = IndexCompletionProjector.Deletions(application.Plan, application.EffectOutcomes),
        };
    }
}

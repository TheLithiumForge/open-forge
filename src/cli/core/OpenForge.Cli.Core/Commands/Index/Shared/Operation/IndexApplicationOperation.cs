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

namespace OpenForge.Cli.Core.Commands.Index.Shared.Operation;

internal sealed class IndexApplicationOperation(
    WorkspaceLockManager lockManager,
    MutationRevalidator revalidator,
    FileChangeApplier fileChangeApplier,
    IndexProjectionReader projectionReader)
{
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
            return FinishBeforePreparation(application, findingCode);
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
        var updateRegionIndices = regions
            .Select((region, index) => (region, index))
            .Where(item => item.region.Action == IndexRegionAction.Update)
            .Select(item => item.index)
            .ToArray();
        for (var index = 0; index < prepared.Application.Plan.Updates.Count; index++)
        {
            var regionIndex = updateRegionIndices[index];
            FileChangeReceipt receipt;
            try
            {
                receipt = await _fileChangeApplier.ApplyAsync(
                        lease: prepared.Lease,
                        change: prepared.Application.Plan.Updates[index],
                        check: prepared.Validation.Checks[index],
                        recoveryPreparation: prepared.Preparation,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                regions[regionIndex] = regions[regionIndex].WithOutcome(IndexRegionOutcome.Unknown);
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: IndexFindingCode.Interrupted,
                    source: regions[regionIndex].Source);
            }
            catch (Exception)
            {
                regions[regionIndex] = regions[regionIndex].WithOutcome(IndexRegionOutcome.Unknown);
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: IndexFindingCode.OperationFailed,
                    source: regions[regionIndex].Source);
            }

            var mapping = IndexMutationMapper.ReadReceipt(receipt);
            regions[regionIndex] = regions[regionIndex].WithOutcome(mapping.Outcome);
            if (!mapping.ShouldContinue)
            {
                return FinishPrepared(
                    prepared: prepared,
                    regions: regions,
                    findingCode: mapping.FindingCode
                        ?? throw new InvalidOperationException(
                            "A stopped Index receipt requires its finding code."),
                    source: regions[regionIndex].Source);
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
                    cancellationToken)
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
        IndexLogicalSource? source = null)
        => Finish(
            application: application,
            regions: application.Plan.Regions,
            recovery: new IndexRecovery(IndexRecoveryState.NotCreated, residualPath: null),
            finding: IndexMutationMapper.CreateFinding(
                code: findingCode,
                cause: cause,
                source: source));

    private static IndexOperationOutcome FinishPrepared(
        IndexPreparedApplication prepared,
        IReadOnlyList<IndexRegion> regions,
        IndexFindingCode findingCode,
        string? cause = null,
        IndexLogicalSource? source = null)
        => Finish(
            application: prepared.Application,
            regions: regions,
            recovery: new IndexRecovery(
                state: IndexRecoveryState.Retained,
                residualPath: prepared.Preparation.BundlePath),
            finding: IndexMutationMapper.CreateFinding(
                code: findingCode,
                cause: cause,
                source: source));

    private static IndexOperationOutcome Finish(
        IndexApplicationContext application,
        IReadOnlyList<IndexRegion> regions,
        IndexRecovery recovery,
        IndexFinding? finding)
        => new()
        {
            Request = application.Request,
            Selection = application.Plan.Input.Projection.Selection.Selection,
            Regions = regions,
            Recovery = recovery,
            Findings = finding is null ? [] : [finding],
        };
}

using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Projection;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Index.Models.Operation;

internal enum IndexProjectionReadState
{
    Cancelled,
    SelectionIncomplete,
    Projected,
}

internal sealed record IndexProjectionReadResult
{
    private IndexProjectionReadResult(
        IndexProjectionReadState state,
        IndexSelection selection,
        IndexProjectionFormation? projection,
        IEnumerable<IndexFinding> findings)
    {
        var findingValues = findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Index projection-read findings cannot contain null members.",
                nameof(findings)))
            .ToArray();
        ValidateCoherence(state, projection, findingValues);
        State = state;
        Selection = selection;
        Projection = projection;
        Findings = new ReadOnlyCollection<IndexFinding>(
            IndexResult.OrderFindings(findingValues).ToArray());
    }

    internal IndexProjectionReadState State { get; }

    internal IndexSelection Selection { get; }

    internal IndexProjectionFormation? Projection { get; }

    internal IReadOnlyList<IndexFinding> Findings { get; }

    internal static IndexProjectionReadResult Cancelled(
        IndexRequest request,
        IndexFinding finding)
    {
        if (finding.Code != IndexFindingCode.Interrupted)
        {
            throw new ArgumentException(
                "A cancelled Index projection read requires its interrupted finding.",
                nameof(finding));
        }

        return new IndexProjectionReadResult(
            state: IndexProjectionReadState.Cancelled,
            selection: IndexSelection.NotEstablished(request.HasExplicitSources
                ? IndexSelectionOrigin.ExplicitSources
                : IndexSelectionOrigin.AutomaticLoader),
            projection: null,
            findings: [finding]);
    }

    internal static IndexProjectionReadResult SelectionIncomplete(IndexSelectionResolution resolution)
    {
        if (resolution.IsComplete)
        {
            throw new ArgumentException(
                "An incomplete Index projection read requires incomplete selection facts.",
                nameof(resolution));
        }

        return new IndexProjectionReadResult(
            state: IndexProjectionReadState.SelectionIncomplete,
            selection: resolution.Selection,
            projection: null,
            findings: resolution.Findings);
    }

    internal static IndexProjectionReadResult Projected(IndexProjectionFormation projection)
    {
        return new IndexProjectionReadResult(
            state: IndexProjectionReadState.Projected,
            selection: projection.Selection.Selection,
            projection: projection,
            findings: projection.Findings);
    }

    private static void ValidateCoherence(
        IndexProjectionReadState state,
        IndexProjectionFormation? projection,
        IReadOnlyList<IndexFinding> findings)
    {
        var valid = state switch
        {
            IndexProjectionReadState.Cancelled => projection is null
                && findings.Count == 1
                && findings[0].Code == IndexFindingCode.Interrupted,
            IndexProjectionReadState.SelectionIncomplete => projection is null
                && findings.Count != 0,
            IndexProjectionReadState.Projected => projection is not null
                && projection.Findings.SequenceEqual(findings),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Index projection-read state is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException(
                "Index projection-read facts do not match their state.");
        }
    }
}

internal sealed record IndexOperationOutcome
{
    public required IndexRequest Request { get; init; }

    public required IndexSelection Selection { get; init; }

    public required IReadOnlyList<IndexRegion> Regions { get; init; }

    public required IndexRecovery Recovery { get; init; }

    public required IReadOnlyList<IndexFinding> Findings { get; init; }
}

internal sealed class IndexApplicationContext
{
    internal IndexApplicationContext(
        IndexPlan plan,
        Guid operationId)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException(
                "An Index application operation ID cannot be empty.",
                nameof(operationId));
        }

        Plan = plan;
        OperationId = operationId;
    }

    internal IndexPlan Plan { get; }

    internal Guid OperationId { get; }

    internal IndexRequest Request => Plan.Input.Request;
}

internal sealed class IndexPreparedApplication
{
    internal IndexPreparedApplication(
        IndexApplicationContext application,
        WorkspaceLockLease lease,
        MutationValidationResult validation,
        RecoveryBundlePreparation preparation)
    {
        if (!lease.IsHeld
            || lease.Request.OperationId != application.OperationId
            || !preparation.MatchesOperation(lease.Request)
            || validation.State != MutationValidationState.Valid
            || validation.Checks.Count != application.Plan.Updates.Count
            || application.Plan.Updates.Any(change => !preparation.MatchesChange(lease.Request, change)))
        {
            throw new ArgumentException(
                "A prepared Index application requires one held matching operation, validation, and recovery authority.");
        }

        Application = application;
        Lease = lease;
        Validation = validation;
        Preparation = preparation;
    }

    internal IndexApplicationContext Application { get; }

    internal WorkspaceLockLease Lease { get; }

    internal MutationValidationResult Validation { get; }

    internal RecoveryBundlePreparation Preparation { get; }
}

internal sealed record IndexValidationMappingInput
{
    public required IndexPlan Plan { get; init; }

    public required MutationValidationResult Validation { get; init; }
}

internal sealed record IndexMutationFindingMapping
{
    internal IndexMutationFindingMapping(
        IndexFindingCode? findingCode,
        IndexLogicalSource? source,
        string? cause)
    {
        if (findingCode is null && (source is not null || cause is not null))
        {
            throw new ArgumentException(
                "A successful Index validation mapping cannot carry failure details.");
        }

        FindingCode = findingCode;
        Source = source;
        Cause = cause;
    }

    internal IndexFindingCode? FindingCode { get; }

    internal IndexLogicalSource? Source { get; }

    internal string? Cause { get; }
}

internal sealed record IndexFreshVerificationInput
{
    public required IndexPlan Original { get; init; }

    public required IndexProjectionReadResult Fresh { get; init; }
}

internal sealed record IndexPreparationMapping
{
    private IndexPreparationMapping(
        RecoveryBundlePreparation? preparation,
        IndexRecovery recovery,
        IndexFindingCode? findingCode)
    {
        var valid = preparation is not null
            ? recovery.State == IndexRecoveryState.Retained && findingCode is null
            : recovery.State is IndexRecoveryState.NotCreated or IndexRecoveryState.Unknown
                && findingCode is not null;
        if (!valid)
        {
            throw new ArgumentException(
                "Index recovery preparation facts are not coherent.");
        }

        Preparation = preparation;
        Recovery = recovery;
        FindingCode = findingCode;
    }

    internal RecoveryBundlePreparation? Preparation { get; }

    internal IndexRecovery Recovery { get; }

    internal IndexFindingCode? FindingCode { get; }

    internal bool CanApply => Preparation is not null;

    internal static IndexPreparationMapping Prepared(RecoveryBundlePreparation preparation)
    {
        return new IndexPreparationMapping(
            preparation: preparation,
            recovery: new IndexRecovery(IndexRecoveryState.Retained, preparation.BundlePath),
            findingCode: null);
    }

    internal static IndexPreparationMapping Rejected(
        IndexRecovery recovery,
        IndexFindingCode findingCode)
        => new(
            preparation: null,
            recovery: recovery,
            findingCode: findingCode);
}

internal sealed record IndexReceiptMapping
{
    internal IndexReceiptMapping(
        IndexRegionOutcome outcome,
        IndexFindingCode? findingCode,
        bool shouldContinue)
    {
        var valid = outcome switch
        {
            IndexRegionOutcome.Verified => findingCode is null && shouldContinue,
            IndexRegionOutcome.NotStarted
                or IndexRegionOutcome.Applied
                or IndexRegionOutcome.Unknown => findingCode is not null && !shouldContinue,
            IndexRegionOutcome.NotEstablished
                or IndexRegionOutcome.AlreadyCurrent
                or IndexRegionOutcome.NotRequested => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Index region outcome is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException("Index receipt mapping facts are not coherent.");
        }

        Outcome = outcome;
        FindingCode = findingCode;
        ShouldContinue = shouldContinue;
    }

    internal IndexRegionOutcome Outcome { get; }

    internal IndexFindingCode? FindingCode { get; }

    internal bool ShouldContinue { get; }
}

internal sealed record IndexDeletionMapping
{
    internal IndexDeletionMapping(
        IndexRecovery recovery,
        IndexFindingCode? findingCode)
    {
        var valid = recovery.State switch
        {
            IndexRecoveryState.Removed => findingCode is null,
            IndexRecoveryState.Retained => findingCode is IndexFindingCode.RecoveryArtifactRetained
                or IndexFindingCode.RecoveryFailed
                or IndexFindingCode.Interrupted,
            IndexRecoveryState.Unknown => findingCode is IndexFindingCode.RecoveryFailed
                or IndexFindingCode.Interrupted,
            IndexRecoveryState.NotRequired or IndexRecoveryState.NotCreated => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The Index recovery state is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException("Index recovery deletion facts are not coherent.");
        }

        Recovery = recovery;
        FindingCode = findingCode;
    }

    internal IndexRecovery Recovery { get; }

    internal IndexFindingCode? FindingCode { get; }
}

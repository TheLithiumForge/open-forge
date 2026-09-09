using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

internal sealed class RepairPostVerifier(DoctorDiagnosisReader diagnosisReader)
{
    private readonly DoctorDiagnosisReader _diagnosisReader = diagnosisReader;

    internal async ValueTask<(DoctorDiagnosisRead Diagnosis, RepairPostVerification Verification)> VerifyLibraryAsync(
        RepairPlan plan,
        RepairLibraryExecution execution,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(execution);
        if (execution.ReferenceReceipts.IsDefault || execution.LibraryReceipts.IsDefault)
        {
            throw new ArgumentException("Library post-verification must retain every atomic effect receipt.", nameof(execution));
        }

        var diagnosis = await _diagnosisReader.ReadAsync(
            new DoctorRequest(plan.Request.Workspace),
            cancellationToken).ConfigureAwait(false);
        if (plan.Effects.Count == 0)
        {
            var coverage = ReadLibraryCoverage(diagnosis.Observation);
            var selectedState = RepairCoverageMapper.ReadPostDiagnosisState(coverage);
            RepairVerificationState postConditions;
            if (selectedState == RepairPostDiagnosisState.Complete)
            {
                postConditions = RepairVerificationState.Verified;
            }
            else if (selectedState == RepairPostDiagnosisState.Blocked)
            {
                postConditions = RepairVerificationState.Failed;
            }
            else
            {
                postConditions = RepairVerificationState.Planned;
            }

            var libraryVerification = new RepairVerification(
                RepairVerificationState.Verified,
                RepairVerificationState.Verified,
                postConditions);
            return (diagnosis, new RepairPostVerification(
                libraryVerification,
                new RepairPostDiagnosis(selectedState, coverage, []),
                []));
        }

        var completePlanRequired = execution.UnexpectedFailure is null
            && execution.Cancellation is null
            && execution.ReferenceReceipts.Length == plan.Effects.Count;
        var verification = await VerifyCoreAsync(
            plan,
            execution.ReferenceReceipts,
            completePlanRequired,
            diagnosis,
            cancellationToken).ConfigureAwait(false);
        return (diagnosis, verification);
    }

    private static RepairDiagnosisCoverage ReadLibraryCoverage(DoctorObservation observation)
    {
        var ordinary = RepairCoverageMapper.Read(observation);
        var library = ReadCoverage(observation.Libraries.State);
        RepairCoverageState selected;
        if (ordinary.WorkspaceAndPath == RepairCoverageState.Blocked
            || library == RepairCoverageState.Blocked)
        {
            selected = RepairCoverageState.Blocked;
        }
        else if (ordinary.WorkspaceAndPath == RepairCoverageState.Incomplete
            || library == RepairCoverageState.Incomplete)
        {
            selected = RepairCoverageState.Incomplete;
        }
        else
        {
            selected = RepairCoverageState.Complete;
        }

        return new RepairDiagnosisCoverage(
            ordinary.WorkspaceAndPath,
            ordinary.RouteAndHeading,
            ordinary.LocalReferences,
            selected);
    }

    private static RepairCoverageState ReadCoverage(OperationalViewState state)
        => state switch
        {
            OperationalViewState.Complete => RepairCoverageState.Complete,
            OperationalViewState.Incomplete or OperationalViewState.Interrupted => RepairCoverageState.Incomplete,
            OperationalViewState.Blocked => RepairCoverageState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library view state is not defined."),
        };

    internal ValueTask<RepairPostVerification> VerifyAsync(
        RepairPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        CancellationToken cancellationToken)
        => VerifyCoreAsync(plan, receipts, completePlanRequired: true, cancellationToken);

    internal ValueTask<RepairPostVerification> VerifyRetainedAsync(
        RepairPreparedApplication application,
        CancellationToken cancellationToken)
        => VerifyCoreAsync(application.Plan, application.Receipts, completePlanRequired: false, cancellationToken);

    internal static RepairPostVerification Unavailable(RepairPlan plan, IReadOnlyList<FileChangeReceipt> receipts)
        => new(
            new RepairVerification(
                targets: RepairVerificationState.Unknown,
                resultingBytes: RepairVerificationState.Unknown,
                postConditions: RepairVerificationState.Unknown,
                effects: plan.Effects.Select(effect => new RepairEffectVerification(
                    effect, receipts.SingleOrDefault(receipt => ReferenceEquals(receipt.Change, effect.FileChange)),
                    Targets: RepairVerificationState.Unknown, ResultingBytes: RepairVerificationState.Unknown))),
            RepairPostDiagnosis.NotRequested, []);

    private async ValueTask<RepairPostVerification> VerifyCoreAsync(
        RepairPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        bool completePlanRequired,
        CancellationToken cancellationToken)
    {
        var read = await _diagnosisReader.ReadAsync(
            new DoctorRequest(plan.Request.Workspace),
            cancellationToken).ConfigureAwait(false);
        return await VerifyCoreAsync(plan, receipts, completePlanRequired, read, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask<RepairPostVerification> VerifyCoreAsync(
        RepairPlan plan,
        IReadOnlyList<FileChangeReceipt> receipts,
        bool completePlanRequired,
        DoctorDiagnosisRead read,
        CancellationToken cancellationToken)
    {
        var coverage = RepairCoverageMapper.Read(read.Observation);
        var findings = ReadCoverageFindings(coverage).ToList();
        findings.AddRange(RepairRemainingFindingReader.Read(read.Observation.LocalReferences));
        var appliedEffects = plan.Effects.Where(effect => receipts.Any(receipt =>
            ReferenceEquals(receipt.Change, effect.FileChange) && receipt.EffectState == FilesystemEffectState.Applied)).ToArray();
        var effects = new List<RepairEffectVerification>();
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        foreach (var effect in plan.Effects)
        {
            var receipt = receipts.SingleOrDefault(value => ReferenceEquals(value.Change, effect.FileChange));
            var applied = receipt?.EffectState == FilesystemEffectState.Applied;
            var actual = await validator.ValidateAsync(plan.Request.Workspace,
                applied || completePlanRequired ? effect.IntendedState.Expectation : effect.ExpectedState.Expectation, cancellationToken)
                .ConfigureAwait(false);
            var bytes = ReadBytesVerification(receipt, actual.State, completePlanRequired);
            var targets = RepairVerificationState.NotRequested;
            if (applied || completePlanRequired)
            {
                var targetsMatch = plan.Steps.Where(step => ReferenceEquals(step.Effect, effect)).All(step =>
                    read.Observation.LocalReferences.References.Any(reference =>
                        MatchesOccurrence(appliedEffects, step.SelectedProposal, reference)));
                targets = targetsMatch ? RepairVerificationState.Verified : RepairVerificationState.Failed;
            }

            effects.Add(new RepairEffectVerification(effect, receipt, Targets: targets, ResultingBytes: bytes));
        }

        var noOpsVerified = plan.Steps.Where(step => step.NoOp is not null).All(step =>
            read.Observation.LocalReferences.References.Any(reference => MatchesOccurrence(appliedEffects, step.SelectedProposal, reference)));
        var verification = new RepairVerification(
            targets: noOpsVerified ? Aggregate(effects.Select(effect => effect.Targets)) : RepairVerificationState.Failed,
            resultingBytes: Aggregate(effects.Select(effect => effect.ResultingBytes)),
            postConditions: coverage.SelectedScope == RepairCoverageState.Complete
                ? RepairVerificationState.Verified : RepairVerificationState.Planned,
            effects: effects);
        if (verification.Targets is RepairVerificationState.Failed or RepairVerificationState.Unknown
            || verification.ResultingBytes is RepairVerificationState.Failed or RepairVerificationState.Unknown)
        {
            findings.Add(new RepairFinding(
                RepairFindingCode.VerificationFailed,
                "The fresh Repair target or observed file bytes did not match the established application facts."));
        }

        return new RepairPostVerification(
            verification,
            new RepairPostDiagnosis(RepairCoverageMapper.ReadPostDiagnosisState(coverage), coverage, findings),
            findings);
    }

    private static RepairVerificationState ReadBytesVerification(
        FileChangeReceipt? receipt, FileExpectationValidationState actual, bool completePlanRequired)
    {
        if (receipt?.EffectState == FilesystemEffectState.Unknown)
        {
            return RepairVerificationState.Unknown;
        }

        if (actual != FileExpectationValidationState.Matched)
        {
            return RepairVerificationState.Failed;
        }

        return receipt?.EffectState switch
        {
            FilesystemEffectState.Applied => receipt.VerificationState == FilesystemVerificationState.Verified
                ? RepairVerificationState.Verified : RepairVerificationState.Failed,
            FilesystemEffectState.NotStarted or null => completePlanRequired
                ? RepairVerificationState.Failed : RepairVerificationState.NotRequested,
            FilesystemEffectState.Unknown => RepairVerificationState.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.EffectState, "The file effect state is not defined."),
        };
    }

    private static RepairVerificationState Aggregate(IEnumerable<RepairVerificationState> states)
    {
        var values = states.ToArray();
        if (values.Contains(RepairVerificationState.Failed))
        {
            return RepairVerificationState.Failed;
        }

        if (values.Contains(RepairVerificationState.Unknown))
        {
            return RepairVerificationState.Unknown;
        }

        return values.All(value => value == RepairVerificationState.Verified)
            ? RepairVerificationState.Verified : RepairVerificationState.Planned;
    }

    private static bool MatchesOccurrence(IReadOnlyList<RepairEffect> appliedEffects, RepairSelectedProposal selected, LocalReferenceObservation reference)
    {
        var originalOffset = selected.Proposal.Occurrence.ByteOffset;
        var offset = originalOffset + appliedEffects
            .Where(effect => effect.SourceCanonicalPath == selected.Proposal.SourceCanonicalPath)
            .SelectMany(effect => effect.Changes)
            .Where(change => change.Occurrence.ByteOffset < originalOffset)
            .Sum(change => Encoding.UTF8.GetByteCount(change.IntendedDestination) - change.Occurrence.ByteLength);
        return reference.SourcePath == selected.Proposal.SourceCanonicalPath
            && reference.DestinationLocation?.ByteOffset == offset
            && reference.DestinationLocation.ByteLength == Encoding.UTF8.GetByteCount(selected.Resolution.IntendedDestination)
            && reference.Destination == selected.Resolution.IntendedDestination
            && reference.Facts.Target.Path == Uri.UnescapeDataString(selected.Resolution.Target.CanonicalTargetPath)
            && reference.Facts.Target.Resolution == SourceLinkTargetResolution.Complete
            && Uri.UnescapeDataString(reference.Facts.Fragment ?? string.Empty)
                == Uri.UnescapeDataString(selected.Resolution.Target.TargetFragment ?? string.Empty);
    }

    private static IEnumerable<RepairFinding> ReadCoverageFindings(RepairDiagnosisCoverage coverage)
    {
        if (coverage.SelectedScope == RepairCoverageState.Blocked)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisBlocked,
                "Fresh post-diagnosis blocked one required Repair domain.");
        }
        else if (coverage.SelectedScope != RepairCoverageState.Complete)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisIncomplete,
                "Fresh post-diagnosis did not complete every required Repair domain.");
        }
    }
}

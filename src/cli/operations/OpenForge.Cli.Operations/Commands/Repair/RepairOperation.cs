using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Repair;

internal sealed class RepairOperation
{
    private readonly RepairOperationComponents _components;

    internal RepairOperation()
        : this(RepairOperationFactory.CreateDefaultComponents())
    {
    }

    internal RepairOperation(RepairOperationComponents components)
    {
        ArgumentNullException.ThrowIfNull(components);
        _components = components;
    }

    internal async ValueTask<RepairResult> ExecuteAsync(
        RepairRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.HasSelectionAuthority)
        {
            return RepairResultBuilder.SelectionRequired(request);
        }

        IReadOnlyList<RepairFinding> initialFindings = [];
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var diagnosis = await _components.DiagnosisReader.ReadAsync(
                new DoctorRequest(request.Workspace),
                cancellationToken).ConfigureAwait(false);
            var coverage = RepairCoverageMapper.Read(diagnosis);
            var initialScope = RepairAuthoredReferenceBoundary.Filter(
                diagnosis.Observation.LocalReferences.References,
                RepairAuthoredReferenceBoundary.ReadRouteDocuments(diagnosis.Observation.Routes.Sources));
            var libraryFindings = RepairRemainingFindingReader.ReadLibrary(diagnosis.Result.Diagnosis).ToArray();
            initialFindings = [
                .. RepairRemainingFindingReader.Read(initialScope.References),
                .. RepairAuthoredReferenceBoundary.IncompleteFindings(initialScope.IncompleteSources),
                .. libraryFindings,
            ];
            var catalogue = await _components.CatalogueReader.ReadAsync(
                request.Workspace,
                diagnosis.Observation.LocalReferences,
                cancellationToken,
                request.Relinks).ConfigureAwait(false);
            catalogue = catalogue with
            {
                Findings = [.. catalogue.Findings, .. libraryFindings],
            };
            var libraryProposals = RepairLibraryCatalogueReader.Read(diagnosis.Result.Diagnosis);
            var facts = new RepairInteractionFacts(
                request,
                coverage,
                catalogue,
                libraryProposals,
                initialFindings);

            if (request.SelectionMode == RepairSelectionMode.InteractivePrompt)
            {
                return await ExecuteInteractiveAsync(
                    facts,
                    cancellationToken).ConfigureAwait(false);
            }

            var plan = BuildAuthorizedPlan(facts);
            return await CompletePlanAsync(
                facts,
                plan,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return await BoundaryAsync(request, CancelledFinding(), initialFindings).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return await BoundaryAsync(request, new RepairFinding(
                RepairFindingCode.OperationFailed,
                $"Repair failed unexpectedly: {exception.GetType().Name}: {exception.Message}"), initialFindings).ConfigureAwait(false);
        }
    }

    private async ValueTask<RepairResult> ExecuteInteractiveAsync(
        RepairInteractionFacts facts,
        CancellationToken cancellationToken)
    {
        var request = facts.Request;
        var catalogue = facts.Catalogue;
        var libraryProposals = facts.LibraryProposals;
        var policy = new CliPromptPolicy(request.AllowInteraction);
        var promptRelinks = new List<RepairRelinkRequest>();
        var safeInputs = catalogue.Proposals
            .Where(value => value.Proposal.IsSafeExact)
            .ToArray();

        if (CanPlan(facts) && safeInputs.Length != 0)
        {
            var safePlan = RepairPlanner.Build(request, safeInputs);
            var safePreflight = await _components.Application.PreflightAsync(safePlan, cancellationToken)
                .ConfigureAwait(false);
            if (safePreflight.Outcome.Preflight.State == RepairPreflightState.Ready)
            {
                if (request.Mode == RepairMode.Apply && HasChanges(safePlan))
                {
                    var safePreview = Preview(
                        facts,
                        safePlan,
                        ReadFindings(facts, safePlan),
                        safePreflight.Outcome);
                    var confirmation = await _components.Interaction.Confirm(
                        safePreview,
                        new RepairConfirmationQuestion(
                            RepairConfirmationKind.Safe,
                            safePlan.Selection.Selected.Count),
                        policy,
                        cancellationToken).ConfigureAwait(false);
                    if (confirmation.State == CliPromptState.Unavailable)
                    {
                        return await BoundaryAsync(facts, ConfirmationRequiredFinding())
                            .ConfigureAwait(false);
                    }

                    if (confirmation.State != CliPromptState.Answered || !confirmation.Value)
                    {
                        return await BoundaryAsync(facts, CancelledFinding())
                            .ConfigureAwait(false);
                    }
                }
            }

            promptRelinks.AddRange(ToRelinks(safePlan.Selection.Selected));
        }

        foreach (var proposal in catalogue.Proposals.Where(value => value.Proposal.IsGuided)
            .Select(value => value.Proposal))
        {
            var answer = await _components.Interaction.SelectReference(
                new RepairReferencePromptQuestion(proposal),
                policy,
                cancellationToken).ConfigureAwait(false);
            if (answer.State == CliPromptState.Unavailable)
            {
                return await BoundaryAsync(facts, SelectionUnavailableFinding())
                    .ConfigureAwait(false);
            }

            if (answer.State != CliPromptState.Answered)
            {
                return await BoundaryAsync(facts, CancelledFinding())
                    .ConfigureAwait(false);
            }

            if (!answer.Value.Skipped && answer.Value.Relink is { } relink)
            {
                promptRelinks.Add(relink);
            }
        }

        ImmutableArray<RepairLibraryRecoveryProposal> selectedLibraries = [];
        if (!libraryProposals.IsEmpty)
        {
            var selected = ImmutableArray.CreateBuilder<RepairLibraryRecoveryProposal>();
            foreach (var proposal in libraryProposals)
            {
                var answer = await _components.Interaction.SelectLibrary(
                    new RepairLibraryPromptQuestion(proposal),
                    policy,
                    cancellationToken).ConfigureAwait(false);
                if (answer.State == CliPromptState.Unavailable)
                {
                    return await BoundaryAsync(facts, SelectionUnavailableFinding())
                        .ConfigureAwait(false);
                }

                if (answer.State != CliPromptState.Answered)
                {
                    return await BoundaryAsync(facts, CancelledFinding())
                        .ConfigureAwait(false);
                }

                if (answer.Value.Selected)
                {
                    selected.Add(proposal);
                }
            }

            selectedLibraries = selected.ToImmutable();
        }

        var plan = libraryProposals.IsEmpty
            ? RepairPlanner.Build(request, catalogue.Proposals, promptRelinks)
            : RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
            {
                Request = request,
                References = [.. catalogue.Proposals],
                Libraries = libraryProposals,
                PromptRelinks = [.. promptRelinks],
                PromptLibraries = selectedLibraries,
            });
        return await CompletePlanAsync(
            facts,
            plan,
            cancellationToken,
            new RepairConfirmationQuestion(RepairConfirmationKind.Final, CountChanges(plan)),
            policy).ConfigureAwait(false);
    }

    private async ValueTask<RepairResult> CompletePlanAsync(
        RepairInteractionFacts facts,
        RepairPlan plan,
        CancellationToken cancellationToken,
        RepairConfirmationQuestion? confirmationQuestion = null,
        CliPromptPolicy? policy = null)
    {
        var request = facts.Request;
        var findings = ReadFindings(facts, plan);
        var outcome = BeforeApplication();
        IReadOnlyList<FileExpectation> targets = [];
        if (CanPlan(facts) && !plan.IsBlocked)
        {
            var preflight = await _components.Application.PreflightAsync(plan, cancellationToken)
                .ConfigureAwait(false);
            outcome = preflight.Outcome;
            targets = preflight.Targets;
            if (outcome.Preflight.State == RepairPreflightState.Ready && request.Mode == RepairMode.Apply)
            {
                if (request.SelectionMode == RepairSelectionMode.InteractivePrompt
                    && HasChanges(plan)
                    && confirmationQuestion is { } question)
                {
                    var preview = Preview(facts, plan, findings, outcome);
                    var confirmation = await _components.Interaction.Confirm(
                        preview,
                        question,
                        policy ?? new CliPromptPolicy(request.AllowInteraction),
                        cancellationToken).ConfigureAwait(false);
                    if (confirmation.State == CliPromptState.Unavailable)
                    {
                        return await BoundaryAsync(facts, ConfirmationRequiredFinding())
                            .ConfigureAwait(false);
                    }

                    if (confirmation.State != CliPromptState.Answered || !confirmation.Value)
                    {
                        return await BoundaryAsync(facts, CancelledFinding())
                            .ConfigureAwait(false);
                    }
                }

                outcome = await _components.Application.ExecuteAsync(plan, targets, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        if (request.Mode == RepairMode.Apply && outcome.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested)
        {
            outcome = outcome with
            {
                PostDiagnosis = await new RepairBoundaryDiagnosisReader(_components.DiagnosisReader).ReadAsync(request)
                    .ConfigureAwait(false),
            };
        }

        return RepairResultBuilder.Build(new RepairResultInput
        {
            Request = request,
            Diagnosis = facts.Coverage,
            Plan = plan,
            Findings = [.. findings, .. outcome.Findings],
            InitialFindings = facts.InitialFindings,
            Preflight = outcome.Preflight,
            Application = outcome.Application,
            Verification = outcome.Verification,
            Recovery = outcome.Recovery,
            PostDiagnosis = outcome.PostDiagnosis,
            LibraryExecution = outcome.LibraryExecution,
        });
    }

    private static RepairPlan BuildAuthorizedPlan(
        RepairInteractionFacts facts)
        => facts.LibraryProposals.IsEmpty
            ? RepairPlanner.Build(facts.Request, facts.Catalogue.Proposals)
            : RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
            {
                Request = facts.Request,
                References = [.. facts.Catalogue.Proposals],
                Libraries = facts.LibraryProposals,
                PromptRelinks = [],
                PromptLibraries = null,
            });

    private static RepairResult Preview(
        RepairInteractionFacts facts,
        RepairPlan plan,
        IReadOnlyList<RepairFinding> findings,
        RepairApplicationOutcome outcome)
    {
        var request = facts.Request;
        var previewRequest = request.Mode == RepairMode.DryRun
            ? request
            : new RepairRequest(
                request.Workspace,
                RepairMode.DryRun,
                request.Automatic,
                request.Relinks,
                request.AllowInteraction);
        return RepairResultBuilder.Build(new RepairResultInput
        {
            Request = previewRequest,
            Diagnosis = facts.Coverage,
            Plan = plan,
            Findings = [.. findings, .. outcome.Findings],
            InitialFindings = facts.InitialFindings,
            Preflight = outcome.Preflight,
            Application = outcome.Application,
            Verification = outcome.Verification,
            Recovery = outcome.Recovery,
            PostDiagnosis = outcome.PostDiagnosis,
            LibraryExecution = outcome.LibraryExecution,
        });
    }

    private static IReadOnlyList<RepairFinding> ReadFindings(
        RepairInteractionFacts facts,
        RepairPlan plan)
        => [
            .. facts.Catalogue.Findings,
            .. ReadCoverageFindings(facts.Coverage),
            .. plan.Conflicts.Select(RepairResultBuilder.Conflict),
        ];

    private static IReadOnlyList<RepairRelinkRequest> ToRelinks(
        IReadOnlyList<RepairSelectedProposal> selected)
        => [.. selected.Select(value => new RepairRelinkRequest(
            new RepairSourceLocation(
                value.Proposal.SourceCanonicalPath,
                value.Proposal.Occurrence.Line,
                value.Proposal.Occurrence.Column),
            value.Proposal.ExpectedDestination,
            value.Resolution.Target))];

    private static bool CanPlan(RepairInteractionFacts facts)
        => facts.Coverage.SelectedScope == RepairCoverageState.Complete
            && !facts.Catalogue.Findings.Any(finding => finding.Code == RepairFindingCode.DiagnosisIncomplete);

    private static bool HasChanges(RepairPlan plan)
        => plan.Effects.Count != 0 || !plan.LibrarySteps.IsEmpty;

    private static int CountChanges(RepairPlan plan)
        => plan.Selection.Selected.Count;

    private async ValueTask<RepairResult> BoundaryAsync(
        RepairInteractionFacts facts,
        RepairFinding finding)
        => await BoundaryAsync(facts.Request, finding, facts.InitialFindings).ConfigureAwait(false);

    private async ValueTask<RepairResult> BoundaryAsync(
        RepairRequest request,
        RepairFinding finding,
        IReadOnlyList<RepairFinding> initialFindings)
    {
        var diagnosis = await new RepairBoundaryDiagnosisReader(_components.DiagnosisReader)
            .ReadAsync(request).ConfigureAwait(false);
        return RepairResultBuilder.Boundary(request, finding, diagnosis, initialFindings);
    }

    private static RepairFinding CancelledFinding()
        => new(RepairFindingCode.Interrupted, RepairDefinitions.CancelledMessage);

    private static RepairFinding ConfirmationRequiredFinding()
        => new(
            RepairFindingCode.ConfirmationRequired,
            RepairDefinitions.ConfirmationRequiredMessage);

    private static RepairFinding SelectionUnavailableFinding()
        => new(
            RepairFindingCode.SelectionRequired,
            RepairDefinitions.SelectionRequiredMessage);

    private static IEnumerable<RepairFinding> ReadCoverageFindings(RepairDiagnosisCoverage coverage)
    {
        if (coverage.SelectedScope == RepairCoverageState.Blocked)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisBlocked,
                "A required Repair diagnosis domain is blocked.");
        }
        else if (coverage.SelectedScope != RepairCoverageState.Complete)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisIncomplete,
                "A required Repair diagnosis domain is incomplete.");
        }
    }

    private static RepairApplicationOutcome BeforeApplication()
        => new(
            RepairPreflight.NotRequested,
            RepairApplication.NotRequested,
            RepairVerification.NotRequested,
            RepairRecovery.NotRequired,
            RepairPostDiagnosis.NotRequested,
            []);
}

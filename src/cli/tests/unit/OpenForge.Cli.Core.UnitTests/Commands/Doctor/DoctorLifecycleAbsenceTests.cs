using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorLifecycleAbsenceTests
{
    [Fact(DisplayName = "The complete operational proof establishes Framework absence without claiming Extension absence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task CompleteOperationalProofEstablishesOnlyFrameworkAbsence()
    {
        var support = new DoctorOperationTestSupport();
        support.ExtensionLifecycle.View = MissingExtensionView();

        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        var framework = Assert.Single(result.Diagnosis.Domains, domain =>
            domain.Domain == DoctorDomainKind.FrameworkLifecycle);
        Assert.Equal(OperationalLifecycleState.Absent, framework.Lifecycle);
        Assert.Contains(framework.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkInstallAbsent);
        Assert.DoesNotContain(framework.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkInstallIncomplete);
        var action = Assert.Single(framework.Actions);
        Assert.Equal(DoctorNextOperation.Install, action.Operation);

        var extension = Assert.Single(result.Diagnosis.Domains, domain =>
            domain.Domain == DoctorDomainKind.ExtensionLifecycle);
        var finding = Assert.Single(extension.Findings, candidate =>
            candidate.Kind == DoctorFindingKind.ExtensionLifecycleDocumentMissing);
        Assert.Equal(OperationalLifecycleState.Incomplete, extension.Lifecycle);
        Assert.Equal(OperationalSourceAvailability.Unavailable, extension.SourceAvailability);
        Assert.DoesNotContain(finding.Evidence, evidence =>
            evidence is DoctorStateEvidence { State: DoctorObservedState.Absent });
        Assert.Empty(extension.Actions);
    }

    [Fact(DisplayName = "Missing lifecycle documents without safely absent routes remain incomplete")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task IncompleteRouteInventoryPreventsFrameworkAbsence()
    {
        var support = new DoctorOperationTestSupport();
        support.Routes.SourceInventory = RouteSourceInventoryState.Incomplete;
        support.ExtensionLifecycle.View = MissingExtensionView();

        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        var framework = Assert.Single(result.Diagnosis.Domains, domain =>
            domain.Domain == DoctorDomainKind.FrameworkLifecycle);
        Assert.Equal(OperationalLifecycleState.Incomplete, framework.Lifecycle);
        Assert.Contains(framework.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkInstallIncomplete);
        Assert.DoesNotContain(framework.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkInstallAbsent);
        Assert.Empty(framework.Actions);
    }

    [Fact(DisplayName = "A recovery residual prevents Framework absence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task RecoveryResidualPreventsFrameworkAbsence()
    {
        var support = new DoctorOperationTestSupport();
        var draft = RecoveryBundleCandidateSnapshot.IncompleteDraft(Path.Combine(
            DoctorOperationTestSupport.Workspace().LexicalRoot,
            "operation-00000000000000000000000000000000.draft"));
        support.RecoveryResiduals.View = new RecoveryResidualDoctorView(
            OperationalViewState.Complete,
            [RecoveryDoctorCandidateObservation.Create(draft, comparison: null)],
            Cause: null);
        support.ExtensionLifecycle.View = MissingExtensionView();

        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        var framework = Assert.Single(result.Diagnosis.Domains, domain =>
            domain.Domain == DoctorDomainKind.FrameworkLifecycle);
        Assert.Equal(OperationalLifecycleState.Incomplete, framework.Lifecycle);
        Assert.DoesNotContain(framework.Findings, finding =>
            finding.Kind == DoctorFindingKind.FrameworkInstallAbsent);
        Assert.Empty(framework.Actions);
    }

    private static ExtensionLifecycleDoctorView MissingExtensionView()
        => ExtensionLifecycleDoctorView.Create(
            ExtensionLifecycleDoctorAssessment.Create(
                OperationalViewState.Incomplete,
                ExtensionLifecycleSectionState.DocumentMissing,
                ExtensionManagedSetState.Empty,
                OperationalLifecycleState.Incomplete,
                OperationalSourceAvailability.Unavailable),
            new LifecycleReadResult(
                LifecycleReadState.Missing,
                LifecycleExtensionTrust.Incomplete,
                [],
                "The lifecycle document is missing."),
            [],
            DoctorOperationTestSupport.CreateCompleteOwnership(),
            ExtensionLifecycleDoctorFacts.Create([], []));
}

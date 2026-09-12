using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorOperationTests
{
    [Fact(DisplayName = "Doctor reads each contributor once in the frozen domain order and retains every domain"), Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task ReadsEachContributorOnceInFrozenOrderAndRetainsEveryDomain()
    {
        var support = new DoctorOperationTestSupport();
        var workspace = DoctorOperationTestSupport.Workspace();
        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(new DoctorRequest(workspace), CancellationToken.None);

        Assert.Equal(
            [
                "workspace-entry",
                "recovery-residuals",
                "routes",
                "local-references",
                "framework-lifecycle",
                "extension-lifecycle",
            ],
            support.Calls);
        Assert.Equal(6, support.Calls.Count);
        Assert.Equal("doctor", result.Command);
        Assert.Equal(workspace, result.Workspace);
        Assert.Equal(
            [
                DoctorDomainKind.WorkspaceEntry,
                DoctorDomainKind.RecoveryResiduals,
                DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                DoctorDomainKind.LocalReferences,
                DoctorDomainKind.FrameworkLifecycle,
                DoctorDomainKind.ExtensionLifecycle,
            ],
            result.Diagnosis.Domains.Select(domain => domain.Domain));
        Assert.True(DoctorDiagnosis.ReadOnly);
        Assert.False(DoctorDiagnosis.ChangesMade);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
    }

    [Fact(DisplayName = "Doctor consumes every supplied Extension source observation without first-source fallback"), Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public async Task ConsumesEveryExtensionSourceObservationWithoutFallback()
    {
        var support = new DoctorOperationTestSupport();
        support.ExtensionLifecycle.View = CreatePluralSourceView();
        var result = await new DoctorOperation(support.Catalogue)
            .ExecuteAsync(
                new DoctorRequest(DoctorOperationTestSupport.Workspace()),
                CancellationToken.None);

        var extension = Assert.Single(
            result.Diagnosis.Domains,
            domain => domain.Domain == DoctorDomainKind.ExtensionLifecycle);
        Assert.Equal(
            [
                DoctorFindingKind.ExtensionSourceUnavailable,
                DoctorFindingKind.ExtensionCatalogueUnavailable,
            ],
            extension.Findings.Select(finding => finding.Kind));
        Assert.Equal(
            ["missing-source", "defective-catalogue"],
            extension.Findings.Select(finding => finding.Provenance.Path));
        Assert.Equal(DoctorCoverageState.Incomplete, extension.Coverage);
    }

    private static ExtensionLifecycleDoctorView CreatePluralSourceView()
    {
        var packages = new[]
        {
            new LifecycleInstalledPackage
            {
                Id = "embedded-package",
                Version = "1.0.0",
                Source = null,
                Dependencies = [],
                Paths = [],
            },
            new LifecycleInstalledPackage
            {
                Id = "available-package",
                Version = "1.0.0",
                Source = "available-source",
                Dependencies = [],
                Paths = [],
            },
            new LifecycleInstalledPackage
            {
                Id = "missing-package",
                Version = "1.0.0",
                Source = "missing-source",
                Dependencies = [],
                Paths = [],
            },
            new LifecycleInstalledPackage
            {
                Id = "defective-package",
                Version = "1.0.0",
                Source = "defective-catalogue",
                Dependencies = [],
                Paths = [],
            },
        };
        return ExtensionLifecycleDoctorView.Create(
            ExtensionLifecycleDoctorAssessment.Create(
                OperationalViewState.Complete,
                ExtensionLifecycleSectionState.Present,
                ExtensionManagedSetState.Empty,
                OperationalLifecycleState.Trusted,
                OperationalSourceAvailability.Available),
            new LifecycleReadResult(
                LifecycleReadState.Complete,
                LifecycleExtensionTrust.Trusted,
                packages,
                cause: null),
            [
                new ExtensionSourceObservation(
                    RecordedSource: null,
                    DoctorOperationTestSupport.Source(
                        ExtensionSourceReadState.Complete,
                        ExtensionSourceKind.EmbeddedCatalogue,
                        "embedded-source",
                        cause: null)),
                new ExtensionSourceObservation(
                    "available-source",
                    DoctorOperationTestSupport.Source(
                        ExtensionSourceReadState.Complete,
                        ExtensionSourceKind.Package,
                        "available-source",
                        cause: null)),
                new ExtensionSourceObservation(
                    "missing-source",
                    DoctorOperationTestSupport.Source(
                        ExtensionSourceReadState.Missing,
                        ExtensionSourceKind.Package,
                        "missing-source",
                        "The package source is missing.",
                        ExtensionSourceFailureKind.Unavailable)),
                new ExtensionSourceObservation(
                    "defective-catalogue",
                    DoctorOperationTestSupport.Source(
                        ExtensionSourceReadState.Invalid,
                        ExtensionSourceKind.Catalogue,
                        "defective-catalogue",
                        "The catalogue is invalid.",
                        ExtensionSourceFailureKind.Invalid)),
            ],
            DoctorOperationTestSupport.CreateCompleteOwnership(),
            ExtensionLifecycleDoctorFacts.Create([], []));
    }
}

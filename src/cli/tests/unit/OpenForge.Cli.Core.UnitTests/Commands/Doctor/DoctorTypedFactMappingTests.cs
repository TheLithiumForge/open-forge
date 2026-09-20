using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorTypedFactMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor retains each exact target from one partial Framework recovery final")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void PartialFrameworkRecoveryRetainsPerTargetComparisonEvidence()
    {
        var workspace = DoctorOperationTestSupport.Workspace();
        var attribution = RecoveryBundleAttribution.Create(
            RecoveryBundleProducer.Framework,
            RecoveryBundleOperation.Install,
            workspace);
        var bundlePath = Path.Combine(workspace.PhysicalRoot, "framework-recovery.zip");
        var verified = new RecoveryBundleVerifiedRead
        {
            BundlePath = bundlePath,
            WorkspacePhysicalPath = workspace.PhysicalRoot,
            WorkspaceKey = WorkspaceIdentity.Key(workspace.PhysicalRoot),
            Command = "open-forge install",
            Attribution = attribution,
            OperationId = Guid.NewGuid(),
            Entries = [],
        };
        var comparison = RecoveryBundleComparison.Create(
            bundlePath,
            attribution,
            ImmutableArray.Create(
                RecoveryBundleTargetComparison.Prior(
                    "first.md",
                    RecoveryContentIdentity.FromBytes("prior"u8)),
                RecoveryBundleTargetComparison.Intended(
                    "second.md",
                    observed: null)));
        var candidate = RecoveryDoctorCandidateObservation.Create(
            RecoveryBundleCandidateSnapshot.VerifiedFinal(verified),
            comparison);
        var findings = new List<DoctorFinding>();

        FrameworkRecoveryDoctorInspector.Inspect(
            new RecoveryResidualDoctorView(
                OperationalViewState.Complete,
                [candidate],
                Cause: null),
            findings);

        var finding = Assert.Single(findings);
        Assert.Equal(DoctorFindingKind.FrameworkPartialRecovery, finding.Kind);
        Assert.Equal(2, finding.Evidence.Count);
        Assert.All(finding.Evidence, evidence => Assert.IsType<DoctorComparisonEvidence>(evidence));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor maps exact Extension dependency-list drift with source and installed evidence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ExtensionDependencyComparisonRetainsExpectedAndActualLists()
    {
        var installed = new InstalledExtensionObservation
        {
            Id = "example",
            Version = "1.0.0",
            Source = "catalogue/example",
            SourceAvailability = OperationalSourceAvailability.Available,
            Dependencies = ["installed"],
            Paths = [],
        };
        var source = ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = "example",
                Name = "Example",
                Description = "Example package",
                Version = "1.0.0",
                Dependencies = ["source"],
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = "extension.json",
                Payload = [],
            });
        var findings = new List<DoctorFinding>();

        ExtensionPackageDoctorInspector.Inspect(
            [ExtensionInstalledPackageComparison.DependencyMismatch(installed, source)],
            findings);

        var finding = Assert.Single(findings);
        Assert.Equal(DoctorFindingKind.ExtensionDependencyIncompatible, finding.Kind);
        var evidence = Assert.IsType<DoctorComparisonEvidence>(Assert.Single(finding.Evidence));
        Assert.Equal("source", evidence.Expected);
        Assert.Equal("installed", evidence.Actual);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor retains every exact producer-owned candidate basis")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void CandidateProjectionRetainsAllFourTypedBases()
    {
        const string sourcePath = ".agents/source.md";
        const string destination = "target.md";
        var reference = new LocalReferenceObservation
        {
            SourcePath = sourcePath,
            RoutePath = sourcePath,
            Kind = LocalReferenceKind.Link,
            Destination = destination,
            Label = MarkdownLinkLabelFact.Supported("Target"),
            Location = null,
            DestinationLocation = null,
            Facts = new SourceLinkDestinationFacts
            {
                Fragment = null,
                Target = new SourceLinkTarget
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = ".agents/target.md",
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Missing,
                    Network = null,
                },
                Finding = null,
            },
            Fragment = LocalReferenceFragmentObservation.NotRequested(),
            Canonicalizations = [],
        };
        var candidate = LocalReferenceCandidateObservation.Create(
            sourcePath,
            destination,
            new SourceLinkIdentity
            {
                Id = "target",
                Path = ".agents/target.md",
            },
            Enum.GetValues<LocalReferenceCandidateBasisKind>()
                .Select(kind => LocalReferenceCandidateBasis.Create(
                    kind,
                    kind.ToString(),
                    location: null))
                .ToArray());

        var set = LocalReferenceDoctorCandidateProjector.Project(
            reference,
            [candidate]);

        var item = Assert.Single(set.Items);
        Assert.Equal(
            Enum.GetValues<DoctorCandidateBasisKind>(),
            item.Evidence.Select(evidence => evidence.Kind));
    }
}

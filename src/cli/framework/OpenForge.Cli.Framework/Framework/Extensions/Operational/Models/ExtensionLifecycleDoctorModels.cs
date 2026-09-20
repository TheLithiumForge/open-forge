using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal sealed class ExtensionLifecycleDoctorFacts
{
    private ExtensionLifecycleDoctorFacts(
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets,
        IReadOnlyList<ExtensionInstalledPackageComparison> packages,
        ExtensionBridgeRegistrationFacts bridgeRegistrations)
    {
        Targets = targets;
        Packages = packages;
        BridgeRegistrations = bridgeRegistrations;
    }

    internal IReadOnlyList<ExtensionManagedTargetDoctorObservation> Targets { get; }

    internal IReadOnlyList<ExtensionInstalledPackageComparison> Packages { get; }

    internal ExtensionBridgeRegistrationFacts BridgeRegistrations { get; }

    internal static ExtensionLifecycleDoctorFacts Create(
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets,
        IReadOnlyList<ExtensionInstalledPackageComparison> packages,
        ExtensionBridgeRegistrationFacts? bridgeRegistrations = null)
    {
        ArgumentNullException.ThrowIfNull(targets);
        ArgumentNullException.ThrowIfNull(packages);
        if (targets.Any(target => target is null)
            || packages.Any(package => package is null))
        {
            throw new ArgumentException(
                "Extension Doctor facts cannot contain null observations.");
        }

        return new ExtensionLifecycleDoctorFacts(
            targets.ToArray(),
            packages.ToArray(),
            bridgeRegistrations ?? ExtensionBridgeRegistrationFacts.Incomplete(
                "Typed Extension bridge-registration observations are unavailable."));
    }
}

internal sealed class ExtensionLifecycleDoctorView
{
    private ExtensionLifecycleDoctorView(
        ExtensionLifecycleDoctorAssessment assessment,
        IReadOnlyList<ExtensionSourceObservation> sources,
        WorkspaceOwnershipRead ownership,
        ExtensionLifecycleDoctorFacts facts)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(facts);
        if (sources.Any(source => source is null))
        {
            throw new ArgumentException(
                "Extension Doctor source observations cannot contain null members.",
                nameof(sources));
        }

        if (facts.Targets.Count == 0 != (assessment.ManagedSet == ExtensionManagedSetState.Empty))
        {
            throw new ArgumentException(
                "The Extension managed-set state must identify an empty target set exactly.",
                nameof(facts));
        }

        Assessment = assessment;
        Sources = sources.ToArray();
        Ownership = ownership;
        Targets = facts.Targets;
        Packages = facts.Packages;
        BridgeRegistrations = facts.BridgeRegistrations;
    }

    internal ExtensionLifecycleDoctorAssessment Assessment { get; }

    internal OperationalViewState State => Assessment.State;

    internal IReadOnlyList<ExtensionSourceObservation> Sources { get; }

    internal WorkspaceOwnershipRead Ownership { get; }

    internal ExtensionLifecycleSectionState Section => Assessment.Section;

    internal IReadOnlyList<ExtensionManagedTargetDoctorObservation> Targets { get; }

    internal IReadOnlyList<ExtensionInstalledPackageComparison> Packages { get; }

    internal ExtensionBridgeRegistrationFacts BridgeRegistrations { get; }

    internal ExtensionManagedSetState ManagedSet => Assessment.ManagedSet;

    internal OperationalLifecycleState LifecycleState => Assessment.Lifecycle;

    internal OperationalSourceAvailability SourceAvailability => Assessment.SourceAvailability;

    internal static ExtensionLifecycleDoctorView Create(
        ExtensionLifecycleDoctorAssessment assessment,
        IReadOnlyList<ExtensionSourceObservation> sources,
        WorkspaceOwnershipRead ownership,
        ExtensionLifecycleDoctorFacts facts)
        => new(assessment, sources, ownership, facts);
}

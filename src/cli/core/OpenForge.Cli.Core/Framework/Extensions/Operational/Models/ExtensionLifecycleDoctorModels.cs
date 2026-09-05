using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal sealed class ExtensionLifecycleDoctorFacts
{
    private ExtensionLifecycleDoctorFacts(
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets,
        IReadOnlyList<ExtensionInstalledPackageComparison> packages)
    {
        Targets = targets;
        Packages = packages;
    }

    internal IReadOnlyList<ExtensionManagedTargetDoctorObservation> Targets { get; }

    internal IReadOnlyList<ExtensionInstalledPackageComparison> Packages { get; }

    internal static ExtensionLifecycleDoctorFacts Create(
        IReadOnlyList<ExtensionManagedTargetDoctorObservation> targets,
        IReadOnlyList<ExtensionInstalledPackageComparison> packages)
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
            packages.ToArray());
    }
}

internal sealed class ExtensionLifecycleDoctorView
{
    private ExtensionLifecycleDoctorView(
        ExtensionLifecycleDoctorAssessment assessment,
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionSourceObservation> sources,
        LifecycleOwnershipReadResult ownership,
        ExtensionLifecycleDoctorFacts facts)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(lifecycle);
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
        Lifecycle = lifecycle;
        Sources = sources.ToArray();
        Ownership = ownership;
        Targets = facts.Targets;
        Packages = facts.Packages;
    }

    internal ExtensionLifecycleDoctorAssessment Assessment { get; }

    internal OperationalViewState State => Assessment.State;

    internal LifecycleReadResult Lifecycle { get; }

    internal IReadOnlyList<ExtensionSourceObservation> Sources { get; }

    internal LifecycleOwnershipReadResult Ownership { get; }

    internal ExtensionLifecycleSectionState Section => Assessment.Section;

    internal IReadOnlyList<ExtensionManagedTargetDoctorObservation> Targets { get; }

    internal IReadOnlyList<ExtensionInstalledPackageComparison> Packages { get; }

    internal ExtensionManagedSetState ManagedSet => Assessment.ManagedSet;

    internal OperationalLifecycleState LifecycleState => Assessment.Lifecycle;

    internal OperationalSourceAvailability SourceAvailability => Assessment.SourceAvailability;

    internal static ExtensionLifecycleDoctorView Create(
        ExtensionLifecycleDoctorAssessment assessment,
        LifecycleReadResult lifecycle,
        IReadOnlyList<ExtensionSourceObservation> sources,
        LifecycleOwnershipReadResult ownership,
        ExtensionLifecycleDoctorFacts facts)
        => new(assessment, lifecycle, sources, ownership, facts);
}

using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionPackageDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.ExtensionLifecycle;

    internal static void Inspect(
        IReadOnlyList<ExtensionInstalledPackageComparison> packages,
        ICollection<DoctorFinding> findings)
    {
        foreach (var package in packages)
        {
            var descriptor = ReadDescriptor(package);
            if (descriptor is null)
            {
                continue;
            }

            var evidence = package.State == ExtensionInstalledPackageComparisonState.DependencyMismatch
                && package.SourcePackage is { } dependencySource
                ? new DoctorEvidence[]
                {
                    new DoctorComparisonEvidence(
                        string.Join(",", dependencySource.Dependencies),
                        string.Join(",", package.Installed.Dependencies)),
                }
                : package.SourcePackage is { } source
                ? new DoctorEvidence[]
                {
                    new DoctorComparisonEvidence(
                        source.Version,
                        package.Installed.Version ?? "unavailable"),
                }
                : new DoctorEvidence[]
                {
                    new DoctorAvailabilityEvidence(package.Installed.SourceAvailability),
                };
            findings.Add(DoctorDomainSupport.Create(
                descriptor,
                DoctorDomainSupport.Subject(
                    DoctorSubjectKind.Extension,
                    package.Installed.Source,
                    package.Installed.Id),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.ExtensionLifecycle,
                    package.Installed.Source),
                evidence));
        }
    }

    private static DoctorFindingDescriptor? ReadDescriptor(
        ExtensionInstalledPackageComparison package)
    {
        if (string.IsNullOrWhiteSpace(package.Installed.Version))
        {
            return DoctorDomainSupport.Warning(
                DoctorFindingKind.ExtensionVersionInvalid,
                "The installed Extension version is unavailable.",
                DoctorResolutionLane.ManualDecision);
        }

        return package.State switch
        {
            ExtensionInstalledPackageComparisonState.Current => null,
            ExtensionInstalledPackageComparisonState.Missing => DoctorDomainSupport.Warning(
                DoctorFindingKind.ExtensionUnknownId,
                package.Cause ?? "The installed Extension identity is unknown to its exact source.",
                DoctorResolutionLane.ManualDecision),
            ExtensionInstalledPackageComparisonState.VersionMismatch =>
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.ExtensionVersionInvalid,
                    package.Cause ?? "The installed Extension version conflicts with its exact source.",
                    DoctorResolutionLane.ManualDecision),
            ExtensionInstalledPackageComparisonState.DependencyMismatch =>
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.ExtensionDependencyIncompatible,
                    package.Cause ?? "The installed Extension dependencies conflict with its exact source.",
                    DoctorResolutionLane.ManualDecision),
            ExtensionInstalledPackageComparisonState.Ambiguous => DoctorDomainSupport.Error(
                DoctorFindingKind.ExtensionDuplicateId,
                package.Cause ?? "The exact Extension source has a duplicate identity."),
            ExtensionInstalledPackageComparisonState.SourceUnavailable => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(package),
                package.State,
                "The Extension package comparison state is not defined."),
        };
    }
}

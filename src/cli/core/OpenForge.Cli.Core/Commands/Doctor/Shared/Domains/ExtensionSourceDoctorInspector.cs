using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionSourceDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.ExtensionLifecycle;

    internal static void Inspect(
        ExtensionSourceObservation source,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        var read = source.Read;
        if (read.State == ExtensionSourceReadState.Complete)
        {
            return;
        }

        var descriptor = ReadDescriptor(read);
        var path = source.RecordedSource ?? read.Identity;
        if (descriptor is not null)
        {
            findings.Add(DoctorDomainSupport.Create(
                descriptor,
                DoctorDomainSupport.Subject(DoctorSubjectKind.Extension, path, path),
                DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.ExtensionSource, path),
                [new DoctorStateEvidence(ReadState(read.State))]));
        }
        else
        {
            var sourceCoverage = read.State == ExtensionSourceReadState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete;
            limitations.Add(DoctorDomainSupport.Limitation(
                sourceCoverage,
                read.Cause ?? $"Extension source {path} is unavailable without a safely distinguishable finding kind."));
        }

        coverage = DoctorDomainSupport.Combine(
            coverage,
            read.State == ExtensionSourceReadState.Blocked ? DoctorCoverageState.Blocked : DoctorCoverageState.Incomplete);
    }

    private static DoctorFindingDescriptor? ReadDescriptor(ExtensionSourceReadResult read)
    {
        if (read.FailureKind == ExtensionSourceFailureKind.DependencyIncomplete)
        {
            return DoctorDomainSupport.Warning(
                DoctorFindingKind.ExtensionDependencyMissing,
                read.Cause ?? "An Extension dependency is incomplete.",
                DoctorResolutionLane.ManualDecision);
        }

        if (read.FailureKind == ExtensionSourceFailureKind.DependencyCycle)
        {
            return DoctorDomainSupport.Error(DoctorFindingKind.ExtensionDependencyCycle, read.Cause ?? "Extension dependencies contain a cycle.");
        }

        if (read.FailureKind == ExtensionSourceFailureKind.DependencyConflict)
        {
            return DoctorDomainSupport.Warning(
                DoctorFindingKind.ExtensionDependencyIncompatible,
                read.Cause ?? "Extension dependency requirements conflict.",
                DoctorResolutionLane.ManualDecision);
        }

        if (read.FailureKind == ExtensionSourceFailureKind.IdentityAmbiguous)
        {
            return DoctorDomainSupport.Error(
                DoctorFindingKind.ExtensionDuplicateId,
                read.Cause ?? "The Extension source contains an ambiguous package identity.");
        }

        if (read.Kind == ExtensionSourceKind.Catalogue)
        {
            if (read.FailureKind == ExtensionSourceFailureKind.ManifestMissing)
            {
                return DoctorDomainSupport.Warning(
                    DoctorFindingKind.ExtensionManifestMissing,
                    read.Cause ?? "An expected Extension manifest is missing.",
                    DoctorResolutionLane.ManualDecision);
            }

            return DoctorDomainSupport.Error(DoctorFindingKind.ExtensionCatalogueUnavailable, read.Cause ?? "The Extension catalogue is unavailable.");
        }

        if (read.FailureKind is ExtensionSourceFailureKind.PackageInvalid
            || read.State == ExtensionSourceReadState.Invalid && read.Kind == ExtensionSourceKind.Package)
        {
            return DoctorDomainSupport.Error(DoctorFindingKind.ExtensionManifestMalformed, read.Cause ?? "The Extension package manifest is malformed.");
        }

        return read.State is ExtensionSourceReadState.Missing or ExtensionSourceReadState.Unavailable
            ? DoctorDomainSupport.Information(
                DoctorFindingKind.ExtensionSourceUnavailable,
                read.Cause ?? "The Extension source is unavailable.")
            : null;
    }

    private static DoctorObservedState ReadState(ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Missing => DoctorObservedState.Missing,
            ExtensionSourceReadState.Invalid => DoctorObservedState.Invalid,
            ExtensionSourceReadState.Blocked => DoctorObservedState.Blocked,
            ExtensionSourceReadState.Unavailable => DoctorObservedState.Unavailable,
            ExtensionSourceReadState.Cancelled => DoctorObservedState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A complete Extension source does not produce a finding."),
        };
}

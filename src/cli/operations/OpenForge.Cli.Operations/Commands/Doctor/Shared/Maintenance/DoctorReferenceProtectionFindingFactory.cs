using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;

internal static class DoctorReferenceProtectionFindingFactory
{
    internal static DoctorFinding? Read(LocalReferenceObservation reference, RoutedMetadataPolicyContext policy)
    {
        if (reference.Facts.Target.Kind != SourceLinkTargetKind.Local
            || reference.Facts.Target.Resolution == SourceLinkTargetResolution.Complete && reference.Canonicalizations.Count == 0) return null;
        var descriptor = LocalReferenceDoctorDescriptorReader.Read(reference);
        if (descriptor is null) return null;
        var path = ProtectedPath(reference, policy);
        if (path is null) return null;
        return Protect(LocalReferenceDoctorFindingFactory.Create(reference, descriptor), path);
    }

    internal static DoctorFinding ProtectCanonicalization(LocalReferenceObservation reference, DoctorFinding finding, RoutedMetadataPolicyContext policy)
        => ProtectedPath(reference, policy) is { } path
            ? Protect(finding, path)
            : finding;

    private static DoctorFinding Protect(DoctorFinding finding, string path)
        => finding with
        {
            ProtectedReferencePath = path,
            Resolution = DoctorResolutionLane.ManualDecision,
            Proposal = null,
            Actions = [],
        };

    private static string? ProtectedPath(LocalReferenceObservation reference, RoutedMetadataPolicyContext policy)
    {
        if (!ReferenceProtectionReader.Allows(reference.SourcePath, policy)) return reference.SourcePath;
        if (!reference.Facts.IsFolder && reference.Facts.Target.Path is { } target && !ReferenceProtectionReader.Allows(target, policy)) return target;
        return null;
    }
}

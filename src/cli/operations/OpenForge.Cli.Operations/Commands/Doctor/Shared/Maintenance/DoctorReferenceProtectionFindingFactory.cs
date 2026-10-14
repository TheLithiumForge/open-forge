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
        var path = ProtectedPath(reference, policy);
        if (path is null) return null;
        var protection = ReferenceProtectionReader.Read(path, policy);
        var action = protection.Owner is { } owner ? DoctorMaintenanceReader.ManagingUpdate(owner) : DoctorMaintenanceGuidance.Manual(path);
        var descriptor = LocalReferenceDoctorDescriptorReader.Read(reference);
        return LocalReferenceDoctorFindingFactory.Create(reference,
            DoctorDomainSupport.Warning(descriptor?.Kind ?? DoctorFindingKind.ReferenceTargetMissing,
                "The reference correction crosses a protected content boundary.",
                protection.Owner is null ? DoctorResolutionLane.ManualDecision : DoctorResolutionLane.TargetedOperation, action)) with
        {
            Maintenance = new()
            {
                Condition = DoctorMaintenanceCondition.ProtectedReference,
                ProtectedPath = path,
                ProtectedByNativePackage = protection.Protection == RoutedMetadataEligibility.NativePackage,
            },
        };
    }

    private static string? ProtectedPath(LocalReferenceObservation reference, RoutedMetadataPolicyContext policy)
    {
        if (!ReferenceProtectionReader.Allows(reference.SourcePath, policy)) return reference.SourcePath;
        if (reference.Facts.Target.Path is { } target && !ReferenceProtectionReader.Allows(target, policy)) return target;
        return null;
    }
}

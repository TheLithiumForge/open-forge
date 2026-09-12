using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class LocalReferenceDoctorDescriptorReader
{
    internal static DoctorFindingDescriptor? Read(LocalReferenceObservation observation)
    {
        if (observation.Fragment.State == LocalReferenceFragmentState.Unverified)
        {
            return DoctorDomainSupport.Error(
                DoctorFindingKind.ReferenceFragmentUnverified,
                observation.Facts.Finding?.Cause ?? "The target fragment could not be verified.");
        }

        if (observation.Fragment.State == LocalReferenceFragmentState.CanonicalCorrection)
        {
            return null;
        }

        return observation.Facts.Target.Resolution switch
        {
            SourceLinkTargetResolution.Complete => DoctorDomainSupport.Information(
                DoctorFindingKind.ReferenceTargetValid,
                "The local reference target is valid."),
            SourceLinkTargetResolution.Missing => DoctorDomainSupport.Warning(
                DoctorFindingKind.ReferenceTargetMissing,
                observation.Facts.Finding?.Cause ?? "The local reference target is missing.",
                DoctorResolutionLane.GuidedChoice),
            SourceLinkTargetResolution.FragmentMissing => DoctorDomainSupport.Warning(
                DoctorFindingKind.ReferenceFragmentMissing,
                observation.Facts.Finding?.Cause ?? "The local target fragment is missing.",
                DoctorResolutionLane.GuidedChoice),
            SourceLinkTargetResolution.Malformed => DoctorDomainSupport.Warning(DoctorFindingKind.ReferenceDestinationMalformed, "The destination is malformed.", DoctorResolutionLane.ManualDecision),
            SourceLinkTargetResolution.Absolute => DoctorDomainSupport.Warning(
                DoctorFindingKind.ReferenceDestinationAbsolute,
                "The destination uses an unsupported absolute local form.",
                DoctorResolutionLane.ManualDecision),
            SourceLinkTargetResolution.Query => DoctorDomainSupport.Warning(
                DoctorFindingKind.ReferenceDestinationQuery,
                "The destination contains an unsupported query.",
                DoctorResolutionLane.ManualDecision),
            SourceLinkTargetResolution.EncodingUnsupported => DoctorDomainSupport.Error(DoctorFindingKind.ReferenceDestinationEncoding, "The destination encoding is unsupported."),
            SourceLinkTargetResolution.OutsideWorkspace => DoctorDomainSupport.Error(DoctorFindingKind.ReferenceTargetOutsideWorkspace, "The target leaves the selected workspace."),
            SourceLinkTargetResolution.PhysicalEscape => DoctorDomainSupport.Error(DoctorFindingKind.ReferenceTargetPhysicalEscape, "The target resolves outside the physical workspace boundary."),
            SourceLinkTargetResolution.Ambiguous => DoctorDomainSupport.Error(DoctorFindingKind.ReferenceTargetAlias, "The target identity is ambiguous."),
            SourceLinkTargetResolution.Unreadable => DoctorDomainSupport.Error(DoctorFindingKind.ReferenceTargetUnreadable, "The target cannot be read completely."),
            SourceLinkTargetResolution.Unsupported => DoctorDomainSupport.Information(DoctorFindingKind.ReferenceTargetUnsupported, "The target kind is outside the supported local boundary."),
            SourceLinkTargetResolution.ExternalUnchecked => DoctorDomainSupport.Information(DoctorFindingKind.ReferenceExternalUnchecked, "The external target was not fetched."),
            _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.Facts.Target.Resolution, "The target resolution is not defined."),
        };
    }
}

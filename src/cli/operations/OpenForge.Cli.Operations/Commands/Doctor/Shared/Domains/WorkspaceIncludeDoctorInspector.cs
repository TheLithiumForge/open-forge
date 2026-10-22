using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceIncludeDoctorInspector
{
    internal static void Inspect(RouteIncludedFilesObservation? includes, ICollection<DoctorFinding> findings)
    {
        foreach (var file in includes?.Files ?? [])
        {
            if (file.Missing)
                findings.Add(Create(file, DoctorDomainSupport.Error(DoctorFindingKind.WorkspaceRootMissing,
                    "The included file is missing. Restore the file or exclude its path.", DoctorResolutionLane.ManualDecision)));
            else if (file.Registered && file.Rejection is { } rejection)
                findings.Add(Create(file, DoctorDomainSupport.Warning(DoctorFindingKind.WorkspacePathInvalid,
                    $"{rejection} Update or remove its registration.", DoctorResolutionLane.ManualDecision)));

            if (file.Registered && !file.Listed)
                findings.Add(Create(file, DoctorDomainSupport.Information(DoctorFindingKind.WorkspaceDetached,
                    "The registered include has no loader row, so agents do not read it. Include it again to list it, or exclude it to remove the registration.")));
        }
    }

    private static DoctorFinding Create(RouteIncludedFileObservation file, DoctorFindingDescriptor descriptor)
        => DoctorDomainSupport.Create(descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, file.Path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry,
                file.Registered ? WorkspaceSettingsDefinitions.RelativePath : SourceLogicalPath.LoaderPath),
            [new DoctorAuthoredValueEvidence(file.Path, Location: null)]) with
        { IsIncludedFile = true };
}

using System.Globalization;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceSettingsVersionDoctorFindingFactory
{
    internal static DoctorFinding Create(int version)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Information(
                DoctorFindingKind.WorkspaceSettingsVersionUnknown,
                string.Create(CultureInfo.InvariantCulture,
                    $"{WorkspaceSettingsDefinitions.RelativePath} declares schemaVersion {version}, which this CLI does not recognize. This CLI uses the settings keys it knows.")),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, WorkspaceSettingsDefinitions.RelativePath, WorkspaceSettingsDefinitions.SchemaVersionProperty),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, WorkspaceSettingsDefinitions.RelativePath),
            [new DoctorAuthoredValueEvidence(version.ToString(CultureInfo.InvariantCulture), Location: null)]);
}

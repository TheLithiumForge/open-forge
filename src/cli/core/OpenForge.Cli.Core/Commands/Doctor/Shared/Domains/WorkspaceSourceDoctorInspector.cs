using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceSourceDoctorInspector
{
    internal static void Inspect(RouteDoctorView routes, ICollection<DoctorFinding> findings)
    {
        foreach (var source in routes.Sources)
        {
            foreach (var issue in source.WorkspaceIssues)
            {
                var descriptor = issue.Kind switch
                {
                    RouteWorkspaceSourceIssueKind.FrontmatterMalformed => DoctorDomainSupport.Warning(
                        DoctorFindingKind.WorkspaceFrontmatterMalformed,
                        "The source frontmatter metadata is malformed.",
                        DoctorResolutionLane.ManualDecision),
                    RouteWorkspaceSourceIssueKind.FrontmatterDuplicate => DoctorDomainSupport.Warning(
                        DoctorFindingKind.WorkspaceFrontmatterDuplicate,
                        "The source frontmatter contains one exact duplicate metadata key.",
                        DoctorResolutionLane.ManualDecision),
                    RouteWorkspaceSourceIssueKind.ParseIncomplete => DoctorDomainSupport.Information(
                        DoctorFindingKind.WorkspaceParseIncomplete,
                        "The supported source could not be parsed completely."),
                    _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Kind, "The workspace source issue kind is not defined."),
                };
                Add(findings, issue.Path, descriptor, issue.Location);
            }
        }

    }

    private static void Add(
        ICollection<DoctorFinding> findings,
        string path,
        DoctorFindingDescriptor descriptor,
        Framework.Sources.Models.Locations.SourceLocation? location = null)
        => findings.Add(DoctorDomainSupport.Create(
            descriptor,
            new DoctorSubject
            {
                Kind = DoctorSubjectKind.Path,
                Path = path,
                Identifier = null,
                Location = location,
            },
            new DoctorProvenance
            {
                Domain = DoctorDomainKind.WorkspaceEntry,
                Source = DoctorProvenanceSource.WorkspaceEntry,
                Path = path,
                Location = location,
            },
            [new DoctorStateEvidence(descriptor.Kind switch
            {
                DoctorFindingKind.WorkspaceFrontmatterMalformed => DoctorObservedState.Malformed,
                DoctorFindingKind.WorkspaceFrontmatterDuplicate => DoctorObservedState.Invalid,
                DoctorFindingKind.WorkspaceParseIncomplete => DoctorObservedState.Incomplete,
                DoctorFindingKind.WorkspaceDetached => DoctorObservedState.Present,
                _ => throw new ArgumentOutOfRangeException(nameof(descriptor), descriptor.Kind, "The workspace source finding kind is not defined."),
            })]));
}

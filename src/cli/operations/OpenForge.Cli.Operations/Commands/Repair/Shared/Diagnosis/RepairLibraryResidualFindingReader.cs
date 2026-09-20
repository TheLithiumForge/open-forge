using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairLibraryResidualFindingReader
{
    internal static IEnumerable<RepairFinding> Read(
        IEnumerable<DoctorFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        foreach (var finding in findings)
        {
            if (finding.Proposal is not null
                || finding.Severity == DoctorFindingSeverity.Information
                || !IsLibrarySubject(finding.Subject.Kind))
            {
                continue;
            }

            yield return new RepairFinding(
                RepairFindingCode.ManualFindingRemaining,
                finding.Message,
                ReadSourcePath(finding.Subject))
            {
                Observation = new RepairDiagnosisObservation(finding.Subject.Path, finding.Subject.Identifier),
            };
        }
    }

    private static bool IsLibrarySubject(DoctorSubjectKind kind)
        => kind is DoctorSubjectKind.Library
            or DoctorSubjectKind.LibrarySourceRoot
            or DoctorSubjectKind.LibraryMapping
            or DoctorSubjectKind.LibraryProjection
            or DoctorSubjectKind.LibraryResidual;

    private static string? ReadSourcePath(DoctorSubject subject)
    {
        var path = subject.Kind == DoctorSubjectKind.LibraryResidual
            ? subject.Library?.Residual?.Entry.Input.Context.Entry.TargetPath ?? subject.Path
            : subject.Path;
        if (path is null)
        {
            return null;
        }

        try
        {
            return RepairPathValidation.ValidateMarkdown(path, nameof(path));
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}

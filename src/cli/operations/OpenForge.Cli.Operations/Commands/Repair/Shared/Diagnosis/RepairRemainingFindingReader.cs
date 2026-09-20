using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairRemainingFindingReader
{
    internal static IEnumerable<RepairFinding> ReadLibrary(DoctorDiagnosis diagnosis)
    {
        ArgumentNullException.ThrowIfNull(diagnosis);
        return RepairLibraryResidualFindingReader.Read(
            diagnosis.Domains.SelectMany(domain => domain.Findings));
    }

    internal static IEnumerable<RepairFinding> Read(
        IEnumerable<LocalReferenceObservation> references)
    {
        ArgumentNullException.ThrowIfNull(references);
        foreach (var reference in references)
        {
            if (reference.Facts.Target.Kind != SourceLinkTargetKind.Local)
            {
                continue;
            }

            var resolution = reference.Facts.Target.Resolution;
            if (resolution == SourceLinkTargetResolution.Complete && reference.Canonicalizations.Count == 0)
            {
                continue;
            }

            var guided = resolution == SourceLinkTargetResolution.Missing && reference.Kind == LocalReferenceKind.Link;
            yield return new RepairFinding(
                guided ? RepairFindingCode.GuidedFindingRemaining : RepairFindingCode.ManualFindingRemaining,
                guided ? "A bounded guided local-reference finding remains." : "A local-reference finding remains for explicit review.",
                reference.SourcePath, reference.DestinationLocation);
        }
    }
}

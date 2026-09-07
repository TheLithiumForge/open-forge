using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairRemainingFindingReader
{
    internal static IEnumerable<RepairFinding> Read(LocalReferenceDoctorView view)
    {
        foreach (var reference in view.References)
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

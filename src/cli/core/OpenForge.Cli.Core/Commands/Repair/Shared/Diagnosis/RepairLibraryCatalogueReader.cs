using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairLibraryCatalogueReader
{
    internal static ImmutableArray<RepairLibraryRecoveryProposal> Read(DoctorDiagnosis diagnosis)
    {
        ArgumentNullException.ThrowIfNull(diagnosis);
        var proposals = ImmutableArray.CreateBuilder<RepairLibraryRecoveryProposal>();
        foreach (var finding in diagnosis.Domains.SelectMany(domain => domain.Findings))
        {
            switch (finding.Proposal?.Kind)
            {
                case null:
                case DoctorProposalKind.ReferenceCanonicalization:
                    break;
                case DoctorProposalKind.LibraryResidualRecovery:
                    proposals.Add(new RepairLibraryRecoveryProposal(finding.Proposal?.LibraryRecovery
                        ?? throw new ArgumentException("A Library recovery proposal payload is required.", nameof(diagnosis))));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosis), "The Doctor proposal kind is not defined.");
            }
        }

        return proposals.ToImmutable();
    }
}

using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorProposalHumanRenderer
{
    internal static void Append(StringBuilder builder, DoctorExactProposal proposal, CliView view)
    {
        switch (proposal.Kind)
        {
            case DoctorProposalKind.ReferenceCanonicalization:
                var reference = proposal.Reference ?? throw new ArgumentException("A reference proposal payload is required.", nameof(proposal));
                builder.AppendLine($"""
                        Proposed equivalent link:
                          Current: {DoctorHumanRenderer.Text(reference.ExpectedValue)}
                          Proposed: {DoctorHumanRenderer.Text(reference.IntendedValue)}
                    """);
                break;
            case DoctorProposalKind.LibraryResidualRecovery:
                builder.AppendLine("    Proposed Library recovery:");
                DoctorLibraryRecoveryPresentation.Append(builder,
                    proposal.LibraryRecovery ?? throw new ArgumentException("A Library recovery payload is required.", nameof(proposal)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(proposal), proposal.Kind, "The Doctor proposal kind is not defined.");
        }

        DoctorEvidenceHumanRenderer.AppendSubject(builder, proposal.Subject, "      ");
        if (view == CliView.Expanded)
        {
            builder.AppendLine($"""
                      Applies within: {DoctorWireVocabulary.Boundary(proposal.Boundary.Kind)} {DoctorHumanRenderer.Text(proposal.Boundary.Path ?? "unavailable")}
                      Verification: {Verification(proposal.Verification)}
                      Recovery: {Recovery(proposal.Recovery)}
                """);
        }
    }

    private static string Verification(DoctorProposalVerificationKind value) => value switch
    {
        DoctorProposalVerificationKind.SameTargetIdentity => "the link must still reach the same target",
        DoctorProposalVerificationKind.ResultingBytes => "check the resulting file bytes",
        DoctorProposalVerificationKind.NoFollowPriorState => "check the recorded prior state without following links",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The proposal verification is not defined."),
    };

    private static string Recovery(DoctorProposalRecoveryKind value) => value switch
    {
        DoctorProposalRecoveryKind.NoPersistentState => "no persistent recovery state",
        DoctorProposalRecoveryKind.RepairReceiptRequired => "a Repair receipt is required",
        DoctorProposalRecoveryKind.VerifiedLibraryResidual => "use the verified Library recovery record",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The proposal recovery is not defined."),
    };
}

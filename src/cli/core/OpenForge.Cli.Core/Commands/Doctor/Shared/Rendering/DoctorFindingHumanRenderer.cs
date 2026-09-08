using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorFindingHumanRenderer
{
    internal static void Append(
        StringBuilder builder,
        IReadOnlyList<DoctorFinding> findings,
        CliView view)
    {
        foreach (var finding in findings)
        {
            builder.AppendLine(
                $"  {DoctorFindingWireVocabulary.Severity(finding.Severity)} {DoctorDefinitions.ReadFindingKind(finding.Kind)}: {DoctorHumanRenderer.Text(finding.Message)}");
            AppendSubject(builder, finding.Subject, view);
            builder.AppendLine($"    resolution: {DoctorFindingWireVocabulary.Resolution(finding.Resolution)}");
            if (finding.Candidates is { } candidates)
            {
                builder.AppendLine($"    candidates: {DoctorFindingWireVocabulary.Cardinality(candidates.Cardinality)}");
                foreach (var candidate in candidates.Items)
                {
                    AppendSubject(builder, candidate.Subject, view, "      ");
                    if (view == CliView.Expanded)
                    {
                        foreach (var basis in candidate.Evidence)
                        {
                            builder.AppendLine($"        basis: {DoctorFindingWireVocabulary.CandidateBasis(basis.Kind)} {DoctorHumanRenderer.Text(basis.Value ?? "unavailable")}");
                            AppendLocation(builder, basis.Location, "          ");
                        }

                        AppendProvenance(builder, candidate.Provenance, "        ");
                    }
                }
            }

            if (finding.Proposal is { } proposal)
            {
                AppendProposal(builder, proposal, view);
            }

            if (view == CliView.Expanded)
            {
                AppendEvidence(builder, finding.Evidence);
                AppendProvenance(builder, finding.Provenance, "    ");
            }

            DoctorActionHumanRenderer.Append(builder, finding.Actions, view, "    ");
        }
    }

    private static void AppendSubject(
        StringBuilder builder,
        DoctorSubject subject,
        CliView view,
        string indent = "    ")
    {
        var identity = subject.Path ?? subject.Identifier ?? "unavailable";
        builder.AppendLine($"{indent}subject: {DoctorFindingWireVocabulary.Subject(subject.Kind)} {DoctorHumanRenderer.Text(identity)}");
        if (view == CliView.Expanded && subject.Location is { } location)
        {
            builder.AppendLine($"{indent}location: {location.Line}:{location.Column}");
        }
    }

    private static void AppendEvidence(StringBuilder builder, IReadOnlyList<DoctorEvidence> evidence)
    {
        foreach (var item in evidence)
        {
            var (value, location) = item switch
            {
                DoctorAvailabilityEvidence state => (DoctorWireVocabulary.SourceAvailability(state.State), null),
                DoctorStateEvidence state => (DoctorFindingWireVocabulary.ObservedState(state.State), null),
                DoctorComparisonEvidence comparison => ($"{comparison.Expected} -> {comparison.Actual}", null),
                DoctorIntegrityEvidence integrity => (DoctorFindingWireVocabulary.Integrity(integrity.State), null),
                DoctorAuthoredValueEvidence authored => (authored.Value, authored.Location),
                DoctorCandidateBasisEvidence basis => ($"{DoctorFindingWireVocabulary.CandidateBasis(basis.Basis)} {basis.Value}", basis.Location),
                _ => throw new ArgumentOutOfRangeException(nameof(evidence), item.Kind, "The Doctor evidence kind is not defined."),
            };
            builder.AppendLine($"    evidence {DoctorFindingWireVocabulary.Evidence(item.Kind)}: {DoctorHumanRenderer.Text(value)}");
            AppendLocation(builder, location, "      ");
        }
    }

    private static void AppendProposal(StringBuilder builder, DoctorExactProposal proposal, CliView view)
    {
        builder.AppendLine($"    proposal: {DoctorFindingWireVocabulary.Proposal(proposal.Kind)}");
        if (view != CliView.Expanded)
        {
            return;
        }

        AppendSubject(builder, proposal.Subject, view, "      ");
        switch (proposal.Kind)
        {
            case DoctorProposalKind.ReferenceCanonicalization:
                var reference = proposal.Reference ?? throw new ArgumentException("A reference proposal payload is required.", nameof(proposal));
                builder.AppendLine($"      expected: {DoctorHumanRenderer.Text(reference.ExpectedValue)}");
                builder.AppendLine($"      intended: {DoctorHumanRenderer.Text(reference.IntendedValue)}");
                break;
            case DoctorProposalKind.LibraryResidualRecovery:
                DoctorLibraryRecoveryPresentation.Append(builder,
                    proposal.LibraryRecovery ?? throw new ArgumentException("A Library recovery payload is required.", nameof(proposal)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(proposal), proposal.Kind, "The Doctor proposal kind is not defined.");
        }
        builder.AppendLine($"      boundary: {DoctorWireVocabulary.Boundary(proposal.Boundary.Kind)} {DoctorHumanRenderer.Text(proposal.Boundary.Path ?? "unavailable")}");
        builder.AppendLine($"      verification: {DoctorFindingWireVocabulary.Verification(proposal.Verification)}");
        builder.AppendLine($"      recovery: {DoctorFindingWireVocabulary.Recovery(proposal.Recovery)}");
    }

    private static void AppendProvenance(StringBuilder builder, DoctorProvenance provenance, string indent)
    {
        builder.AppendLine($"{indent}provenance: {DoctorFindingWireVocabulary.Provenance(provenance.Source)}");
        if (provenance.Path is { } path)
        {
            builder.AppendLine($"{indent}  path: {DoctorHumanRenderer.Text(path)}");
        }

        AppendLocation(builder, provenance.Location, $"{indent}  ");
    }

    private static void AppendLocation(
        StringBuilder builder,
        OpenForge.Cli.Core.Framework.Sources.Models.Locations.SourceLocation? location,
        string indent)
    {
        if (location is not null)
        {
            builder.AppendLine($"{indent}location: {location.Line}:{location.Column} bytes {location.ByteOffset}+{location.ByteLength}");
        }
    }
}

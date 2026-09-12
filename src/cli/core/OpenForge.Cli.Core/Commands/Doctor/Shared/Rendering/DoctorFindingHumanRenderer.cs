using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorFindingHumanRenderer
{
    internal static void Append(StringBuilder builder, IReadOnlyList<DoctorFinding> findings, CliView view)
    {
        foreach (var group in findings.GroupBy(finding => finding.Subject))
        {
            builder.AppendLine();
            DoctorEvidenceHumanRenderer.AppendSubject(builder, group.Key, "  ");
            var explanations = new HashSet<string>(StringComparer.Ordinal);
            foreach (var finding in group)
            {
                var heading = $"{DoctorHumanVocabulary.Severity(finding.Severity)}  {DoctorFindingTitles.Read(finding.Kind)} [{DoctorDefinitions.ReadFindingKind(finding.Kind)}]";
                if (view == CliView.Compact)
                {
                    builder.AppendLine($"  {heading}; {DoctorHumanVocabulary.Resolution(finding.Resolution)}");
                }
                else
                {
                    builder.AppendLine($"  {heading}");
                }

                if (HasSeparateExplanation(finding.Kind) && explanations.Add(finding.Message))
                {
                    builder.AppendLine($"    {DoctorHumanRenderer.Text(finding.Message)}");
                }

                if (view == CliView.Expanded)
                {
                    builder.AppendLine($"    Resolution: {DoctorHumanVocabulary.Resolution(finding.Resolution)}");
                }
            }

            foreach (var candidates in group.Select(finding => finding.Candidates).OfType<DoctorCandidateSet>().Distinct(DoctorCandidateSetComparer.Instance))
            {
                AppendCandidates(builder, candidates, view);
            }

            foreach (var proposal in group.Select(finding => finding.Proposal).OfType<DoctorExactProposal>().Distinct())
            {
                DoctorProposalHumanRenderer.Append(builder, proposal, view);
            }

            if (view == CliView.Expanded)
            {
                DoctorEvidenceHumanRenderer.Append(builder, group.SelectMany(finding => finding.Evidence).Distinct());
                foreach (var provenance in group.Select(finding => finding.Provenance).Distinct())
                {
                    DoctorEvidenceHumanRenderer.AppendProvenance(builder, provenance, group.Key, "    ");
                }
            }

            DoctorActionHumanRenderer.Append(builder, group.SelectMany(finding => finding.Actions).Distinct().ToArray(), view, "    ");
        }
    }

    private static void AppendCandidates(StringBuilder builder, DoctorCandidateSet candidates, CliView view)
    {
        builder.AppendLine($"    Possible targets: {candidates.Items.Count}; none selected");
        foreach (var candidate in candidates.Items)
        {
            DoctorEvidenceHumanRenderer.AppendSubject(builder, candidate.Subject, "      ");
            if (view == CliView.Expanded)
            {
                foreach (var match in candidate.Evidence.Distinct())
                {
                    builder.AppendLine($"        {DoctorHumanVocabulary.CandidateBasis(match.Kind)}{DoctorEvidenceHumanRenderer.Location(match.Location)}: {DoctorHumanRenderer.Text(match.Value ?? "unavailable")}");
                }

                DoctorEvidenceHumanRenderer.AppendProvenance(builder, candidate.Provenance, candidate.Subject, "        ");
            }
        }
    }

    private static bool HasSeparateExplanation(DoctorFindingKind kind)
        => kind is not (DoctorFindingKind.ReferenceTargetValid
            or DoctorFindingKind.ReferenceCandidateFilename
            or DoctorFindingKind.ReferenceCandidateTitle
            or DoctorFindingKind.ReferenceCandidateLiteralContent
            or DoctorFindingKind.ReferenceCandidateRouteNeighborhood
            or DoctorFindingKind.ReferenceCandidatesNone
            or DoctorFindingKind.ReferenceCandidatesOne
            or DoctorFindingKind.ReferenceCandidatesSeveral);
}

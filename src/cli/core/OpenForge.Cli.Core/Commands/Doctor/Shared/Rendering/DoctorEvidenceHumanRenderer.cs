using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorEvidenceHumanRenderer
{
    internal static void AppendSubject(StringBuilder builder, DoctorSubject subject, string indent)
    {
        builder.AppendLine($"{indent}{DoctorHumanVocabulary.Subject(subject.Kind)}: {DoctorHumanRenderer.Text(subject.Path ?? subject.Identifier ?? "unavailable")}{Location(subject.Location)}");
        if (subject.Path is not null && subject.Identifier is { } identifier && !string.Equals(subject.Path, identifier, StringComparison.Ordinal))
        {
            var label = subject.Kind == DoctorSubjectKind.SourceOccurrence ? "Destination" : "ID";
            builder.AppendLine($"{indent}  {label}: {DoctorHumanRenderer.Text(identifier)}");
        }
    }

    internal static void Append(StringBuilder builder, IEnumerable<DoctorEvidence> evidence)
    {
        foreach (var item in evidence)
        {
            var text = item switch
            {
                DoctorAvailabilityEvidence state => $"Source: {DoctorWireVocabulary.SourceAvailability(state.State)}",
                DoctorStateEvidence state => $"Observed: {DoctorFindingWireVocabulary.ObservedState(state.State)}",
                DoctorComparisonEvidence comparison => $"Expected: {DoctorHumanRenderer.Text(comparison.Expected)}; observed: {DoctorHumanRenderer.Text(comparison.Actual)}",
                DoctorIntegrityEvidence integrity => $"Verification: {DoctorFindingWireVocabulary.Integrity(integrity.State)}",
                DoctorAuthoredValueEvidence authored => $"Written value{Location(authored.Location)}: {DoctorHumanRenderer.Text(authored.Value)}",
                DoctorCandidateBasisEvidence match => $"{DoctorHumanVocabulary.CandidateBasis(match.Basis)}{Location(match.Location)}: {DoctorHumanRenderer.Text(match.Value ?? "unavailable")}",
                _ => throw new ArgumentOutOfRangeException(nameof(evidence), item.Kind, "The Doctor evidence kind is not defined."),
            };
            builder.AppendLine($"    {text}");
        }
    }

    internal static void AppendProvenance(StringBuilder builder, DoctorProvenance provenance, DoctorSubject subject, string indent)
    {
        var source = DoctorHumanVocabulary.Provenance(provenance.Source);
        if (provenance.Path is { } path && (path != subject.Path || provenance.Location != subject.Location))
        {
            builder.AppendLine($"{indent}Read from: {source}, {DoctorHumanRenderer.Text(path)}{Location(provenance.Location)}");
        }
        else
        {
            builder.AppendLine($"{indent}Read from: {source}");
            if (provenance.Path is null && provenance.Location is not null)
            {
                builder.AppendLine($"{indent}  Source position{Location(provenance.Location)}");
            }
        }
    }

    internal static string Location(SourceLocation? location)
        => location is null ? string.Empty : string.Create(CultureInfo.InvariantCulture, $":{location.Line}:{location.Column}");
}

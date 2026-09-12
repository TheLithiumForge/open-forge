using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorDiagnosticRenderer
{
    internal static string? Render(CliPresentationRequest<DoctorResult> presentation)
    {
        var entries = presentation.Result.Diagnosis.Domains
            .SelectMany(domain => domain.Limitations.Select(limitation => (
                Domain: domain.Domain,
                Kind: limitation.Kind,
                limitation.Message)))
            .ToArray();
        if (entries.Length == 0)
        {
            return null;
        }

        var builder = new StringBuilder("Doctor diagnostics");
        foreach (var entry in entries)
        {
            builder.AppendLine();
            builder.Append(DoctorWireVocabulary.Domain(entry.Domain));
            builder.Append('/');
            builder.Append(DoctorWireVocabulary.Limitation(entry.Kind));
            builder.Append(": ");
            builder.Append(DoctorHumanRenderer.Text(entry.Message));
        }

        return builder.ToString();
    }
}

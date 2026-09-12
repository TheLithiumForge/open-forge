using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusDiagnosticRenderer
{
    internal static string? Render(CliPresentationRequest<StatusResult> presentation)
    {
        if (presentation.Result.Findings.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder("Status diagnostics");
        foreach (var finding in presentation.Result.Findings)
        {
            builder.AppendLine();
            builder.Append(StatusDefinitions.ReadFindingCode(finding.Code));
            if (finding.Subject is { } subject)
            {
                builder.Append(": ");
                builder.Append(Bounded(subject));
            }
        }

        return builder.ToString();
    }

    private static string Bounded(string value)
    {
        const int maximum = 256;
        var safe = string.Concat(value.Select(character => char.IsControl(character) ? '\uFFFD' : character));
        return safe.Length <= maximum ? safe : safe[..maximum];
    }
}

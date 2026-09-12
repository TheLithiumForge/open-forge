using System.Globalization;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextDiagnosticRenderer
{
    private const int MaximumLength = 4095;

    internal static string? Render(CliPresentationRequest<ContextResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace={Escape(result.Workspace?.LexicalRoot ?? "none")}",
            string.Create(CultureInfo.InvariantCulture, $"requested={result.Selection.RequestedSources.Count}"),
            FormatSourceCount(result.Selection.SourceCount),
            string.Create(CultureInfo.InvariantCulture, $"links={result.Links.Count}"),
            string.Create(CultureInfo.InvariantCulture, $"findings={result.Findings.Count}"),
            $"next={(result.Next is null ? "none" : "present")}",
        };
        values.AddRange(result.Findings.Select(finding =>
            $"finding={ContextDefinitions.Read(finding.Code).Code}:subject={Escape(finding.Subject ?? "none")}:cause={Escape(finding.Cause)}"));
        var diagnostic = string.Join("; ", values);
        return diagnostic.Length <= MaximumLength ? diagnostic : diagnostic[..MaximumLength];
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal);

    private static string FormatSourceCount(int? count)
        => count is { } value
            ? string.Create(CultureInfo.InvariantCulture, $"sources={value}")
            : "sources=unknown";
}

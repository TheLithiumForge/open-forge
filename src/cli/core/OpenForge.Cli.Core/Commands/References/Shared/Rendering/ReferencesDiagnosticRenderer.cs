using System.Globalization;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesDiagnosticRenderer
{
    private const int MaximumDiagnosticLength = 4095;

    internal static string? Render(CliPresentationRequest<ReferencesResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var values = new List<string>
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace={ReadWorkspace(result)}",
            $"source={ReadSource(result)}",
            $"direction={ReadDirection(result)}",
            $"incoming={ReadSection(result.Incoming)}",
            $"outgoing={ReadSection(result.Outgoing)}",
            $"findings={result.Findings.Count.ToString(CultureInfo.InvariantCulture)}",
            $"next={(result.Next is null ? "none" : "present")}",
        };
        foreach (var finding in result.Findings)
        {
            var subject = finding.Subject is null
                ? "none"
                : ReferencesTextEscaping.Clamp(
                    ReferencesTextEscaping.Escape(finding.Subject),
                    ReferencesTextEscaping.DiagnosticValueLimit);
            values.Add(
                $"finding={ReferencesDefinitions.ReadMachineName(finding.Code)}:subject={subject}:cause={ReferencesTextEscaping.Escape(finding.Cause)}");
        }

        return ReferencesTextEscaping.Clamp(
            string.Join("; ", values),
            MaximumDiagnosticLength);
    }

    private static string ReadWorkspace(ReferencesResult result)
        => result.Workspace is null
            ? "none"
            : ReferencesTextEscaping.Clamp(
                ReferencesTextEscaping.Escape(result.Workspace.LexicalRoot),
                ReferencesTextEscaping.DiagnosticValueLimit);

    private static string ReadSource(ReferencesResult result)
        => result.Source is null
            ? "none"
            : ReferencesTextEscaping.Clamp(
                ReferencesTextEscaping.Escape(result.Source.Id),
                ReferencesTextEscaping.DiagnosticValueLimit);

    private static string ReadDirection(ReferencesResult result)
        => result.RequestedDirection?.ToString().ToLowerInvariant() ?? "none";

    private static string ReadSection(Models.Result.ReferencesSection? section)
        => section is null
            ? "none"
            : $"{ReadCoverage(section.Coverage)}/{CliStatusDefinitions.Read(section.Status).MachineName}/{section.OccurrenceCount.ToString(CultureInfo.InvariantCulture)}";

    private static string ReadCoverage(ReferencesCoverage coverage)
        => coverage switch
        {
            ReferencesCoverage.Complete => "complete",
            ReferencesCoverage.Incomplete => "incomplete",
            ReferencesCoverage.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The References coverage is not defined."),
        };
}

using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorHumanRenderer
{
    internal static string Render(CliPresentationRequest<DoctorResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var view = presentation.Presentation.View;
        var builder = new StringBuilder();
        CliHumanText.AppendHeader(builder, presentation, $"{DoctorHumanVocabulary.Outcome(result.Status)} No files changed.");
        builder.AppendLine($"Checks: {DoctorWireVocabulary.Coverage(result.Diagnosis.Coverage)}");
        DoctorCountsHumanRenderer.Append(builder, result.Diagnosis.Counts, string.Empty);
        foreach (var domain in result.Diagnosis.Domains)
        {
            builder.AppendLine();
            AppendDomain(builder, domain, view);
        }

        var represented = result.Diagnosis.Domains
            .SelectMany(domain => domain.Actions.Concat(domain.Findings.SelectMany(finding => finding.Actions)))
            .ToHashSet();
        DoctorActionHumanRenderer.Append(builder, result.Diagnosis.Actions.Where(action => !represented.Contains(action)).ToArray(), view, string.Empty);
        return builder.ToString().TrimEnd();
    }

    private static void AppendDomain(StringBuilder builder, DoctorDomainReport domain, CliView view)
    {
        builder.AppendLine($"{DoctorHumanVocabulary.Domain(domain.Domain)}: checks {DoctorWireVocabulary.Coverage(domain.Coverage)}");
        if (domain.Lifecycle is { } lifecycle)
        {
            builder.AppendLine($"  Installation record: {DoctorWireVocabulary.Lifecycle(lifecycle)}");
        }

        if (domain.SourceAvailability is { } source)
        {
            builder.AppendLine($"  Source: {DoctorWireVocabulary.SourceAvailability(source)}");
        }

        if (domain.Libraries is { } libraries)
        {
            DoctorLibraryPresentation.Append(builder, libraries);
        }

        DoctorCountsHumanRenderer.Append(builder, domain.Counts, "  ");
        foreach (var limitation in domain.Limitations)
        {
            builder.AppendLine($"  Check {DoctorWireVocabulary.Limitation(limitation.Kind)}: {Text(limitation.Message)}");
        }

        DoctorFindingHumanRenderer.Append(builder, domain.Findings, view);
        var represented = domain.Findings.SelectMany(finding => finding.Actions).ToHashSet();
        DoctorActionHumanRenderer.Append(builder, domain.Actions.Where(action => !represented.Contains(action)).ToArray(), view, "  ");
    }

    internal static string Text(string value)
        => CliHumanText.Text(value);
}

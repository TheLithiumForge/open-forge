using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorHumanRenderer
{
    internal static string Render(CliPresentationRequest<DoctorResult> presentation)
    {
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var result = presentation.Result;
        var builder = new StringBuilder();
        builder.AppendLine("Open Forge doctor");
        builder.AppendLine($"Workspace: {Text(result.Workspace?.LexicalRoot ?? "unavailable")}");
        builder.AppendLine($"Selected by: {Selection(result)}");
        builder.AppendLine($"Result: {DoctorWireVocabulary.Status(result.Status)}");
        builder.AppendLine($"Coverage: {DoctorWireVocabulary.Coverage(result.Diagnosis.Coverage)}");
        builder.AppendLine("Read-only: yes; changes made: no");
        builder.AppendLine($"Limitations: {result.Diagnosis.Domains.Sum(domain => domain.Limitations.Count)}");
        DoctorCountsHumanRenderer.Append(builder, result.Diagnosis.Counts, string.Empty);
        DoctorActionHumanRenderer.Append(builder, result.Diagnosis.Actions, presentation.Presentation.View, string.Empty);
        builder.AppendLine();
        foreach (var domain in result.Diagnosis.Domains)
        {
            AppendDomain(builder, domain, presentation.Presentation.View);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendDomain(StringBuilder builder, DoctorDomainReport domain, CliView view)
    {
        builder.AppendLine(DoctorWireVocabulary.Domain(domain.Domain));
        builder.AppendLine($"  coverage: {DoctorWireVocabulary.Coverage(domain.Coverage)}");
        if (domain.Lifecycle is { } lifecycle)
        {
            builder.AppendLine($"  lifecycle: {DoctorWireVocabulary.Lifecycle(lifecycle)}");
        }

        if (domain.SourceAvailability is { } source)
        {
            builder.AppendLine($"  source availability: {DoctorWireVocabulary.SourceAvailability(source)}");
        }

        DoctorCountsHumanRenderer.Append(builder, domain.Counts, "  ");
        foreach (var limitation in domain.Limitations)
        {
            builder.AppendLine($"  limitation {DoctorWireVocabulary.Limitation(limitation.Kind)}: {Text(limitation.Message)}");
        }

        DoctorActionHumanRenderer.Append(builder, domain.Actions, view, "  ");
        DoctorFindingHumanRenderer.Append(builder, domain.Findings, view);
    }

    private static string Selection(DoctorResult result)
        => result.Workspace is { } workspace
            ? DoctorWireVocabulary.WorkspaceSelection(workspace.SelectedBy)
            : "unavailable";

    internal static string Text(string value)
    {
        const int maximum = 512;
        var safe = string.Concat(value.Select(character => char.IsControl(character) ? '\uFFFD' : character));
        return safe.Length <= maximum ? safe : safe[..maximum];
    }
}
